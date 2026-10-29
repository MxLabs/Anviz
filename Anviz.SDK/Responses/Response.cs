using Anviz.SDK.Utils;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// The RET value of a response: whether the device could do the command
    /// </summary>
    public enum ResponseStatus : byte
    {
        Success = 0x00, // operation successful
        Fail = 0x01, // operation failed
        Full = 0x04, // user full
        Empty = 0x05, // user empty
        NoUser = 0x06, // user not exist
        TimeOut = 0x08, // capture timeout
        UserOccupied = 0x0A, // user already exists
        FingerOccupied = 0x0B, // fingerprint already exists
    }

    public class Response
    {
        private const int HEADER_LENGTH = 6; // STX + CH + ACK

        public ulong DeviceID { get; }

        public byte[] DATA { get; }

        public byte ResponseCode { get; }

        /// <summary>
        /// The RET value. Packets the device sends by itself (ACK below 0x80) have none and are Success
        /// </summary>
        public ResponseStatus Status { get; }

        internal Response(byte[] data, ulong deviceId, byte ACK, ResponseStatus status)
        {
            DATA = data;
            DeviceID = deviceId;
            ResponseCode = ACK;
            Status = status;
        }

        /// <summary>
        /// Reads a whole packet, also when RET is not success, so the stream stays aligned.
        /// Returns null when the connection is closed
        /// </summary>
        internal static async Task<Response> FromStream(NetworkStream stream, CancellationToken ct)
        {
            var header = new byte[HEADER_LENGTH];
            try
            {
                if (!await ReadAsyncContinuously(stream, header, 0, HEADER_LENGTH, ct, true))
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }

            var STX = header[0];
            if (STX != 0xA5)
            {
                throw new Exception("Invalid header");
            }
            var CH = Bytes.Split(header, 1, 4);
            var ACK = header[5];
            var hasRet = ACK >= 0x80;
            var base_offset = HEADER_LENGTH + (hasRet ? 3 : 2); // [RET] + LEN
            var head = new byte[base_offset];
            header.CopyTo(head, 0);
            await ReadAsyncContinuously(stream, head, HEADER_LENGTH, base_offset - HEADER_LENGTH, ct);

            var status = hasRet ? (ResponseStatus)head[HEADER_LENGTH] : ResponseStatus.Success;
            var LEN = (int)Bytes.Read(Bytes.Split(head, base_offset - 2, 2));

            var data = new byte[base_offset + LEN + 2];
            head.CopyTo(data, 0);
            await ReadAsyncContinuously(stream, data, base_offset, LEN + 2, ct);

            var PacketData = Bytes.Split(data, base_offset, LEN);
            var CRC = Bytes.Split(data, LEN + base_offset, 2);
            var ComputedCRC = (CRC[1] << 8) + CRC[0];
            var ExpectedCRC = CRC16.Compute(data, LEN + base_offset);
            if (ComputedCRC != ExpectedCRC)
            {
                throw new Exception("Invalid CRC");
            }
            return new Response(PacketData, Bytes.Read(CH), ACK, status);
        }

        /// <summary>
        /// Reads exactly size bytes: TCP can deliver a packet in several pieces.
        /// When allowClosed, returns false if the connection is closed before the first byte
        /// </summary>
        private static async Task<bool> ReadAsyncContinuously(NetworkStream stream, byte[] data, int offset, int size, CancellationToken ct, bool allowClosed = false)
        {
            int readed = 0;
            while (readed < size)
            {
                var amount = await stream.ReadAsync(data, offset + readed, size - readed, ct);
                if (amount == 0)
                {
                    if (allowClosed && readed == 0)
                    {
                        return false;
                    }
                    throw new Exception("Partial packet read");
                }
                readed += amount;
            }
            return true;
        }
    }
}
