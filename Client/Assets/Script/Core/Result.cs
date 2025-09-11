using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public struct ApiResult<T>
    {
        public bool Ok;
        public int StatusCode;
        public string ErrorCode; // 서버 에러코드 맵핑용
        public string Message;
        public T Data;

        public static ApiResult<T> Success(T data, int statusCode = 200)
        => new ApiResult<T> { Ok = true, StatusCode = statusCode, Data = data };


        public static ApiResult<T> Fail(string errorCode, string message, int statusCode)
        => new ApiResult<T> { Ok = false, ErrorCode = errorCode, Message = message, StatusCode = statusCode };
    }
}
