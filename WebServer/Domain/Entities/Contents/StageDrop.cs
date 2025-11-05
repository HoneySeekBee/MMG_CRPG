using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Contents
{

    public sealed class StageDrop
    {
        public int Id { get; private set; }
        public int StageId { get; private set; }
        public int ItemId { get; private set; }
        public decimal Rate { get; private set; }     // 0..1
        public short MinQty { get; private set; }     // ≥0
        public short MaxQty { get; private set; }     // ≥Min
        public bool FirstClearOnly { get; private set; }

        public StageDrop(int itemId, decimal rate, short minQty, short maxQty, bool firstClearOnly = false)
        {
            ItemId = itemId;
            Rate = rate;
            MinQty = minQty;
            MaxQty = maxQty;
            FirstClearOnly = firstClearOnly;
        }

        public void Validate()
        {
            if (ItemId <= 0) throw new DomainException("INVALID_ITEM", "ItemId required.");
            if (Rate < 0m || Rate > 1.0m) throw new DomainException("INVALID_RATE", "Rate must be between 0 and 1.");
            if (MinQty < 0) throw new DomainException("INVALID_QTY_MIN", "MinQty must be ≥ 0.");
            if (MaxQty < MinQty) throw new DomainException("INVALID_QTY_MAX", "MaxQty must be ≥ MinQty.");
        }
    }
}
