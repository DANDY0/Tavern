using System.Collections.Generic;
using System.IO;
using GrandDevs.Networking;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class PlayerTurnHandler
    {
        private INetworkManager _networkManager;
        private readonly RoundEventController _roundEventController;
        private readonly GameplayController _gameplayController;
        
        private readonly Player _player;
        private Transform _playerTransform;
        private Board _board;
        
        private List<Cell> _currentAvailableCells;
        private Cell _targetCell;

        public PlayerTurnHandler(Player player)
        {
            _player = player;
            var gameplayManager = GameClient.Get<IGameplayManager>();
            
            _board = gameplayManager.Board;
            _networkManager = GameClient.Get<INetworkManager>();
            _roundEventController = gameplayManager.GetController<RoundEventController>();
            _gameplayController = gameplayManager.GetController<GameplayController>();

            _roundEventController.OnRoundStarted += OnRoundStartState;
            _roundEventController.OnRoundProcessing += OnRoundProcessingState;
            _roundEventController.OnRoundEnded += OnRoundEndState;
        }

        public void Dispose()
        {
            _currentAvailableCells = null;
            _targetCell = null;
            _roundEventController.OnRoundStarted -= OnRoundStartState;
            _roundEventController.OnRoundProcessing -= OnRoundProcessingState;
            _roundEventController.OnRoundEnded -= OnRoundEndState;
        }

        public void ChooseTargetCell(Cell cell)
        {
            _targetCell = cell;
            HideHighlightedCells(false);
            cell.SetCellChoose(true);
            
            SendChoseCell();
        }

        public List<Cell> GetAvailableCells() => _currentAvailableCells ?? FindAvailableCells();

        private void OnRoundStartState(RoundStartedData startedData)
        {
            if(_player.IsPlayerDead())
                return;

            if (_player.GetPlayerData().IsLocalPlayer && !_player.IsFreeze)
            {
                _targetCell = null;
                ShowAvailableCells(false);
            }
        }

        private void OnRoundProcessingState(RoundProcessingData data)
        {
            var grid = data.grid;
            foreach (var cell in grid)
            {
                bool isPlayer = cell.element.type == Common.Enumerators.UnitType.Character.ToString();
                if(isPlayer && cell.element.id == _player.GetPlayerData().PlayerID)
                    _player.FreezePlayer(cell.element.movementLocked);
            }
            
            if (_player.GetPlayerData().IsLocalPlayer)
            {
                HideHighlightedCells(false);
                HideChosenCell();
            }
        }

        private void OnRoundEndState()
        {
        }
        
        private List<Cell> FindAvailableCells()
        {
            List<Vector2> movePattern = GetStepsFromStepArea(_player.GetPlayerData().StepArea, _player.Cell);
            List<Cell> availableCells = _board.GetAvailableCells(movePattern);
            _currentAvailableCells = availableCells;
            return _currentAvailableCells;
        }

        private List<Vector2> GetStepsFromStepArea(List<List<int>> stepArea, Cell currentCell)
        {
            List<Vector2> possibleSteps = new List<Vector2>();

            int centerX = stepArea.Count / 2;
            int centerY = stepArea[0].Count / 2;
        
            for (int x = 0; x < stepArea.Count; x++)
            {
                for (int y = 0; y < stepArea[x].Count; y++)
                {
                    if (stepArea[x][y] == 1)
                    {
                        int deltaX = x - centerX;
                        int deltaY = y - centerY;

                        possibleSteps.Add(new Vector2(currentCell.X + deltaX, currentCell.Y + deltaY));
                    }
                }
            }

            return possibleSteps;
        }
        
        private void SendChoseCell()
        {
            var gameEvent = new ClientGameplayEvent<MoveCharacterData>()
            {
                data = new MoveCharacterData()
                {
                    to = new NetworkVector2Int
                    {
                        x = _targetCell.X,
                        y = _targetCell.Y
                    }
                },
                type = Enumerators.GameplayEventType.MoveCharacter
            };
            _networkManager.SendGameEvent(gameEvent);
            
            LogDataToFile(gameEvent);
        }
        
        private void LogDataToFile(ClientGameplayEvent<MoveCharacterData> data)
        {
            string path = Path.Combine(Application.persistentDataPath, "gameDataLog.txt");

            string logEntry = $"Event Type: {data.type}\n"
                              +$"Player ID: {_gameplayController.GetLocalPlayer().GetPlayerData().PlayerID}\n"
                              + $"Data To: x={data.data.to.x}, y={data.data.to.y}\n"
                              + "------------------------------\n";

            File.AppendAllText(path, logEntry);
        }

        public void ShowAvailableCells(bool isOtherPlayer)
        {
            FindAvailableCells();
            if (isOtherPlayer)
            {
                foreach (var row in _currentAvailableCells)
                {
                    row.SetEnemyCellHighlight(true);
                    // Debug.Log($"Available CELL: {row.X}: {row.Y}");
                }
                // Debug.Log("END");
            }
            else
                foreach (var row in _currentAvailableCells)
                    row.SetCellHighlight(true);
        }

        public void HideHighlightedCells(bool isOtherPlayer)
        {
            if (isOtherPlayer)
                foreach (var cell in _currentAvailableCells)
                    cell.SetEnemyCellHighlight(false);
            else
                foreach (var cell in _currentAvailableCells)
                    cell.SetCellHighlight(false);
        }

        private void HideChosenCell()
        {
            foreach (var cell in _currentAvailableCells)
                if (cell.IsChosen)
                    cell.SetCellChoose(false);
        }
    }
}