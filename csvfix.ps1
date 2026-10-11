$ErrorActionPreference = 'Continue'
Set-Location 'c:\Users\Shadow\Documents\TubeMailGorilla'
git checkout -- Tools/CsvCheck/Program.cs 2>&1 | Out-String | Write-Output
Remove-Item 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\CsvCheck\Debug.cs' -Force -ErrorAction SilentlyContinue
'=== CsvCheck dir now ==='
Get-ChildItem 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\CsvCheck' -Name
