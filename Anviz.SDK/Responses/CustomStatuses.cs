using Anviz.SDK.Utils;
using System;
using System.Collections.Generic;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// The attendance statuses a device offers by name (0x70, 0x71), up to 16: in ANSI, 10 bytes
    /// each, or in UTF-16, 20 bytes each, as the device keeps them: read them first and change
    /// them, to write them back in the same encoding
    /// </summary>
    public class CustomStatuses
    {
        public const int MAX_STATUSES = 16;

        public List<string> Names { get; }
        public bool Unicode { get; }

        public CustomStatuses(bool unicode)
        {
            Names = new List<string>();
            Unicode = unicode;
        }

        internal CustomStatuses(byte[] data)
        {
            Unicode = data.Length > 161;
            var size = Unicode ? 20 : 10;
            Names = new List<string>();
            for (var i = 0; i < data[0] && i < MAX_STATUSES; i++)
            {
                var text = Bytes.Split(data, 1 + i * size, size);
                Names.Add(Unicode ? Bytes.GetUnicodeString(text) : Bytes.GetAnsiString(text));
            }
        }

        internal byte[] ToArray()
        {
            if (Names.Count > MAX_STATUSES)
            {
                throw new ArgumentException("A device offers 16 statuses at most", "Names");
            }
            var size = Unicode ? 20 : 10;
            var ret = new byte[1 + MAX_STATUSES * size];
            ret[0] = (byte)Names.Count;
            for (var i = 0; i < Names.Count && i < MAX_STATUSES; i++)
            {
                var text = Unicode ? Bytes.WriteUnicode(size, Names[i]) : Bytes.WriteAnsi(size, Names[i]);
                text.CopyTo(ret, 1 + i * size);
            }
            return ret;
        }
    }
}
