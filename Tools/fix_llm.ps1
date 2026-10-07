$p = 'c:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Maui.Unlocked/Services/LLMService.cs'
$lines = [IO.File]::ReadAllLines($p)
$out = New-Object System.Collections.Generic.List[string]
foreach ($t in $lines) {
  if ($t -match 'userPrompt = string\.Concat\(Enumerable\.Repeat') { continue }
  $out.Add($t)
}
[IO.File]::WriteAllLines($p, $out, [Text.UTF8Encoding]::new($false))
Write-Output ('removed marker line, now ' + $out.Count + ' lines')


