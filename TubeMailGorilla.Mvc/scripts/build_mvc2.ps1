param()
cd 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc'
dotnet build 'TubeMailGorilla.Mvc.csproj' > build6.log 2>&1
$exit = $LASTEXITCODE
Write-Host ('BUILD_EXIT=' + $exit)
