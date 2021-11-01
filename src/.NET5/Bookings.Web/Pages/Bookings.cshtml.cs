using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Bookings.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.OData.Client;

namespace Bookings.Web.Pages
{
    public class BookingsModel : PageModel
    {
        private readonly IPublicClientApplication clientApplication;
        private readonly IConfiguration config;
        private readonly GraphService graphService;

        public BookingsModel(IPublicClientApplication app, IConfiguration configuration)
        {
            this.clientApplication = app;
            this.config = configuration;
            this.graphService = GetGraphService();
        }

        public BookingBusiness[] Businesses { get; private set; }
        public BookingStaffMember[] StaffMembers { get; private set; }
        public BookingService[] AvailableServices { get; private set; }
        public IEnumerable<BookingAppointment> Appointments { get; private set; }

        public void OnGet()
        {
            Businesses = graphService.BookingBusinesses.ToArray();
        }

        public void OnGetSelect(string id)
        {
            Businesses = graphService.BookingBusinesses.ToArray();

            // Play with the newly minted booking business
            var selectedBusiness = graphService.BookingBusinesses.ByKey(id);

            if (selectedBusiness != null)
            {
                StaffMembers = selectedBusiness.StaffMembers.ToArray();

                AvailableServices = selectedBusiness.Services.ToArray();

                Appointments = selectedBusiness.Appointments.GetAllPages();
            }
        }

        #region private helpers
        private GraphService GetGraphService()
        {

            var authenticationResult = clientApplication.AcquireTokenByUsernamePassword(new[] { "Bookings.Read.All" }, 
                config["Bookings_Username"],
                new NetworkCredential("", config["Bookings_Password"]).SecurePassword)
                .ExecuteAsync().Result;

            var graphService = new GraphService(
                GraphService.ServiceRoot,
                () => authenticationResult.CreateAuthorizationHeader());

            return graphService;
        }
        #endregion
    }
}
