param()
$p = 'C:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Mvc/Services/CampaignJob.Send.cs'
$get = Get-Content $p
$new = $get -replace 'public partial class CampaignJob', 'public partial class CampaignJob'
# Ensure it is partial (already partial per the compile error)
$new = $new -replace 'public class CampaignJob', 'public partial class CampaignJob'
$new | Set-Content $p -Encoding UTF8
Write-Host ('CampaignJob.Send.cs replaced')
