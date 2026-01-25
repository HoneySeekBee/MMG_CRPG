$ErrorActionPreference = "Stop"

& "$PSScriptRoot\verify-server.ps1"
& "$PSScriptRoot\verify-admin.ps1"

Write-Host "OK: All .NET verified"
