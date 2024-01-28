using GrandDevs.Tavern.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;

namespace GrandDevs.Tavern
{
    public class DataManager : IService, IDataManager
    {
        public event Action DataLoadedEvent;

        private readonly string StaticDataPrivateKey = "FPXrKfU2RxYddZpQdJb2VjDW2DwAzReCtedVhvfUQvcALCJf44apEg2BfadsE7RyffBwnGSE";

        public CachedUserData CachedUserLocalData { get; set; }

        public readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            Converters =
            {
                new StringEnumConverter(),
            }
        };

        private Dictionary<Enumerators.GameDataType, string> _gameDataPathes;

        private Dictionary<Enumerators.SpreadsheetDataType, SpreadsheetInfo> _spreadsheetsInfo;

        private ILocalizationManager _localizationManager;

        public bool DataIsLoaded { get; private set; }

        public void Init()
        {
            _gameDataPathes = new Dictionary<Enumerators.GameDataType, string>()
            {
                 { Enumerators.GameDataType.UserData, $"{Application.persistentDataPath}/{Enumerators.GameDataType.UserData}.dat" },
            };

            _localizationManager = GameClient.Get<ILocalizationManager>();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            SaveAllData();
        }

        public void LoadConfigurationData()
        {
            FillSpreadsheetsInfo();

            LoadAllData();
        }

        public void SaveAllData()
        {
            foreach (Enumerators.GameDataType key in _gameDataPathes.Keys)
            {
                SaveData(key);
            }
        }

        public async void LoadAllData()
        {
            foreach (Enumerators.GameDataType key in _gameDataPathes.Keys)
            {
                LoadData(key);
            }

            await StartLoadSpreadsheetsData();

            DataIsLoaded = true;

            DataLoadedEvent?.Invoke();
        }

        public void SaveData(Enumerators.GameDataType gameDataType)
        {
            string data = string.Empty;
            string dataPath = _gameDataPathes[gameDataType];

            switch (gameDataType)
            {
                case Enumerators.GameDataType.UserData:
                    CachedUserLocalData.isFirstLaunch = false;
                    data = Serialize(CachedUserLocalData);
                    break;
                

                default:break;
            }

            if (data.Length > 0)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                PlayerPrefs.SetString(gameDataType.ToString(), data);
                PlayerPrefs.Save();
#else
                if (!File.Exists(dataPath)) File.Create(dataPath).Close();

                File.WriteAllText(dataPath, data);
#endif
            }
        }

        public void LoadData(Enumerators.GameDataType gameDataType)
        {
            string dataPath = _gameDataPathes[gameDataType];

            switch (gameDataType)
            {
                case Enumerators.GameDataType.UserData:
                    CachedUserLocalData = DeserializeFromPath<CachedUserData>(dataPath, gameDataType);
                    if (CachedUserLocalData == null)
                    {
                        CachedUserLocalData = new CachedUserData();
                        CachedUserLocalData.name = "Default_Name";
                        CachedUserLocalData.token = String.Empty;
                        CachedUserLocalData.isFirstLaunch = true;
                        CachedUserLocalData.lastChosenCharacterMenu = 0;
                        CachedUserLocalData.lastChosenCharacterInventory = 0;
                        CachedUserLocalData.appLanguage = _localizationManager.DefaultLanguage;
                        CachedUserLocalData.musicVolume = 1f;
                        CachedUserLocalData.soundVolume = 1f;

                        SaveData(Enumerators.GameDataType.UserData);
                    }

                    GameClient.Get<ISoundManager>().SoundVolume = CachedUserLocalData.soundVolume;
                    GameClient.Get<ISoundManager>().MusicVolume = CachedUserLocalData.musicVolume;
                    break;
              
                default: break;
            }
        }

        public SpreadsheetInfo GetSpreadsheetByType(Enumerators.SpreadsheetDataType type)
        {
            return _spreadsheetsInfo[type];
        }

        public T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data, JsonSerializerSettings);
        }

        public T DeserializeFromPath<T>(string path, Enumerators.GameDataType type) where T : class
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!PlayerPrefs.HasKey(type.ToString()))
                return null;

            return JsonConvert.DeserializeObject<T>(Decrypt(PlayerPrefs.GetString(type.ToString())), JsonSerializerSettings);
#else
            if (!File.Exists(path))
                return null;

            return JsonConvert.DeserializeObject<T>(Decrypt(File.ReadAllText(path)), JsonSerializerSettings);
#endif
        }

        public string Serialize(object @object, Formatting formatting = Formatting.Indented)
        {
            return Encrypt(JsonConvert.SerializeObject(@object, formatting));
        }

        private string Decrypt(string data)
        {
            return Constants.DataEncrypted ? Utilites.Decrypt(data, StaticDataPrivateKey) : data;
        }

        private string Encrypt(string data)
        {
            return Constants.DataEncrypted ? Utilites.Encrypt(data, StaticDataPrivateKey) : data;
        }

        private async Task StartLoadSpreadsheetsData()
        {
            foreach (var item in _spreadsheetsInfo)
            {
                await item.Value.LoadData();
            }
        }

        private void FillSpreadsheetsInfo()
        {
            _spreadsheetsInfo = new Dictionary<Enumerators.SpreadsheetDataType, SpreadsheetInfo>();

            if(_localizationManager.LocalizationData.refreshLocalizationAtStart)
                _spreadsheetsInfo.Add(Enumerators.SpreadsheetDataType.Localization, new SpreadsheetInfo(_localizationManager.LocalizationData.localizationGoogleSpreadsheet));
        }
    }
}