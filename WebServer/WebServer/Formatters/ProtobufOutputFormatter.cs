using Google.Protobuf;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace WebServer.Formatters
{
    public sealed class ProtobufOutputFormatter : OutputFormatter
    {
        public const string MediaType = "application/x-protobuf";
        public ProtobufOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(MediaType));
        }

        protected override bool CanWriteType(Type type)
            => typeof(IMessage).IsAssignableFrom(type);

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var resp = context.HttpContext.Response;
            resp.ContentType = MediaType;

            var msg = (IMessage)context.Object!;
            using var ms = new MemoryStream();
            msg.WriteTo(ms);
            ms.Position = 0;
            resp.ContentLength = ms.Length;
            await ms.CopyToAsync(resp.Body);
        }
    }
}
