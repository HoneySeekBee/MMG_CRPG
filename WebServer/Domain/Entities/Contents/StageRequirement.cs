using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Contents
{ 
    public sealed class StageRequirement
    {
        public int Id { get; private set; }
        public int StageId { get; private set; }
        public int? RequiredStageId { get; private set; }
        public short? MinAccountLevel { get; private set; }

        public StageRequirement(int? requiredStageId = null, short? minAccountLevel = null)
        {
            RequiredStageId = requiredStageId;
            MinAccountLevel = minAccountLevel;
        }

        public void Validate()
        {
            if (RequiredStageId is null && MinAccountLevel is null)
                throw new DomainException("INVALID_REQUIREMENT", "At least one requirement must be set.");
            if (MinAccountLevel is < 1)
                throw new DomainException("INVALID_ACCOUNT_LEVEL", "MinAccountLevel must be ≥ 1 when set.");
        }
    }
}
