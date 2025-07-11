using System.Text.Json;

namespace TiaMcpServer.ModelContextProtocol
{
    public class JsonRpcMessage
    {
        public string jsonrpc { get; set; } = "2.0";
        public object? id { get; set; }
        public object? result { get; set; }
        public object? error { get; set; }
    }

    public static class JsonRpcMessageWrapper
    {
        public static string ToJson(object id, object? result = null, object? error = null)
        {
            var message = new JsonRpcMessage
            {
                id = id,
                result = result,
                error = error
            };

            return JsonSerializer.Serialize(message);
        }
    }

}
