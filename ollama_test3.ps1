$p = Start-Process -FilePath 'powershell' `
    -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File','c:\Users\Shadow\Documents\TubeMailGorilla\ollama_test3_worker.ps1' `
    -WindowStyle Hidden -PassThru
Write-Output ('OLLAMA TEST3 LAUNCHED pid=' + $p.Id)
