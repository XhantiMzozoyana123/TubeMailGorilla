$root = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked'
$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\llm-callers.txt'
$pattern = 'LLMService|EnsureReadyAsync|StartModelWarmup|SupportsVision|SelectImageSample|GenerateAdvisoryAsync|GenerateTextAsync|\.ModelPath|_llm\.'
$lines = Get-ChildItem $root -Recurse -Include *.cs,*.xaml |
  Select-String -Pattern $pattern |
  Where-Object { $_.Path -notmatch '\\Services\\LLMService\.cs$' } |
  ForEach-Object {
    $rel = $_.Path.Replace($root + '\', '')
    '{0}:{1}: {2}' -f $rel, $_.LineNumber, $_.Line.Trim()
  }
$lines | Set-Content $out
"total: $($lines.Count)" | Add-Content $out
