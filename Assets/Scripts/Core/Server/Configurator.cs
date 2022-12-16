using System.IO;
using IniParser;
using IniParser.Model;
using UnityEngine;

namespace Core.Server
{
    public class Configurator : MonoBehaviour
    {
        public static FileIniDataParser parser;
        public static IniData data;

        private string _serverConfigurationFile = "Configuration.ini";

#if UNITY_EDITOR
        private void Awake()
        {
            parser = new FileIniDataParser();
            data = parser.ReadFile($@"{Application.dataPath}/Configuration.ini");
        }
#else
        private void Awake()
        {
            var path = Path.Combine(Application.streamingAssetsPath, _serverConfigurationFile);
            parser = new FileIniDataParser();
            data = parser.ReadFile(path);
        }
#endif
    }
}