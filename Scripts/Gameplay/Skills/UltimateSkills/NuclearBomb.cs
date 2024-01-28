using System;
using System.Collections.Generic;
using GrandDevs.Networking;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class NuclearBomb : BaseSkill, IUltimateSkill
    {
        public NuclearBomb(Player player) : base(player)
        {
        }

        public override void Execute(RoundProcessingData.ActionData actionData)
        {
            base.Execute(actionData);

            Enumerators.ActionType actionDataType = actionData.type.Parse<Enumerators.ActionType>();
            
            var attackRange = _player.GetActionRange(actionDataType);
            _cellView.PlayActionParticles(_board.GetCellsInRadius(_currentCell, attackRange), actionDataType);
            _currentCell.GetCharacter().PlayAnimation(Constants.AttackHorizontalHash);

            var targets = actionData.parameters.targets;
            if (targets != null) 
                DamagePlayers(targets);

            Debug.LogWarning($"ATTACK NuclearBomb ID: {_board.GetCell(actionData.parameters.actor.x,actionData.parameters.actor.y).GetCharacter().GetPlayerData().PlayerID}" +
                             $" ACTOR {actionData.parameters.actor.x} : {actionData.parameters.actor.y}");
        }

        private void DamagePlayers(List<RoundProcessingData.TargetData> targets)
        {
            foreach (var target in targets)
            {
                var player = _board.GetCell(target.position.x, target.position.y).GetCharacter();
                player.View.PlayAnimation(Constants.TakeSoftDamageHash);
                player.Health.ChangeHealth(-target.value);
            }
        }
    }
}