using GrandDevs.Tavern.Common;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class SoundManager : IService, ISoundManager
    {
        private ILoadObjectsManager _loadObjectsManager;

        private List<SoundSource> _soundSources;

        private Transform _soundContainer;

        private AudioListener _audioListener;

        public SoundData SoundData { get; private set; }

        public float SoundVolume { get; set; } = 1f;

        public float MusicVolume { get; set; } = 1f;

        public void Dispose()
        {
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _soundSources = new List<SoundSource>();
            _soundContainer = new GameObject("[SoundContainer]").transform;
            _audioListener = _soundContainer.gameObject.AddComponent<AudioListener>();
            if (MainApp.Instance.transform != null) 
                _soundContainer.SetParent(MainApp.Instance.transform, false);


            SoundData = _loadObjectsManager.GetObjectByPath<SoundData>("Data/SoundData");
        }

        public void Update()
        {
            for (int i = 0; i < _soundSources.Count; i++)
            {
                _soundSources[i].Update();

                if (_soundSources[i].IsSoundEnded())
                {
                    _soundSources[i].Dispose();
                    _soundSources.RemoveAt(i--);
				}
            }   
        }

        public void PlayClick()
        {
            PlaySound(Enumerators.SoundType.Click);
        }

        public void PlaySound(Enumerators.SoundType soundType)
        {
            if (soundType == Enumerators.SoundType.Unknown)
                return;

            var soundInfo = SoundData.sounds.Find(item => item.type == soundType);

            SoundSource foundSameSource = _soundSources.Find(soundSource => soundSource.SoundType == soundType);

            if (foundSameSource != null)
            {
                if (!soundInfo.sfx)
                    return;
            }

            AudioClip sound = soundInfo.clip;
            SoundParameters parameters = new SoundParameters()
            {
                Loop = soundInfo.loop,
                Volume = soundInfo.volume,
                SFX = soundInfo.sfx,
                CrossFade = soundInfo.crossFade
            };

            _soundSources.Add(new SoundSource(_soundContainer, sound, soundType, parameters));
        }

        public void StopSound(Enumerators.SoundType soundType)
        {
            for (int i = 0; i < _soundSources.Count; i++)
            {
                if (_soundSources[i].SoundType == soundType)
                {
                    _soundSources[i].StopPlaying();
                }
            }
        }

        private class SoundSource
        {
            private ISoundManager _soundManager;

            private bool _crossFadeEnded;
            private bool _crossFadeStarted;
            private bool _crossFadeInStarted;
            private bool _crossFadeInEnded;
            private float _crossFadeStep;
            private bool _prepareToEnd;

            public GameObject SoundSourceObject { get; }
            public AudioClip Sound { get; }
            public AudioSource AudioSource { get; }
            public Enumerators.SoundType SoundType { get; }
            public SoundParameters SoundParameters { get; }

            public SoundSource(Transform parent, AudioClip sound, Enumerators.SoundType soundType, SoundParameters parameters)
            {
                _soundManager = GameClient.Get<ISoundManager>();

                Sound = sound;
                SoundType = soundType;
                SoundParameters = parameters;

                SoundSourceObject = new GameObject($"[Sound] - {SoundType} - {Time.time}");
                SoundSourceObject.transform.SetParent(parent);
                AudioSource = SoundSourceObject.AddComponent<AudioSource>();
                AudioSource.clip = Sound;
                AudioSource.loop = SoundParameters.Loop;

                AudioSource.Play();
            }

            public void Update()
			{
                float targetVolume = SoundParameters.SFX ? _soundManager.SoundVolume : _soundManager.MusicVolume;

                if (_crossFadeStarted)
                {
                    AudioSource.volume -= _crossFadeStep;

                    if (AudioSource.volume <= 0)
                    {
                        AudioSource.volume = 0f;

                        _crossFadeStarted = false;
                        _crossFadeEnded = true;

						if (_prepareToEnd)
						{
                            AudioSource.Stop();
                        }
                    }
                }
                else if (_crossFadeInStarted)
				{
                    AudioSource.volume += _crossFadeStep;

                    if (AudioSource.volume >= targetVolume)
                    {
                        AudioSource.volume = targetVolume;

                        _crossFadeInEnded = true;
                        _crossFadeInStarted = false;
                    }
                }
                else
                {
                    AudioSource.volume = targetVolume;
                }

				if (AudioSource.isPlaying && SoundParameters.CrossFade && !_crossFadeStarted)
				{
                    if(AudioSource.time >= Mathf.Max(AudioSource.clip.length * 0.9f, AudioSource.clip.length - _soundManager.SoundData.crossFadeOutTime))
					{
                        PrepareCrossFade(_soundManager.SoundData.crossFadeOutTime);
                    }
				}

                if (AudioSource.isPlaying && SoundParameters.CrossFade && !_crossFadeInStarted && !_crossFadeInEnded)
                {
                    if (AudioSource.time < Mathf.Min(AudioSource.clip.length * 0.1f, _soundManager.SoundData.crossFadeInTime))
                    {
                        AudioSource.volume = 0f;
                        _crossFadeInStarted = true;

                        CalculateCrossFadeStep(targetVolume, _soundManager.SoundData.crossFadeInTime);
                    }
                }
            }

            public bool IsSoundEnded()
            {
                return !AudioSource.loop && !AudioSource.isPlaying && ((!_crossFadeStarted && _crossFadeEnded && SoundParameters.CrossFade) || !SoundParameters.CrossFade);
            }

            public void Dispose()
            {
                AudioSource.Stop();
                MonoBehaviour.Destroy(SoundSourceObject);
            }

            public void StopPlaying()
			{
                AudioSource.loop = false;

                if (SoundParameters.CrossFade)
                {
                    _prepareToEnd = true;
                    PrepareCrossFade(_soundManager.SoundData.crossFadeOutTime);
                }
				else
				{
                    AudioSource.Stop();
                }
            }

            private float CalculateCrossFadeStep(float volume, float time)
			{
                _crossFadeStep = volume / time * Time.deltaTime;

                return _crossFadeStep;
            }

            private void PrepareCrossFade(float time)
			{
                CalculateCrossFadeStep(AudioSource.volume, time);
                _crossFadeStarted = true;
                _crossFadeInEnded = false;
            }
        }

        public class SoundParameters
        {
            public bool Loop { get; set; } = false;
            public bool SFX { get; set; } = true;
            public bool CrossFade { get; set; }
            public float Volume { get; set; } = 1f;
        }
    }
}