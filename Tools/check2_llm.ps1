$p = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\LLMService.cs'
$t = [IO.File]::ReadAllText($p)
foreach ($name in @('LoadImageEmbeds', 'ResolveImages', 'SelectImageSample', 'EnsureLoadedAsync', 'RunTextInferenceAsync', 'RunVisionInferenceAsync')) {
  $count = ([regex]::Matches($t, $name)).Count
  Write-Output ($name + '=' + $count)
}
