# Bookings Sample - Native Console

This sample shows a console application that uses the Microsoft Bookings API.

In order to run the sample, register a Native Azure AD application and customize the
sample with your own Application ID and Redirect URLs.

* Login to the Azure Portal
* Open [Microsoft Azure Portal/Azure Active Directory/App Registrations](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps)
* Register a new application by clicking 'New application registration'
* Select 'Native application' and give it a name and a redirect URI.
* Once the application is registered, update `Program.cs` and replace the values of `ClientApplicationAppId` and `ClientApplicationRedirectUri`.
* Edit the application manifest in Azure, and include this under requiredResourceAccess, 
so the application will require access to the Bookings API*:
```json
{
    "resourceAppId": "a6f98bd3-1059-4225-8f94-fce712c45742",
    "resourceAccess": [{ "id": "e859af95-b7e5-4e0c-aa65-829dd1c3a60f", "type": "Scope" }]
}
```
**The plan is to make this available thru the normal Azure Portal UI, so one is not required to edit manifests manually.*

See also:
[Authentication Scenarios: Native Application to Web API](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-authentication-scenarios#native-application-to-web-api)

