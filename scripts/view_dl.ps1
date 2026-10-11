param() $lines = Get-Content "Controllers/ContactsController.cs"; $idx = 0; foreach($l in $lines){ if($l -match "DownloadSnapshots"){ Write-Host "LINE [$($idx)] $l" }; $idx++ }
