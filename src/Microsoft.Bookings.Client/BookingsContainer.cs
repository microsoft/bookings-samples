// ---------------------------------------------------------------------------
// <copyright file="BookingsContainer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Microsoft.Bookings.Client
{
    using System;
    using System.Net;

    using Microsoft.OData.Client;
    using Microsoft.OData.Core;

    public partial class BookingsContainer
    {
        /// <summary>
        /// The resource identifier for the Bookings ODATA API when used by 3rd party applications.
        /// </summary>
        public const string ResourceId = "https://microsoft.onmicrosoft.com/bookingsodataapi";

        /// <summary>
        /// The default AAD instance to use when authenticating.
        /// </summary>
        public const string DefaultAadInstance = "https://login.microsoftonline.com/common/";

        /// <summary>
        /// The default v1 service root
        /// </summary>
        public static readonly Uri DefaultV1ServiceRoot = new Uri("https://bookings.office.net/api/v1.0/");

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingsContainer"/> class.
        /// </summary>
        /// <param name="serviceRoot">The service root.</param>
        /// <param name="getAuthenticationHeader">A delegate that returns the authentication header to use in each request.</param>
        public BookingsContainer(Uri serviceRoot, Func<string> getAuthenticationHeader)
            : this(serviceRoot)
        {
            this.BuildingRequest += (s, e) => e.Headers.Add("Authorization", getAuthenticationHeader());
        }

        /// <summary>
        /// Gets or sets the odata.maxpagesize preference header.
        /// </summary>
        /// <remarks>
        /// Using the Prefer header we can control the resulting page size of certain operations,
        /// in particular of GET bookingBusinesses(id)/appointments and bookingBusinesses(id)/customers.
        /// </remarks>
        public int? MaxPageSize
        {
            get;
            set;
        } = null;

        /// <summary>
        /// Gets or sets the odata.continue-on-error preference header.
        /// </summary>
        /// <remarks>
        /// Using the Prefer header we can control if batch operations stop or continue on error.
        /// </remarks>
        public bool ContinueOnError
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the web proxy to use when sending requests.
        /// </summary>
        public IWebProxy WebProxy
        {
            get;
            set;
        }

        partial void OnContextCreated()
        {
            // Default to send only the properties that were set on a data object
            this.EntityParameterSendOption = EntityParameterSendOption.SendOnlySetProperties;

            // Allows new results to override cached results, if the object is not changed. 
            this.MergeOption = MergeOption.PreserveChanges;

            if (this.BaseUri.AbsoluteUri[this.BaseUri.AbsoluteUri.Length - 1] != '/')
            {
                throw new ArgumentException("BaseUri must end with '/'");
            }

            this.BuildingRequest += (s, e) => e.Headers.Add("client-request-id", Guid.NewGuid().ToString());

            this.SendingRequest2 += (s, e) =>
                {
                    var requestMessage = e.RequestMessage as HttpWebRequestMessage;
                    if (requestMessage != null)
                    {
                        var preferenceHeader = new ODataRequestOnHttpWebRequest(requestMessage.HttpWebRequest).PreferHeader();
                        preferenceHeader.MaxPageSize = this.MaxPageSize;
                        preferenceHeader.ContinueOnError = this.ContinueOnError;

                        requestMessage.HttpWebRequest.Proxy = this.WebProxy;
                    }
                };
        }
    }
}