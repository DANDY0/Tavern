using System;
using System.Collections.Generic;
using DG.Tweening;
using GrandDevs.Tavern.Common;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class CellView
    {
        private readonly ILoadObjectsManager _loadObjectsManager;

        // private SpriteRenderer _backgroundRenderer;
        private SpriteRenderer _activeRenderer;
        private SpriteRenderer _enemyRenderer;
        private SpriteRenderer _chooseRenderer;
        private SpriteRenderer _attackRenderer;

        private Color _defaultColor = new Color(1f, 1f, 1f, 150 / 255f);
        private Color _highlightColor = new Color(0.41f, 0.76f, 0.44f);
        private Color _enemyHighlightColor = new Color(0.6f, 0.65f, 0.76f);
        private Color _chooseColor = new Color(0.96f, 0.89f, 0.54f);

        private ParticleSystem _defaultAttackVfx;
        private ParticleSystem _defenseVfx;
        private ParticleSystem _healVfx;
        private ParticleSystem _defenseEqualHealthVfx;
        private ParticleSystem _healthHalfEnemiesVfx;
        private ParticleSystem _turnVfx;

        private bool _isChosen;
        private bool _isHighlighted;
        private Vector3 _centerOffset = new Vector3(0, 1, 0);
        private float _fadeDuration = .6f;

        public CellView(Transform selfObject)
        {
            _activeRenderer = selfObject.transform.Find("Sprite_Active").GetComponent<SpriteRenderer>();
            _chooseRenderer = selfObject.transform.Find("Sprite_Chosen").GetComponent<SpriteRenderer>();
            _attackRenderer = selfObject.transform.Find("Sprite_Attack").GetComponent<SpriteRenderer>();
            _enemyRenderer = selfObject.transform.Find("Sprite_Enemy").GetComponent<SpriteRenderer>();

            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            LoadPlayerVfx();
        }

        public void SetCellHighlight(bool state)
        {
            _activeRenderer.enabled = state;
            _isHighlighted = state;
        }

        public void SetEnemyCellHighlight(bool state)
        {
            _enemyRenderer.enabled = state;
            if (!state)
                ReturnLocalPlayerColor();
        }

        public void SetAttackRangeCell(bool state)
        {
            // _activeRenderer.enabled = !state;
            _attackRenderer.enabled = state;
            if (!state)
                ReturnLocalPlayerColor();
        }


        public void SetCellChoose(bool state)
        {
            if (state)
            {
                _activeRenderer.enabled = false;
                _chooseRenderer.enabled = true;
            }
            else
                _chooseRenderer.enabled = false;

            _isChosen = state;
        }

        private void ReturnLocalPlayerColor()
        {
            if (_isChosen)
                _chooseRenderer.enabled = true;
            if (_isHighlighted)
                _activeRenderer.enabled = true;
            if (!_isChosen && !_isHighlighted)
                _activeRenderer.enabled = false;
        }

        public void PlayActionParticles(List<Cell> cells, Enumerators.ActionType actionType,
            bool isCenterOffset = false)
        {
            var actionParticle = GetActionParticle(actionType);
            Vector3 offset = isCenterOffset ? _centerOffset : Vector3.zero;
            foreach (var cell in cells)
            {
                var particleSystem =
                    MonoBehaviour.Instantiate(actionParticle, cell.GetCellCenter() + offset, Quaternion.identity);
                if (actionType == Enumerators.ActionType.FreezeMoveForOther)
                    ExclusiveRotate(particleSystem, new Vector3(-90, 0, 0));
            }
        }

        public void PlayActionParticles(Cell cell, Enumerators.ActionType actionType, bool isCenterOffset = false)
        {
            var actionParticle = GetActionParticle(actionType);
            Vector3 offset = isCenterOffset ? _centerOffset : Vector3.zero;
            var particleSystem = MonoBehaviour.Instantiate(actionParticle, cell.GetCellCenter() + offset, Quaternion.identity);
            if (actionType == Enumerators.ActionType.FreezeMoveForOther)
                ExclusiveRotate(particleSystem, new Vector3(-90, 0, 0));
        }
        
        public ParticleSystem PlayTurnEffect(Cell cell, bool isCenterOffset = false)
        {
            Vector3 offset = isCenterOffset ? _centerOffset : Vector3.zero;
            var particleSystem = MonoBehaviour.Instantiate(_turnVfx, cell.GetCellCenter() + offset, Quaternion.identity);
            return particleSystem;
        }


        private void ExclusiveRotate(ParticleSystem particleSystem, Vector3 eulerValue) => 
            particleSystem.transform.localEulerAngles = eulerValue;

        private ParticleSystem GetActionParticle(Enumerators.ActionType actionType)
        {
            switch (actionType)
            {
                case Enumerators.ActionType.AttackVertical:
                case Enumerators.ActionType.AttackHorizontal:
                case Enumerators.ActionType.AttackAround:
                case Enumerators.ActionType.AttackRange:
                case Enumerators.ActionType.AttackRegular:
                case Enumerators.ActionType.NuclearBomb:
                    return _defaultAttackVfx;
                case Enumerators.ActionType.DefenseEqualHealthSelf:
                    return _defenseEqualHealthVfx;
                case Enumerators.ActionType.HealthIsHalfToOther:
                    return _healthHalfEnemiesVfx;
                case Enumerators.ActionType.FreezeMoveForOther:
                    return _defaultAttackVfx;
                case Enumerators.ActionType.HealSelf:
                    return _healVfx;
                case Enumerators.ActionType.DefenseSelf:
                    return _defenseVfx;
                case Enumerators.ActionType.MoveSelf:
                    break;
                case Enumerators.ActionType.Unknown:

                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }

            return null;
        }

        private void LoadPlayerVfx()
        {
            _defaultAttackVfx = _loadObjectsManager.GetObjectByPath<ParticleSystem>(Constants.DefaultAttackVfxPath);
            _defenseVfx = _loadObjectsManager.GetObjectByPath<ParticleSystem>(Constants.DefenceVfxPath);
            _healVfx = _loadObjectsManager.GetObjectByPath<ParticleSystem>(Constants.HealVfxPath);
            _defenseEqualHealthVfx =
                _loadObjectsManager.GetObjectByPath<ParticleSystem>(Constants.DefenseEqualHealthPath);
            _healthHalfEnemiesVfx =
                _loadObjectsManager.GetObjectByPath<ParticleSystem>(Constants.HealthHalfEnemiesPath);
            _turnVfx = _loadObjectsManager.GetObjectByPath<ParticleSystem>(Constants.TurnVfxPath);
        }

        public void FadeTransition(SpriteRenderer spriteRenderer, Enumerators.CellType type)
        {
            DOTween.To(() => spriteRenderer.color,
                    x => spriteRenderer.color = x,
                    new Color(1f, 1f, 1f, 0f),
                    _fadeDuration)
                .OnComplete(() =>
                {
                    spriteRenderer.sprite = _loadObjectsManager.GetObjectByPath<Sprite>($"Images/Rune_{(int)type}");


                    DOTween.To(() => spriteRenderer.color,
                        x => spriteRenderer.color = x,
                        Color.white,
                        _fadeDuration);
                });
        }

        public void Dispose()
        {
            
        }
    }
}