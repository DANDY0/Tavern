using GrandDevs.Networking;

namespace GrandDevs.Tavern

{
    public class AttackRegularAction: BaseAction
    {
        public AttackRegularAction(BaseSkill actionSkill) : base(actionSkill)
        {
        }

        public override void Act(RoundProcessingData.ActionData actionData)
        {
            _actionSkill.Execute(actionData);
        }
    }
    
}