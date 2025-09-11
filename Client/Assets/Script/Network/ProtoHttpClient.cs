using Game.Core;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace Game.Network
{
    public class ProtoHttpClient : MonoBehaviour
    {
        public static ProtoHttpClient Instance { get; private set; }
        [SerializeField] private ApiConfig config; [SerializeField] private string authToken;
        void Awake() 
        { 
            if (Instance != null && Instance != this) 
            { 
                Destroy(gameObject); 
                return; 
            } 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
        public void SetToken(string token) => authToken = token;


        public IEnumerator Get<T>(string path, MessageParser<T> parser, Action<ApiResult<T>> cb) where T : IMessage<T>
        => Send<T>(UnityWebRequest.kHttpVerbGET, path, null, parser, cb);


        public IEnumerator Post<TReq, TRes>(string path, TReq msg, MessageParser<TRes> parser, Action<ApiResult<TRes>> cb)
        where TReq : IMessage where TRes : IMessage<TRes>
        {
            var bytes = msg.ToByteArray();
            return Send(UnityWebRequest.kHttpVerbPOST, path, bytes, parser, cb);
        }


        private IEnumerator Send<T>(string method, string path, byte[] body, MessageParser<T> parser, Action<ApiResult<T>> cb) where T : IMessage<T>
        {
            int attempts = 0; ApiResult<T> last = default;
            while (attempts <= config.RetryCount)
            {
                using (var req = new UnityWebRequest(config.BaseUrl + path, method))
                {
                    if (body != null) { req.uploadHandler = new UploadHandlerRaw(body); req.SetRequestHeader("Content-Type", "application/x-protobuf"); }
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.timeout = config.DefaultTimeoutSec;
                    req.SetRequestHeader("Accept", "application/x-protobuf");
                    if (!string.IsNullOrEmpty(authToken)) req.SetRequestHeader("Authorization", $"Bearer {authToken}");


                    yield return req.SendWebRequest();


                    if (req.result == UnityWebRequest.Result.Success && req.responseCode >= 200 && req.responseCode < 300)
                    {
                        try
                        {
                            var data = parser.ParseFrom(req.downloadHandler.data);
                            cb(ApiResult<T>.Success(data, (int)req.responseCode));
                            yield break;
                        }
                        catch (Exception e) { last = ApiResult<T>.Fail("CLIENT_PROTO_PARSE", e.Message, (int)req.responseCode); }
                    }
                    else { var msg = req.error ?? req.downloadHandler?.text; last = ApiResult<T>.Fail("HTTP_ERROR", msg, (int)req.responseCode); }
                }
                attempts++; if (attempts <= config.RetryCount) yield return new WaitForSeconds(config.RetryBackoffSec * attempts);
            }
            cb(last);
        }
    }
}