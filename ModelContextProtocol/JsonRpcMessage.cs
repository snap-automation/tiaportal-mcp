using ModelContextProtocol.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TiaMcpServer.ModelContextProtocol
{

    public class JsonRpcMessage
    {
        public string jsonrpc { get; set; } = "2.0";
        public object? id { get; set; }
        public object? result { get; set; }
        public object? error { get; set; }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Success(object id, object result)
        {
            var message = new JsonRpcMessage
            {
                id = id,
                result = result
            };
            return JsonSerializer.Serialize(message, _jsonOptions);
        }

        public static string SuccessData(object id, object data, string? message = null, object? metadata = null)
        {
            var result = new
            {
                success = true,
                message = message != null ? "Success: " + message : "Success",
                timestamp = DateTime.UtcNow.ToString("O"),
                data,
                metadata
            };

            return Success(id, result);
        }

        public static string SuccessList<T>(object id, IEnumerable<T> ienumerable, string? message = null)
        {
            var itemsList = ienumerable.ToList();
            var result = new
            {
                success = true,
                items = itemsList,
                count = itemsList.Count,
                message = message != null ? "Success: " + message : "Success",
                timestamp = DateTime.UtcNow.ToString("O")
            };

            return Success(id, result);
        }

        public static string Error(object id, int code, string message, object? data = null)
        {
            var jsonRpcMessage = new JsonRpcMessage
            {
                id = id,
                error = new JsonRpcError
                {
                    code = code,
                    message = "Error: " + message,
                    data = data
                }
            };

            return JsonSerializer.Serialize(jsonRpcMessage, _jsonOptions);
        }

        public static string Exception(object id, int code, string message, object? data = null)
        {
            var jsonRpcMessage = new JsonRpcMessage
            {
                id = id,
                error = new JsonRpcError
                {
                    code = code,
                    message = "Exception: " + message,
                    data = data
                }
            };

            return JsonSerializer.Serialize(jsonRpcMessage, _jsonOptions);
        }

        public static JsonRpcMessage? Parse(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<JsonRpcMessage>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static JsonRpcMessage ParseStrict(string json)
        {
            return JsonSerializer.Deserialize<JsonRpcMessage>(json, _jsonOptions)
                ?? throw new JsonException("Failed to deserialize JSON-RPC message");
        }

    }

}
