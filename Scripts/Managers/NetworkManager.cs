// #define DEBUG_BUILD_MULTIWINDOW

using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Threading.Tasks;
using UnityEngine;
using GrandDevs.Networking;

namespace GrandDevs.Tavern
{
    public class NetworkManager : IService, INetworkManager
    {
        public ApiRequestHandler APIRequestHandler { get; private set; }
        public string UPID { get; private set; }

        public string mode;

        private IDataManager _dataManager;
        private IGameplayManager _gameplayManager;
        private IUserProfileManager _userProfileManager;
        private SocketIONetwork _socket;
        private LoginHandler _loginHandler;
        
        public NetworkManager()
        {
            UPID = SystemInfo.deviceUniqueIdentifier;
        }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _userProfileManager = GameClient.Get<IUserProfileManager>();
            
            _gameplayManager.GameplayEndedEvent += GameplayEndedEventHandler;
            _socket = SocketIONetwork.Get();
            
            APIRequestHandler = new ApiRequestHandler(this);
        }
        
        private async void GameplayEndedEventHandler()
        {
            // uncomment when will post every inventory change
            // await GetUserInventoryAsync();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            _gameplayManager.GameplayEndedEvent -= GameplayEndedEventHandler;
        }

        public void Connect()
        {
            _socket.Connect();
        }

        public void Disconnect()
        {
            _socket.Disconnect();
        }

        public async void FindAndJoinRoom()
        {
            await InitializeUserProfileAsync();

            var userProfile = _userProfileManager.UserProfile;
            if (userProfile != null)
            {
                var characterID = _dataManager.CachedUserLocalData.lastChosenCharacterMenu;
                var item = _userProfileManager.Inventory.GetCharacters()[characterID];
                
                _socket.FindAndJoinRoom(new RoomConfig()
                {
                    @private = false,
                    type = mode,
                    password = string.Empty,
                    platform = "Neutral"
                }, new PlayerConfig()
                {
                    name = userProfile.name,
                    userId = userProfile.id,
                    parameters = new NetworkDictionary(new KeyValuePair<string, object>("character", item.stats.character),
                                                       new KeyValuePair<string, object>("characterItemId", item.id))
                });
            }
            else
            {
                Debug.LogError("Failed to retrieve user profile");
            }
        }

        private async Task InitializeUserProfileAsync()
        { 
            _userProfileManager.UserProfile = await APIRequestHandler.GetUserProfileAsync();
        }
        
        public void SendGameEvent<T>(ClientGameplayEvent<T> gameEvent) where T : IGameData
        {
            _socket.SendGameplayMessage(gameEvent);
        }

        #region Subscriptions

