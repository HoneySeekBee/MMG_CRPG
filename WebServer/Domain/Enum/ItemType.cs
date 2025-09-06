using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum BindType
    {
        None = 0,
        OnPickup = 1,
        OnEquip = 2
    }

    public enum ItemEffectScope
    {
        OnUse = 0,
        OnEquip = 1,
        Passive = 2
    }

    public enum ItemPriceType
    {
        Buy = 0,
        Sell = 1,
        Upgrade = 2,
        Craft = 3
    }
}
