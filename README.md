# Lockouts

VB.NET Windows service (User Lockout Monitoring Service) that polls Active Directory for locked-out accounts and sends SMTP email alerts using an HTML template. The worker reads a configurable user list and LDAP paths on a one-minute timer, logs to the Application event log, and ships with ServiceSetup MSI projects plus short design docs. Intended for helpdesk and AD ops teams that need timely lockout notifications without watching Event Viewer all day.

**Source last updated:** 2012-07-07

| Field | Value |
| --- | --- |
| Language | VB.NET |
| Target | .NET Framework 2.0 |
| Output | Windows service + MSI setup |

## Solution structure

| Project | Language | Type | Purpose |
| --- | --- | --- | --- |
| `UserLockoutSvc` | VB.NET | Windows service | Poll AD lockouts and email alerts |
| ServiceSetup | WiX/setup | MSI | Installer projects for the service |

## How to open

Open `UserLockoutSvc.sln` in Visual Studio.

## Requirements

- Visual Studio 2012 to 2017
- .NET Framework 2.0
- WiX Toolset / WixSetup extension (for MSI projects)

## Attribution and provenance

Working copy from my Historical Dev folder. No third-party source attribution markers were identified during this review.

## License

MIT. See `LICENSE`.
