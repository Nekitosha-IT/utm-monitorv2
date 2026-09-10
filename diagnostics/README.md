# EGAIS Inspector diagnostics

`latest.log` is the sanitized diagnostic log exported from the running application.

The log is intentionally kept in the repository so development can continue from real runtime failures instead of guesses.

## Publish the current log

From the repository root in PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\publish-diagnostics.ps1
```

The script:

1. reads `%LOCALAPPDATA%\EGAISInspector\logs\egais-inspector-YYYY-MM-DD.log`;
2. removes common secrets/tokens/password-like values;
3. writes `diagnostics/latest.log`;
4. commits and pushes the diagnostic update to `main`.

Do not put private keys, KEP files, passwords, GitHub tokens, or certificate private material into logs.
