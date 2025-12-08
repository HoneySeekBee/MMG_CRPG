using Application.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems
{
    public class CrowdControlSystem
    {
        private const int TickMs = 100;

        public void Run(CombatRuntimeState state)
        {
            foreach (var a in state.ActiveActors.Values)
            {
                if (a.StunMs > 0)
                {
                    a.StunMs -= TickMs;
                    if (a.StunMs <= 0) a.Stunned = false;
                }

                if (a.SilenceMs > 0)
                {
                    a.SilenceMs -= TickMs;
                    if (a.SilenceMs <= 0) a.Silenced = false;
                }

                if (a.FreezeMs > 0)
                {
                    a.FreezeMs -= TickMs;
                    if (a.FreezeMs <= 0) a.Frozen = false;
                }

                if (a.RootMs > 0)
                {
                    a.RootMs -= TickMs;
                    if (a.RootMs <= 0) a.Rooted = false;
                }

                if (a.KnockdownMs > 0)
                {
                    a.KnockdownMs -= TickMs;
                    if (a.KnockdownMs <= 0) a.KnockedDown = false;
                }
            }
        }
    }
}
