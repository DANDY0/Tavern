using GrandDevs.Networking;

namespace GrandDevs.Tavern
{
    public class HealAction: BaseAction
    {
        public HealAction(BaseSkill actionBaseSkill) : base(actionBaseSkill)
        {
        }

        public override void Act(RoundProcessingData.ActionData actionData)
        {
            _actionSkill.Execute(actionData);
        }
    }
}