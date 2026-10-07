$head = [IO.File]::ReadAllLines([IO.Path]::Combine($env:TEMP, 'llm_head.cs'))
for ($i = 0; $i -lt $head.Count; $i++) {
  if ($head[$i] -match '^\s*private .*(\(|=>)') { Write-Output (('{0}: {1}' -f ($i + 1), $head[$i].Trim())) }
}
