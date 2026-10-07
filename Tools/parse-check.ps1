$tokens = $null
$errors = $null
$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\parse-check-result.txt'
try {
  [System.Management.Automation.Language.Parser]::ParseFile('c:\Users\Shadow\Documents\TubeMailGorilla\Tools\Install-Ollama.ps1', [ref]$tokens, [ref]$errors) | Out-Null
  if ($errors.Count) {
    ($errors | ForEach-Object { "{0}: {1}" -f $_.Extent.StartLineNumber, $_.Message }) | Set-Content $out
  } else {
    "PARSE OK" | Set-Content $out
  }
} catch {
  "CHECK FAILED: $($_.Exception.Message)" | Set-Content $out
}
