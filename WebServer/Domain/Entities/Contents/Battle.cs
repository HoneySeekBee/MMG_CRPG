using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Contents
{
    public class Battle
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public bool Active { get; private set; }
        public string SceneKey { get; private set; } = string.Empty;
        public bool CheckMulti { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // 기본 생성자 (EF Core용)
        protected Battle() { }
         
        public Battle(string name, string sceneKey, bool active = false, bool checkMulti = false)
        {
            Name = name;
            SceneKey = sceneKey;
            Active = active;
            CheckMulti = checkMulti;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string sceneKey, bool active, bool checkMulti)
        {
            Name = name;
            SceneKey = sceneKey;
            Active = active;
            CheckMulti = checkMulti;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate() => Active = true;
        public void Deactivate() => Active = false;
    }
}
