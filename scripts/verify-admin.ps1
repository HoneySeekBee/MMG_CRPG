$ErrorActionPreference = "Stop"

Write-Host "== Verify AdminTool =="

dotnet --version | Out-Host

Push-Location AdminTool
dotnet restore
dotnet build -c Release
dotnet test -c Release
Pop-Location

Write-Host "OK: AdminTool"
