using System;
using System.Collections.Generic;
using GrandDevs.Networking;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class DefenseEqualHealthSelf: BaseSkill
    {
        public DefenseEqualHealthSelf(Player player) : base(player)
        {
        }

        public override void Execute(RoundProcessingData.ActionData actionData)
        {
            base.Execute(actionData);

            Enumerators.ActionType actionDataType = (Enumerators.ActionType)Enum.Parse(typeof(Enumerators.ActionType), actionData.type);
            
            _cellView.PlayActionParticles(_currentCell, actionDataType, true);
            _currentCell.GetCharacter().PlayAnimation(Constants.DefenseHash);
            
            var targets = actionData.parameters.targets;
            if(targets!=null)
                SetSelfDefense(targets);

            Debug.LogWarning($"DefenseEqualHealthSelf ID: {_board.GetCell(actionData.parameters.actor.x,actionData.parameters.actor.y).GetCharacter().GetPlayerData().PlayerID}" +
                             $"  ACTOR {actionData.parameters.actor.x} : {actionData.parameters.actor.y}");
        }

        private void SetSelfDefense(List<RoundProcessingData.TargetData> targets)
        {
            foreach (var target in targets)
            {
                var player = _board.GetCell(target.position.x, target.position.y).GetCharacter();
                player.Health.SetMaxDefense();
            }
        }
    }
}