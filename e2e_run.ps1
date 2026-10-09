$py = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\python.exe'
Remove-Item 'C:\Users\Shadow\Documents\TubeMailGorilla\e2e_result_18.txt' -ErrorAction SilentlyContinue
$p = Start-Process -FilePath $py -ArgumentList 'C:\Users\Shadow\Documents\TubeMailGorilla\e2e_test.py' -WindowStyle Hidden -PassThru
$p.Id | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\e2e.pid'
Write-Output ("E2E LAUNCHED pid=" + $p.Id)
