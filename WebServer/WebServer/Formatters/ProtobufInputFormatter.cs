using Google.Protobuf;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Reflection;

namespace WebServer.Formatters
{
    public sealed class ProtobufInputFormatter : InputFormatter
    {
        public const string MediaType = "application/x-protobuf";
        public ProtobufInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(MediaType));
        }

        protected override bool CanReadType(Type type)
            => typeof(IMessage).IsAssignableFrom(type);

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var http = context.HttpContext;
            using var ms = new MemoryStream();
            await http.Request.Body.CopyToAsync(ms);
            ms.Position = 0;

            var t = context.ModelType; // e.g. Contracts.Protos.GuestAuthRequest
            var parserProp = t.GetProperty("Parser", BindingFlags.Public | BindingFlags.Static);
            if (parserProp?.GetValue(null) is not object parser)
                return await InputFormatterResult.FailureAsync();

            var parseFrom = parser.GetType().GetMethod("ParseFrom", new[] { typeof(Stream) });
            if (parseFrom is null) return await InputFormatterResult.FailureAsync();

            var message = parseFrom.Invoke(parser, new object[] { ms });
            return await InputFormatterResult.SuccessAsync(message);
        }
    }
}
