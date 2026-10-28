using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System.Collections.Concurrent;

namespace Anviz.SDK.Tests
{
    /// <summary>
    /// The employees and fingerprint templates of a fake device, answering the commands that read
    /// and write them as a device does
    /// </summary>
    public sealed class FakeEmployeeStore
    {
        private const byte GET_RECORD_INFO = 0x3C;
        private const byte GET_STAFF_DATA = 0x72;
        private const byte SET_STAFF_DATA = 0x73;
        private const byte DELETE_STAFF_DATA = 0x4C;
        private const byte DELETE_STAFF_DATA_ALL = 0x4D;
        private const byte SET_FPTEMPLATE = 0x45;
        private const int FINGERS_OFFSET = 35;

        private readonly object sync = new object();
        private readonly SortedDictionary<ulong, byte[]> users = new SortedDictionary<ulong, byte[]>();
        private readonly Dictionary<(ulong, Finger), byte[]> templates = new Dictionary<(ulong, Finger), byte[]>();
        private int cursor;

        /// <summary>
        /// How many times each command was received
        /// </summary>
        public ConcurrentDictionary<byte, int> Commands { get; } = new ConcurrentDictionary<byte, int>();

        /// <summary>
        /// How long writing a fingerprint takes: it is the slow command
        /// </summary>
        public TimeSpan TemplateDelay { get; set; }

        /// <summary>
        /// Called while a template is written, to see how many devices write at the same time
        /// </summary>
        public Action<bool>? OnTemplate { get; set; }

        /// <summary>
        /// Fails the writing of templates, as a full or broken device
        /// </summary>
        public bool FailTemplates { get; set; }

        public FakeEmployeeStore(FakeDevice fake)
        {
            fake.Handler = (cmd, data) => Handle(fake, cmd, data);
        }

        public ICollection<ulong> Ids
        {
            get { lock (sync) { return users.Keys.ToList(); } }
        }

        public UserInfo User(ulong id)
        {
            lock (sync) { return new UserInfo(users[id], 0); }
        }

        public byte[]? Template(ulong id, Finger finger)
        {
            lock (sync) { return templates.TryGetValue((id, finger), out var t) ? t : null; }
        }

        /// <summary>
        /// Puts an employee on the device directly, as if enrolled on it
        /// </summary>
        public void Add(UserInfo user, params Finger[] fingers)
        {
            lock (sync)
            {
                var data = user.ToArray();
                Bytes.Write(2, Fingers.EncodeFingers(fingers)).CopyTo(data, FINGERS_OFFSET);
                users[user.Id] = data;
                foreach (var finger in fingers)
                {
                    templates[(user.Id, finger)] = new byte[338];
                }
            }
        }

        private async Task<Reply?> Handle(FakeDevice fake, byte cmd, byte[] data)
        {
            Commands.AddOrUpdate(cmd, 1, (k, n) => n + 1);
            switch (cmd)
            {
                case GET_RECORD_INFO:
                    lock (sync)
                    {
                        var stats = new byte[18];
                        Bytes.Write(3, (ulong)users.Count).CopyTo(stats, 0);
                        return Reply.Ok(stats);
                    }
                case GET_STAFF_DATA:
                    lock (sync)
                    {
                        if (data[0] == 1)
                        {
                            cursor = 0;
                        }
                        var page = users.Values.Skip(cursor).Take(data[1]).ToList();
                        cursor += page.Count;
                        return Reply.Ok(new[] { (byte)page.Count }.Concat(page.SelectMany(u => u)).ToArray());
                    }
                case SET_STAFF_DATA:
                    lock (sync)
                    {
                        for (var i = 0; i < data[0]; i++)
                        {
                            var user = data.Skip(1 + i * UserInfo.RECORD_LENGTH).Take(UserInfo.RECORD_LENGTH).ToArray();
                            var id = Bytes.Read(user.Take(5).ToArray());
                            // the fingerprint state cannot be set: it stays what it was
                            var fingers = users.TryGetValue(id, out var old) ? old.Skip(FINGERS_OFFSET).Take(2).ToArray() : new byte[2];
                            fingers.CopyTo(user, FINGERS_OFFSET);
                            users[id] = user;
                        }
                        return Reply.Ok(new byte[] { 0x0F, 0xFF });
                    }
                case DELETE_STAFF_DATA:
                    lock (sync)
                    {
                        var id = Bytes.Read(data.Take(5).ToArray());
                        if (!users.Remove(id))
                        {
                            return Reply.Error(ResponseStatus.NoUser);
                        }
                        foreach (var key in templates.Keys.Where(k => k.Item1 == id).ToList())
                        {
                            templates.Remove(key);
                        }
                        return Reply.Ok(Array.Empty<byte>());
                    }
                case DELETE_STAFF_DATA_ALL:
                    lock (sync)
                    {
                        users.Clear();
                        templates.Clear();
                        return Reply.Ok(Array.Empty<byte>());
                    }
                case SET_FPTEMPLATE:
                    {
                        OnTemplate?.Invoke(true);
                        await Task.Delay(TemplateDelay);
                        OnTemplate?.Invoke(false);
                        if (FailTemplates)
                        {
                            return Reply.Error(ResponseStatus.Fail);
                        }
                        lock (sync)
                        {
                            var id = Bytes.Read(data.Take(5).ToArray());
                            var finger = (Finger)(data[5] - 1);
                            if (!users.TryGetValue(id, out var user))
                            {
                                return Reply.Error(ResponseStatus.NoUser);
                            }
                            templates[(id, finger)] = data.Skip(6).ToArray();
                            var enrolled = Fingers.DecodeFingers(Bytes.Read(user.Skip(FINGERS_OFFSET).Take(2).ToArray()));
                            if (!enrolled.Contains(finger))
                            {
                                enrolled.Add(finger);
                            }
                            Bytes.Write(2, Fingers.EncodeFingers(enrolled)).CopyTo(user, FINGERS_OFFSET);
                            return Reply.Ok(Array.Empty<byte>());
                        }
                    }
                default:
                    return fake.DefaultReply(cmd);
            }
        }
    }
}
