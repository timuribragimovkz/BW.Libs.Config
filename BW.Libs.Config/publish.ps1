#!/usr/bin/env pwsh
# Publish script for BW.Libs packages to AWS CodeArtifact
# Reads source URL from nuget.config automatically

param(
    [string]$Configuration = "Release",
    [string]$SourceName = "bruceware-libs",
    [string]$AwsProfile = "bruceware"
)

$ErrorActionPreference = "Stop"

Write-Host "🔐 Getting CodeArtifact token (using profile: $AwsProfile)..." -ForegroundColor Cyan
$token = aws codeartifact get-authorization-token `
    --profile $AwsProfile `
    --domain bruceware `
    --domain-owner 560719246675 `
    --region eu-central-1 `
    --query authorizationToken `
    --output text

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to get CodeArtifact token. Is AWS CLI configured?"
    exit 1
}

Write-Host "✅ Token retrieved" -ForegroundColor Green

Write-Host "🔧 Updating NuGet source credentials..." -ForegroundColor Cyan
dotnet nuget update source $SourceName `
    --username aws `
    --password $token `
    --store-password-in-clear-text

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to update NuGet source"
    exit 1
}

Write-Host "✅ Credentials updated" -ForegroundColor Green

Write-Host "🔨 Building $Configuration configuration..." -ForegroundColor Cyan
dotnet build --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}

Write-Host "✅ Build succeeded" -ForegroundColor Green

Write-Host "📦 Pushing packages to $SourceName..." -ForegroundColor Cyan

$packages = Get-ChildItem -Path "." -Recurse -Filter "*.nupkg" | Where-Object {
    $_.FullName -like "*\bin\$Configuration\*" -and $_.Name -notlike "*.symbols.nupkg"
}

if ($packages.Count -eq 0) {
    Write-Error "No packages found in bin\$Configuration folders"
    exit 1
}

foreach ($package in $packages) {
    Write-Host "  → $($package.Name)" -ForegroundColor Yellow
    dotnet nuget push $package.FullName --source $SourceName --api-key aws --skip-duplicate

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to push $($package.Name)"
        exit 1
    }
}

Write-Host "✅ All packages published successfully!" -ForegroundColor Green
