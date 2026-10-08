$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$zip = Join-Path $PSScriptRoot 'bin\Release\SimpleTweaksJapanese\latest.zip'
if (-not (Test-Path $zip)) { throw '先にBUILD.batを実行してください。' }
$tmp = Join-Path $env:TEMP ('SimpleTweaksJapaneseVerify_' + [Guid]::NewGuid().ToString('N'))
try {
    Expand-Archive $zip -DestinationPath $tmp
    $manifestPath = Join-Path $tmp 'SimpleTweaksJapanese.json'
    $dllPath = Join-Path $tmp 'SimpleTweaksJapanese.dll'
    if (-not (Test-Path $manifestPath)) { throw 'SimpleTweaksJapanese.json がありません。' }
    if (-not (Test-Path $dllPath)) { throw 'SimpleTweaksJapanese.dll がありません。' }
    $manifest = Get-Content $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($manifest.InternalName -ne 'SimpleTweaksJapanese') { throw "InternalName: $($manifest.InternalName)" }
    if ($manifest.AssemblyVersion -ne '1.15.0.7002') { throw "AssemblyVersion: $($manifest.AssemblyVersion)" }
    if (Test-Path (Join-Path $tmp 'SimpleTweaksPlugin.dll')) { throw '旧DLLが混入しています。' }
    Write-Host '検証OK: 本家と競合しない配布物です。'
} finally {
    if (Test-Path $tmp) { Remove-Item $tmp -Recurse -Force }
}
