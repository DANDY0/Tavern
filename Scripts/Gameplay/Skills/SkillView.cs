using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GrandDevs.Networking;
using GrandDevs.Tavern.Helpers;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class SkillView
    {
        private readonly CameraController _cameraController;
        private readonly Board _board;
        private readonly float _rotateToTargetTime = 0.3f;

        private List<GameConfig.ActionData> _actionsData;
        
        public SkillView()
        {
            var gameplayManager = GameClient.Get<IGameplayManager>();
            _board = gameplayManager.Board;
            _actionsData = gameplayManager.GameConfig.actions;
            _cameraController = gameplayManager.GetController<CameraController>();
        }

        public void PlayAttackAction(Player player, List<RoundProcessingData.TargetData> targets,
            Enumerators.ActionType attackType)
        {
            player.PlayAnimation(GetAnimationHash(attackType));
            ShowAttackRange(player.Cell, player.GetActionRange(attackType));

            if (IsSoloTarget(targets))
            {
                var targetPlayer = GetTargetCell(targets[0]).GetCharacter();
                FocusCameraOnAttack(player, targets[0], attackType);
                LookAtTarget(player, targetPlayer.GetPlayerTransform().position);
                
                if (IsRangeType(player))
                {
                    var targetCell = GetTargetCell(targets[0]);
                    player.View.ThrowFireballToTarget(targetCell.GetCellCenter());
                }
            }
        }

        public void HandleMiss(Player player) => player.View.PlayMissVfx();
        
        private void ShowAttackRange(Cell cell, int range)
        {
            var rangeCells = _board.GetCellsInRadius(cell,range);
            foreach (var rangeCell in rangeCells) 
                rangeCell.CellView.SetAttackRangeCell(true);
            InternalTools.DoActionDelayed(() =>
            {
                foreach (var rangeCell in rangeCells) 
                    rangeCell.CellView.SetAttackRangeCell(false);
            }, 2f);
        }
        
        private void FocusCameraOnAttack(Player player, RoundProcessingData.TargetData targetData,
            Enumerators.ActionType attackType)
        {
            // var cellRange = GetCellRange(attackType.ToString());
            // var attackRange = player.GetPlayerData().Range + cellRange;
            var attackRange = player.GetActionRange(attackType);
            _cameraController.FocusCamera(player.Cell,
                _board.GetCell(targetData.position.x, targetData.position.y), attackRange);
        }
        
        private Cell GetTargetCell(RoundProcessingData.TargetData targetData) => _board.GetCell(targetData.position.x, targetData.position.y);
        
        private bool IsRangeType(Player player) => player.GetPlayerData().AttackType == Enumerators.AttackType.Range;

        private bool IsSoloTarget(List<RoundProcessingData.TargetData> targets) => targets.Count == 1;

        private void LookAtTarget(Player player, Vector3 targetPosition)
        {
            var playerTransform = player.GetPlayerTransform();
            var direction = (targetPosition - playerTransform.position).normalized;
            direction.y = 0;

            playerTransform.DOLookAt(playerTransform.position + direction, _rotateToTargetTime);
        }

        private int GetAnimationHash(Enumerators.ActionType attackType)
        {
            switch (attackType)
            {
                case Enumerators.ActionType.Unknown:
                    break;
                case Enumerators.ActionType.AttackAround:
                    return Constants.AttackAroundHash;
                case Enumerators.ActionType.HealSelf:
                    break;
                case Enumerators.ActionType.AttackHorizontal:
                    return Constants.AttackAroundHash;
                case Enumerators.ActionType.AttackVertical:
                    return Constants.AttackAroundHash;
                case Enumerators.ActionType.DefenseSelf:
                    break;
                case Enumerators.ActionType.MoveSelf:
                    break;
                case Enumerators.ActionType.AttackRegular:
                    return Constants.AttackAroundHash;
                case Enumerators.ActionType.AttackRange:
                    return Constants.AttackAroundHash;
                case Enumerators.ActionType.NuclearBomb:
                    return Constants.AttackAroundHash;
                default:
                    throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null);
            }

            return -1;
        }
    }
}