namespace Anviz.SDK.Utils
{
    public enum BiometricType
    {
        Unknown = 0,
        Finger = 1,
        Face = 2,
        Iris = 3
    }

    public static class BiometricTypes
    {
        public static BiometricType DecodeBiometricType(byte[] data)
        {
            switch (Bytes.GetAsciiString(data))
            {
                case "FACE7": //FACEPASS7
                    return BiometricType.Face;
                case "W1": //W1 (PRO)
                case "TC-B-N": //TC550
                    return BiometricType.Finger;
                default:
                    return BiometricType.Unknown;
            }
        }
    }
}
