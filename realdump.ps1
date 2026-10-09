$f = 'C:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Services\ComfyUiImageGenerationService.cs'
[System.IO.File]::ReadAllText($f) | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\real_service_22.cs' -Encoding utf8
Write-Output 'DUMPED'
