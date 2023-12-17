using Anviz.SDK.Commands;
using Anviz.SDK.Utils;
using System;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    internal class InquireCardCommand : Command
    {
        private const byte INQUIRE_CARD = 0x7E;
        public InquireCardCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(INQUIRE_CARD, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task<ulong> InquireCard(int retries = 5)
        {
            while (retries-- > 0)
            {
                var res = await DeviceStream.SendCommand(new InquireCardCommand(DeviceId));
                var card = Bytes.Read(res.DATA);
                if (card != 0)
                {
                    return card;
                }
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
            return 0;
        }
    }
}