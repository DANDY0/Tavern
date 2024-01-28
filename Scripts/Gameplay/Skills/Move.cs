using System;
using DG.Tweening;
using GrandDevs.Networking;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class Move : BaseSkill
    {
        private readonly float _maxJumpDuration = 1.7f;
        private readonly float _rotateToTargetTime = 0.3f;
        private readonly float _jumpHeight = 4f;
        private readonly float _speedCoefficient = 5f;

        public Move(Player player) : base(player)
        {
        }

        public override void Execute(RoundProcessingData.ActionData actionData)
        {
            base.Execute(actionData);
            Debug.Log("EXECUTE MOVE");
            // if(actionData.parameters.target == _player.GetPlayerData().PlayerID)
            ExecuteMove(actionData, 0);

            // Debug.LogWarning($"MOVE ACTOR {actionData.parameters.actor.x} : {actionData.parameters.actor.y}");
        }

        private void ExecuteMove(RoundProcessingData.ActionData actionData, int stepIndex)
        {
            if (stepIndex >= actionData.parameters.to.Count)
                return;
            var targetCell = _board.GetCell(actionData.parameters.to[stepIndex].x, actionData.parameters.to[stepIndex].y);
            var targetPosition = targetCell.GetCellCenter();

            LookAtTarget(_player, targetPosition, () => 
            {
                MovePlayerToTargetCell(_player, targetPosition, () => 
                {
                    if (stepIndex == actionData.parameters.to.Count - 1)
                        MoveCallback(targetCell);
                    else
                        ExecuteMove(actionData, stepIndex + 1);
                });
            });

        }

        private void LookAtTarget(Player player, Vector3 targetPosition, Action callback)
        {
            var playerTransform = player.GetPlayerTransform();
            var direction = (targetPosition - playerTransform.position).normalized;
            direction.y = 0;

            playerTransform.DOLookAt(playerTransform.position + direction, _rotateToTargetTime)
                .OnComplete(() => callback?.Invoke());
        }

        private void MovePlayerToTargetCell(Player player, Vector3 targetPosition, Action callback)
        {
            _player.PlayAnimation(Constants.MoveHash);
            var playerTransform = player.GetPlayerTransform();
            var duration = Vector3.Distance(playerTransform.position, targetPosition) / _speedCoefficient;

            playerTransform.DOMove(targetPosition, duration).SetEase(Ease.Linear).OnComplete(() => callback?.Invoke());
        }

        
        private void MoveCallback(Cell targetCell)
        {
            var character = _currentCell.GetCharacter();
            if(IsMyChar(character))
                _currentCell.SetUnit(null);
            
            Debug.LogWarning($"SET {_currentCell.X}:{_currentCell.Y} Empty");
            targetCell.SetUnit(_player);
            Debug.LogWarning($"SET {targetCell.X}:{targetCell.Y} Character {_player.GetPlayerData().PlayerID}");
            // _player.PlayAnimationBool(Constants.MoveHash, false);
            _player.PlayAnimation(Constants.IdleHash);
        }

        private bool IsMyChar(Player character)
        {
            return character != null && character.GetPlayerData().PlayerID == _player.GetPlayerData().PlayerID;
        }
    }
}
