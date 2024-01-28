using System;
using GrandDevs.Networking;
using GrandDevs.Tavern;
using GrandDevs.Tavern.Helpers;
using UnityEngine;
using static GrandDevs.Tavern.Common.Enumerators;

public abstract class BaseSkill
{
    protected readonly Board _board;
    protected readonly Player _player;
    protected readonly SkillView _skillView;
    protected CellView _cellView;
    protected Cell _currentCell;
    protected ActionType _actionType;
    
    protected BaseSkill(Player player)
    {
        _board = GameClient.Get<IGameplayManager>().Board;
        _player = player;
        _skillView = new SkillView();
    }

    public virtual void Execute(RoundProcessingData.ActionData actionData)
    {
        _currentCell = _player.Cell;
        _cellView = _currentCell.CellView;
        _actionType = (ActionType)Enum.Parse(typeof(ActionType), actionData.type);
        
        if (this is not Move)
        {
            var particles = _cellView.PlayTurnEffect(_currentCell);
            InternalTools.DoActionDelayed(() => MonoBehaviour.Destroy(particles.gameObject), actionData.duration);
            _currentCell.Execute();
        }
    }
}