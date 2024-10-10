﻿namespace Anviz.SDK.Utils
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
        public static BiometricType DecodeBiometricType(string code)
        {
            switch (code)
            {
                case "FACE7": //FACEPASS7
                    return BiometricType.Face;
                case "VF30PRO": //VF30 (PRO)
                case "W1": //W1 (PRO)
                case "TC-B-N": //TC550
                case "M7A+-N": //M7
                    return BiometricType.Finger;
                default:
                    return BiometricType.Unknown;
            }
        }
    }
}
