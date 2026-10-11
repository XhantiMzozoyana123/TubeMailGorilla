param()
cd 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc'
dotnet build --no-restore 'TubeMailGorilla.Mvc.csproj' > build5.log 2>&1
$exit = $LASTEXITCODE
Write-Host ('BUILD_EXIT=' + $exit)
