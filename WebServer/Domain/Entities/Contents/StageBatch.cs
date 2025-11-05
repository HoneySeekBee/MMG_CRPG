using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Contents
{
    public class StageBatch
    {
        public int Id { get; private set; }          // stage_batch_id
        public int StageId { get; private set; }
        public int BatchNum { get; private set; }
        public string AssetKey { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        protected StageBatch() { }

        public StageBatch(int batchNum, string assetKey)
        {
            BatchNum = batchNum;
            AssetKey = assetKey;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(int batchNum, string assetKey)
        {
            BatchNum = batchNum;
            AssetKey = assetKey;
            UpdatedAt = DateTime.UtcNow;
        } 
    } 
}
