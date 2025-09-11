using System;
using System.Collections;
using Contracts.Protos;
using Game.Core;
using Game.Network;
using Google.Protobuf;

namespace Game.Auth
{
    public class ProtoAuthService
    {
        private readonly ProtoHttpClient http;
        public ProtoAuthService(ProtoHttpClient client) { http = client; }


        public IEnumerator LoginGuest(string deviceId, Action<ApiResult<AuthResponse>> cb)
        {
            var req = new GuestAuthRequest { DeviceId = deviceId };
            yield return http.Post<GuestAuthRequest, AuthResponse>(ApiRoutes.AuthGuest, req, AuthResponse.Parser, res => { if (res.Ok) http.SetToken(res.Data.AccessToken); cb(res); });
        }


        public IEnumerator Refresh(string refreshToken, Action<ApiResult<AuthResponse>> cb)
        {
            var req = new RefreshRequest { RefreshToken = refreshToken };
            yield return http.Post<RefreshRequest, AuthResponse>(ApiRoutes.AuthRefresh, req, AuthResponse.Parser, res => { if (res.Ok) http.SetToken(res.Data.AccessToken); cb(res); });
        }
    }
}