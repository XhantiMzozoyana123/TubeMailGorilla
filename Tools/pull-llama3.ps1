$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\pull-llama3.log'
$exe = "$env:LOCALAPPDATA\Programs\Ollama\ollama.exe"
'start=' + (Get-Date).ToString('HH:mm:ss') | Set-Content $out
& $exe pull llama3:latest *>> $out
'exit=' + $LASTEXITCODE | Add-Content $out
'done=' + (Get-Date).ToString('HH:mm:ss') | Add-Content $out
& $exe list *>> $out
