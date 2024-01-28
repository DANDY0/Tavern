using System.Collections.Generic;
using GrandDevs.Networking;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;
namespace GrandDevs.Tavern
{
    public class FreezeMoveForOther: BaseSkill, IUltimateSkill
    {
        public FreezeMoveForOther(Player player) : base(player)
        {
        }
        
        public override void Execute(RoundProcessingData.ActionData actionData)
        {
            base.Execute(actionData);

            Enumerators.ActionType actionDataType = actionData.type.Parse<Enumerators.ActionType>();
            
            // _cellView.PlayActionParticles(GetAttackRange(_currentCell), actionDataType);
            _currentCell.GetCharacter().PlayAnimation(Constants.AttackHorizontalHash);

            var targets = actionData.parameters.targets;
            if (targets != null)
            {
                // _cameraController.Focus(_currentCell.GetWorldPosition());
                DamagePlayers(targets);
            }

            Debug.LogWarning($"FreezeMoveForOther ID: {_board.GetCell(actionData.parameters.actor.x,actionData.parameters.actor.y).GetCharacter().GetPlayerData().PlayerID}" +
                             $" ACTOR {actionData.parameters.actor.x} : {actionData.parameters.actor.y}");
        }

        private void DamagePlayers(List<RoundProcessingData.TargetData> targets)
        {
            foreach (var target in targets)
            {
                var player = _board.GetCell(target.position.x, target.position.y).GetCharacter();
                player.View.PlayAnimation(Constants.TakeSoftDamageHash);
                player.View.FreezePlayer(true);
                // player.Health.ChangeHealth(-target.value);
            }
        }
    }
}