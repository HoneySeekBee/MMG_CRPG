using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game.Logging
{
    public static class Logger
    {
        public static void Info(string msg) => Debug.Log($"[INFO] {msg}");
        public static void Warn(string msg) => Debug.LogWarning($"[WARN] {msg}");
        public static void Error(string msg) => Debug.LogError($"[ERROR] {msg}");
    }
}