using System;

namespace TiaMcpServer.ModelContextProtocol
{
    public class McpException : Exception
    {
        public int ErrorCode { get; }

        public McpException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public McpException(int errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
