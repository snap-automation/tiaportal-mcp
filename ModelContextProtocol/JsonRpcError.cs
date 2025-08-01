using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiaMcpServer.ModelContextProtocol
{
    public class JsonRpcError
    {
        public int code { get; set; }
        public string message { get; set; } = string.Empty;
        public object? data { get; set; }  // optional
    }
}
