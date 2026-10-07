$root = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked'
$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\gguf-refs.txt'
$pattern = 'ChatModelPath|VisionModelPath|ModelFileName|VisionModelFileName|GpuLayerCount|ContextSize|ModelDirectory|ModelUrl|ChatModelFile'
$lines = Get-ChildItem $root -Recurse -Include *.cs |
  Select-String -Pattern $pattern |
  ForEach-Object { '{0}:{1}: {2}' -f $_.Path, $_.LineNumber, $_.Line.Trim() }
if (-not $lines) { $lines = @('(no matches)') }
$lines | Set-Content $out
