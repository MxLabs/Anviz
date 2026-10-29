using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System.Collections.Concurrent;
using Record = Anviz.SDK.Responses.Record;

namespace Anviz.SDK.Tests
{
    /// <summary>
    /// The attendance records and the real time mode of a fake device, answering the commands that
    /// read and delete them as a device does
    /// </summary>
    public sealed class FakeRecordStore
    {
        private const byte GET_ADVANCED_SETTINGS = 0x32;
        private const byte SET_ADVANCED_SETTINGS = 0x33;
        private const byte GET_RECORD_INFO = 0x3C;
        private const byte GET_RECORDS = 0x40;
        private const byte DELETE_RECORDS = 0x4E;
        private const byte REALTIME_RECORD = 0xDF;

        private readonly object sync = new object();
        private readonly FakeDevice fake;
        private readonly List<byte[]> records = new List<byte[]>();
        private readonly byte[] advanced = new byte[15];
        private int cursor;

        /// <summary>
        /// How many times each command was received
        /// </summary>
        public ConcurrentDictionary<byte, int> Commands { get; } = new ConcurrentDictionary<byte, int>();

        /// <summary>
        /// Called when the records are about to be deleted, to make one right before
        /// </summary>
        public Action? OnDelete { get; set; }

        public bool RealTimeMode
        {
            get { lock (sync) { return advanced[4] > 0; } }
            set { lock (sync) { advanced[4] = (byte)(value ? 1 : 0); } }
        }

        public int Count
        {
            get { lock (sync) { return records.Count; } }
        }

        public FakeRecordStore(FakeDevice fake)
        {
            this.fake = fake;
            fake.Handler = (cmd, data) => Task.FromResult(Handle(cmd, data));
        }

        public static byte[] Bytes14(ulong user, DateTime time)
        {
            var record = new byte[Record.RECORD_LENGTH];
            Bytes.Write(5, user).CopyTo(record, 0);
            Bytes.Write(4, DateConversions.DateTimeToRecord(time)).CopyTo(record, 5);
            return record;
        }

        /// <summary>
        /// A verification on the device: the record is kept, and sent in real time when the mode is
        /// on, twice when asked as a W2PRO does
        /// </summary>
        public async Task Verify(ulong user, DateTime time, int sent = 1)
        {
            var record = Bytes14(user, time);
            bool realTime;
            lock (sync)
            {
                records.Add(record);
                realTime = advanced[4] > 0;
            }
            for (var i = 0; realTime && i < sent; i++)
            {
                await fake.Send(REALTIME_RECORD, record);
            }
        }

        /// <summary>
        /// A record kept without sending it, as one made while disconnected
        /// </summary>
        public void Keep(ulong user, DateTime time)
        {
            lock (sync)
            {
                records.Add(Bytes14(user, time));
            }
        }

        private Reply? Handle(byte cmd, byte[] data)
        {
            Commands.AddOrUpdate(cmd, 1, (k, n) => n + 1);
            switch (cmd)
            {
                case GET_ADVANCED_SETTINGS:
                    lock (sync) { return Reply.Ok((byte[])advanced.Clone()); }
                case SET_ADVANCED_SETTINGS:
                    lock (sync) { data.Take(15).ToArray().CopyTo(advanced, 0); }
                    return Reply.Ok(Array.Empty<byte>());
                case GET_RECORD_INFO:
                    lock (sync)
                    {
                        var stats = new byte[18];
                        Bytes.Write(3, (ulong)records.Count).CopyTo(stats, 12);
                        Bytes.Write(3, (ulong)records.Count).CopyTo(stats, 15);
                        return Reply.Ok(stats);
                    }
                case GET_RECORDS:
                    lock (sync)
                    {
                        if (data[0] != 0)
                        {
                            cursor = 0;
                        }
                        var page = records.Skip(cursor).Take(data[1]).ToList();
                        cursor += page.Count;
                        return Reply.Ok(new[] { (byte)page.Count }.Concat(page.SelectMany(r => r)).ToArray());
                    }
                case DELETE_RECORDS:
                    OnDelete?.Invoke();
                    lock (sync)
                    {
                        if (data[0] == 0)
                        {
                            records.Clear();
                        }
                        return Reply.Ok(Array.Empty<byte>());
                    }
                default:
                    return fake.DefaultReply(cmd);
            }
        }
    }
}
