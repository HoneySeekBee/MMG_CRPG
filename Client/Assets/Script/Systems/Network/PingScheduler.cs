using Client.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingScheduler : MonoBehaviour
{
    private PingNetwork _ping;
    private Coroutine _coroutine;
    private void Awake()
    {
        _ping = new PingNetwork();
    }
    private void OnEnable()
    {
        StartPing();
    }
    private void OnDisable()
    {
        StopPing();
    }
    public void StartPing()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        _coroutine = StartCoroutine(PingLoop());
    }

    public void StopPing()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    private IEnumerator PingLoop()
    {
        yield return new WaitForSeconds(0.1f);
        while (true)
        {
            yield return _ping.SendPing();

            yield return new WaitForSeconds(5f);
        }
    }
}
