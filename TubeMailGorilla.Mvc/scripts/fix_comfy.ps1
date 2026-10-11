param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/ComfyUiImageGenerationService.cs'
$get = Get-Content $p
$new = $get -replace 'public sealed class ComfyUiImageGenerationService', 'public sealed partial class ComfyUiImageGenerationService'
$new | Set-Content $p -Encoding UTF8
Write-Host ('ComfyUiImageGenerationService replaced')
