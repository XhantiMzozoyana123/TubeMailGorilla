$ErrorActionPreference = 'SilentlyContinue'
$log = Join-Path $PSScriptRoot 'url_check.txt'
Remove-Item $log -ErrorAction SilentlyContinue
foreach ($pair in @(
  @{ n='7zr';        u='https://www.7-zip.org/a/7zr.exe' },
  @{ n='comfyui';    u='https://github.com/comfyanonymous/ComfyUI/releases/latest/download/ComfyUI_windows_portable_nvidia.7z' },
  @{ n='sd15';       u='https://huggingface.co/stable-diffusion-v1-5/stable-diffusion-v1-5/resolve/main/v1-5-pruned-emaonly.safetensors' }
)) {
  $code = & curl.exe -s -o NUL -I -L -w '%{http_code} final=%{url_effective}' --max-time 40 $pair.u 2>&1
  Add-Content $log ("{0}: {1}" -f $pair.n, $code)
}
Add-Content $log 'URLCHECK DONE'
Write-Output 'URLCHECK DONE'
