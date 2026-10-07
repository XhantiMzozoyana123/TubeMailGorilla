$p = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\LLMService.cs'
$t = [IO.File]::ReadAllText($p)
Write-Output 'RESOLVE_IMAGES_PRESENT'
if ($t.Contains('ResolveImages(base64Images)')) { Write-Output 'CALL_OK' }
if ($t.Contains('LoadImageEmbeds')) { Write-Output 'LOAD_OK' }
if ($t.Contains('SelectImageSample')) { Write-Output 'SELECT_OK' }
if ($t.Contains('private List<string>? ResolveImages')) { Write-Output 'DEF_RESOLVE_OK' }
if ($t.Contains('private async Task<bool> LoadImageEmbeds')) { Write-Output 'DEF_LOAD_OK' }
