using System;
using System.Collections.Generic;
using GrandDevs.Networking;
using GrandDevs.Tavern.Helpers;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class AttackAround : BaseSkill
    {
        private readonly float _delayBeforeDamage = 0.5f;

        public AttackAround(Player player) : base(player)
        {
        }

        public override void Execute(RoundProcessingData.ActionData actionData)
        {
            base.Execute(actionData);

            var attackType = ParseActionType(actionData.type);
            var actorCell = _board.GetCell(actionData.parameters.actor.x,actionData.parameters.actor.y);
            var player = actorCell.GetCharacter();
            var targets = actionData.parameters.targets;
            if (TargetsExist(targets))
            {
                _skillView.PlayAttackAction(player, targets, attackType);
                ApplyDelayedDamage(targets, attackType);
            }
            else 
                _skillView.HandleMiss(player);
            
            Debug.LogWarning($"ATTACK AROUND ID: {_board.GetCell(actionData.parameters.actor.x, actionData.parameters.actor.y).GetCharacter().GetPlayerData().PlayerID}" +
                             $"  ACTOR {actionData.parameters.actor.x} : {actionData.parameters.actor.y}");
        }

        private bool TargetsExist(List<RoundProcessingData.TargetData> targets) => targets != null && targets.Count > 0;

        private Enumerators.ActionType ParseActionType(string actionType) => (Enumerators.ActionType)Enum.Parse(typeof(Enumerators.ActionType), actionType);

        private void ApplyDelayedDamage(List<RoundProcessingData.TargetData> targets, Enumerators.ActionType actionDataType)
        {
            InternalTools.DoActionDelayed(() => DamagePlayers(targets, actionDataType), _delayBeforeDamage);
        }

        private void DamagePlayers(List<RoundProcessingData.TargetData> targets, Enumerators.ActionType actionDataType)
        {
            foreach (var target in targets)
            {
                var targetPlayer = _board.GetCell(target.position.x, target.position.y).GetCharacter();
                targetPlayer.View.PlayAnimation(Constants.TakeSoftDamageHash);
                // _cellView.PlayActionParticles(player.Cell, actionDataType);
                targetPlayer.Health.ChangeHealth(-target.value);
            }
        }
    }
}