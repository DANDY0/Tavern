using GrandDevs.Networking;

namespace GrandDevs.Tavern
{
    public abstract class BaseAction
    {
        protected BaseSkill _actionSkill;
        protected Board _board;
        
        protected BaseAction(BaseSkill actionSkill = null)
        {
            _actionSkill = actionSkill;
            _board = GameClient.Get<IGameplayManager>().Board;
        }

        public virtual void Act(RoundProcessingData.ActionData actionData) { }
        
    }    
}
