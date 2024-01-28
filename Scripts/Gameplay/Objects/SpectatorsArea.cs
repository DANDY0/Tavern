using System;
using System.Collections.Generic;
using DG.Tweening;
using GrandDevs.Networking;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class SpectatorsArea
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly RoomController _roomController;
        private readonly GameplayController _gameplayController;

        private readonly float _maxJumpDuration = 4f;
        private readonly float _rotateToTargetTime = 0.2f;
        private readonly float _jumpHeight = 4f;
        private readonly float _speedCoefficient = 5f;

        private List<Player> _deadPlayers;
        private Transform _spectatorsZoneParent;
        private Transform _arenaCenter;
        private List<Transform> _playerSpots;
        private int _currentLevel;


        public SpectatorsArea()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _roomController = _gameplayManager.GetController<RoomController>();
            _gameplayController = _gameplayManager.GetController<GameplayController>();
            _gameplayController.OnPlayersSpawned += PlayersSpawnedEventHandler ;
            // _gameplayManager.GameplayStartedEvent += GameplayStartedEventHandler;
            _roomController.OnJoinedRoom += JoinedRoomEventHandler;
        }

        public void TryPlaceInSpectatorsArea(Player player)
        {
            if (!player.IsSpectator())
                PlaceInSpectatorsArea(player);
        }

        public void Dispose()
        {
            // _gameplayManager.GameplayStartedEvent -= GameplayStartedEventHandler;
            _gameplayController.OnPlayersSpawned -= PlayersSpawnedEventHandler ;
            _roomController.OnJoinedRoom -= JoinedRoomEventHandler;
        }

        private void JoinedRoomEventHandler(RoomInfo data) => _currentLevel = data.level;
        
        private void PlayersSpawnedEventHandler() => Init();

        private void Init()
        {
            Debug.LogWarning("INIT SPEACATORS");
            
            _spectatorsZoneParent = GameObject.Find($"GameField/DeathZone/Level_{_roomController.CurrentLevel}").transform;
            _playerSpots = new List<Transform>();
            foreach (Transform child in _spectatorsZoneParent)
                _playerSpots.Add(child);
            _arenaCenter = GameObject.Find("GameField/ArenaCenter").transform;
        }

        private void PlaceInSpectatorsArea(Player player)
        {
            player.SetIsSpectator(true);
            player.PlayAnimation(Constants.MoveHash);
            PlacePlayerTween(player, _playerSpots[player.GetPlayerData().PlayerID].position,
                () => PlacePlayerCallback(player));
        }

        private void PlacePlayerCallback(Player player)
        {
            LookAtTarget(player, _arenaCenter.position);
            player.PlayAnimation(Constants.IdleHash);
        }

        private void PlacePlayerTween(Player player, Vector3 targetPosition, Action callback)
        {
            var playerTransform = player.GetPlayerTransform();
            var startPosition = playerTransform.position;

            var controlPoint = (startPosition + targetPosition) / 2.0f;
            controlPoint.y += _jumpHeight;

            var duration = Mathf.Min(Vector3.Distance(startPosition, targetPosition) / _speedCoefficient,
                _maxJumpDuration);

            float time = 0;
            Vector3 position;

            DOTween.To(() => time, x => time = x, 1, duration).OnUpdate(() =>
            {
                position = GetBezierPoint(startPosition, controlPoint, targetPosition, time);
                playerTransform.position = position;
            }).OnComplete(() => callback?.Invoke());
        }

        private void LookAtTarget(Player player, Vector3 targetPosition)
        {
            var playerTransform = player.GetPlayerTransform();
            var direction = (targetPosition - playerTransform.position).normalized;
            direction.y = 0;

            playerTransform.DOLookAt(playerTransform.position + direction, _rotateToTargetTime);
        }

        private Vector3 GetBezierPoint(Vector3 start, Vector3 controlPoint, Vector3 end, float t)
        {
            var u = 1 - t;
            var tt = t * t;
            var uu = u * u;

            var p = uu * start;
            p += 2 * u * t * controlPoint;
            p += tt * end;

            return p;
        }
    }
}