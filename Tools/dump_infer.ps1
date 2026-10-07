$head = [IO.File]::ReadAllLines([IO.Path]::Combine($env:TEMP, 'llm_head.cs'))
for ($i = 394; $i -lt 500 -and $i -lt $head.Count; $i++) {
  Write-Output (('{0}: {1}' -f ($i + 1), $head[$i]))
}
