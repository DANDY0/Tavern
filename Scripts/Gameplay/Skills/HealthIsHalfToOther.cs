using System;
using System.Collections.Generic;
using GrandDevs.Networking;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class HealthIsHalfToOther: BaseSkill
    {
        public HealthIsHalfToOther(Player player) : base(player)
        {
        }

        public override void Execute(RoundProcessingData.ActionData actionData)
        {
            base.Execute(actionData);
            
            Enumerators.ActionType actionDataType = (Enumerators.ActionType)Enum.Parse(typeof(Enumerators.ActionType), actionData.type);
            
            _currentCell.GetCharacter().PlayAnimation(Constants.HealHash);

            var targets = actionData.parameters.targets;
            if(targets!=null)
                SetHalfEnemiesHp(targets, actionDataType);
           
            Debug.LogWarning($"HealthIsHalfToOther ID: {_board.GetCell(actionData.parameters.actor.x,actionData.parameters.actor.y).GetCharacter().GetPlayerData().PlayerID}" +
                             $"  ACTOR {actionData.parameters.actor.x} : {actionData.parameters.actor.y}");
        }

        private void SetHalfEnemiesHp(List<RoundProcessingData.TargetData> targets, Enumerators.ActionType actionType)
        {
            foreach (var target in targets)
            {
                var cell = _board.GetCell(target.position.x, target.position.y);
                var player = cell.GetCharacter();
                player.Health.SetHealth(target.value);
                cell.CellView.PlayActionParticles(cell, actionType, true);
            }
        }
    }
}