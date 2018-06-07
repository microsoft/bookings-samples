 # Microsoft Bookings API in Microsoft Graph

> Microsoft Bookings API is now available in the Microsoft Graph.  
> See the [Graph documentation](https://developer.microsoft.com/en-us/graph/docs/concepts/booking-concept-overview) for how to use this API with Graph.  
> In particular, use the Graph endpoint, application registration and authentication mechanisms for your application.

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
    "resourceAccess": [{ "id": "994019ca-1a02-4fd8-9fd6-987c7c09c426", "type": "Scope" }]
}
```
**The plan is to make this available thru the normal Azure Portal UI, so one is not required to edit manifests manually.*

## Consenting to use Bookings API

Before you can use the Bookings API in your tenant with your own application ID you'll need to consent to its usage.
You can use the `InitializeTenant.ps1` PowerShell script in this sample to do this.

This will create the necessary service principal in your AzureAD tenant and you'll be
able to see it in the Azure Portal under Active Directory->Enterprise Applications->Bookings ODATA API.

Failing to do this will result in authentication errors that read like this:
*AADSTS65005: The app needs access to a service ("https://microsoft.onmicrosoft.com/bookingsodataapi") that your organization "\<guid>" has not subscribed to or enabled*


See also:
[Authentication Scenarios: Native Application to Web API](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-authentication-scenarios#native-application-to-web-api)

