using Cinemachine;
using DG.Tweening;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class PlayerView
    {
        private readonly ILoadObjectsManager _loadObjectsManager;
        private readonly GameObject _selfObject;
        
        private CinemachineVirtualCamera _playerCameraFirst;
        private CinemachineVirtualCamera _playerCameraThird;
        private Animator _animator;
        private ParticleSystem _takeDamageVfx;
        private ParticleSystem _missVfxText;
        private ParticleSystem _fireBallVfx;
        private ParticleSystem _explosionFireballVfx;
        private ParticleSystem _freezeVfx;
        
        private readonly Vector3 _offsetMissVfx = new Vector3(0,3f,0);
        private readonly Vector3 _offsetPlayerAction = new Vector3(0,1.6f,0);
        private readonly float _fireballDuration = .8f;

        private ParticleSystem _currentFreezeDebuff;

        public PlayerView(GameObject playerObject)
        {
            _selfObject = playerObject;
            _animator = _selfObject.GetComponentInChildren<Animator>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _playerCameraFirst = _selfObject.GetComponentInChildren<CinemachineVirtualCamera>();
            _playerCameraFirst = _selfObject.transform.Find("Container/PlayerCameraFirst").GetComponent<CinemachineVirtualCamera>();
            _playerCameraThird = _selfObject.transform.Find("Container/PlayerCameraThird").GetComponent<CinemachineVirtualCamera>();
            LoadPlayerVfx();
        }

        public Transform GetPlayerTransform() => _selfObject.transform;

        public void PlayAnimation(int animationHash) => _animator.SetTrigger(animationHash);

        public void PlayAnimationBool(int animationHash, bool state) => _animator.SetBool(animationHash, state);

        public void SetAnimationBool(int animationHash, bool state) => _animator.SetBool(animationHash, state);

        public void PlayTakeDamageVfx() => MonoBehaviour.Instantiate(_takeDamageVfx, _selfObject.transform.position, Quaternion.identity);
        
        public void FreezePlayer(bool state)
        {
            if (state)
            {
                if(_currentFreezeDebuff!=null)
                    Object.Destroy(_currentFreezeDebuff.gameObject);
                _currentFreezeDebuff = MonoBehaviour.Instantiate(_freezeVfx, _selfObject.transform.position, Quaternion.identity);
                
            }
            else
            {
                if(_currentFreezeDebuff!=null)
                    Object.Destroy(_currentFreezeDebuff.gameObject);
            }
        }

        public void ShowPlayerStats()
        {
            
        }
        
        public void PlayMissVfx() => MonoBehaviour.Instantiate(_missVfxText, _selfObject.transform.position + _offsetMissVfx, Quaternion.identity);

        public void ThrowFireballToTarget(Vector3 targetPos)
        {
            targetPos += _offsetPlayerAction;
            var startPos = _selfObject.transform.position + _offsetPlayerAction;
            var fireballInstance = MonoBehaviour.Instantiate(_fireBallVfx, startPos, Quaternion.identity);
            
            fireballInstance.transform.DOMove(targetPos, _fireballDuration)
                .OnKill(() => {
                    var explosionInstance = MonoBehaviour.Instantiate(_explosionFireballVfx, targetPos, Quaternion.identity);
                    MonoBehaviour.Destroy(fireballInstance.gameObject);
                });
        }
        
        public void Destroy() => MonoBehaviour.Destroy(_selfObject);

        public CinemachineVirtualCamera GetCameraFirst() => _playerCameraFirst;
        public CinemachineVirtualCamera GetCameraThird() => _playerCameraThird;

        private void LoadPlayerVfx()
        {
            _takeDamageVfx = _loadObjectsManager.GetObjectByPath<ParticleSystem>(Constants.DefaultAttackVfxPath);
            _missVfxText = _loadObjectsManager.GetObjectByPath<ParticleSystem>(Constants.MissVfxPath);
            _fireBallVfx = _loadObjectsManager.GetObjectByPath<ParticleSystem>(Constants.FireballVfxPath);
            _freezeVfx = _loadObjectsManager.GetObjectByPath<ParticleSystem>(Constants.FreezeVfxPath);
            _explosionFireballVfx = _loadObjectsManager.GetObjectByPath<ParticleSystem>(Constants.ExplosionFireballVfxPath);
        }
    }
}