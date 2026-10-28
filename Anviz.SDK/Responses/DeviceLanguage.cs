namespace Anviz.SDK.Responses
{
    /// <summary>
    /// A language a device can show (0x18, 0x19)
    /// </summary>
    public enum DeviceLanguage : byte
    {
        SimplifiedChinese = 0,
        TraditionalChinese = 1,
        English = 2,
        French = 3,
        German = 4,
        Spanish = 5,
        Portuguese = 6,
        Italian = 7,
        Bulgarian = 8,
        Slovak = 9,
        Hungarian = 10,
        Slovene = 11,
        Turkish = 12,
        Polish = 13,
        Indonesian = 14,
        Romanian = 15,
        Russian = 16,
        None = 0xFF
    }
}
