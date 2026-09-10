# EGAIS Inspector

Desktop WPF application for human-readable ЕГАИС analysis: UTM status, certificates, marks, Inform A/B, TTN and movement history.

## Stack
- .NET 8 / C# 12 / WPF
- MVVM / CommunityToolkit.Mvvm
- WPF-UI Fluent Design
- EF Core 8 + SQLite
- ZXing.Net
- Velopack
- xUnit

## Security
Private keys and KEP material are never stored in the database or GitHub. GitHub tokens must be protected with Windows DPAPI.

## UTM
The application is designed for local/network UTM endpoints and does not auto-discover arbitrary ports. A UTM connection is configured explicitly by the user.
