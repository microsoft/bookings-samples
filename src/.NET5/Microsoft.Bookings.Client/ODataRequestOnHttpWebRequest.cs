// ---------------------------------------------------------------------------
// <copyright file="ODataRequestOnHttpWebRequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Microsoft.Bookings.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    using Microsoft.OData;

    /// <summary>
    /// Adapter that implements IODataRequestMessage on top of an HttpWebRequest
    /// </summary>
    /// <remarks>
    /// Use request.GetODataRequest() to get an instance of this class from service code.
    /// Note that only methods that are currently needed are being implemented.
    /// </remarks>
    internal class ODataRequestOnHttpWebRequest : IODataRequestMessage
    {
        private readonly HttpWebRequest requestMessage;

        public ODataRequestOnHttpWebRequest(HttpWebRequest requestMessage) => this.requestMessage = requestMessage;

        public IEnumerable<KeyValuePair<string, string>> Headers => throw new NotImplementedException();

        public string Method
        {
            get => this.requestMessage.Method;
            set => throw new NotImplementedException();
        }

        public Uri Url
        {
            get => this.requestMessage.RequestUri;
            set => throw new NotImplementedException();
        }

        public string GetHeader(string headerName) => this.requestMessage.Headers[headerName];

        public void SetHeader(string headerName, string headerValue) => this.requestMessage.Headers[headerName] = headerValue;

        public Stream GetStream() => throw new NotImplementedException();
    }
}