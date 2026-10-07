$p = 'c:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Maui.Unlocked/Services/LLMService.cs'
$lines = [IO.File]::ReadAllLines($p)
Write-Output ('total lines: ' + $lines.Length)
$out = New-Object System.Collections.Generic.List[string]
$skipMediaMarkerLine = $false
foreach ($t in $lines) {
  if ($t -match 'var\.mediaMarker\s*=') { Write-Output 'DROPPED mediaMarker line'; continue }
  if ($t -match 'userPrompt = string\.Concat\(Enumerable\.Repeat') { Write-Output 'DROPPED marker-prepend line'; continue }
  if ($t -match '^\s*if\s*\(ResolveImages\(base64Images\) is \{\s*\} images\)\s*$') { Write-Output 'DROPPED stray ResolveImages-if line'; continue }
  if ($t -match '^\s*if\s*\(base64Images is \{\s*Count:\s*>\s*0\s*\} && interactive\.ClipModel is') { Write-Output 'DROPPED stray base64Images-if line'; continue }
  if ($t -match 'LoadImageEmbeds\(image, interactive, cts\.Token\)') { Write-Output 'DROPPED stray LoadImageEmbeds line'; continue }
  if ($t -match '^\s*session = interactive\.CreateSession\(effectiveSystemPrompt\);$') {
    $out.Add('            var session = interactive.CreateSession(effectiveSystemPrompt);')
    Write-Output 'FIXED session declaration'
    continue
  }
  $out.Add($t)
}
[IO.File]::WriteAllLines($p, $out, [Text.UTF8Encoding]::new($true))
Write-Output ('now ' + $out.Count + ' lines')
