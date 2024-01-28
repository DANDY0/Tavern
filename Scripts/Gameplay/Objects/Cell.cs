using System;
using DG.Tweening;
using GrandDevs.Networking;
using UnityEngine;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class Cell
    {
        public int X { get; }
        public int Y { get; }
        public bool IsChosen { get; private set; }
        public float Width => 2f;
        public float Height => 2f;
        public CellView CellView { get; set; }
        public Enumerators.CellType CellType { get; set; }

        private readonly ILoadObjectsManager _loadObjectsManager;
        private readonly GameObject _selfObject;
        private readonly SpriteRenderer _spriteRenderer;
        private readonly RoundEventController _roundEventController;

        private MoveAction _cellMoveAction;
        private IUnit CellUnit { get; set; }
        private Enumerators.UnitType UnitType { get; set; }
        private Material _dissolveMaterial;
        private float _dissolveValue = 1f;
        private Color _dissolveColor = new Color(0.08f, 0f, 0f);
        
        private Sequence _dissolveSequence;

        public Cell(int x, int y, Transform parent)
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _roundEventController = GameClient.Get<IGameplayManager>().GetController<RoundEventController>();
            _roundEventController.OnRoundStarted += RoundStartedEventHandler;

            X = x;
            Y = y;
            
            _selfObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Item_Cell"), parent, false);
            _spriteRenderer = _selfObject.transform.Find("Sprite_Preview").GetComponent<SpriteRenderer>();
            _selfObject.transform.localPosition = new Vector3(X * Width, 0, Y * -Height);
            
            CellView = new CellView(_selfObject.transform);
            _spriteRenderer.material = new Material(_spriteRenderer.sharedMaterial);
            _dissolveMaterial = _spriteRenderer.material;

        }

        public void Set(Enumerators.CellType type, bool withTransition = false)
        {
            CellType = type;
            SetCellAppearance(type, withTransition);
        }

        public void SetUnit(IUnit unit)
        {
            CellUnit = unit;
            if (CellUnit == null)
                UnitType = Enumerators.UnitType.Empty;
            else
            { 
                CellUnit.AssignCell(this);
                UnitType = CellUnit.UnitType;
            }
        }

        public Player GetCharacter()
        {
            if (CellUnit is Player)
                return (Player)CellUnit;
            return null;
        }
        
        public Vector3 GetWorldPosition() => _selfObject.transform.position;
        
        public Vector3 GetCellCenter() => _selfObject.transform.position + Constants.CellOffset;

        public void SetCellHighlight(bool state) => CellView.SetCellHighlight(state);

        public void SetEnemyCellHighlight(bool state) => CellView.SetEnemyCellHighlight(state);

        public void SetCellChoose(bool state)
        {
            IsChosen = state;
            CellView.SetCellChoose(state);
        }

        public void Dispose()
        {
            CellView.Dispose();
            CellView = null;
            _roundEventController.OnRoundStarted -= RoundStartedEventHandler;

            if (_dissolveSequence != null)
            {
                _dissolveSequence.Kill();
                _dissolveSequence = null;
            }

            MonoBehaviour.Destroy(_selfObject);
        }


        public void Execute()
        {
            DissolveEffect();
        }


        private void DissolveEffect()
        {
            if (_dissolveSequence != null)
            {
                _dissolveSequence.Kill();
                _dissolveSequence = null;
            }

            _dissolveSequence = DOTween.Sequence();
            _dissolveSequence.Append(DOTween.To(() => _spriteRenderer.color, color => _spriteRenderer.color = color, _dissolveColor, 1f));
            _dissolveSequence.Append(DOTween.To(() => _dissolveValue, x => _dissolveValue = x, 0f, 1f)
                .OnUpdate(() => { _dissolveMaterial.SetFloat("_DissolvePower", _dissolveValue); })
                .OnComplete(() => { _spriteRenderer.sprite = null; }));

            _dissolveSequence.Play();
        }

        private void SetCellAppearance(Enumerators.CellType type, bool withTransition = false)
        {

            _dissolveValue = 1f;
            _dissolveMaterial.SetFloat("_DissolvePower", _dissolveValue);
            
            if (withTransition)
                CellView.FadeTransition(_spriteRenderer, type);
            else
                _spriteRenderer.sprite = _loadObjectsManager.GetObjectByPath<Sprite>($"Images/Rune_{(int)type}");
        }


        private void RoundStartedEventHandler(RoundStartedData startedData)
        {
            Debug.LogWarning($"Cell X: {X} Y: {Y} Unit Type = {UnitType}");
        }
    }
}