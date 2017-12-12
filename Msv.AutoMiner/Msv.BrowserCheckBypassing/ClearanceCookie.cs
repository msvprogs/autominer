using System;
using System.Net;

namespace Msv.BrowserCheckBypassing
{
    public class ClearanceCookie
    {
        public Cookie Id { get; set; }
        public Cookie Clearance { get; set; }

        public DateTime LastSolved { get; set; }
    }
}
