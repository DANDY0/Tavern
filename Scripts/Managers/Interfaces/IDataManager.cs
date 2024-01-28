using GrandDevs.Tavern.Common;
using System;

namespace GrandDevs.Tavern
{
    public interface IDataManager
    {
        event Action DataLoadedEvent;

        CachedUserData CachedUserLocalData { get; set; }

        bool DataIsLoaded { get; }

        void LoadConfigurationData();

        void SaveAllData();

        void SaveData(Enumerators.GameDataType gameDataType);

        T Deserialize<T>(string data);

        SpreadsheetInfo GetSpreadsheetByType(Enumerators.SpreadsheetDataType type);
    }
}
