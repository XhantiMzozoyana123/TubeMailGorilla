$rootU='c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked'
$filesU=Get-ChildItem -Recurse -File -Path $rootU | Where-Object { $_.FullName -notmatch '\\bin\\' -and $_.FullName -notmatch '\\obj\\' } | ForEach-Object { $_.FullName.Substring($rootU.Length+1) } | Sort-Object
$filesU | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\unlocked_src.txt' -Encoding utf8
$rootM='c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Mvc'
$filesM=Get-ChildItem -Recurse -File -Path $rootM | Where-Object { $_.FullName -notmatch '\\bin\\' -and $_.FullName -notmatch '\\obj\\' } | ForEach-Object { $_.FullName.Substring($rootM.Length+1) } | Sort-Object
$filesM | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\mvc_src.txt' -Encoding utf8
'ok' | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\list2done.txt' -Encoding utf8
