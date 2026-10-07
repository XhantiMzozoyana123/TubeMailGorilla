$head = [IO.File]::ReadAllText([IO.Path]::Combine($env:TEMP, 'llm_head.cs'))
foreach ($name in @('LoadImageEmbeds', 'ResolveImages', 'SelectImageSample', 'LoadMediaAsync', '_mtmdParameters', 'MediaMarker')) {
  $count = ([regex]::Matches($head, $name)).Count
  Write-Output ($name + '=' + $count)
}
