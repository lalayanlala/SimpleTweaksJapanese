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
Write-Host '日本語版をビルドしています…'
& dotnet build '.\SimpleTweaksPlugin.csproj' -c Release "-p:DalamudLibPath=$hookDirectory\"
if ($LASTEXITCODE -ne 0) { throw 'ビルドに失敗しました。上のエラーをご確認ください。' }
$package = Join-Path $PSScriptRoot 'bin\Release\SimpleTweaksPlugin\latest.zip'
$pluginDirectory = Join-Path $PSScriptRoot '..\Plugin'
if (-not (Test-Path $package)) { throw 'ビルド結果のZIPが見つかりません。' }
Expand-Archive -Path $package -DestinationPath $pluginDirectory -Force
foreach ($assetDirectory in @('TweakPreviews', 'Decorations', 'images')) {
    Copy-Item (Join-Path $PSScriptRoot $assetDirectory) $pluginDirectory -Recurse -Force
}
Write-Host '完了しました。Pluginフォルダにプラグインを出力しました。'
