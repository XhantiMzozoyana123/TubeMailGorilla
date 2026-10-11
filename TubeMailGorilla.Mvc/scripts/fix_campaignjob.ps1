param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/CampaignJob.cs'
$get = Get-Content $p
$new = $get -replace 'public class CampaignJob', 'public partial class CampaignJob'
$new | Set-Content $p -Encoding UTF8
Write-Host ('CampaignJob replaced: {0} -> {1}' -f (($get -join ' ') -split 'public class CampaignJob').Length, ($new -join ' ').Split('public partial class CampaignJob').Length)
