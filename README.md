# Lockouts

VB.NET Windows service (User Lockout Monitoring Service) that polls Active Directory for locked-out accounts and sends SMTP email alerts using an HTML template. The worker reads a configurable user list and LDAP paths on a one-minute timer, logs to the Application event log, and ships with ServiceSetup MSI projects plus short design docs. Intended for helpdesk and AD ops teams that need timely lockout notifications without watching Event Viewer all day.

**Source last updated:** 2012-07-07

---

## Contents

- `UserLockoutSvc.sln`
- `UserLockoutSvc/` - service source, setup artifacts, and related docs

## Attribution and provenance

No third-party source attribution markers were identified during this review.

## Requirements

- Visual Studio 2012, .NET Framework 2.0

## License

MIT. See `LICENSE`.
