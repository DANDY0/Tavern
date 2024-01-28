using GrandDevs.Networking;
using System;
using System.Collections.Generic;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class HealSelf: BaseSkill
    {
        public HealSelf(Player player) : base(player)
        {
        }

        public override void Execute(RoundProcessingData.ActionData actionData)
        {
            base.Execute(actionData);
            
            Enumerators.ActionType actionDataType = (Enumerators.ActionType)Enum.Parse(typeof(Enumerators.ActionType), actionData.type);
            
            _cellView.PlayActionParticles(_currentCell, actionDataType);
           _currentCell.GetCharacter().PlayAnimation(Constants.HealHash);

           var targets = actionData.parameters.targets;
           if(targets!=null)
            TakeHeal(targets);
           
           Debug.LogWarning($"HEAL ID: {_board.GetCell(actionData.parameters.actor.x,actionData.parameters.actor.y).GetCharacter().GetPlayerData().PlayerID}" +
                            $"  ACTOR {actionData.parameters.actor.x} : {actionData.parameters.actor.y}");

        }

        private void TakeHeal(List<RoundProcessingData.TargetData> targets)
        {
            foreach (var target in targets)
            {
                var player = _board.GetCell(target.position.x, target.position.y).GetCharacter();
                player.Health.ChangeHealth(target.value);
            }
        }
    }
}