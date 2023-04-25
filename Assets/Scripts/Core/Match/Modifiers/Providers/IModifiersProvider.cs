#if !UNITY_ANDROID

using Core.Map;
using Core.Match.Server;

namespace Core.Match.Modifiers.Providers
{
    public interface IModifiersProvider
    {
        public void Provide(MatchServer match, LevelInfo level);
    }
}

#endif