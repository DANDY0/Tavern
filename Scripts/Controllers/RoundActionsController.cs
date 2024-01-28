using System;
using System.Collections.Generic;
using DG.Tweening;
using GrandDevs.Networking;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class RoundActionsController : IController
    {
        private readonly IGameplayManager _gameplayManager;

        private GameplayController _gameplayController;
        private RoundEventController _roundEventController;
        private Board _board;
        private Sequence _actionsSequence;

        public event Action<int> OnCombatActionProcessed;

        public RoundActionsController()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
        }

        public void Init()
        {
            _gameplayController = _gameplayManager.GetController<GameplayController>();
            _roundEventController = _gameplayManager.GetController<RoundEventController>();
            _roundEventController.OnRoundProcessing += ProcessRoundActionsSequence;
            _gameplayManager.GameplayStartedEvent += GameplayStartedEventHandler;
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            _roundEventController.OnRoundProcessing -= ProcessRoundActionsSequence;
            _gameplayManager.GameplayStartedEvent -= GameplayStartedEventHandler;
        }

        public void ResetAll()
        {
            _actionsSequence.Kill();
        }

        private void GameplayStartedEventHandler()
        {
            _board = _gameplayManager.Board;
        }

        private void ProcessRoundActionsSequence(RoundProcessingData roundProcessingData)
        {
            var actions = roundProcessingData.actions;
            Debug.LogError($"ACTIONS COUNT: {actions.Count}");
            _actionsSequence = CreateMainSequence(actions);
            _actionsSequence.Play();
        }

        private Sequence CreateMainSequence(List<RoundProcessingData.ActionData> actions)
        {
            Sequence sequence = DOTween.Sequence();

            float duration = AddMoveSelfActionsToSequence(actions, sequence);
            sequence.AppendInterval(duration);
            AddCombatActionsToSequence(actions, sequence);

            return sequence;
        }

        private float AddMoveSelfActionsToSequence(List<RoundProcessingData.ActionData> actions, Sequence sequence)
        {
            float duration = 0f;

            foreach (var action in actions)
            {
                if (GetActionType(action.type) == Enumerators.ActionType.MoveSelf)
                {
                    sequence.InsertCallback(0, () => ProcessMoveAction(action));
                    
                    duration = action.duration;
                }
            }

            return duration;
        }

        private void AddCombatActionsToSequence(List<RoundProcessingData.ActionData> actions, Sequence sequence)
        {
            foreach (var action in actions)
                if (GetActionType(action.type) != Enumerators.ActionType.MoveSelf)
                    AddActionToSequence(action, sequence);
        }

        private void AddActionToSequence(RoundProcessingData.ActionData action, Sequence sequence)
        {
            TweenCallback currentActionCallback;
            Enumerators.ActionType actionType = GetActionType(action.type);

            switch (actionType)
            {
                case Enumerators.ActionType.AttackHorizontal:
                case Enumerators.ActionType.AttackVertical:
                case Enumerators.ActionType.AttackAround:
                case Enumerators.ActionType.AttackRegular:
                case Enumerators.ActionType.AttackRange:
                case Enumerators.ActionType.HealSelf:
                case Enumerators.ActionType.DefenseSelf:
                case Enumerators.ActionType.NuclearBomb:
                case Enumerators.ActionType.DefenseEqualHealthSelf:
                case Enumerators.ActionType.HealthIsHalfToOther:
                case Enumerators.ActionType.FreezeMoveForOther:
                    currentActionCallback = () => ProcessCombatAction(action);
                    sequence.AppendCallback(currentActionCallback).AppendInterval(action.duration);
                    break;
                case Enumerators.ActionType.Unknown:
                    Debug.LogError($"Unknown action type: {action.type}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessMoveAction(RoundProcessingData.ActionData actionData)
        {
            var playerID = actionData.parameters.target;
            var player = _gameplayController.GetPlayerByID(playerID);
            player.ExecuteMoveAction(actionData);
        }

        private void ProcessCombatAction(RoundProcessingData.ActionData actionData)
        {
            var cell = GetCellFromActionData(actionData.parameters.actor);

            switch (GetActionType(actionData.type))
            {
                case Enumerators.ActionType.NuclearBomb:
                case Enumerators.ActionType.DefenseEqualHealthSelf:
                case Enumerators.ActionType.HealthIsHalfToOther:
                case Enumerators.ActionType.FreezeMoveForOther:
                    Debug.LogWarning("PROCESS Ult ACTION");
                    cell.GetCharacter().ExecuteAction(actionData);
                    break;
                default:
                    Debug.LogWarning("PROCESS Combat ACTION");
                    cell.GetCharacter().ExecuteAction(actionData);
                    break;
            }

            var playerID = _board.GetCell(actionData.parameters.actor.x, actionData.parameters.actor.y).GetCharacter().GetPlayerData().PlayerID;
            OnCombatActionProcessed?.Invoke(playerID);
        }

        private Enumerators.ActionType GetActionType(string type)
        {
            if (Enum.TryParse(type, true, out Enumerators.ActionType result))
                return result;
            return Enumerators.ActionType.Unknown;
        }

        private Cell GetCellFromActionData(RoundProcessingData.TargetPosition actor)
        {
            if (actor == null) return null;
            return _board.GetCell(actor.x, actor.y);
        }
    }
}