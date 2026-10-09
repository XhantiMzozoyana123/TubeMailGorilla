$py = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\python_embeded\python.exe'
Remove-Item 'C:\Users\Shadow\Documents\TubeMailGorilla\e2e2_result_23.txt' -ErrorAction SilentlyContinue
$p = Start-Process -FilePath $py -ArgumentList 'C:\Users\Shadow\Documents\TubeMailGorilla\e2e_test2.py' -WindowStyle Hidden -PassThru
$p.Id | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\e2e2.pid'
Write-Output ("E2E2 LAUNCHED pid=" + $p.Id)
