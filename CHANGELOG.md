## Unreleased
BREAKING CHANGES:

* `Bytes.Write` throws `ArgumentOutOfRangeException` when a value does not fit in its bytes, instead of silently truncating it. It is used for employee IDs (5 bytes), cards (4 bytes), the amount of `ClearRecords` (3 bytes), `SetDeviceID`, `AdvancedSettings`, `TcpParameters` and `Record`

IMPROVEMENTS:
* String helpers to write ASCII, UTF-16 and ANSI (Latin-1) fields, and BCD and IPv4 helpers

BUG FIXES:
* Strings end at their first zero: devices leave bytes of an older value after it (a serial number read as `QWSERF2354234\0m`)

## v2.0.18
FEATURES:
* `DeleteEmployeesData()` deletes all the employees, `DeleteEmployeesData(employeeID, method)` deletes one verification method of an employee
* M7 and A350C devices

IMPROVEMENTS:
* `BasicSettings.Language` is now an enum (it was a `byte`)

## v2.0.17
FEATURES:
* `ScheduledBell.HasValue`

IMPROVEMENTS:
* NuGet package built and published by GitHub Actions

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
