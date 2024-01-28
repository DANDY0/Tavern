using GrandDevs.Tavern.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrandDevs.Networking;
using Enumerators = GrandDevs.Networking.Enumerators;

namespace GrandDevs.Tavern
{
    public interface INetworkManager
    {
        public ApiRequestHandler APIRequestHandler { get; }
        public string UPID { get; }
        Task<string> PostRequest(string url, string json, Dictionary<string, string> headers = null);
        Task<string> GetRequest(string url, Dictionary<string, string> headers = null);

        public void Connect();
        public void Disconnect();
        public void FindAndJoinRoom();
        public void SendGameEvent<T>(ClientGameplayEvent<T> gameEvent) where T : IGameData;
        public void SubscribeSocketWithParam<T>(Enumerators.NetworkEventType networkEventType, Action<T> subscribeAction);
        public void SubscribeSocketWithoutParam(Enumerators.NetworkEventType networkEventType, Action subscribeAction);

        public void UnSubscribeSocketWithParam<T>(Enumerators.NetworkEventType networkEventType, Action<T> subscribeAction);
        public void UnSubscribeSocketWithoutParam(Enumerators.NetworkEventType networkEventType, Action subscribeAction);

    }
}