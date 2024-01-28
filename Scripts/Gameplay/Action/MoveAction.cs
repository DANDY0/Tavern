using GrandDevs.Networking;

namespace GrandDevs.Tavern
{
    public class MoveAction: BaseAction 
    {
        public MoveAction(BaseSkill actionSkill) : base(actionSkill)
        {
        }
        
        public override void Act(RoundProcessingData.ActionData actionData)
        {
            _actionSkill.Execute(actionData);
        }

    }
}