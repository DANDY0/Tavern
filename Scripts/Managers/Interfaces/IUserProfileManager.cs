using External.Essentials.GrandDevs.SocketIONetworking.Scripts;

namespace GrandDevs.Tavern
{
    public interface IUserProfileManager
    {
        Inventory Inventory { get; }
        public MapActionsHandler MapActionsHandler { get; }
        public APIModel.GetUserProfileResponse.UserData UserProfile { get; set; }

        bool IsLocalPlayer(string userID);
        bool IsStaminaEnough();
    }

}