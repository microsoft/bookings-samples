using System;
using System.Linq;
using System.Net;
using System.Security;
using Microsoft.Bookings.Client;
using Microsoft.OData.Client;
using System.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Configuration;

namespace BookingsSampleNativeConsole
{
    public class Program
    {
        // See README.MD for instructions on how to get your own values for these two settings.
        // See also https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-authentication-scenarios#native-application-to-web-api
        private static string clientApplicationAppId;
        private static string tenantId;

        public static void Main()
        {
            try
            {
                var config = GetConfiguration();
                tenantId = config["Bookings_TenantID"];
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    Console.WriteLine("Update sample to include your own Tenant ID");
                    return;
                }

                clientApplicationAppId = config["Bookings_ClientID"];
                if (string.IsNullOrWhiteSpace(clientApplicationAppId))
                {
                    Console.WriteLine("Update sample to include your own client application ID");
                    return;
                }

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
                var graphService = new GraphService(
                    GraphService.ServiceRoot,
                    () => authenticationResult.CreateAuthorizationHeader());

                // Fiddler makes it easy to look at the request/response payloads. Use it automatically if it is running.
                // https://www.telerik.com/download/fiddler
                if (System.Diagnostics.Process.GetProcessesByName("fiddler").Any())
                {
                    graphService.WebProxy = new WebProxy(new Uri("http://localhost:8888"), false);
                }

                // Get the list of booking businesses that the logged on user can see.
                // NOTE: I'm not using 'async' in this sample for simplicity;
                // the ODATA client library has full support for async invocations.
                var bookingBusinesses = graphService.BookingBusinesses.ToArray();
                foreach (var _ in bookingBusinesses)
                {
                    Console.WriteLine(_.DisplayName);
                }

                if (bookingBusinesses.Length == 0)
                {
                    Console.WriteLine("Enter a name for a new booking business, or leave empty to exit.");
                }
                else
                {
                    Console.WriteLine("Type the name of the booking business to use or enter a new name to create a new booking business, or leave empty to exit.");
                }

                var businessName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(businessName))
                {
                    return;
                }

                // See if the name matches one of the entities we have (this is searching the local array)
                var bookingBusiness = bookingBusinesses.FirstOrDefault(_ => _.DisplayName == businessName);
                if (bookingBusiness == null)
                {
                    // If we don't have a match, create a new bookingBusiness.
                    // All we need to pass is the display name, but we could pass other properties if needed.
                    // This NewEntityWithChangeTracking is a custom extension to the standard ODATA library to make it easy.
                    // Keep in mind there are other patterns that could be used, revolving around DataServiceCollection.
                    // The trick is that the data object must be tracked by a DataServiceCollection and then we need
                    // to save with SaveChangesOptions.PostOnlySetProperties.
                    bookingBusiness = graphService.BookingBusinesses.NewEntityWithChangeTracking();
                    bookingBusiness.DisplayName = businessName;
                    Console.WriteLine("Creating new booking business...");
                    graphService.SaveChanges(SaveChangesOptions.PostOnlySetProperties);

                    Console.WriteLine($"Booking Business Created: {bookingBusiness.Id}. Press any key to continue.");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Using existing booking business.");
                }

                // Play with the newly minted booking business
                var business = graphService.BookingBusinesses.ByKey(bookingBusiness.Id);

                // Add an external staff member (these are easy, as we don't need to find another user in the AD).
                // For an internal staff member, the application might query the user or the Graph to find other users.
                var staff = business.StaffMembers.FirstOrDefault();
                if (staff == null)
                {
                    staff = business.StaffMembers.NewEntityWithChangeTracking();
                    staff.EmailAddress = "staff1@contoso.com";
                    staff.DisplayName = "Staff1";
                    staff.Role = BookingStaffRole.ExternalGuest;
                    Console.WriteLine("Creating staff member...");
                    graphService.SaveChanges(SaveChangesOptions.PostOnlySetProperties);
                    Console.WriteLine("Staff created.");
                }
                else
                {
                    Console.WriteLine($"Using staff member {staff.DisplayName}");
                }

                // Add an Appointment
                var newAppointment = business.Appointments.NewEntityWithChangeTracking();
                newAppointment.CustomerEmailAddress = "customer@contoso.com";
                newAppointment.CustomerName = "John Doe";
                newAppointment.ServiceId = business.Services.First().Id; // assuming we didn't deleted all services; we might want to double check first like we did with staff.
                newAppointment.StaffMemberIds.Add(staff.Id);
                newAppointment.Reminders.Add(new BookingReminder { Message = "Hello", Offset = TimeSpan.FromHours(1), Recipients = BookingReminderRecipients.AllAttendees });
                var start = DateTime.Today.AddDays(1).AddHours(13).ToUniversalTime();
                var end = start.AddHours(1);
                newAppointment.Start = new DateTimeTimeZone { DateTime = start.ToString("o"), TimeZone = "UTC" };
                newAppointment.End = new DateTimeTimeZone { DateTime = end.ToString("o"), TimeZone = "UTC" };
                Console.WriteLine("Creating appointment...");
                graphService.SaveChanges(SaveChangesOptions.PostOnlySetProperties);
                Console.WriteLine("Appointment created.");

                // Query appointments.
                // Note: the server imposes a limit on the number of appointments returned in each request
                // so clients must use paging or request a calendar view with business.GetCalendarView().
                foreach (var appointment in business.Appointments.GetAllPages())
                {
                    // DateTimeTimeZone comes from Graph and it uses string for the DateTime, not sure why.
                    // Perhaps we could tweak the generated proxy (or add extension method) to automatically
                    // do this ToString/Parse for us, so it does not pollute the entire code.
                    Console.WriteLine($"{DateTime.Parse(appointment.Start.DateTime).ToLocalTime()}: {appointment.ServiceName} with {appointment.CustomerName}");
                }

                // In order for customers to interact with the booking business we need to publish its public page.
                // We can also Unpublish() to hide it from customers, but where is the fun in that?
                Console.WriteLine("Publishing booking business public page...");
                business.Publish().Execute();

                // Let the user play with the public page
                Console.WriteLine(business.GetValue().PublicUrl);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
        }

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddEnvironmentVariables();

            return builder.Build();
        }
    }
}
