$ErrorActionPreference = 'Continue'
$dir = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\CsvCheck'
$bak = Join-Path $dir 'Program.cs.bak'
$lines = Get-Content $bak
$out = foreach ($ln in $lines) {
  if ($ln -match 'startsWith') { 'Check(csv.Length > 0 && csv[0] == (char)65279, "build emits UTF-8 BOM for Excel");' }
  elseif ($ln -match 'includes') { 'Check(parsed.Count == 2 && parsed[0].Email == "alice@example.com", "quoted commas do not create phantom columns");' }
  else { $ln }
}
$out | Set-Content $bak -Encoding utf8
Remove-Item (Join-Path $dir 'Program.cs') -Force
Rename-Item $bak 'Program.cs'
'restored with fixed expectations'
