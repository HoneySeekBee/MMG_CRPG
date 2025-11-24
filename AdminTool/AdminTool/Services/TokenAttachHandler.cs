using Application.Users;
using System.Net.Http.Headers;
using System.Net;

namespace AdminTool.Services
{
    public class TokenAttachHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _context;

        public TokenAttachHandler(IHttpContextAccessor context)
        {
            _context = context;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _context.HttpContext;
            var token = httpContext?.Session.GetString("access_token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
