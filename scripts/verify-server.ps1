$ErrorActionPreference = "Stop"

Write-Host "== Verify WebServer =="

dotnet --version | Out-Host

Push-Location WebServer
dotnet restore
dotnet build -c Release
dotnet test -c Release
Pop-Location

Write-Host "OK: WebServer"
