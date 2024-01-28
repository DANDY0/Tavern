using GrandDevs.Networking;

namespace GrandDevs.Tavern
{
    public class UltimateAction: BaseAction
    {
        public UltimateAction(BaseSkill actionSkill) : base(actionSkill)
        {
        }

        public override void Act(RoundProcessingData.ActionData actionData)
        {
            _actionSkill.Execute(actionData);
        }

        public void SetUltimateSkill(BaseSkill skill) => _actionSkill = skill;
        
    }
}