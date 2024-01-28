using System;
using System.Collections.Generic;
using GrandDevs.Networking;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class DefenceSelf: BaseSkill
    {
        public DefenceSelf(Player player) : base(player)
        {
        }

        public override void Execute(RoundProcessingData.ActionData actionData)
        {
            base.Execute(actionData);
            
            _cellView.PlayActionParticles(_currentCell, _actionType);
            _currentCell.GetCharacter().PlayAnimation(Constants.DefenseHash);
            
            var targets = actionData.parameters.targets;
            if(targets!=null)
                TakeDefense(targets);

            Debug.LogWarning($"DEFENSE ID: {_board.GetCell(actionData.parameters.actor.x,actionData.parameters.actor.y).GetCharacter().GetPlayerData().PlayerID}" +
                             $"  ACTOR {actionData.parameters.actor.x} : {actionData.parameters.actor.y}");
        }

        private void TakeDefense(List<RoundProcessingData.TargetData> targets)
        {
            foreach (var target in targets)
            {
                var player = _board.GetCell(target.position.x, target.position.y).GetCharacter();
                player.Health.SetDefense(target.value);
            }
        }
    }
}