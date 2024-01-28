using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using GrandDevs.Networking;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class Player: IUnit
    {
        public Cell Cell { get; set; }
        public Enumerators.UnitType UnitType { get; set; }
        public HealthBar Health { get; private set; }
        public bool IsFreeze{ get; private set; }
        public PlayerView View;
        public event Action<bool> OnPlayerFreeze; 
    
        private PlayerActionsHandler _actionsHandler;
        private PlayerTurnHandler _playerTurnHandler;
        private PlayerData _playerData;
        private BaseAction _moveAction;
        private bool _isSpectator;
        private bool _isDead;
        
        public Player(PlayerData playerData) => _playerData = playerData;

        public void Init(GameObject playerSelfObject)
        {
            UnitType = Enumerators.UnitType.Character;
            var gameConfig = GameClient.Get<IGameplayManager>().GameConfig;
            _playerData.SetCharacterData(gameConfig.characters[_playerData.CharacterID]);

            View = new PlayerView(playerSelfObject);
            Health = new HealthBar(_playerData.MaxHealth, this);
            
            _actionsHandler = new PlayerActionsHandler(_playerData.Actions, this);
            _playerTurnHandler = new PlayerTurnHandler(this);
            
            Health.OnPlayerDead += PlayerDeadEventHandler;
        }
        
        public Transform GetPlayerTransform() => View.GetPlayerTransform();

        public PlayerData GetPlayerData() => _playerData;

        public PlayerTurnHandler GetPlayerTurnHandler() => _playerTurnHandler;

        public void PlayAnimation(int animationHash) => View.PlayAnimation(animationHash);
        public void PlayAnimationBool(int animationHash, bool state) => View.PlayAnimationBool(animationHash, state);

        public void AssignCell(Cell cell) => Cell = cell;

        public bool IsPlayerDead() => _isDead;

        public bool IsSpectator() => _isSpectator;
        
        public void SetIsSpectator(bool state) => _isSpectator = state;

        public void Dispose()
        {
            _playerTurnHandler.Dispose();
            _playerTurnHandler = null;
            View.Destroy();
        }

        public void ExecuteAction(RoundProcessingData.ActionData actionData) => _actionsHandler.ExecuteAction(actionData);

        public void ExecuteMoveAction(RoundProcessingData.ActionData actionData) => _actionsHandler.ExecuteMove(actionData);

        public int GetActionRange(Enumerators.ActionType attackType) => _actionsHandler.GetActionRange(attackType);

        public int GetActionRange(Enumerators.CellType attackType) => _actionsHandler.GetActionRange(attackType);

        private void PlayerDeadEventHandler()
        {
            _isDead = true;
            View.FreezePlayer(false);
            View.SetAnimationBool(Constants.DeadBoolHash, true);
            View.PlayAnimation(Constants.DeadHash);
            Cell.SetUnit(null);
        }

        public void FreezePlayer(bool state)
        {
            if(IsFreeze == state)
                return;
            IsFreeze = state;
            if(!IsFreeze)
                View.FreezePlayer(false);
            OnPlayerFreeze?.Invoke(state);
        }
    }

    public interface IUnit
    {
        Cell Cell { get; set; }
        Enumerators.UnitType UnitType { get; set; }
        void AssignCell(Cell cell);
    }

    public class PlayerData
    {
        public readonly int PlayerID;
        public readonly bool IsLocalPlayer;
        public readonly string Name;
        public int CharacterID;
        public Enumerators.AttackType AttackType;
        public int Initiative;
        public List<GameConfig.ActionInfo> Actions { get; private set; }
        public int MaxHealth { get; private set; }
        public int Defense { get; private set; }
        public int Range { get; private set; }
        public List<List<int>> StepArea { get; private set; }
        
        public PlayerData(int playerID, int characterID,bool isLocalPlayer, string name)
        {
            PlayerID = playerID;
            CharacterID = characterID;
            IsLocalPlayer = isLocalPlayer;
            Name = name;
        }
        public void SetCharacterData(GameConfig.CharacterData characterData)
        {
            Initiative = characterData.initiative;
            Actions = characterData.actions;
            MaxHealth = characterData.health;
            Defense = characterData.defense;
            StepArea = characterData.stepArea;
            Range = characterData.range;
            AttackType = (Enumerators.AttackType)Enum.Parse(typeof(Enumerators.AttackType), characterData.type);
        }
    }
}