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
        private IEnumerator Send<T>(string method, string path, byte[] body, MessageParser<T> parser, Action<ApiResult<T>> cb)
    where T : IMessage<T>
        {
            int attempts = 0;
            ApiResult<T> last = default;

            while (attempts <= config.RetryCount)
            {
                using (var req = new UnityWebRequest(config.BaseUrl + path, method))
                {
                    // 요청 구성
                    if (body != null)
                    {
                        req.uploadHandler = new UploadHandlerRaw(body);
                        req.SetRequestHeader("Content-Type", "application/x-protobuf");
                    }
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.timeout = config.DefaultTimeoutSec;
                    req.SetRequestHeader("Accept", "application/x-protobuf");
                    if (!string.IsNullOrEmpty(authToken))
                        req.SetRequestHeader("Authorization", $"Bearer {authToken}");

                    // 전송
                    yield return req.SendWebRequest();

                    // 디버그(원문 확인)
                    var bytes = req.downloadHandler?.data;
                    var ctype = req.GetResponseHeader("Content-Type");
                    var len = bytes?.Length ?? 0;
                    Debug.Log($"[DEBUG_RAW_RESPONSE] code={req.responseCode}, ctype={ctype}, len={len}, text={req.downloadHandler.text}");

                    // HTTP 성공
                    if (req.result == UnityWebRequest.Result.Success && req.responseCode >= 200 && req.responseCode < 300)
                    {
                        // 빈 바디 방어
                        if (bytes == null || bytes.Length == 0)
                        {
                            last = ApiResult<T>.Fail("EMPTY_BODY", "Response body is empty", (int)req.responseCode);
                            cb(last);
                            yield break; // 파싱/바디 오류는 재시도 무의미 → 즉시 종료
                        }

                        try
                        {
                            // 1차: 표준 경로
                            var data = parser.ParseFrom(bytes);
                            cb(ApiResult<T>.Success(data, (int)req.responseCode));
                            yield break;
                        }
                        catch (NullReferenceException nre)
                        {
                            // 2차: 우회 경로(특정 타입에서만 ParseFrom NRE 시)
                            Debug.LogWarning($"[Parser NullRef → MergeFrom fallback] {typeof(T).Name} : {nre.Message}");
                            try
                            {
                                var msg = (Google.Protobuf.IMessage)Activator.CreateInstance(typeof(T));
                                msg.MergeFrom(bytes);
                                cb(ApiResult<T>.Success((T)msg, (int)req.responseCode));
                                yield break;
                            }
                            catch (Exception e2)
                            {
                                last = ApiResult<T>.Fail("CLIENT_PROTO_PARSE", e2.ToString(), (int)req.responseCode);
                                cb(last);
                                yield break; // 파싱 실패는 재시도 X
                            }
                        }
                        catch (Google.Protobuf.InvalidProtocolBufferException ipe)
                        {
                            last = ApiResult<T>.Fail("CLIENT_PROTO_PARSE", ipe.ToString(), (int)req.responseCode);
                            cb(last);
                            yield break; // 파싱 실패는 재시도 X
                        }
                        catch (Exception e)
                        {
                            last = ApiResult<T>.Fail("CLIENT_PROTO_PARSE", e.ToString(), (int)req.responseCode);
                            cb(last);
                            yield break; // 파싱 실패는 재시도 X
                        }
                    }
                    else
                    {
                        // HTTP 에러(이 케이스만 리트라이 고려)
                        var msg = req.error ?? req.downloadHandler?.text;
                        last = ApiResult<T>.Fail("HTTP_ERROR", msg, (int)req.responseCode);
                    }
                }

                // HTTP 에러만 백오프 후 재시도
                attempts++;
                if (attempts <= config.RetryCount)
                    yield return new WaitForSeconds(config.RetryBackoffSec * attempts);
            }

            // 최종 실패 콜백
            cb(last);
        }
    }
}