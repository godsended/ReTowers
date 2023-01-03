using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Core.Castle
{
    public class CastleEntity
    {
        public Tower Tower { get; private set; }
        public Wall Wall { get; private set; }
        public List<Resource> Resources { get; private set; }

        public CastleEntity([CanBeNull] Tower tower = null, [CanBeNull] Wall wall = null, 
            [CanBeNull] List<Resource> resources = null)
        {
            this.Tower = tower ?? new Tower();
            this.Wall = wall ?? new Wall();
            Resources = resources ?? new List<Resource>()
            {
                new Resource("Resource_1"),
                new Resource("Resource_2"),
                new Resource("Resource_3")
            };
        }

        public Resource GetResource(string name)
        {
            return Resources.FirstOrDefault(r => r.Name == name);
        }
    }
}