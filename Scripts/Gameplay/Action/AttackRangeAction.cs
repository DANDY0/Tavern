using GrandDevs.Networking;

namespace GrandDevs.Tavern
{
    public class AttackRangeAction: BaseAction
    {
        public AttackRangeAction(BaseSkill actionSkill) : base(actionSkill)
        {
        }

        public override void Act(RoundProcessingData.ActionData actionData)
        {
            _actionSkill.Execute(actionData);
        }
    }
}