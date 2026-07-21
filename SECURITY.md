# Security Policy

## Supported versions

Security fixes are provided for the latest published version.

| Version | Supported |
|---|---|
| 0.2.x | Yes |
| Earlier | No |

## Reporting a vulnerability

Do not open a public issue for a vulnerability that could expose local information, execute unintended programs, or disrupt display recovery.

Until a dedicated security contact is published, contact RenderNorth privately through the security-reporting option on the repository owner's GitHub profile. Include the affected version, reproduction steps, expected impact, and any proposed mitigation. Remove unrelated machine names and monitor device paths from submitted logs.

Please allow reasonable time for confirmation and remediation before public disclosure.

## Scope notes

The application stores profiles and logs locally beside the executable. Those files may contain the Windows machine name and monitor device paths. The project has no built-in network communication, update service, account system, or telemetry.
