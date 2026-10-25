using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// The administrator cards of a T5A (0x1C, 0x1D): the card that adds users, the one that
    /// deletes them and the duress card, each 24 or 32 bits long
    /// </summary>
    public class AdminCards
    {
        public ulong AddCard { get; set; }
        public ulong DeleteCard { get; set; }
        public ulong DuressCard { get; set; }
        public bool AddCardIs32Bits { get; set; }
        public bool DeleteCardIs32Bits { get; set; }
        public bool DuressCardIs32Bits { get; set; }

        public AdminCards()
        {
        }

        internal AdminCards(byte[] data)
        {
            AddCard = Bytes.Read(Bytes.Split(data, 0, 4));
            DeleteCard = Bytes.Read(Bytes.Split(data, 4, 4));
            DuressCard = Bytes.Read(Bytes.Split(data, 8, 4));
            AddCardIs32Bits = (data[12] & 1) != 0;
            DeleteCardIs32Bits = (data[12] & 2) != 0;
            DuressCardIs32Bits = (data[12] & 4) != 0;
        }

        internal byte[] ToArray()
        {
            var ret = new byte[13];
            Bytes.Write(4, AddCard).CopyTo(ret, 0);
            Bytes.Write(4, DeleteCard).CopyTo(ret, 4);
            Bytes.Write(4, DuressCard).CopyTo(ret, 8);
            ret[12] = (byte)((AddCardIs32Bits ? 1 : 0) | (DeleteCardIs32Bits ? 2 : 0) | (DuressCardIs32Bits ? 4 : 0));
            return ret;
        }
    }
}
