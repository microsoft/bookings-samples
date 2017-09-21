# Overview of the Microsoft Bookings API
The [Microsoft Bookings API][API] is an [ODATA][] service which allows [Office 365 Business Premium][O365]
subscribers to manage their [Bookings][] data.

NOTE: The Microsoft Bookings API and its samples are currently in early preview; expect changes.

## Getting Started
In order to use the [Bookings API][API] you will need:
* **An [Office 365 Business Premium][O365] tenant in which you can create users and [Bookings][] resources.**  
  *You can create a trial tenant by going to [products.office.com][Bookings], click **Buy now with Office 365** and select **Free Trial**.*

* **You'll need to register client applications in [Azure Active Directory][AAD] and request permissions to call the [Bookings API][API].**  
  *See [this sample](https://github.com/Microsoft/bookings-samples/blob/master/src/BookingsSampleNativeConsole/README.md)
   for additional instructions, as right now we need you to consent to use the Bookings API using a particular client ID before using yours.*

* **Clone the [bookings-samples][Samples] repository
  and update your local copy with the Application ID and Redirect Uri of your Azure AD applications.**


## Authentication using Azure Active Directory
Applications that wish to access the [Bookings API][API] must have a registration in the [Azure Active Directory][AAD].

The application must require access to the [Bookings API][API], by including the following `requiredResourceAccess` in its manifest*:
```json
{
    "resourceAppId": "a6f98bd3-1059-4225-8f94-fce712c45742",
    "resourceAccess": [{ "id": "994019ca-1a02-4fd8-9fd6-987c7c09c426", "type": "Scope" }]
}
```
**The plan is to make this available thru the normal Azure Portal UI, so one is not required to edit manifests manually.*

When obtaining a token from AzureAD, the target *resourceId* for Bookings is this:
```csharp
"https://microsoft.onmicrosoft.com/bookingsodataapi"
```

Access tokens containing the identity of an Office 365 user with subscriptions
and permissions to use Bookings must be sent as a `Bearer` token in the `Authentication` header of the HTTPS requests sent to Bookings.

Applications can use the [Active Directory Authentication Libraries (ADAL)][ADAL] to obtain tokens.
An example for obtaining a token in a native C# application using ADAL goes like this:
```csharp
var authenticationContext = new AuthenticationContext("https://login.microsoftonline.com/common/");

var authenticationResult = await authenticationContext.AcquireTokenAsync(
  "https://microsoft.onmicrosoft.com/bookingsodataapi",
  clientApplication_ClientId,
  clientApplication_RedirectUri,
  new PlatformParameters(PromptBehavior.RefreshSession));

// The results of this call are sent as the Authorization header of each HTTPS request to Bookings.
var authorizationHeader = authenticationResult.CreateAuthorizationHeader();
```  

See also [Active Directory Authentication Scenarios][Auth].


## Service Endpoint
The [Microsoft Bookings API][API] implements the [ODATA][] protocol.
Client applications can access the service using any language or platform capable of sending HTTP requests,
using the following parameters:

| &nbsp;             |Address                                           |
|--------------------|:-------------------------------------------------|
|**Service Root**    |https://bookings.office.net/api/v1.0/             |
|**Service Metadata**|https://bookings.office.net/api/v1.0/$metadata    |


## Client Libraries
.Net applications can use ODATA v4 client-side proxy classes, such as the ones
provided by the [Microsoft ODATA Stack][Client].

*Note: the code generators work with Visual Studio 2017, even thou their docs
says it works up to VS2015; I suppose the documentation was not updated.*

The [bookings-samples][Samples] repository contains such a generated client library.
With it, a C# application can initialize a `BookingContainer` with code like this:
```csharp
var authenticationContext = new AuthenticationContext(BookingsContainer.DefaultAadInstance, TokenCache.DefaultShared);

var authenticationResult = await authenticationContext.AcquireTokenAsync(
    BookingsContainer.ResourceId,
    clientApplicationAppId,
    clientApplicationRedirectUri,
    new PlatformParameters(PromptBehavior.RefreshSession)).Result;

var bookingsContainer = new BookingsContainer(
    BookingsContainer.DefaultV1ServiceRoot,
    () => authenticationResult.CreateAuthorizationHeader());
```

## Booking Entities
[Microsoft Bookings][Bookings] provides Office 365 subscribers
with the ability to create a set of *booking businesses*.
Each *booking business* offers a set of *services* and these services are performed
by one or more *staff members*. A *customer* of the booking business can book an
*appointment*, and the system ensures that the calendars of the people
involved are updated.

With this in mind, the [Bookings API][API] consists of an [ODATA][] container 
where we can find the *booking businesses* available to the user whose token
is used to access the API, and in each *booking business* we find the nested
entity sets of appointments, customers, services and staff members:

* **bookingsContainer** 
  * **bookingBusinesses**, a set of ***bookingBusiness***
    * **appointments**, a set of ***bookingAppointment***
    * **customers**, a set of ***bookingCustomer***
    * **services**, a set of ***bookingService***
    * **staffMembers**, a set of ***bookingStaffMember***

### bookingBusiness
The **bookingBusiness** is the central entity of the [Bookings API][API].
It contains properties about the business and navigation properties to 
related entities, such as staff, customers and their appointments.

### bookingAppointment
The **bookingAppointment** represents a particular *service*,
performed by a set of *staff members*, in a given date & time,
to a particular *customer*.

### bookingCustomer
The **bookingCustomer** represents a customer of the booking business.

### bookingService
The **bookingService** contains information about a particular service
provided by the booking business, such as its name, price and the staff
that usually provides such service.

### bookingStaffMember
The **bookingStaffMember** represents a staff member that provides
services for the booking business. Staff members might be part of the
Office 355 tenant where the booking business is configured or they
might user e-mail services from any other e-mail provider.  

## Sample Request URLs
According to the [ODATA][] protocol, accessing Bookings data means 
sending HTTPS requests with GET/POST/PATCH/DELETE methods to the URL
of the corresponding entity or entity set.

For example, to list the booking businesses a user has access to in 
the Office 365 tenant, an application will issue the following request*:
```http
GET https://bookings.office.net/api/v1.0/bookingBusinesses HTTP/1.1
Authorization: Bearer <the access token obtained from AAD> 
```
**Additional headers are not shown, for simplicity sake*

To create a new `bookingBusiness`, the application sends a `POST` to the entity set:
```http
POST https://bookings.office.net/api/v1.0/bookingBusinesses HTTP/1.1
Content-Type: application/json;odata.metadata=minimal
Authorization: Bearer <the access token obtained from AAD> 

{"displayName":"Hair Salon"}
```

In the responses of `GET` and `POST` the application can find the `id`
of the entities. Having the identity of the booking business, the application
can operate on the nested entity sets.

For example, to get a list of the available `services` in a booking business, the application sends:
```http
GET https://bookings.office.net/api/v1.0/bookingBusinesses('hairSalon_6d574ab4c3@contoso.onmicrosoft.com')/services HTTP/1.1
Authorization: Bearer <the access token obtained from AAD> 
```

To operate on an individual nested entity, the request must reference the full path
to that entity. For example, to `PATCH` a particular service, the application sends:
```http
PATCH https://bookings.office.net/api/v1.0/bookingBusinesses('hairSalon_6d574ab4c3@contoso.onmicrosoft.com')/services('03e1f203-35df-4c35-ade8-97ca35de5e81') HTTP/1.1
Content-Type: application/json;odata.metadata=minimal
Authorization: Bearer <the access token obtained from AAD> 

{"displayName":"Hair Salon"}
```

*Note:  when `POST`ing or `PATCH`ing, send only the properties that need to be set.
Client applications that use generated proxies should use the facilities
the library provides for tracking changes to its data objects.*

To `DELETE` an entity, the application sends:
```http
DELETE https://bookings.office.net/api/v1.0/bookingBusinesses('hairSalon_6d574ab4c3@contoso.onmicrosoft.com')/services('03e1f203-35df-4c35-ade8-97ca35de5e81') HTTP/1.1
Authorization: Bearer <the access token obtained from AAD> 
```

The pattern is similar for all nested entity sets.

## Permission Scopes
The API currently supports the following four permission scopes:

| Id                                   | Role                            | Description                                                                                |
|--------------------------------------|:--------------------------------|--------------------------------------------------------------------------------------------|
| 3fa01074-63ac-4e14-9847-2f61e55eb130 | Bookings.Read.All               | Read Bookings appointments, businesses, customers, services and staff                      |
| fbe314ff-3c42-48b4-8698-634edfa0e310 | Bookings.ReadWrite.Appointments | Read and write Bookings appointments and customers and read businesses, services and staff |
| 64745dc5-03fc-45d7-9afd-13bfed754231 | Bookings.ReadWrite.All          | Read and write Bookings appointments, businesses, customers, services and staff            |
| 994019ca-1a02-4fd8-9fd6-987c7c09c426 | Bookings.Manage                 | Read, write and manage Bookings appointments, businesses, customers, services and staff    |

* `Bookings.Read.All` is intended for read-only applications.  
   *Typical target user is the customer of a booking business.*

* `Bookings.ReadWrite.Appointments` is intended for scheduling applications, which need to manipulate appointments and customers, but do not change the fundamental information about the booking business or its services and staff members.  
   *Typical target user would be the customer of a booking business.*

* `Bookings.ReadWrite.All` is intended for management applications that manipulate existing businesses, including management of their services and staff members, but are not allowed to create, delete or change the published status of a booking business.  
   *Typical target user would be the support staff of an organization.*

* `Bookings.Manage` is intended for a full management experience.  
   *Typical target user would be the administrators of an organization.*

Permissions for individual operations are listed below:

|            Verb | Noun        |Bookings.Read.All|Bookings.ReadWrite.Appointments|Bookings.ReadWrite.All|Bookings.Manage|
|----------------:|:------------|:---------------:|:-----------------------------:|:--------------------:|:-------------:|
|             Get | Appointment |        *        |               *               |           *          |       *       |
| GetCalendarView | Appointment |        *        |               *               |           *          |       *       |
|        GetCount | Appointment |        *        |               *               |           *          |       *       |
|         GetList | Appointment |        *        |               *               |           *          |       *       |
|             Get | Business    |        *        |               *               |           *          |       *       |
|        GetCount | Business    |        *        |               *               |           *          |       *       |
|         GetList | Business    |        *        |               *               |           *          |       *       |
|             Get | Customer    |        *        |               *               |           *          |       *       |
|        GetCount | Customer    |        *        |               *               |           *          |       *       |
|         GetList | Customer    |        *        |               *               |           *          |       *       |
|             Get | Service     |        *        |               *               |           *          |       *       |
|        GetCount | Service     |        *        |               *               |           *          |       *       |
|         GetList | Service     |        *        |               *               |           *          |       *       |
|             Get | StaffMember |        *        |               *               |           *          |       *       |
|        GetCount | StaffMember |        *        |               *               |           *          |       *       |
|         GetList | StaffMember |        *        |               *               |           *          |       *       |
|          Cancel | Appointment |                 |               *               |           *          |       *       |
|          Delete | Appointment |                 |               *               |           *          |       *       |
|           Patch | Appointment |                 |               *               |           *          |       *       |
|            Post | Appointment |                 |               *               |           *          |       *       |
|          Delete | Customer    |                 |               *               |           *          |       *       |
|           Patch | Customer    |                 |               *               |           *          |       *       |
|            Post | Customer    |                 |               *               |           *          |       *       |
|           Patch | Business    |                 |                               |           *          |       *       |
|          Delete | Service     |                 |                               |           *          |       *       |
|           Patch | Service     |                 |                               |           *          |       *       |
|            Post | Service     |                 |                               |           *          |       *       |
|          Delete | StaffMember |                 |                               |           *          |       *       |
|           Patch | StaffMember |                 |                               |           *          |       *       |
|            Post | StaffMember |                 |                               |           *          |       *       |
|          Delete | Business    |                 |                               |                      |       *       |
|            Post | Business    |                 |                               |                      |       *       |
|         Publish | Business    |                 |                               |                      |       *       |
|       Unpublish | Business    |                 |                               |                      |       *       |


See also:
* [ODATA Documentation][ODATA]
* [Microsoft ODATA Stack][Client]
* [Active Directory Authentication Libraries][ADAL]
* [Bookings Samples][Samples]

 [API]:      https://bookings.office.net/api/v1.0/
 [ODATA]:    http://www.odata.org/documentation/ "ODATA"
 [O365]:     https://products.office.com/en-us/business/office-365-business-premium
             "Office 365 Business Premium"
 [Bookings]: https://products.office.com/en-us/business/scheduling-and-booking-app
             "Microsoft Bookings"
 [AAD]:      https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-developers-guide
             "Azure Active Directory"
 [Auth]:     https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-authentication-scenarios
             "Active Directory Authentication Scenarios"
 [ADAL]:     https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-authentication-libraries
             "Active Directory Authentication Libraries"
 [Client]:   https://odata.github.io
             "Microsoft ODATA Stack"
 [Samples]:  https://github.com/Microsoft/bookings-samples
             "bookings-samples"
