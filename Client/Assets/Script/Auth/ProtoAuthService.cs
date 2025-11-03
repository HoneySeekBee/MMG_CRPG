using System;
using System.Collections;
using Contracts.Protos;
using Game.Core;
using Game.Network;
using Google.Protobuf;
using static System.Net.WebRequestMethods;

namespace Game.Auth
{
    public class ProtoAuthService
    {
        private readonly ProtoHttpClient _http;
        public ProtoAuthService(ProtoHttpClient http) { _http = http; }
        public IEnumerator LoginGuest(string deviceId, Action <ApiResult<AuthResponse>> cb)
        {
            var req = new GuestAuthRequest { DeviceId = deviceId };
            yield return _http.Post(ApiRoutes.AuthGuest, req, AuthResponse.Parser, res =>
            {
                if (res.Ok) _http.SetToken(res.Data.AccessToken);
                cb(res);
            });
        }

        public IEnumerator Refresh(string refreshToken, Action< ApiResult<AuthResponse>> cb)
        {
            var req = new RefreshRequest { RefreshToken = refreshToken };
            yield return _http.Post(ApiRoutes.AuthRefresh, req, AuthResponse.Parser, res =>
            {
                if (res.Ok) _http.SetToken(res.Data.AccessToken);
                cb(res);
            });
        }
    }
}