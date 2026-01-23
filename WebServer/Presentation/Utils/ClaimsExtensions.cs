using System.Security.Claims;

namespace WebServer.Utils
{
    public static class ClaimsExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;

            if (id == null)
                throw new Exception("USER_ID_NOT_FOUND_IN_CLAIMS");

            return int.Parse(id);
        }
    }
}
