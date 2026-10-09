$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$o = @()
$o += '=== build/publish scripts ==='
Get-ChildItem -Path $root -Filter '*.ps1' -File |
  Where-Object { $_.Name -match 'build|publish|run' } |
  ForEach-Object { $o += $_.Name }
$o += '=== solutions ==='
Get-ChildItem -Path $root -Filter '*.sln' -File -ErrorAction SilentlyContinue |
  ForEach-Object { $o += $_.Name }
Get-ChildItem -Path $root -Filter '*.csproj' -File -Recurse -Depth 3 -ErrorAction SilentlyContinue |
  ForEach-Object { $o += ('proj: ' + $_.FullName) }
$o += '=== git status ==='
$g = git -C $root status --short 2>&1 | Select-Object -First 40
$o += $g
$o += '=== README build hints ==='
$o | Set-Content ($root + '\buildinfo_24.txt') -Encoding utf8
Write-Output 'BUILDINFO DONE'
