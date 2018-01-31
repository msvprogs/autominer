using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Msv.HttpTools
{
    public class CorrectHttpException : ApplicationException
    {
        public HttpStatusCode Status { get; }
        public string StatusDescription { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public MemoryStream Body { get; }

        public CorrectHttpException(
            HttpStatusCode status, 
            string statusDescription,
            IReadOnlyDictionary<string, string> headers,
            MemoryStream body)
            : base($"HTTP error {status:D} ({statusDescription})")
        {
            Status = status;
            StatusDescription = statusDescription;
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }
    }
}
