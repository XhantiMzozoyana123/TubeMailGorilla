# Launcher: run the Ollama test detached (model load + generation can exceed
# the foreground command budget); worker writes ollama_test_45.txt when done.
$p = Start-Process -FilePath 'powershell' `
    -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File','c:\Users\Shadow\Documents\TubeMailGorilla\ollama_test_worker.ps1' `
    -WindowStyle Hidden -PassThru
Write-Output ('OLLAMA TEST LAUNCHED pid=' + $p.Id)
