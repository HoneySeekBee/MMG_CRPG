using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Contents
{
    public sealed class StageFirstClearReward
    {
        public int Id { get; private set; }
        public int StageId { get; private set; }
        public int ItemId { get; private set; }
        public short Qty { get; private set; } // >0

        public StageFirstClearReward(int itemId, short qty)
        {
            ItemId = itemId;
            Qty = qty;
        }

        public void Validate()
        {
            if (ItemId <= 0) throw new DomainException("INVALID_ITEM", "ItemId required.");
            if (Qty <= 0) throw new DomainException("INVALID_QTY", "Qty must be > 0.");
        }
    }
}
