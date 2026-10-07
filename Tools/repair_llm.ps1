$p = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\LLMService.cs'
$lines = [IO.File]::ReadAllLines($p)
$out = New-Object System.Collections.Generic.List[string]
$i = 0
while ($i -lt $lines.Count) {
  $n = $i + 1
  $t = $lines[$i]
  if ($n -ge 512 -and $n -le 523) {
    if ($n -eq 512) {
      $out.Add('        // Images are encoded by the vision projector and attached to the')
      $out.Add('        // executor BEFORE the prompt runs, so the model sees the frames.')
      $out.Add('        if (ResolveImages(base64Images) is { } images)')
      $out.Add('        {')
      $out.Add('            foreach (var image in images)')
      $out.Add('                if (!await LoadImageEmbeds(image, interactive, cts.Token).ConfigureAwait(false))')
      $out.Add('                    return "LLM Error: Could not process the image.";')
      $out.Add('        }')
    }
    $i++
    continue
  }
  $out.Add($t)
  $i++
}
[IO.File]::WriteAllLines($p, $out)
Write-Output 'REPAIRED'
