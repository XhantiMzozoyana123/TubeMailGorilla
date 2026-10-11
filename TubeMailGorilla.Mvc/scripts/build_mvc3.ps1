param()
cd 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc'
dotnet build 'TubeMailGorilla.Mvc.csproj'
$exit = $LASTEXITCODE
Write-Host ('BUILD_EXIT=' + $exit)
