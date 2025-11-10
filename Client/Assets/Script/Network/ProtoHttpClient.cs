using Game.Core;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Game.Network
{
    public class ProtoHttpClient
    {
        private readonly ApiConfig _config; 
        private string _authToken; 
        
        public ProtoHttpClient(ApiConfig config) { _config = config; Debug.Log($"[Http] Base={_config.BaseUrl}"); }
        public void SetToken(string token) { _authToken = token; }
        public IEnumerator Get<T>(string path, MessageParser<T> parser, Action<ApiResult<T>> cb) where T : IMessage<T>
          => Send(UnityWebRequest.kHttpVerbGET, path, null, parser, cb);

        public IEnumerator Post<TReq, TRes>(string path, TReq msg, MessageParser<TRes> parser, Action<ApiResult<TRes>> cb)
            where TReq : IMessage where TRes : IMessage<TRes>
        {
            var bytes = msg?.ToByteArray();
            return Send(UnityWebRequest.kHttpVerbPOST, path, bytes, parser, cb);
        }

        public IEnumerator Put<TReq, TRes>(string path, TReq msg, MessageParser<TRes> parser, Action<ApiResult<TRes>> cb)
            where TReq : IMessage where TRes : IMessage<TRes>
        {
            var bytes = msg?.ToByteArray();
            return Send(UnityWebRequest.kHttpVerbPUT, path, bytes, parser, cb);
        }

        private IEnumerator Send<T>(string method, string path, byte[] body, MessageParser<T> parser, Action<ApiResult<T>> cb)
            where T : IMessage<T>
        {
            int attempts = 0; ApiResult<T> last = default;

            while (attempts <= _config.RetryCount)
            {
                using (var req = new UnityWebRequest(_config.BaseUrl + path, method))
                {
                    if (body != null)
                    {
                        req.uploadHandler = new UploadHandlerRaw(body);
                        req.SetRequestHeader("Content-Type", "application/x-protobuf");
                    }
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.timeout = _config.DefaultTimeoutSec;
                    req.SetRequestHeader("Accept", "application/x-protobuf");
                    if (!string.IsNullOrEmpty(_authToken))
                        req.SetRequestHeader("Authorization", $"Bearer {_authToken}");

                    yield return req.SendWebRequest();
                    Debug.Log($"[HTTP] code={req.responseCode}, result={req.result}, error={req.error}, ctype={req.GetResponseHeader("Content-Type")}");
                    var bytes = req.downloadHandler?.data;
                    if (req.result == UnityWebRequest.Result.Success && req.responseCode >= 200 && req.responseCode < 300)
                    { 
                        try
                        {
                            var data = parser.ParseFrom(bytes ?? Array.Empty<byte>());
                            cb(ApiResult<T>.Success(data, (int)req.responseCode));
                            yield break;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[HTTP] Proto Parse Fail: {e}");
                            last = ApiResult<T>.Fail("CLIENT_PROTO_PARSE", e.ToString(), (int)req.responseCode);
                            cb(last); yield break;
                        }
                    }
                    else
                    {
                        var msg = req.error ?? req.downloadHandler?.text;
                        last = ApiResult<T>.Fail("HTTP_ERROR", msg, (int)req.responseCode);
                    }
                }
                attempts++;
                if (attempts <= _config.RetryCount)
                    yield return new WaitForSeconds(_config.RetryBackoffSec * attempts);
            }
            cb(last);
        }
    } 
} 