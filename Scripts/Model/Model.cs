using System.Collections.Generic;
using GrandDevs.Tavern.Common;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using GrandDevs.Tavern.Helpers;
using System.Threading.Tasks;

namespace GrandDevs.Tavern
{
    public class CachedUserData
    {
        public bool isFirstLaunch;
        public string name;
        public string token;
        public int lastChosenCharacterMenu;
        public int lastChosenCharacterInventory;
        
        public Enumerators.Language appLanguage;
        public float musicVolume;
        public float soundVolume;
    }

    public class SpreadsheetInfo
    {
        public string spreadsheetId = string.Empty;
        public string gid = "0";
        public string format = "csv";

        public bool IsLoaded { get; private set; }
        public string Data { get; private set; }

        public SpreadsheetInfo(string spreadsheetId)
        {
            this.spreadsheetId = spreadsheetId;
        }

        public SpreadsheetInfo(string spreadsheetId, string gid)
        {
            this.spreadsheetId = spreadsheetId;
            this.gid = gid;
        }

        public SpreadsheetInfo(string spreadsheetId, string gid, string format)
        {
            this.spreadsheetId = spreadsheetId;
            this.gid = gid;
            this.format = format;
        }

        public async Task LoadData()
        {
            Data = await GameClient.Get<INetworkManager>().GetRequest($"https://docs.google.com/spreadsheets/export?id={spreadsheetId}&exportFormat={format}&gid={gid}");
            IsLoaded = Data != null;

#if UNITY_EDITOR
            if(IsLoaded)
                Debug.Log($"Loaded spreadsheet: {spreadsheetId}:{gid}");
#endif
        }

        public List<T> GetObject<T>()
        {
            if (!IsLoaded)
                return null;
            return InternalTools.ParseCSV<T>(Data);
        }

        public void Dispose()
        {
            Data = null;
        }
    }
}