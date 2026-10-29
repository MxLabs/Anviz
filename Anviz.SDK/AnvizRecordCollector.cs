using Anviz.SDK.Events;
using Anviz.SDK.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anviz.SDK
{
    /// <summary>
    /// Takes the attendance records of devices, as they connect and in real time, hands them to a
    /// store and deletes them from the device once stored: the device keeps a record until the
    /// store took it, so a record made while the device was disconnected, or that the store failed,
    /// comes again later. A record can come twice (after a restart between storing and deleting):
    /// the store should take the same record twice as once.
    /// </summary>
    /// <remarks>
    /// The device is a buffer: the records are deleted with <see cref="AnvizDevice.DeleteAllRecords"/>,
    /// not marked as read, because a W2PRO marks the new records read as they are downloaded, even
    /// when the store then fails.
    /// </remarks>
    public sealed class AnvizRecordCollector : IDisposable
    {
        private readonly Func<RecordBatch, CancellationToken, Task<bool>> store;
        private readonly ConcurrentDictionary<string, Worker> workers = new ConcurrentDictionary<string, Worker>();
        private readonly AnvizServer server;
        private int disposed;

        /// <summary>
        /// Turns the real time mode of a device on when it is added, so that it sends each
        /// record as it is made
        /// </summary>
        public bool EnableRealTime { get; set; } = true;

        /// <summary>
        /// How often a device is asked whether it has records: for devices that do not send them in
        /// real time, or a record whose packet was lost. One command each time
        /// </summary>
        public TimeSpan PollInterval { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// How long to wait after the store did not take the records, or a command failed, before
        /// trying again
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// A command to a device or the store failed; the records stay on the device
        /// </summary>
        public event EventHandler<RecordCollectorErrorEventArgs> Error;

        /// <summary>
        /// Collects from the devices given with <see cref="Add"/>. The store answers true once it
        /// keeps the records: only then they are deleted from the device
        /// </summary>
        public AnvizRecordCollector(Func<RecordBatch, CancellationToken, Task<bool>> store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <summary>
        /// Collects from every device of the server, as it connects
        /// </summary>
        public AnvizRecordCollector(AnvizServer server, Func<RecordBatch, CancellationToken, Task<bool>> store) : this(store)
        {
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            server.DeviceConnected += OnDeviceConnected;
            server.DeviceDisconnected += OnDeviceDisconnected;
        }

        private void OnDeviceConnected(object sender, DeviceConnectedEventArgs e)
        {
            Add(e.Serial, e.Device);
        }

        private void OnDeviceDisconnected(object sender, DeviceDisconnectedEventArgs e)
        {
            // the server disposed the device before raising this: a new connection of the same
            // serial, already added, is connected and stays
            if (workers.TryGetValue(e.Serial, out var worker) && !worker.Device.Connected)
            {
                Remove(e.Serial);
            }
        }

        /// <summary>
        /// Starts collecting from a device; a device already added with this serial is replaced
        /// </summary>
        public void Add(string serial, AnvizDevice device)
        {
            if (Volatile.Read(ref disposed) == 1)
            {
                throw new ObjectDisposedException(nameof(AnvizRecordCollector));
            }
            var worker = new Worker(serial, device);
            workers.AddOrUpdate(serial, worker, (s, old) =>
            {
                old.Stop();
                return worker;
            });
            device.ReceivedRecord += worker.OnRecord;
            worker.Task = Task.Run(() => Run(worker));
        }

        /// <summary>
        /// Stops collecting from a device, leaving its records there
        /// </summary>
        public bool Remove(string serial)
        {
            if (!workers.TryRemove(serial, out var worker))
            {
                return false;
            }
            worker.Stop();
            return true;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 1)
            {
                return;
            }
            if (server != null)
            {
                server.DeviceConnected -= OnDeviceConnected;
                server.DeviceDisconnected -= OnDeviceDisconnected;
            }
            foreach (var serial in workers.Keys.ToList())
            {
                Remove(serial);
            }
        }

        private async Task Run(Worker worker)
        {
            var token = worker.Token;
            if (EnableRealTime)
            {
                try
                {
                    var settings = await worker.Device.GetAdvancedSettings();
                    if (!settings.RealTimeMode)
                    {
                        settings.RealTimeMode = true;
                        await worker.Device.SetAdvancedSettings(settings);
                    }
                }
                catch (Exception ex)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    RaiseError(worker, ex);
                }
            }
            var drain = true;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (drain)
                    {
                        drain = !await Drain(worker);
                    }
                    var woken = await worker.Wait(drain ? RetryDelay : PollInterval);
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    if (!woken)
                    {
                        // the poll, or the retry: one command when the device has no records
                        drain = true;
                    }
                    else if (!drain)
                    {
                        drain = !await StoreRealTime(worker);
                    }
                    // while the store fails, records in real time stay on the device for the next drain
                }
                catch (Exception ex)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    RaiseError(worker, ex);
                    drain = true;
                }
            }
        }

        /// <summary>
        /// Stores the records of the device that are not stored yet, then deletes them all, if no
        /// other record came in the meantime. False when the store did not take them
        /// </summary>
        private async Task<bool> Drain(Worker worker)
        {
            var device = worker.Device;
            // records keep coming: after a few rounds the next drain goes on
            for (var round = 0; round < 5; round++)
            {
                var stats = await device.GetDownloadInformation();
                if (stats.AllRecordAmount == 0)
                {
                    worker.OnDevice.Clear();
                    return true;
                }
                var records = await device.DownloadRecords();
                var fresh = records.Where(r => !worker.Stored(r)).ToList();
                if (fresh.Count > 0)
                {
                    if (!await Store(worker, fresh, RecordSource.Download))
                    {
                        return false;
                    }
                }
                foreach (var record in records)
                {
                    worker.OnDevice.Add(RecordKey.Of(record));
                }
                var after = await device.GetDownloadInformation();
                if (after.AllRecordAmount == (ulong)records.Count)
                {
                    await device.DeleteAllRecords();
                    worker.OnDevice.Clear();
                    return true;
                }
            }
            return true;
        }

        /// <summary>
        /// Stores the records received in real time, then deletes them from the device when it holds
        /// nothing else. False when the store did not take them
        /// </summary>
        private async Task<bool> StoreRealTime(Worker worker)
        {
            var records = worker.TakeReceived().Where(r => !worker.Stored(r)).ToList();
            if (records.Count == 0)
            {
                return true;
            }
            if (!await Store(worker, records, RecordSource.RealTime))
            {
                return false;
            }
            foreach (var record in records)
            {
                worker.OnDevice.Add(RecordKey.Of(record));
            }
            // every record of the device came this way: nothing else to store before deleting
            var stats = await worker.Device.GetDownloadInformation();
            if (stats.AllRecordAmount == (ulong)worker.OnDevice.Count)
            {
                await worker.Device.DeleteAllRecords();
                worker.OnDevice.Clear();
                return true;
            }
            return await Drain(worker);
        }

        private async Task<bool> Store(Worker worker, List<Record> records, RecordSource source)
        {
            bool stored;
            try
            {
                stored = await store(new RecordBatch(worker.Serial, worker.Device, records, source), worker.Token);
            }
            catch (Exception ex)
            {
                if (worker.Token.IsCancellationRequested)
                {
                    throw;
                }
                RaiseError(worker, ex);
                return false;
            }
            if (stored)
            {
                worker.Remember(records);
            }
            return stored;
        }

        private void RaiseError(Worker worker, Exception exception)
        {
            try
            {
                Error?.Invoke(this, new RecordCollectorErrorEventArgs(worker.Serial, worker.Device, exception));
            }
            catch (Exception)
            {
                // nowhere left to report it
            }
        }

        private struct RecordKey : IEquatable<RecordKey>
        {
            private readonly ulong user;
            private readonly long time;
            private readonly byte backup;
            private readonly byte type;
            private readonly ulong work;

            private RecordKey(Record record)
            {
                user = record.UserCode;
                time = record.DateTime.Ticks;
                backup = record.BackupCode;
                type = record.RecordType;
                work = record.WorkType;
            }

            public static RecordKey Of(Record record) => new RecordKey(record);

            public bool Equals(RecordKey other) => user == other.user && time == other.time
                && backup == other.backup && type == other.type && work == other.work;

            public override bool Equals(object obj) => obj is RecordKey other && Equals(other);

            public override int GetHashCode() => (user, time, backup, type, work).GetHashCode();
        }

        private class Worker
        {
            // the records stored recently, so that one downloaded and also received in real time,
            // or received again after its deletion, is stored once
            private const int REMEMBERED = 1000;

            private readonly CancellationTokenSource stop = new CancellationTokenSource();
            private readonly ConcurrentQueue<Record> received = new ConcurrentQueue<Record>();
            private readonly SemaphoreSlim signal = new SemaphoreSlim(0);
            private readonly HashSet<RecordKey> remembered = new HashSet<RecordKey>();
            private readonly Queue<RecordKey> order = new Queue<RecordKey>();

            public string Serial { get; }
            public AnvizDevice Device { get; }
            public CancellationToken Token => stop.Token;
            public Task Task { get; set; }

            /// <summary>
            /// The records the device holds that were stored since it was last emptied
            /// </summary>
            public HashSet<RecordKey> OnDevice { get; } = new HashSet<RecordKey>();

            public Worker(string serial, AnvizDevice device)
            {
                Serial = serial;
                Device = device;
            }

            public void OnRecord(object sender, Record record)
            {
                received.Enqueue(record);
                signal.Release();
            }

            public List<Record> TakeReceived()
            {
                var records = new List<Record>();
                while (received.TryDequeue(out var record))
                {
                    records.Add(record);
                }
                return records;
            }

            /// <summary>
            /// Waits for a record in real time (true) or the delay (false)
            /// </summary>
            public async Task<bool> Wait(TimeSpan delay)
            {
                try
                {
                    var woken = await signal.WaitAsync(delay, stop.Token);
                    // the records arrived together are taken together
                    while (signal.CurrentCount > 0)
                    {
                        signal.Wait(0);
                    }
                    return woken;
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
            }

            public bool Stored(Record record) => remembered.Contains(RecordKey.Of(record));

            public void Remember(IEnumerable<Record> records)
            {
                foreach (var key in records.Select(RecordKey.Of))
                {
                    if (remembered.Add(key))
                    {
                        order.Enqueue(key);
                    }
                }
                while (order.Count > REMEMBERED)
                {
                    remembered.Remove(order.Dequeue());
                }
            }

            public void Stop()
            {
                Device.ReceivedRecord -= OnRecord;
                stop.Cancel();
            }
        }
    }

    public enum RecordSource
    {
        /// <summary>
        /// Downloaded from the device: made while it was not connected, or not stored yet
        /// </summary>
        Download,
        /// <summary>
        /// Sent by the device as it was made
        /// </summary>
        RealTime
    }

    /// <summary>
    /// Records of a device to store
    /// </summary>
    public class RecordBatch
    {
        public string Serial { get; }
        public AnvizDevice Device { get; }
        public IReadOnlyList<Record> Records { get; }
        public RecordSource Source { get; }

        internal RecordBatch(string serial, AnvizDevice device, IReadOnlyList<Record> records, RecordSource source)
        {
            Serial = serial;
            Device = device;
            Records = records;
            Source = source;
        }
    }
}
