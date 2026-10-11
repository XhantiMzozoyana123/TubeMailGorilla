$ErrorActionPreference = 'Continue'
$dir = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\CsvCheck'
Remove-Item "$dir\Program.cs" -Force
Remove-Item "$dir\EmailContact.cs" -Force
Remove-Item "$dir\Debug.cs" -Force -ErrorAction SilentlyContinue
Rename-Item "$dir\Program2.cs" 'Program.cs'
'Program2 promoted; stale files removed'
