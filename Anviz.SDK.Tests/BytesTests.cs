using Anviz.SDK.Utils;
using System.Text;

namespace Anviz.SDK.Tests
{
    public class BytesTests
    {
        [Fact]
        public void StringsEndAtTheFirstZero()
        {
            Assert.Equal("QWSERF2354234", Bytes.GetAsciiString(Encoding.ASCII.GetBytes("QWSERF2354234\0m")));
            Assert.Equal("Anna", Bytes.GetUnicodeString(Encoding.BigEndianUnicode.GetBytes("Anna\0Old")));
        }

        [Fact]
        public void WriteRejectsValuesThatDoNotFit()
        {
            Assert.Equal(new byte[] { 0xFF, 0xFF, 0xFF }, Bytes.Write(3, 0xFFFFFF));
            Assert.Throws<ArgumentOutOfRangeException>(() => Bytes.Write(3, 0x1000000));
        }
    }
}
