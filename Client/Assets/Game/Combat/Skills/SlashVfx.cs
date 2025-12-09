using PixPlays.ElementalVFX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SlashVfx : BaseVfx
{
    [SerializeField] ParticleSystem[] particles;

    public override void Play(VfxData data)
    {
        base.Play(data);

        // 위치 = 캐릭터 무기 또는 손 위치
        transform.position = data.Source;

        // 방향 = 공격 방향
        Vector3 dir = data.Target - data.Source;
        dir.y = 0; // 슬래시는 위/아래 무시하는게 보통
        transform.forward = dir.normalized;

        // 파티클 재생
        foreach (var p in particles)
            p.Play();
    }

    public override void Stop()
    {
        foreach (var p in particles)
            p.Stop();

        base.Stop(); // 자동 destroy
    }
}