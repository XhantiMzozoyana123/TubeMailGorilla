$ck = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\ComfyUI\models\checkpoints\v1-5-pruned-emaonly.safetensors'
$o = @()
if (Test-Path $ck) {
  $o += ('CKPT_MB=' + [math]::Round((Get-Item $ck).Length/1MB,1))
} else {
  $o += 'CKPT=not-placed-yet'
  $files = Get-ChildItem 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\ComfyUI\models\checkpoints' -File -ErrorAction SilentlyContinue
  $o += ('ckpt_dir_files=' + $files.Count)
}
$o += '---LOG TAIL---'
$o += (Get-Content 'C:\Users\Shadow\ComfyUI\setup.log' -Tail 3)
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
Write-Output 'POLL3 DONE'
