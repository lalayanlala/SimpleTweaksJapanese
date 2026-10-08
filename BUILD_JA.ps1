$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw '.NET 10 SDKが必要です。https://dotnet.microsoft.com/download/dotnet/10.0 からSDKを導入してください。'
}

$hookDirectory = Join-Path $env:APPDATA 'XIVLauncher\addon\Hooks\dev'
if ($env:DALAMUD_HOME) { $hookDirectory = $env:DALAMUD_HOME }
if (-not (Test-Path (Join-Path $hookDirectory 'Dalamud.dll'))) {
    throw 'Dalamudの開発用ファイルが見つかりません。XIVLauncherで一度ゲームを起動してください。'
}

Write-Host 'Simple Tweaks 日本語版をビルドしています…'
& dotnet build '.\SimpleTweaksPlugin.csproj' -c Release "-p:DalamudLibPath=$hookDirectory\"
if ($LASTEXITCODE -ne 0) { throw 'ビルドに失敗しました。上のエラーをご確認ください。' }

$package = Join-Path $PSScriptRoot 'bin\Release\SimpleTweaksJapanese\latest.zip'
if (-not (Test-Path $package)) { throw 'ビルド結果のZIPが見つかりません。' }

$pluginDirectory = Join-Path $PSScriptRoot '..\Plugin'
if (Test-Path $pluginDirectory) { Remove-Item $pluginDirectory -Recurse -Force }
New-Item -ItemType Directory -Path $pluginDirectory | Out-Null
Expand-Archive -Path $package -DestinationPath $pluginDirectory -Force

$manifestPath = Join-Path $pluginDirectory 'SimpleTweaksJapanese.json'
$dllPath = Join-Path $pluginDirectory 'SimpleTweaksJapanese.dll'
if (-not (Test-Path $manifestPath)) { throw 'SimpleTweaksJapanese.json が生成されていません。' }
if (-not (Test-Path $dllPath)) { throw 'SimpleTweaksJapanese.dll が生成されていません。' }
$manifest = Get-Content $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($manifest.InternalName -ne 'SimpleTweaksJapanese') {
    throw "InternalNameが不正です: $($manifest.InternalName)"
}
if ($manifest.AssemblyVersion -ne '1.15.0.7002') {
    throw "AssemblyVersionが不正です: $($manifest.AssemblyVersion)"
}
if (Test-Path (Join-Path $pluginDirectory 'SimpleTweaksPlugin.dll')) {
    throw '旧SimpleTweaksPlugin.dllが混入しています。'
}

foreach ($assetDirectory in @('TweakPreviews', 'Decorations', 'images')) {
    $from = Join-Path $PSScriptRoot $assetDirectory
    if (Test-Path $from) { Copy-Item $from $pluginDirectory -Recurse -Force }
}

Write-Host ''
Write-Host 'ビルド成功: InternalName = SimpleTweaksJapanese'
Write-Host "配布ZIP: $package"
Write-Host "開発用DLL: $dllPath"
