using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.ElementalVFX
{
    public class ParticleSystemVfx : VfxReference, ICombatSpeedAffectable
    {
        [SerializeField] ParticleSystem _Vfx;

        public void ApplySpeed(float scale)
        {
            var main = _Vfx.main;
            main.simulationSpeed = scale;
        }

        public override void Play()
        {
            _Vfx.Play();
        }

        public override void Stop()
        {
            _Vfx.Stop();
        }
    }
}
