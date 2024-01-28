using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using External.Essentials.GrandDevs.SocketIONetworking.Scripts;
using GrandDevs.Networking;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class ApiRequestHandler
    {
        public event Action<List<APIModel.GetInventoryResponse.ResponseItemData>> InventoryReceivedEvent;
        public event Action<APIModel.GetUserProfileResponse.UserData> UserReceivedEvent;
        public event Action SetUserEvent;
        public event Action SignInEvent;
        public event Action<APIModel.GameConfigData> GetApiConfigEvent;
        public event Action<APIModel.TravelOnMapResponse> TravelOnMapResponseEvent;
        public event Action<APIModel.GetMeOnMapResponse> GetMeOnMapResponseEvent;

        private readonly INetworkManager _networkManager;
        private readonly IDataManager _dataManager;
        private readonly LoginHandler _loginHandler;

        private string _userTokenKey = "DefaultToken";
        private string _loginCode;

        private Dictionary<Enumerators.ApiEndpoint, string> _apiUrls = new Dictionary<Enumerators.ApiEndpoint, string>
        {
            { Enumerators.ApiEndpoint.SignIn, $"{Constants.Url}users/sign-in" },
            { Enumerators.ApiEndpoint.SetUser, $"{Constants.Url}users/set" },
            { Enumerators.ApiEndpoint.GetUser, $"{Constants.Url}users/get" },
            { Enumerators.ApiEndpoint.GetInventory, $"{Constants.Url}inventory/get" },
            { Enumerators.ApiEndpoint.UseItem, $"{Constants.Url}items/use" },
            { Enumerators.ApiEndpoint.TravelOnMap, $"{Constants.Url}map/travel" },
            { Enumerators.ApiEndpoint.GetMeOnMap, $"{Constants.Url}map/get/me" },
            { Enumerators.ApiEndpoint.GetGameConfig, $"{Constants.Url}config/get" },
            { Enumerators.ApiEndpoint.GetGameConfigVersion, $"{Constants.Url}config/get/version" },
            { Enumerators.ApiEndpoint.PassBattle, $"{Constants.Url}map/battle/pass" }
    };

        public ApiRequestHandler(INetworkManager networkManager)
        {
            _networkManager = networkManager;
            _dataManager = GameClient.Get<IDataManager>();
            _loginHandler = new LoginHandler(_networkManager);

            string loginCode = _loginHandler.ReadLoginCodeFromFile();
            _loginCode = loginCode ?? networkManager.UPID;
        }
        
        #region UserProfile

        private async Task<string> SignIn()
        {
            string url = _apiUrls[Enumerators.ApiEndpoint.SignIn];
    
            APIModel.SignInRequest request = new APIModel.SignInRequest
            {
                loginCode = _loginCode
            };

            string jsonRequest = Tools.Serialize(request, Enumerators.SerializationToolType.NewtonsoftJson);
    
            return await _networkManager.PostRequest(url, jsonRequest);
        }
        
        public async Task SignInAndStoreToken()
        {
            string response = await SignIn();
            if (string.IsNullOrEmpty(response))
            {
                Debug.LogError($"Sign in response is Empty");
                return;
            }

            var signInResponse = Tools.Deserialize<APIModel.LoginResponse>(response, Enumerators.SerializationToolType.NewtonsoftJson);
            string token = signInResponse.result.token;

            _dataManager.CachedUserLocalData.token = token;
            var playerName = GameClient.Get<IDataManager>().CachedUserLocalData.name;
            
            if(_dataManager.CachedUserLocalData.isFirstLaunch)
                await SetUserProfile(token, playerName);

            if (signInResponse.status)
            {
                SignInEvent?.Invoke();
                await GetGameConfigAsync();
                await GetUserInventoryAsync();
            }
            
            
            ShowResponseResult(signInResponse.status, signInResponse.message,
                typeof(APIModel.UserProfileResponse));
        }

        public async Task SetUserProfile(string token, string userName)
        {
            string userProfileUrl = _apiUrls[Enumerators.ApiEndpoint.SetUser];

            APIModel.UserProfileRequest request = new APIModel.UserProfileRequest
            {
                token = token,
                parameters = new APIModel.UserProfileRequest.UserProfileParameters()
                {
                    name = userName
                }
            };

            string jsonRequest = Tools.Serialize(request, Enumerators.SerializationToolType.NewtonsoftJson);
            string response = await _networkManager.PostRequest(userProfileUrl, jsonRequest);

            var userProfileResponse = Tools.Deserialize<APIModel.UserProfileResponse>(response, Enumerators.SerializationToolType.NewtonsoftJson);

            ShowResponseResult(userProfileResponse.status, userProfileResponse.message,
                typeof(APIModel.UserProfileResponse));

            if (userProfileResponse.status) 
                SetUserEvent?.Invoke();
        }

        public async Task<APIModel.GetUserProfileResponse.UserData> GetUserProfileAsync()
        {
            string userProfileUrl = _apiUrls[Enumerators.ApiEndpoint.GetUser];

            var request = new APIModel.GetUserProfileRequest { token = _dataManager.CachedUserLocalData.token };

            string jsonRequest = Tools.Serialize(request, Enumerators.SerializationToolType.NewtonsoftJson);

            string response = await _networkManager.PostRequest(userProfileUrl, jsonRequest);

            var userProfileResponse =
                Tools.Deserialize<APIModel.GetUserProfileResponse>(response,
                    Enumerators.SerializationToolType.NewtonsoftJson);

            if (userProfileResponse.status)
                UserReceivedEvent?.Invoke(userProfileResponse.result.user);

            ShowResponseResult(userProfileResponse.status, userProfileResponse.message,
                typeof(APIModel.GetUserProfileResponse));

            return userProfileResponse.result.user;
        }

        #endregion

        #region Inventory

        public async Task<List<APIModel.GetInventoryResponse.ResponseItemData>> GetUserInventoryAsync()
        {
            string inventoryUrl = _apiUrls[Enumerators.ApiEndpoint.GetInventory];

            var request = new APIModel.GetInventoryRequest { token = _dataManager.CachedUserLocalData.token };

            string jsonRequest = Tools.Serialize(request, Enumerators.SerializationToolType.NewtonsoftJson);
            string response = await _networkManager.PostRequest(inventoryUrl, jsonRequest);

            var inventoryResponse = Tools.Deserialize<APIModel.GetInventoryResponse>(response, Enumerators.SerializationToolType.NewtonsoftJson);

            ShowResponseResult(inventoryResponse.status, inventoryResponse.message,
                typeof(APIModel.GetInventoryResponse));
            if (inventoryResponse.status)
                InventoryReceivedEvent?.Invoke(inventoryResponse.result.elements);
            return inventoryResponse.result.elements;
        }

        public async Task<bool> EquipArtifactAsync(string itemId, string targetCharacterItemId)
        {
            string useItemUrl = _apiUrls[Enumerators.ApiEndpoint.UseItem];

            APIModel.UseItemRequest request = new APIModel.UseItemRequest
            {
                token = _dataManager.CachedUserLocalData.token,
                item = itemId,
                parameters = new APIModel.UseItemRequest.UseItemParameters
                {
                    target = targetCharacterItemId,
                    equip = true
                }
            };

            string jsonRequest = Tools.Serialize(request, Enumerators.SerializationToolType.NewtonsoftJson);
            string response = await _networkManager.PostRequest(useItemUrl, jsonRequest);

            var useItemResponse =
                Tools.Deserialize<APIModel.UseItemResponse>(response, Enumerators.SerializationToolType.NewtonsoftJson);

            ShowResponseResult(useItemResponse.status, useItemResponse.message,
                typeof(APIModel.UseItemResponse));

            return useItemResponse.status;
        }

        public async Task<bool> UnEquipArtifactAsync(string itemId, string targetCharacterItemId)
        {
            string useItemUrl = _apiUrls[Enumerators.ApiEndpoint.UseItem];

            APIModel.UseItemRequest request = new APIModel.UseItemRequest
            {
                token = _dataManager.CachedUserLocalData.token,
                item = itemId,
                parameters = new APIModel.UseItemRequest.UseItemParameters
                {
                    target = targetCharacterItemId,
                    equip = false
                }
            };

            string jsonRequest = Tools.Serialize(request, Enumerators.SerializationToolType.NewtonsoftJson);
            string response = await _networkManager.PostRequest(useItemUrl, jsonRequest);

            var useItemResponse =
                Tools.Deserialize<APIModel.UseItemResponse>(response, Enumerators.SerializationToolType.NewtonsoftJson);

            ShowResponseResult(useItemResponse.status, useItemResponse.message,
                typeof(APIModel.UseItemResponse));

            return useItemResponse.status;
        }

        #endregion

        #region Map

        public async Task<APIModel.TravelOnMapResponse> TravelOnMapAsync(int position)
        {
            string travelOnMapUrl = _apiUrls[Enumerators.ApiEndpoint.TravelOnMap];

            APIModel.TravelOnMapRequest request = new APIModel.TravelOnMapRequest
            {
                token = _dataManager.CachedUserLocalData.token,
                to = position
            };

            string jsonRequest = Tools.Serialize(request, Enumerators.SerializationToolType.NewtonsoftJson);
            string response = await _networkManager.PostRequest(travelOnMapUrl, jsonRequest);

            var travelOnMapResponse = Tools.Deserialize<APIModel.TravelOnMapResponse>(response, Enumerators.SerializationToolType.NewtonsoftJson);
            
            if(travelOnMapResponse.status)
                TravelOnMapResponseEvent?.Invoke(travelOnMapResponse);
            
            ShowResponseResult(travelOnMapResponse.status, travelOnMapResponse.message, typeof(APIModel.TravelOnMapResponse));

            return travelOnMapResponse;
        }

        public async Task<APIModel.GetMeOnMapResponse> GetMeOnMapAsync()
        {
            string getMeOnMapUrl = _apiUrls[Enumerators.ApiEndpoint.GetMeOnMap];

            APIModel.GetMeOnMapRequest request = new APIModel.GetMeOnMapRequest
            {
                token = _dataManager.CachedUserLocalData.token
            };

            string jsonRequest = Tools.Serialize(request, Enumerators.SerializationToolType.NewtonsoftJson);
            string response = await _networkManager.PostRequest(getMeOnMapUrl, jsonRequest);

            var getMeOnMapResponse = Tools.Deserialize<APIModel.GetMeOnMapResponse>(response, Enumerators.SerializationToolType.NewtonsoftJson);
            
            if(getMeOnMapResponse.status)
                GetMeOnMapResponseEvent?.Invoke(getMeOnMapResponse);
            
            ShowResponseResult(getMeOnMapResponse.status, getMeOnMapResponse.message,
                typeof(APIModel.GetMeOnMapResponse));

            return getMeOnMapResponse;
        }

        public async Task<APIModel.BattlePassResponse> PassBattleAsync(int battleId, bool skip)
        {
            string passBattleUrl = _apiUrls[Enumerators.ApiEndpoint.PassBattle];

            APIModel.BattlePassRequest request = new APIModel.BattlePassRequest
            {
                token = _dataManager.CachedUserLocalData.token,
                battleId = battleId.ToString(),
                skip = skip,
            };

            string jsonRequest = Tools.Serialize(request, Enumerators.SerializationToolType.NewtonsoftJson);
            string response = await _networkManager.PostRequest(passBattleUrl, jsonRequest);

            var passBattleResponse = Tools.Deserialize<APIModel.BattlePassResponse>(response, Enumerators.SerializationToolType.NewtonsoftJson);
    
            ShowResponseResult(passBattleResponse.status, passBattleResponse.message, typeof(APIModel.BattlePassResponse));

            Debug.LogError($"Battle Result: {passBattleResponse.status}");
            
            return passBattleResponse;
        }

        #endregion

        #region GameConfig

        private async Task<APIModel.GameConfigResponse> GetGameConfigAsync()
        {
            string gameConfigUrl = _apiUrls[Enumerators.ApiEndpoint.GetGameConfig];

            APIModel.GameConfigRequest request = new APIModel.GameConfigRequest
            {
                token = _dataManager.CachedUserLocalData.token
            };

            string jsonRequest = Tools.Serialize(request, Enumerators.SerializationToolType.NewtonsoftJson);
            string response = await _networkManager.PostRequest(gameConfigUrl, jsonRequest);

            var gameConfigResponse =
                Tools.Deserialize<APIModel.GameConfigResponse>(response,
                    Enumerators.SerializationToolType.NewtonsoftJson);

            if (gameConfigResponse.status)
                GetApiConfigEvent?.Invoke(gameConfigResponse.result.config);
            ShowResponseResult(gameConfigResponse.status, gameConfigResponse.message,
                typeof(APIModel.GameConfigResponse));

            return gameConfigResponse;
        }

        public async Task<APIModel.GameConfigVersionResponse> GetGameConfigVersionAsync()
        {
            string gameConfigVersionUrl = _apiUrls[Enumerators.ApiEndpoint.GetGameConfigVersion];

            APIModel.GameConfigVersionRequest request = new APIModel.GameConfigVersionRequest
            {
                token = _dataManager.CachedUserLocalData.token
            };

            string jsonRequest = Tools.Serialize(request, Enumerators.SerializationToolType.NewtonsoftJson);
            string response = await _networkManager.PostRequest(gameConfigVersionUrl, jsonRequest);

            var gameConfigVersionResponse =
                Tools.Deserialize<APIModel.GameConfigVersionResponse>(response,
                    Enumerators.SerializationToolType.NewtonsoftJson);
            ShowResponseResult(gameConfigVersionResponse.status, gameConfigVersionResponse.message,
                typeof(APIModel.GameConfigVersionResponse));

            return gameConfigVersionResponse;
        }

        #endregion

        private void ShowResponseResult(bool status, string message, Type responseType)
        {
            if (status)
                Debug.Log($"Successful operation {responseType.Name}");
            else
                Debug.LogError($"Error {message}");
        }
    }
}