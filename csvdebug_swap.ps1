$ErrorActionPreference = 'Continue'
Copy-Item 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\CsvCheck\Program.cs' 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\CsvCheck\Program.cs.bak' -Force
Remove-Item 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\CsvCheck\Program.cs' -Force
Rename-Item 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\CsvCheck\Debug.cs' 'Program.cs'
' Municipal swapped to debug'
