<a href="https://github.com/MxLabs/Anviz/actions/workflows/nuget.yml"><img src="https://github.com/MxLabs/Anviz/actions/workflows/nuget.yml/badge.svg"></a> <a href="https://www.nuget.org/packages/Anviz.SDK"><img src="https://img.shields.io/nuget/v/anviz.sdk.svg?style=flat"></a>

# Anviz

.NET library to access and control Anviz devices.

Features 
----------------

- Clear records
- Delete the designated user data
- Enroll user fingerprint or card
- Get advanced device information
- Get basic device information
- Get the date and time
- Get device S/N
- Get device ID
- Get device type information
- Download fingerprint template
- Get record information
- Download records
- Download employees data
- Get TCP/IP parameters
- Reboot device
- Recover to factory settings
- Get and set facepass templates
- Set advanced settings
- Set basic settings
- Set the date and time
- Set device S/N
- Set device ID
- Upload fingerprint template
- Upload staff information
- Upload records
- Set TCP/IP parameters
- Send signal to open lock without verifying user
- Receive device pings
- Read packets asynchronously
- RealTime support
- Automatically answer ping requests
- Get/set TimeZone information
- Get/set Schedule Bells
- Synchronize employees and fingerprints on many devices in parallel, sending only what changed
- `AnvizServer`: many devices at once, by serial number, listening for devices in client mode and/or connecting to devices in server mode, with reconnection, heartbeat and events
- `AnvizRecordCollector`: every attendance record of every device, those made while disconnected too, handed to your store and deleted from the device once stored
- Find devices and set their network over UDP broadcast, as CrossChex does
- Know and clear the administrator flag of users
- Get the device special state (door, sensor, lock) and its capacity
- Get/set the vendor's extended information
- Get/set daylight saving time
- Get/set the server URL and DNS of client mode
- Get/set device configuration 3 (Wiegand, connection password requirement); get configuration 4
- Get/set access groups' time zones
- Get/set attendance statuses, by index or customized by name, and their automatic switching
- Get/set the information code the device shows
- Get, add and delete messages shown to users
- Get/set display languages, WiFi networks and reboot times (models that support them)
- Get and clear personnel changes, check whether a user exists (models that support them)
- Add and delete departments (OA1000)
- Get/set administrator cards (T5A) and password (T50)

Tested devices
--------

 - VF Series
 - W1 Pro
 - P7
 - Facepass 7
 - WF30 Pro
 - A350C
 - M7
 - A380
 - W2 Pro

Contributing
--------

See [CONTRIBUTING.md](CONTRIBUTING.md) for branches, tests and how releases are published.
