using System;
using System.Collections.Generic;
using GrandDevs.Networking;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class AttackVertical: BaseSkill, IUltimateSkill
    {
        public AttackVertical(Player player) : base(player)
        {
        }

        public void AssignUltimateCell(Cell cell)
        {
            _currentCell = cell;
            _cellView = _currentCell.CellView;
        }

        public override void Execute(RoundProcessingData.ActionData actionData)
        {
            base.Execute(actionData);

            Enumerators.ActionType actionDataType = (Enumerators.ActionType)Enum.Parse(typeof(Enumerators.ActionType), actionData.type);
          
            _cellView.PlayActionParticles(GetVerticalAttackRange(_currentCell), actionDataType);
            // _cellView.ShowAttackRange(GetVerticalAttackRange(_currentCell));
            _currentCell.GetCharacter().PlayAnimation(Constants.AttackVerticalHash);

            var targets = actionData.parameters.targets;
            if(targets!=null)
                DamagePlayers(targets);
            
            Debug.LogWarning($"ATTACK VERTICAL ID: {_board.GetCell(actionData.parameters.actor.x,actionData.parameters.actor.y).GetCharacter().GetPlayerData().PlayerID}" +
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

        private List<Cell> GetVerticalAttackRange(Cell currentCell)
        {
            List<Cell> attackCells = new List<Cell>();
    
            var cellsList = _board.GetCellsList();

            foreach (var cell in cellsList)
            {
                if (cell.X == currentCell.X && cell.Y != currentCell.Y)
                {
                    attackCells.Add(cell);
                }
            }
            
            return attackCells;
        }
    }
}