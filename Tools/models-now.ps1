$exe = "$env:LOCALAPPDATA\Programs\Ollama\ollama.exe"
& $exe list | Out-File 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\models-now.txt'
