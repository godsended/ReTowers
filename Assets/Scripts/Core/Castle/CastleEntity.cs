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
        public List<Resource> Resources { get; private set; }

        [JsonConstructor]
        public CastleEntity([CanBeNull] Tower tower = null, [CanBeNull] Wall wall = null, 
            [CanBeNull] List<Resource> resources = null)
        {
            this.Tower = tower ?? new Tower(0, 0);
            this.Wall = wall ?? new Wall(0, 0);
            Resources = resources ?? new List<Resource>()
            {
                new Resource("Resource_1"),
                new Resource("Resource_2"),
                new Resource("Resource_3")
            };
        }

        public CastleEntity(CastleEntity castle)
        {
            Tower = new Tower(castle.Tower.MaxHealth, castle.Tower.Health);
            Wall = new Wall(castle.Wall.MaxHealth, castle.Wall.Health);
            Resources = new List<Resource>();
            foreach (var res in castle.Resources)
            {
                Resource newRes = new Resource(res.Name, res.Value, res.Income);
                Resources.Add(newRes);
            }
        }

        public Resource GetResource(string name)
        {
            return Resources.FirstOrDefault(r => r.Name == name);
        }
    }
}