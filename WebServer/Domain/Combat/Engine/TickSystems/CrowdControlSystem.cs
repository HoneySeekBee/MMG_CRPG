using Domain.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Combat.Engine.TickSystems
{
    public class CrowdControlSystem
    {
        public void Run(CombatRuntimeState state, int dtMs)
        {
            foreach (var a in state.ActiveActors.Values)
            {
                if (a.StunMs > 0)
                {
                    a.StunMs -= dtMs;
                    if (a.StunMs <= 0)
                    {
                        a.StunMs = 0;
                        a.Stunned = false;
                    }
                }

                if (a.SilenceMs > 0)
                {
                    a.SilenceMs -= dtMs;
                    if (a.SilenceMs <= 0)
                    {
                        a.SilenceMs = 0;
                        a.Silenced = false;
                    }
                }

                if (a.FreezeMs > 0)
                {
                    a.FreezeMs -= dtMs;
                    if (a.FreezeMs <= 0)
                    {
                        a.FreezeMs = 0;
                        a.Frozen = false;
                    }
                }

                if (a.RootMs > 0)
                {
                    a.RootMs -= dtMs;
                    if (a.RootMs <= 0)
                    {
                        a.RootMs = 0;
                        a.Rooted = false;
                    }
                }

                if (a.KnockdownMs > 0)
                {
                    a.KnockdownMs -= dtMs;
                    if (a.KnockdownMs <= 0)
                    {
                        a.KnockdownMs = 0;
                        a.KnockedDown = false;
                    }
                }
            }
        }
    }
}
