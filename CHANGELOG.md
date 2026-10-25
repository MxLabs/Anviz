## Unreleased
FEATURES:
* UDP search and network setting of devices (`AnvizDiscovery`, 0x02, 0x03)
* `UserInfo.IsAdmin` and `ClearAdminFlags` (0x3D)
* Special state (0x2F), capacity (0x5D), extended info (0x7A, 0x7B)
* Daylight saving (0x1A, 0x1B), server URL (0x0A, 0x0B), reboot times (0x14, 0x15)
* Device configuration 3 (0x34, 0x35) and 4 (0x36)
* Group time zones (0x52, 0x53), attendance statuses (0x5A, 0x5B), customized statuses (0x70, 0x71), status switching (0x20, 0x21)
* Information code (0x4A, 0x4B), languages (0x18, 0x19), WiFi networks (0x6E, 0x6F)
* Messages (0x56 to 0x59), personnel changes (0x64, 0x1E), user exists (0x0C), departments (0x61, 0x62)
* Administrator cards and password of T5 devices (0x1C, 0x1D)
* `TcpMode.ClientWithServerUrl`

IMPROVEMENTS:
* The fields the protocol calls ANSI are read and written as Latin-1, keeping accents
* `Bytes.Write` says when a value does not fit in its bytes

BUG FIXES:
* Strings end at their first zero: devices leave bytes of an older value after it (a serial number read as `QWSERF2354234\0m`)

## v2.0.16
FEATURES:
* InquireCard command to read current card, like EnrollFingerprint

## v2.0.15
FEATURES:
* Get/set device Scheduled Bells

IMPROVEMENTS:
* `RelayMode` is now an enum
* Check TimeZone ranges

## v2.0.14
FEATURES:
* Get/set device TimeZone information

## v2.0.13
BUG FIXES:
* Pong response was resetting internal state for parsing responses
* `SendCommand` now implements better timeout handling

## v2.0.12
BUG FIXES:
* Nuget signing key error
* `ReceivedRecord` event for RealTime support

## v2.0.11
IMPROVEMENTS:
* Ping requests are now automatically answered

## v2.0.10
IMPROVEMENTS:
* `EnrollFingerprint`: you can now specify number of times to check user FP. Default to 2 (old behaviour)

## v2.0.9
UPGRADE NOTES:
* `GetDeviceID` is now deprecated in favor of `DeviceID` property

## v2.0.0
BREAKING CHANGES:

* `AnvizManager` doesn't need `DeviceID` anymore since it's requested to the connected device automatically
* `TcpParameters` class now implements `IPAddress` for IP fields and `PhysicalAddress` for MAC

FEATURES:

* Set device network info
* Set device SN (only on supported Anviz devices)
* Set device ID
* Reboot device
* Reset device to Factory Settings
* Unlock door
* Get/set basic settings
* Get/set advanced settings
* Set/delete staff data
* Set fingerprint template
* Enroll fingerprint interactively
* Set/get facepass template
* Upload/delete records
* Server mode support
