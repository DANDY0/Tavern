using System.Collections.Generic;
using System.Linq;
using GrandDevs.Networking;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class Board
    {
        private List<Cell> _cells;

        private Transform _parentOfGrid;

        private Vector3 _rowOffset = new Vector3(0,0,0);

        public Board()
        {
            _parentOfGrid = GameObject.Find("GameField/Rows").transform;
        }

        public void Dispose()
        {
            foreach (var item in _cells)
                item.Dispose();
            _cells.Clear();
        }

        public List<Cell> GetAvailableCells(List<Vector2> pattern)
        {
           return _cells.Where(r => pattern.Any(p => p.x == r.X && p.y == r.Y)).ToList();
        }

        public void InitGrid(List<GrandDevs.Networking.GridRow> grid)
        {
            _cells = new List<Cell>();

            Cell cell;
            foreach(var row in grid)
            {
                cell = new Cell(row.x, row.y, _parentOfGrid);
                cell.Set(row.type.Parse<GrandDevs.Tavern.Common.Enumerators.CellType>());
                _cells.Add(cell);
            }
        }

        public void UpdateGrid(List<GrandDevs.Networking.GridRow> grid, bool withTransition)
        {
            for(int i = 0; i < _cells.Count; i++)
            {
                _cells[i].Set(grid[i].type.Parse<GrandDevs.Tavern.Common.Enumerators.CellType>(), withTransition);
            }
        }

        public List<Cell> GetCellsList() => _cells;

        public Cell GetRandomCell()
        {
            if (_cells == null || _cells.Count == 0)
                return null;

            return _cells[UnityEngine.Random.Range(0, _cells.Count)];
        }

        public Cell GetCellWithPlayer(Vector3 inputVector)
        {
            Cell nearestCell = null;
            float shortestDistance = float.MaxValue;

            foreach (Cell row in _cells)
            {
                float currentDistance = Vector3.Distance(inputVector, row.GetWorldPosition() + _rowOffset); 
                if (currentDistance < shortestDistance)
                {
                    shortestDistance = currentDistance;
                    nearestCell = row;
                }
            }

            var character = nearestCell.GetCharacter();
            if (IsPointInsideCell(inputVector, nearestCell) && character!=null && !character.GetPlayerData().IsLocalPlayer)
                return nearestCell;
            else
                return null;
        }
        
        public Cell GetNearestCell(Vector3 inputVector, List<Cell> availableCells)
        {
            Cell nearestCell = null;
            float shortestDistance = float.MaxValue;
            
            foreach (Cell row in _cells)
            {
                float currentDistance = Vector3.Distance(inputVector, row.GetWorldPosition() + _rowOffset); 
                if (currentDistance < shortestDistance)
                {
                    shortestDistance = currentDistance;
                    nearestCell = row;
                }
            }

            if (IsPointInsideCell(inputVector, nearestCell) && availableCells.Contains(nearestCell))
                return nearestCell;
            else
                return null;
        }



        public List<Cell> GetCellsInRadius(Cell centerCell, int attackRange)
        {
            List<Cell> resultCells = new List<Cell>();

            for (int i = centerCell.X - attackRange; i <= centerCell.X + attackRange; i++)
            {
                for (int j = centerCell.Y - attackRange; j <= centerCell.Y + attackRange; j++)
                {
                    Cell cell = GetCellsList().FirstOrDefault(c => c.X == i && c.Y == j);
                    if (cell != null) 
                        resultCells.Add(cell);
                }
            }

            return resultCells;
        }

        public Cell GetCell(int targetCellX, int targetCellY)
        {
            return _cells.FirstOrDefault(cell => cell.X == targetCellX && cell.Y == targetCellY);
        }

        private bool IsPointInsideCell(Vector3 inputVector, Cell cell)
        {
            Vector3 cellPosition = cell.GetCellCenter() /*cell.GetWorldPosition() + _rowOffset*/;
            float halfWidth = cell.Width / 2.0f;
            float halfHeight = cell.Height / 2.0f;

            bool insideX = inputVector.x >= (cellPosition.x - halfWidth) && inputVector.x <= (cellPosition.x + halfWidth);
            bool insideY = inputVector.y >= (cellPosition.y - halfHeight) && inputVector.y <= (cellPosition.y + halfHeight);

            return insideX && insideY;
        }
    }
}