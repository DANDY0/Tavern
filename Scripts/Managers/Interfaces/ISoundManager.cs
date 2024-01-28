
using GrandDevs.Tavern.Common;

namespace GrandDevs.Tavern
{
    public interface ISoundManager
    {
        float SoundVolume { get; set; }

        float MusicVolume { get; set; }

        SoundData SoundData { get; }

        void PlaySound(Enumerators.SoundType soundType);

        void StopSound(Enumerators.SoundType soundType);

        void PlayClick();
    }
}