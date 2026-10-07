$proj = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\TubeMailGorilla.Maui.Unlocked.csproj'
$log = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\build2.log'
$err = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\build2.err.txt'
'marker=start ' + (Get-Date).ToString('HH:mm:ss') | Set-Content $log
$p = Start-Process -FilePath 'dotnet' -ArgumentList @('build', $proj, '-v', 'minimal', '--nologo') `
    -RedirectStandardOutput $log -RedirectStandardError $err -NoNewWindow -PassThru -Wait
'exit=' + $p.ExitCode | Add-Content $log
'marker=done' | Add-Content $log
