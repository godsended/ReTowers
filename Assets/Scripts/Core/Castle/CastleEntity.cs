using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Core.Castle
{
    public class CastleEntity
    {
        public Tower Tower { get; private set; }
        public Wall Wall { get; private set; }
        public List<BattleResource> Resources { get; private set; }

        [JsonConstructor]
        public CastleEntity([CanBeNull] Tower tower = null, [CanBeNull] Wall wall = null, 
            [CanBeNull] List<BattleResource> resources = null)
        {
            this.Tower = tower ?? new Tower(0, 0);
            this.Wall = wall ?? new Wall(0, 0);
            Resources = resources ?? new List<BattleResource>()
            {
                new BattleResource("Resource_1"),
                new BattleResource("Resource_2"),
                new BattleResource("Resource_3")
            };
        }

        public CastleEntity(CastleEntity castle)
        {
            Tower = new Tower(castle.Tower.MaxHealth, castle.Tower.Health);
            Wall = new Wall(castle.Wall.MaxHealth, castle.Wall.Health);
            Resources = new List<BattleResource>();
            foreach (var res in castle.Resources)
            {
                BattleResource newRes = new BattleResource(res.Name, res.Value, res.Income);
                Resources.Add(newRes);
            }
        }

        public BattleResource GetResource(string name)
        {
            return Resources.FirstOrDefault(r => r.Name == name);
        }
    }
}