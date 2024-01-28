using System;
using System.Collections.Generic;
using System.Linq;
using GrandDevs.Networking;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class BoardInputController : IController
    {
        public event Action<Player> OnShowPlayerStats;
        private readonly IAppStateManager _appStateManager;
        private readonly IGameplayManager _gameplayManager;
        
        private RoundEventController _roundController;
        private GameplayController _gameplayController;
        private GameEventController _gameEventController;

        private PlayerTurnHandler _playerTurnHandler;
        private Board _gameBoard;
        private Player _localPlayer;
        private Player _playerInFocus;
        private Cell _cellForPlayerInfo;
        private List<Cell> _attackRangeFocus;
        
        private bool _canChooseTarget;
        private bool _isPlayerFreeze;
        private bool _isCellChosen;
        private bool _isAttackCellPrio;
        
        private RaycastHit _hitInfo;

        public BoardInputController()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
        }

        public void Init()
        {
            _roundController = _gameplayManager.GetController<RoundEventController>();
            _gameplayController = _gameplayManager.GetController<GameplayController>();
            _gameEventController = _gameplayManager.GetController<GameEventController>();
            _gameplayController.OnPlayersSpawned += OnPlayersSpawned;
            _roundController.OnRoundStarted += RoundStartedEventHandler;
            _roundController.OnRoundProcessing += RoundProcessingEventHandler;
        }

        public void Update()
        {
            if (_appStateManager.AppState != Enumerators.AppState.Game)
                return;
    
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                bool hitSomething = Physics.Raycast(ray, out _hitInfo);
                // Debug.Log(_hitInfo.transform.gameObject.name);
                Debug.DrawRay(ray.origin, ray.direction * _hitInfo.distance, Color.red, 1f);
            }

            ShowPlayerStats();
            
            if (_roundController.GetRoundState() == Enumerators.RoundState.RoundStart)
            {
              
                if(_localPlayer!=null && _localPlayer.IsPlayerDead())
                    return;

                if (!_isAttackCellPrio)
                    CheckEnemyPlayerInCell();
                
                if (_canChooseTarget && !_isPlayerFreeze)
                {
                    if(!_isCellChosen)
                        CheckIsMyAttackCell();
                    if(Input.GetMouseButtonDown(0))
                        CheckCellChose();
                }
            }
        }

        private void ShowPlayerStats()
        {
            if (Input.GetMouseButtonDown(1) && TryGetHitEnemy(out Cell foundedCell))
            {
                ShowPlayerStats(foundedCell);
                Debug.Log("SHOW TRUE");
            }
        }

        private void ShowPlayerStats(Cell foundedCell)
        {
            var player = foundedCell.GetCharacter();
            OnShowPlayerStats?.Invoke(player);
        }

        public void Dispose()
        {
            _gameplayController.OnPlayersSpawned -= OnPlayersSpawned;
            _roundController.OnRoundStarted -= RoundStartedEventHandler;
            _roundController.OnRoundProcessing -= RoundProcessingEventHandler;
        }

        public void ResetAll()
        {
            _localPlayer.OnPlayerFreeze -= PlayerFreezeEventHandler;
            _localPlayer.Health.OnPlayerDead -= PlayerDeadEventHandler;

            _localPlayer = null;
            _playerTurnHandler = null; 
            _gameBoard = null;
            _playerInFocus = null;
            _attackRangeFocus = null;
            _canChooseTarget = false;
            _isPlayerFreeze = false;
            _isCellChosen = false;
            _hitInfo = new RaycastHit();
        }

        private void PlayerDeadEventHandler()
        {
            
        }

        private void CheckIsMyAttackCell()
        {
            var cellsToCheck = _playerTurnHandler.GetAvailableCells();
            List<Cell> attackList = new List<Cell>(cellsToCheck);
            attackList.Add(_localPlayer.Cell);

            if (TryGetHitCell(out Cell foundedCell, attackList))
            {
                if (foundedCell != null && IsAttackCell(foundedCell.CellType) && !IsEnemyInCell(foundedCell))
                {
                    // Debug.Log($"ATTACK CELL: {foundedCell.X}: {foundedCell.Y}");
                
                    if (_attackRangeFocus != null && _attackRangeFocus.Count > 0)
                        foreach (var cell in _attackRangeFocus)
                            cell.CellView.SetAttackRangeCell(false);
                    
                    var attackRange = _localPlayer.GetActionRange(foundedCell.CellType);
                    _attackRangeFocus = _gameBoard.GetCellsInRadius(foundedCell, attackRange);
                    foreach (var cell in _attackRangeFocus) 
                        cell.CellView.SetAttackRangeCell(true);
                    _isAttackCellPrio = true;
                }
                else
                {
                    if (_attackRangeFocus == null || _attackRangeFocus.Count == 0)
                        return;
                    foreach (var cell in _attackRangeFocus) 
                        cell.CellView.SetAttackRangeCell(false);
                    _isAttackCellPrio = false;
                }
            }
            else
            {
                if(_attackRangeFocus!=null)
                    foreach (var cell in _attackRangeFocus)
                        cell.CellView.SetAttackRangeCell(false);
                _isAttackCellPrio = false;
            }
        }

        private void CheckCellChose()
        {
            if (TryGetHitCell(out Cell foundedCell,_playerTurnHandler.GetAvailableCells()))
            {
                if (foundedCell != null)
                {
                    _playerTurnHandler.ChooseTargetCell(foundedCell);
                    _isCellChosen = true;
                    _canChooseTarget = false;
                    _isAttackCellPrio = false;
                    HideAttackRange();
                    HideMyAvailableMoves();
                }
            }
        }
        private bool CheckEnemyPlayerInCell()
        {
            if (TryGetHitEnemy(out Cell foundedCell))
            {
                HideAttackRange();
                HideMyAvailableMoves();
                _attackRangeFocus = null;
                var playerInCell = foundedCell.GetCharacter();
                
                if (_playerInFocus != null && _playerInFocus != playerInCell)
                {
                    _playerInFocus.GetPlayerTurnHandler().HideHighlightedCells(true);
                    _playerInFocus = null;
                }

                if (playerInCell != null && !_localPlayer.Equals(playerInCell))
                {
                    _playerInFocus = playerInCell;
                    _playerInFocus.GetPlayerTurnHandler().ShowAvailableCells(true);
                }
              
                return true;
            }
            
            if (_playerInFocus != null)
            {
                _playerInFocus.GetPlayerTurnHandler().HideHighlightedCells(true);
                _playerInFocus = null;
            }
            if(_canChooseTarget && !_isPlayerFreeze)
                ShowMyAvailableMoves();
            
            return false;
        }

        private bool IsEnemyInCell(Cell foundedCell)
        {
            var character = foundedCell.GetCharacter();
            if(character!=null)
                Debug.Log($"Character {character != null} and char id: {character.GetPlayerData().PlayerID} : my ID {_localPlayer.GetPlayerData().PlayerID}");
            return character != null && character.GetPlayerData().PlayerID != _localPlayer.GetPlayerData().PlayerID;
        }
        
        private bool TryGetHitCell(out Cell cell, List<Cell> cellsToCheck = null)
        {
            cell = null;
            if (_hitInfo.collider != null && _hitInfo.collider.CompareTag(Constants.CellTag))
            {
                    cell = _gameBoard.GetNearestCell(_hitInfo.collider.transform.position, cellsToCheck);
                return cell != null;
            }
            return false;
        }

        private bool TryGetHitEnemy(out Cell cell)
        {
            if (_hitInfo.collider != null && _hitInfo.collider.CompareTag(Constants.CellTag))
            {
                cell = _gameBoard.GetCellWithPlayer(_hitInfo.collider.transform.position);
                return cell != null;
            }
            cell = null;
            return false;
        }

        private void RoundProcessingEventHandler(RoundProcessingData data)
        {
            if(_playerInFocus!=null)
                _playerInFocus.GetPlayerTurnHandler().HideHighlightedCells(true);
            if(_attackRangeFocus !=null)
                foreach (var cell in _attackRangeFocus)
                    cell.CellView.SetAttackRangeCell(false);
        }

        private void RoundStartedEventHandler(RoundStartedData roundStartedData)
        {
            _isCellChosen = false;
            _canChooseTarget = true;
        }

        private void OnPlayersSpawned()
        {
            _localPlayer = _gameplayController.GetLocalPlayer();
            _playerTurnHandler = _localPlayer.GetPlayerTurnHandler(); 
            _gameBoard = _gameplayManager.Board;
            _localPlayer.OnPlayerFreeze += PlayerFreezeEventHandler;
            // _localPlayer.OnPlayerFreeze -= PlayerFreezeEventHandler;
            _localPlayer.Health.OnPlayerDead += PlayerDeadEventHandler;
        }

        private void PlayerFreezeEventHandler(bool state)
        {
            _isPlayerFreeze = state;
            _canChooseTarget = !_isPlayerFreeze;
        }

        private void HideAttackRange()
        {
            if (_attackRangeFocus != null && _attackRangeFocus.Count > 0)
            {
                foreach (var cell in _attackRangeFocus) 
                    cell.CellView.SetAttackRangeCell(false);
                _attackRangeFocus.Clear();
            }
        }
        private void ShowMyAvailableMoves() => _playerTurnHandler.ShowAvailableCells(false);
        private void HideMyAvailableMoves() => _playerTurnHandler.HideHighlightedCells(false);
        private bool IsAttackCell(Enumerators.CellType cellType)
        {
            switch (cellType)
            {
                case Enumerators.CellType.AttackRegular:
                    return true;
                case Enumerators.CellType.AttackRange:
                    return true;
                case Enumerators.CellType.Action:
                    return true;
                case Enumerators.CellType.Heal:
                    return false;
                case Enumerators.CellType.Defense:
                    return false;
                case Enumerators.CellType.Empty:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cellType), cellType, null);
            }
        }
        
    }
}
