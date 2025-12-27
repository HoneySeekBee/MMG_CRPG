using PixPlays.ElementalVFX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WeaponTrailVfx : BaseVfx
{
    [SerializeField] private GameObject trailRoot;

    public override void Play(VfxData data)
    {
        base.Play(data);

     
        transform.position = data.Source;
         
        trailRoot.SetActive(true);
    }

    public override void Stop()
    {
        trailRoot.SetActive(false);
        base.Stop();
    }
}