param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/ContactCsvService.cs'
$get = Get-Content $p
$new = $get -replace 'public static class ContactCsvService', 'public static partial class ContactCsvService'
$new | Set-Content $p -Encoding UTF8
Write-Host ('ContactCsvService replaced')
