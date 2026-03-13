using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/ApiConfig")]
public class ApiConfig : ScriptableObject
{
    [Header("Base URL (ex: https://api.example.com)")] public string BaseUrl;
    [Header("CombatServer URL (ex: http://localhost:5001)")] public string CombatServerUrl;
    [Range(1, 10)] public int DefaultTimeoutSec = 10;
    [Range(0, 5)] public int RetryCount = 2;
    [Range(0.1f, 5f)] public float RetryBackoffSec = 0.5f;
}
