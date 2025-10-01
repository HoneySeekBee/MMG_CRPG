using Application.Users;
using System.Net.Http.Headers;
using System.Net;

namespace AdminTool.Services
{
    public class TokenAttachHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _http;

        public TokenAttachHandler(IHttpContextAccessor http)
        {
            _http = http;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var ctx = _http.HttpContext;
             
            var at = ctx?.Session.GetString("access_token");
            if (!string.IsNullOrWhiteSpace(at))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", at);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
