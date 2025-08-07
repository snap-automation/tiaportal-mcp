using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiaMcpServer.Siemens
{
    public class State
    {
        public bool? IsConnected { get; set; }
        public string? Project { get; set; }
        public string? Session { get; set; }
    }
}
