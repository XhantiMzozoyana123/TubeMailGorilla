Get-ChildItem -Recurse -Include *.cs,*.xaml,*.cshtml,*.json -Path 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked' | Select-Object FullName | Out-File 'c:\Users\Shadow\Documents\TubeMailGorilla\unlocked_files.txt'
"---MVC---" | Out-File 'c:\Users\Shadow\Documents\TubeMailGorilla\mvc_files.txt'
Get-ChildItem -Recurse -Include *.cs,*.cshtml,*.json -Path 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Mvc' | Select-Object FullName | Out-File 'c:\Users\Shadow\Documents\TubeMailGorilla\mvc_files.txt' -Append
"done" | Out-File 'c:\Users\Shadow\Documents\TubeMailGorilla\list_done.txt'
