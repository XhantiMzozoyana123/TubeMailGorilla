param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/AIService.cs'
$lines = Get-Content $p
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'Vision|GenerateEdit|RenderSnapshot|Async|SupportedVision|IsVision') {
        Write-Output ('LINE ' + $i + ' : ' + $lines[$i])
    }
}
