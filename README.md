# MagicNetworkAccess

## Modernization status

This repository has been migrated from legacy .NET Framework 4.5.2 projects to SDK-style projects targeting:

- `net10.0-windows` (Library, Console, Windows Service, Unit Tests)

### What changed

- Converted all `*.csproj` files to SDK-style.
- Replaced legacy Windows Service (`ServiceBase` + installer classes) with Generic Host + `BackgroundService` + `UseWindowsService()`.
- Removed Quartz dependency and replaced hourly ARP refresh with an internal async loop.
- Removed legacy `packages.config` package management in favor of `<PackageReference>`.
- Updated log4net package to `2.0.17`.

### Build requirements

- .NET SDK 10.0 (Windows-compatible SDK/runtime)
- Windows environment for running Service/packet-capture functionality

### Notes

Because raw socket capture and Windows service behavior are OS-specific, runtime validation should be done on a Windows host.
