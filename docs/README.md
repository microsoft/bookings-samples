# Overview of the Microsoft Bookings API

The [Microsoft Bookings API][api] allows [Office 365 Business Premium][o365]
subscribers to manage their [Bookings][] data using the [Microsoft Graph][graph].

## Getting Started

In order to use the Bookings API you will need:

- **An [Office 365 Business Premium][o365] tenant in which you can create users and [Bookings][] resources.**  
  _You can create a trial tenant by going to [products.office.com][bookings], click **Buy now with Office 365** and select **Free Trial**._

- **You'll need to register client applications in [Azure Active Directory][aad] and request permissions to call Bookings using Graph.**

  - Open https://portal.azure.com then go to Active Directory->App Registrations
  - Create a new application and take note of its Application ID and Redirect Uri.
  - Edit the settings of the application. Under 'Required Permissions', add API access to 'Microsoft Graph' and select 'Manage bookings information'.

- **Clone the [bookings-samples][samples] repository
  and update your local copy with the Application ID and Redirect Uri of your Azure AD application.**

## Authentication

Applications that wish to access the [Bookings API][api] must acquire tokens to communicate with Graph.
The access token used with the API must contain the identity of an Office 365 user with subscriptions and permissions to use Bookings.

> NOTE: at the time of this writing, the Bookings API does NOT support application-only permissions.

An example for obtaining a token in a native C# application using [Active Directory Authentication Libraries (ADAL)][adal] goes like this:

```csharp
  var clientApplication = PublicClientApplicationBuilder.Create(clientApplicationAppId)
  .WithAuthority(AadAuthorityAudience.AzureAdMyOrg)
  .WithTenantId(tenantId)
  .Build();

  Console.Write("Username:    ");
  string username = Console.ReadLine();
  if (string.IsNullOrEmpty(username))
  {
      Console.WriteLine("Update Sample to include your username");
      return;
  }

  Console.Write("Password:    ");
  SecureString password = new SecureString();
  ConsoleKeyInfo keyinfo;
  do
  {
      keyinfo = Console.ReadKey(true);
      if (keyinfo.Key == ConsoleKey.Enter) break;
      password.AppendChar(keyinfo.KeyChar);
  } while (keyinfo.Key != ConsoleKey.Enter);

  if (password.Length == 0)
  {
      Console.WriteLine("Password needs to be provided for the sample to work");
      return;
  }

  var authenticationResult = clientApplication.AcquireTokenByUsernamePassword(
                      new[] { "Bookings.Read.All" },
                      username, password).ExecuteAsync().Result;
```

## Client Libraries

.Net applications can then use ODATA v4 client-side proxy classes, such as the ones
provided by the [Microsoft ODATA Stack][client].

The [bookings-samples][samples] repository contains such a generated client library, with just the types needed to access Bookings.
With it, a C# application can initialize a `GraphService` with code like this:

```csharp
var graphService = new GraphService(
    GraphService.ServiceRoot,
    () => authenticationResult.CreateAuthorizationHeader());
```

## Booking Entities, Operations and Permissions Scopes

For additional information about Booking entities, operations and permission scopes see the [Microsoft Graph documentation][api].

See also:

- [ODATA Documentation][odata]
- [Microsoft ODATA Stack][client]
- [Active Directory Authentication Libraries][adal]
- [Bookings Samples][samples]

[api]: https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/resources/booking-api-overview "Microsoft Bookings API"
[odata]: http://www.odata.org/documentation/ "ODATA"
[graph]: https://graph.microsoft.com "Microsoft Graph"
[o365]: https://products.office.com/en-us/business/office-365-business-premium "Office 365 Business Premium"
[bookings]: https://products.office.com/en-us/business/scheduling-and-booking-app "Microsoft Bookings"
[aad]: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-developers-guide "Azure Active Directory"
[adal]: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-authentication-libraries "Active Directory Authentication Libraries"
[client]: https://odata.github.io "Microsoft ODATA Stack"
[samples]: https://github.com/Microsoft/bookings-samples "bookings-samples"
