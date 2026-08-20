# Changelog

All notable changes to Xbox360 Deployment Toolkit are documented here.

## [0.5.0] - 2026-08-20

### Added

- Automated regression coverage for JSON persistence, validation, reports, hashes and checklist state.
- Windows CI for every push and pull request to `main`.
- Tagged GitHub Release workflow for a self-contained Windows x64 archive.
- Portfolio-focused project overview, architecture and safety documentation.
- Explicit application and assembly version metadata.

### Changed

- Generated executables, symbols, archives and historical distribution folders are excluded from Git.
- The visible application version is now 0.5.

### Security

- Release artifacts are assembled by CI instead of committing binaries to source control.
- FTP host, username and password fields no longer ship with example connection values.
- Existing DPAPI credential storage and simulation-first safety gates remain unchanged.

## [0.4.0]

- Introduced the single-window XDT workspace and reusable WPF design system.
- Added integrated onboarding, deployment checklists, content catalog, FTP transfer and audit reports.
