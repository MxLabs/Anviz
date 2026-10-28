using Anviz.SDK.Responses;
using System.Text;

namespace Anviz.SDK.Tests
{
    public class UserInfoTests
    {
        [Fact]
        public void AcceptsNamesOf10Characters()
        {
            var user = new UserInfo(1, "0123456789");

            Assert.Equal("0123456789", user.Name);
        }

        [Fact]
        public void RejectsNamesLongerThan10Characters()
        {
            var user = new UserInfo(1, "name");

            Assert.Throws<ArgumentOutOfRangeException>(() => user.Name = "01234567890");
            Assert.Equal("name", user.Name);
        }

        [Fact]
        public void NullNameIsEmpty()
        {
            var user = new UserInfo(1, null!);

            Assert.Equal("", user.Name);
            Assert.All(user.ToArray().Skip(12).Take(20), b => Assert.Equal(0, b));
        }

        [Fact]
        public void NameDoesNotOverwriteTheFieldsAfterIt()
        {
            var user = new UserInfo(1, "0123456789") { Department = 7, Group = 8 };

            var data = user.ToArray();

            Assert.Equal("0123456789", Encoding.BigEndianUnicode.GetString(data, 12, 20));
            Assert.Equal(7, data[32]);
            Assert.Equal(8, data[33]);
        }

        [Fact]
        public void ReadsBackWhatItWrites()
        {
            var user = new UserInfo(12345, "Niccolò") { Card = 987654, Department = 2 };

            var read = new UserInfo(user.ToArray(), 0);

            Assert.Equal(user.Id, read.Id);
            Assert.Equal(user.Name, read.Name);
            Assert.Equal(user.Card, read.Card);
            Assert.Equal(user.Department, read.Department);
        }
    }
}
