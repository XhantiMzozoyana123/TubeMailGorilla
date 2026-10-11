param()
# Find all EmailService.cs files
Get-ChildItem -Path 'C:/Users/Shadow/Documents/TubeMailGorilla' -Recurse -Filter 'EmailService.cs' -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Output $_.FullName
    Write-Output ('  LINES=' + ([System.IO.File]::ReadAllLines($_.FullName)).Length)
}
