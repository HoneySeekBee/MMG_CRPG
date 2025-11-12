using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contents.Battles
{
    public class CreateBattleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string SceneKey { get; set; } = string.Empty;
        public bool Active { get; set; } = false;
        public bool CheckMulti { get; set; } = false;
    }
    public class UpdateBattleRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SceneKey { get; set; } = string.Empty;
        public bool Active { get; set; }
        public bool CheckMulti { get; set; }
    }
}
