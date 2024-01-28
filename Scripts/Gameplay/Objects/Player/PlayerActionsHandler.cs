using System;
using System.Collections.Generic;
using GrandDevs.Networking;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class PlayerActionsHandler
    {
        private readonly List<GameConfig.ActionInfo> _actionsInfo;
        private readonly Player _player;
        private UltimateAction _ultimateAction;
        private AttackRegularAction _attackRegularAction;
        private AttackRangeAction _attackRangeAction;
        private DefenseAction _defenseAction;
        private HealAction _healAction;
        private MoveAction _moveAction;

        public PlayerActionsHandler(List<GameConfig.ActionInfo> actionsInfo, Player player)
        {
            _player = player;
            _actionsInfo = new List<GameConfig.ActionInfo>();
            _actionsInfo = actionsInfo;
            InitPlayerActions();
        }

        public void ExecuteAction(RoundProcessingData.ActionData actionData)
        {
            // Enumerators.CellType cellType = _board.GetCell(actionData.parameters.actor.x,actionData.parameters.actor.y).CellType;
            Enumerators.ActionType actionType = (Enumerators.ActionType) Enum.Parse(typeof(Enumerators.ActionType), actionData.type);
            switch (actionType)
            {
                case Enumerators.ActionType.Unknown:
                case Enumerators.ActionType.AttackAround:
                case Enumerators.ActionType.AttackVertical:
                case Enumerators.ActionType.AttackHorizontal:
                    break;
                case Enumerators.ActionType.HealSelf:
                    _healAction.Act(actionData);
                    break;
                case Enumerators.ActionType.DefenseSelf:
                    _defenseAction.Act(actionData);
                    break;
                case Enumerators.ActionType.AttackRegular:
                    _attackRegularAction.Act(actionData);
                    break;
                case Enumerators.ActionType.AttackRange:
                    _attackRangeAction.Act(actionData);
                    break;
                case Enumerators.ActionType.NuclearBomb:
                case Enumerators.ActionType.DefenseEqualHealthSelf:
                case Enumerators.ActionType.HealthIsHalfToOther:
                case Enumerators.ActionType.FreezeMoveForOther:
                    _ultimateAction.Act(actionData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ExecuteMove(RoundProcessingData.ActionData actionData) => _moveAction.Act(actionData);

        private void InitPlayerActions()
        {
            foreach (var actionInfo in _actionsInfo)
            {
                Enumerators.CellType cellType = (Enumerators.CellType) Enum.Parse(typeof(Enumerators.CellType), actionInfo.cell);
                switch (cellType)
                {
                    case Enumerators.CellType.Action:
                        _ultimateAction = new UltimateAction(CreateSkill(actionInfo.action));
                        break;
                    case Enumerators.CellType.AttackRange:
                        _attackRangeAction = new AttackRangeAction(CreateSkill(actionInfo.action));
                        break;
                    case Enumerators.CellType.AttackRegular:
                        _attackRegularAction = new AttackRegularAction(CreateSkill(actionInfo.action));
                        break;
                    case Enumerators.CellType.Heal:
                        _healAction = new HealAction(CreateSkill(actionInfo.action));
                        break;
                    case Enumerators.CellType.Defense:
                        _defenseAction = new DefenseAction(CreateSkill(actionInfo.action));
                        break;
                  
                    case Enumerators.CellType.Empty:
                        break;
                }
            }

            _moveAction = new MoveAction(new Move(_player));
        }

        private BaseSkill CreateSkill(string actionName)
        {
            Enumerators.ActionType actionType = (Enumerators.ActionType) Enum.Parse(typeof(Enumerators.ActionType), actionName);
            switch (actionType)
            {
                case Enumerators.ActionType.Unknown:
                    break;
                case Enumerators.ActionType.AttackAround:
                    return new AttackAround(_player);
                case Enumerators.ActionType.HealSelf:
                    return new HealSelf(_player);
                case Enumerators.ActionType.AttackHorizontal:
                    return new AttackHorizontal(_player);
                case Enumerators.ActionType.AttackVertical:
                    return new AttackVertical(_player);
                case Enumerators.ActionType.DefenseSelf:
                    return new DefenceSelf(_player);
                case Enumerators.ActionType.AttackRegular:
                    return new AttackAround(_player);
                case Enumerators.ActionType.AttackRange:
                    return new AttackAround(_player);
                case Enumerators.ActionType.NuclearBomb:
                    return new NuclearBomb(_player);
                case Enumerators.ActionType.DefenseEqualHealthSelf:
                    return new DefenseEqualHealthSelf(_player);
                case Enumerators.ActionType.HealthIsHalfToOther:
                    return new HealthIsHalfToOther(_player);
                case Enumerators.ActionType.FreezeMoveForOther:
                    return new FreezeMoveForOther(_player);
                case Enumerators.ActionType.MoveSelf:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        public int GetActionRange(Enumerators.ActionType attackType)
        {
            foreach (var actionInfo in _actionsInfo)
                if (actionInfo.action == attackType.ToString())
                    return actionInfo.range;
            return 0;
        }

        public int GetActionRange(Enumerators.CellType attackType)
        {
            foreach (var actionInfo in _actionsInfo)
                if (actionInfo.cell == attackType.ToString())
                    return actionInfo.range;
            return 0;
        }
        
    }
}