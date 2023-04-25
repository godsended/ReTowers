namespace Core.Utils
{
    public static class ResourcesNamePrettier
    {
        public static string GetIncomePrettyName(string name)
        {
            return name switch
            {
                "Resource_1" => "recruits",
                "Resource_2" => "gems",
                "Resource_3" => "bricks",
                _ => name
            };
        }

        public static string GetResourcePrettyName(string name)
        {
            return name switch
            {
                "Resource_1" => "dungeon",
                "Resource_2" => "magic",
                "Resource_3" => "quarry",
                _ => name
            };
        }
    }
}