        public void SubscribeSocketWithParam<T>(Enumerators.NetworkEventType networkEventType,
            Action<T> subscribeAction)
        {
            switch (networkEventType)
            {
                case Enumerators.NetworkEventType.GameplayEvent:
                    _socket.GameEventReceivedEvent += subscribeAction as Action<GameEvent>;
                    break;
                case Enumerators.NetworkEventType.FindRooms:
                    _socket.RoomsFoundEvent += subscribeAction as Action<List<RoomInfo>>;
                    break;
                case Enumerators.NetworkEventType.JoinRoomFailed:
                    _socket.FailedToJoinRoomEvent += subscribeAction as Action<string>;
                    break;
                case Enumerators.NetworkEventType.JoinedRoom:
                    _socket.JoinedRoomEvent += subscribeAction as Action<RoomInfo>;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(networkEventType), networkEventType, null);
            }
        }

        public void SubscribeSocketWithoutParam(Enumerators.NetworkEventType networkEventType, Action subscribeAction)
        {
            switch (networkEventType)
            {
                case Enumerators.NetworkEventType.Connect:
                    _socket.ConnectedEvent += subscribeAction;
                    break;
                case Enumerators.NetworkEventType.Disconnect:
                    _socket.DisconnectedEvent += subscribeAction;
                    break;
                case Enumerators.NetworkEventType.ConnectError:
                    _socket.ConnectFailedEvent += subscribeAction;
                    break;
                case Enumerators.NetworkEventType.LeaveRoom:
                    _socket.RoomLeftEvent += subscribeAction;
                    break;
                case Enumerators.NetworkEventType.RestoreConnection:
                    _socket.ReconnectedEvent += subscribeAction;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(networkEventType), networkEventType, null);
            }
        }

        public void UnSubscribeSocketWithParam<T>(Enumerators.NetworkEventType networkEventType,
            Action<T> subscribeAction)
        {
            switch (networkEventType)
            {
                case Enumerators.NetworkEventType.GameplayEvent:
                    _socket.GameEventReceivedEvent -= subscribeAction as Action<GameEvent>;
                    break;
                case Enumerators.NetworkEventType.FindRooms:
                    _socket.RoomsFoundEvent -= subscribeAction as Action<List<RoomInfo>>;
                    break;
                case Enumerators.NetworkEventType.JoinRoomFailed:
                    _socket.FailedToJoinRoomEvent -= subscribeAction as Action<string>;
                    break;
                case Enumerators.NetworkEventType.JoinedRoom:
                    _socket.JoinedRoomEvent -= subscribeAction as Action<RoomInfo>;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(networkEventType), networkEventType, null);
            }
        }

        public void UnSubscribeSocketWithoutParam(Enumerators.NetworkEventType networkEventType, Action subscribeAction)
        {
            switch (networkEventType)
            {
                case Enumerators.NetworkEventType.Connect:
                    _socket.ConnectedEvent -= subscribeAction;
                    break;
                case Enumerators.NetworkEventType.Disconnect:
                    _socket.DisconnectedEvent -= subscribeAction;
                    break;
                case Enumerators.NetworkEventType.ConnectError:
                    _socket.ConnectFailedEvent -= subscribeAction;
                    break;
                case Enumerators.NetworkEventType.LeaveRoom:
                    _socket.RoomLeftEvent -= subscribeAction;
                    break;
                case Enumerators.NetworkEventType.RestoreConnection:
                    _socket.ReconnectedEvent -= subscribeAction;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(networkEventType), networkEventType, null);
            }
        }

        #endregion

        public async Task<string> PostRequest(string url, string json, Dictionary<string, string> headers = null)
        {
            using (UnityWebRequest uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                if (headers != null)
                {
                    foreach (var header in headers)
                        uwr.SetRequestHeader(header.Key, header.Value);
                }

                UploadHandlerRaw MyUploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
                MyUploadHandler.contentType = "application/json";
                uwr.uploadHandler = MyUploadHandler;
                DownloadHandler downloadHandler = new DownloadHandlerBuffer();
                uwr.downloadHandler = downloadHandler;

                var operation = uwr.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Delay(100);
                }

                if (uwr.result != UnityWebRequest.Result.Success)
                {
#if UNITY_EDITOR
                    Debug.LogError($"Failed to load: {url} due to error: {uwr.error}");
#endif
                    return null;
                }
                else
                {
                    return System.Text.Encoding.UTF8.GetString(uwr.downloadHandler.data);
                }
            }
        }

        public async Task<string> GetRequest(string url, Dictionary<string, string> headers = null)
        {
            using (UnityWebRequest uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
            {
                if (headers != null)
                {
                    foreach (var header in headers)
                        uwr.SetRequestHeader(header.Key, header.Value);
                }

                DownloadHandler downloadHandler = new DownloadHandlerBuffer();
                uwr.downloadHandler = downloadHandler;

                var operation = uwr.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Delay(100);
                }

                if (uwr.result != UnityWebRequest.Result.Success)
                {
#if UNITY_EDITOR
                    Debug.LogError($"Failed to load: {url} due to error: {uwr.error}");
#endif
                    return null;
                }
                else
                {
                    return System.Text.Encoding.UTF8.GetString(uwr.downloadHandler.data);
                }
            }
        }








    }
}