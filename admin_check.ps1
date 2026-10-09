$o = @()
$id = [Security.Principal.WindowsIdentity]::GetCurrent()
$p = New-Object Security.Principal.WindowsPrincipal($id)
$isAdmin = $p.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
$o += ('IS_ADMIN=' + $isAdmin)
$o += ('USER=' + $id.Name)
# Confirm the driver's max CUDA via nvidia-smi if possible
$o += '---NSMI---'
$o += (& nvidia-smi --query-gpu=driver_version --format=csv,noheader 2>&1)
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\progress.txt'
Write-Output 'ADMINCHECK DONE'
