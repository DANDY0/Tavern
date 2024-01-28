using GrandDevs.Networking;

namespace GrandDevs.Tavern
{
    public class DefenseAction: BaseAction
    {
        public DefenseAction(BaseSkill actionBaseSkill) : base(actionBaseSkill)
        {
        }

        public override void Act(RoundProcessingData.ActionData actionData)
        {
            
            _actionSkill.Execute(actionData);

            /*
            var parameters = actionData.parameters;
            var player = currentCell.GetCharacter();
            var playerID = player.GetPlayerData().PlayerID;
            var playerVfxHandler = player.GetVfxHandler();
            var playerAnimationsHandler = player.GetAnimationsHandler();
            
            var targetUnits = actionData.parameters.targets;

            playerVfxHandler.DefenceSelf(_gameplayController.GetPlayerByID(playerID).GetPlayerTransform());
            playerAnimationsHandler.DefenceSelf();
        */
        }
    }
}