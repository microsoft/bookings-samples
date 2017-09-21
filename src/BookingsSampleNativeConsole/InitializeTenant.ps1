# Run this script and login, so the tenant organization will be initialized to use Bookings ODATA API.

Add-Type -Path .\bin\debug\Microsoft.IdentityModel.Clients.ActiveDirectory.dll
Add-Type -Path .\bin\debug\Microsoft.IdentityModel.Clients.ActiveDirectory.Platform.dll

$resource = "https://microsoft.onmicrosoft.com/bookingsodataapi"
$clientID = "0f7c5802-3326-4b3a-a501-f4bd983f2eaa"
$redirect = new-object System.Uri("https://bookingsodataapi.azurewebsites.net/client")
$authContext = new-object Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext("https://login.windows.net/common/")
$platformParameters = New-Object Microsoft.IdentityModel.Clients.ActiveDirectory.PlatformParameters("Refresh")
$authContext.AcquireTokenAsync($resource,$clientID,$redirect,$platformParameters).Result.CreateAuthorizationHeader() | out-null

