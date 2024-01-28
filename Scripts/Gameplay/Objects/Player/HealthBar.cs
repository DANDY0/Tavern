using System;
using System.Collections.Generic;
using DG.Tweening;
using GrandDevs.Tavern.Helpers;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class HealthBar
    {
        private HealthBarView _healthBarView;
        private readonly float _maxHealth;
        private readonly float _maxDefense;
        private float _currentHealth;
        private float _currentDefense;
        public event Action OnPlayerDead;
        public event Action<float> OnHealthChanged;
        public event Action<float> OnDefenseChanged;

        public HealthBar(int playerMaxHealth, Player player)
        {
            _maxHealth = playerMaxHealth;
            _maxDefense = playerMaxHealth * .5f;
            _currentHealth = _maxHealth;
            _currentDefense = player.GetPlayerData().Defense;
            _healthBarView = new HealthBarView(player.GetPlayerTransform(), player.GetPlayerData().PlayerID,_maxHealth,_maxDefense);
            _healthBarView.UpdateHealthBar(GetCurrentHealthPercentage());
            _healthBarView.UpdateDefenseBar(GetCurrentDefensePercentage());
        }
        
        public void ChangeHealth(float amount)
        {
            if(amount < 0)
            {
                if(_currentDefense > 0)
                {
                    if (_currentDefense >= Math.Abs(amount))
                        SetDefense(amount);
                    else
                    {
                        amount += _currentDefense;
                        SetDefense(0, true);
                        _currentHealth += amount;
                    }
                }
                else
                    _currentHealth += amount;
            }
            else
                _currentHealth += amount;

            UpdateHealth();
        }

        public void LookAtCamera() => _healthBarView.LookAtCamera();

        public void SetDefense(float amount, bool overrideValue = false)
        {
            if (overrideValue)
                _currentDefense = amount;
            else
                _currentDefense += amount;

            _currentDefense = Mathf.Clamp(_currentDefense, 0, _maxDefense);
            var currentDefensePercentage = GetCurrentDefensePercentage();
            OnDefenseChanged?.Invoke(currentDefensePercentage);
            _healthBarView.UpdateDefenseBar(currentDefensePercentage);
        }

        public void SetMaxDefense() => SetDefense(_maxDefense, true);

        public void SetHealth(float value)
        {
            _currentHealth = value;
            UpdateHealth();
        }

        public float GetCurrentHealthPercentage() => _currentHealth / _maxHealth;

        public float GetCurrentDefensePercentage() => _currentDefense / _maxDefense;

        private void UpdateHealth()
        {
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

            var currentHealthPercentage = GetCurrentHealthPercentage();
            OnHealthChanged?.Invoke(currentHealthPercentage);
            _healthBarView.UpdateHealthBar(currentHealthPercentage);

            if (_currentHealth <= 0)
            {
                OnPlayerDead?.Invoke();
                _healthBarView.Hide();
            }
        }
    }
    
    class HealthBarView
    {
        private readonly float _maxHp;
        private readonly float _maxDef;
        private readonly int _dividerValue = 100;
        private Transform _selfTransform;
        private SpriteRenderer _spriteRendererHp;
        private SpriteRenderer _spriteRendererDefense;
        private SpriteRenderer _spriteRendererHPDamageShow;
        private SpriteRenderer _spriteRendererDefDamageShow;
        private SpriteRenderer _spritePlayerID;
        private Camera _mainCamera;
        private Vector3 _originalHpScale;
        private Vector3 _originalDefScale;
        private Transform _dividersParentHp;
        private Transform _dividersParentDef;
        private GameObject _dividerPrefab;
        
        private float _currentHpScale;
        private float _currentDefScale;
        private float _visualEffectDelay = .5f;
        private float _visualEffectDuration = 1f;
        
        private Tweener _hpTween;
        private Tweener _defTween;

        private List<Sprite> _playerIdSprites = new List<Sprite>();

        public HealthBarView(Transform playerTransform, int playerID, float maxHP, float maxDef)
        {
            _maxHp = maxHP;
            _maxDef = maxDef;
            var loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _selfTransform = playerTransform.Find("HpBar").GetComponent<Transform>();
            _spriteRendererHp = playerTransform.Find("HpBar/Back/HP").GetComponent<SpriteRenderer>();
            _spriteRendererDefense = playerTransform.Find("HpBar/Back/Def").GetComponent<SpriteRenderer>();
            _spriteRendererHPDamageShow = playerTransform.Find("HpBar/Back/HPDamageShow").GetComponent<SpriteRenderer>();
            _spriteRendererDefDamageShow = playerTransform.Find("HpBar/Back/DefDamageShow").GetComponent<SpriteRenderer>();
            _spritePlayerID = playerTransform.Find("HpBar/PlayerID").GetComponent<SpriteRenderer>();
            _dividersParentHp = playerTransform.Find("HpBar/Back/DividersHP");
            _dividersParentDef = playerTransform.Find("HpBar/Back/DividersDef");
            
            for (int i = 0; i < Constants.MaxPlayersCount; i++)
                _playerIdSprites.Add(loadObjectsManager.GetObjectByPath<Sprite>($"Sprites/Numbers/number_{i + 1}"));

            _dividerPrefab = loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Hp/Divider");
            _mainCamera = Camera.main;
            _originalHpScale = _spriteRendererHp.transform.localScale;
            _originalDefScale = _spriteRendererDefense.transform.localScale;
            _spritePlayerID.sprite = _playerIdSprites[playerID];
            InitDividers();
        }
        
        private void InitDividers()
        {
            foreach (Transform child in _dividersParentHp)
                UnityEngine.Object.Destroy(child.gameObject);

            foreach (Transform child in _dividersParentDef)
                UnityEngine.Object.Destroy(child.gameObject);

            int dividerCount = Mathf.FloorToInt(_maxHp / _dividerValue);

            for (int i = 1; i < dividerCount; i++)
            {
                float normalizedPosition = (float)i * _dividerValue / _maxHp;
                if (normalizedPosition < 1f)
                {
                    CreateDivider(normalizedPosition, _dividersParentHp);
                    CreateDivider(normalizedPosition, _dividersParentDef);
                }
            }
        }

        private void CreateDivider(float normalizedPosition, Transform parent)
        {
            var dividerInstance = UnityEngine.Object.Instantiate(_dividerPrefab, parent);
            var localPos = dividerInstance.transform.localPosition;
            localPos.x = Mathf.Lerp(0, 1, normalizedPosition);
            dividerInstance.transform.localPosition = localPos;
        }

        public void UpdateHealthBar(float healthPercentage)
        {
            _currentHpScale = _spriteRendererHp.transform.localScale.x;
            _spriteRendererHp.transform.localScale = new Vector3(healthPercentage, _originalHpScale.y, _originalHpScale.z);
            VisualHealthDamage(healthPercentage);
        }

        public void UpdateDefenseBar(float defensePercentage)
        {
            _currentDefScale = _spriteRendererDefense.transform.localScale.x;
            _spriteRendererDefense.transform.localScale = new Vector3(defensePercentage, _originalDefScale.y, _originalDefScale.z);
            VisualDefenseDamage(defensePercentage);
        }

        private void VisualHealthDamage(float healthPercentage)
        {
            _hpTween?.Kill();
    
            InternalTools.DoActionDelayed(() =>
            {
                _hpTween = DOTween.To(() => _currentHpScale, x => _currentHpScale = x, healthPercentage, _visualEffectDuration)
                    .OnUpdate(() =>
                    {
                        _spriteRendererHPDamageShow.transform.localScale = new Vector3(_currentHpScale,
                            _originalHpScale.y, _originalHpScale.z);
                    });
            }, _visualEffectDelay);
        }

        private void VisualDefenseDamage(float defensePercentage)
        {
            _defTween?.Kill();

            InternalTools.DoActionDelayed(() =>
            {
                _defTween = DOTween.To(() => _currentDefScale, x => _currentDefScale = x, defensePercentage, _visualEffectDuration)
                    .OnUpdate(() =>
                    {
                        _spriteRendererDefDamageShow.transform.localScale = new Vector3(_currentDefScale,
                            _originalDefScale.y, _originalDefScale.z);
                    });
            }, _visualEffectDelay);
        }

        public void LookAtCamera()
        {
            _selfTransform.forward = _mainCamera.transform.forward;
        }

        public void Hide()
        {
            _selfTransform.gameObject.SetActive(false);
        }
        
        public void Dispose()
        {
            _hpTween?.Kill();
            _defTween?.Kill();
        }
    }
}

