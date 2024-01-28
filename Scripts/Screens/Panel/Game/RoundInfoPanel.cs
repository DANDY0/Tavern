using System.Collections.Generic;
using DG.Tweening;
using GrandDevs.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public class RoundInfoPanel
    {
        private readonly GameObject _selfObject;
        private readonly IGameplayManager _gameplayManager;
        private readonly RoundEventController _roundEventController;
        private readonly GameEventController _gameEventsController;
        private readonly GameplayController _gameplayController;
        private readonly Transform _contentParent;
        private readonly string _roundTimerPanelPath = "Prefabs/UI/GamePageUI/RoundInfoPanel/RoundInfoPanel";
        private readonly string _roundTimerViewPath = "Prefabs/UI/GamePageUI/RoundInfoPanel/TimerView";
        private readonly string _boardResetViewPath = "Prefabs/UI/GamePageUI/RoundInfoPanel/BoardResetView";
        private readonly string _turnsCounterPath = "Prefabs/UI/GamePageUI/RoundInfoPanel/TurnsCounterView";

        private RoundTimerView _roundTimerView;
        private BoardResetView _boardResetView;
        private TurnsCounterView _turnsCounterView;


        public RoundInfoPanel(Transform contentParent)
        {
            _contentParent = contentParent;
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _roundEventController = _gameplayManager.GetController<RoundEventController>();
            _gameEventsController = _gameplayManager.GetController<GameEventController>();
            _gameplayController = _gameplayManager.GetController<GameplayController>();

            _roundEventController.OnRoundStarted += RoundStartedEventHandler;
            _roundEventController.OnRoundProcessing += RoundProcessingEventHandler;
            _gameplayController.OnPlayersSpawned += PlayersSpawned;
            _gameplayManager.GameplayEndedEvent += GameplayEndedEvent;
            _selfObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>(_roundTimerPanelPath), _contentParent);
        }

        private void GameplayEndedEvent()
        {
            _roundTimerView.Dispose();
            _roundTimerView = null;
            _boardResetView.Dispose();
            _boardResetView = null;
            _turnsCounterView.Dispose();
            _turnsCounterView = null;
        }

        private void PlayersSpawned()
        {
            _roundTimerView = new RoundTimerView(_roundTimerViewPath, _selfObject.transform);
            _boardResetView = new BoardResetView(_boardResetViewPath, _selfObject.transform);
            _turnsCounterView = new TurnsCounterView(_turnsCounterPath, _selfObject.transform);
        }

        private void RoundStartedEventHandler(RoundStartedData data)
        {
            _roundTimerView.StartTimer();
            if(data.grid != null && data.grid.Count > 0)
                _boardResetView.ResetPieces();
        }

        private void RoundProcessingEventHandler(RoundProcessingData data)
        {
            _roundTimerView.StopHideTimer();
            _boardResetView.IncrementRound();
        }

        public void Dispose()
        {
            _roundEventController.OnRoundStarted -= RoundStartedEventHandler;
            _roundEventController.OnRoundProcessing -= RoundProcessingEventHandler;
            _gameplayController.OnPlayersSpawned -= PlayersSpawned;
            _gameplayManager.GameplayEndedEvent -= GameplayEndedEvent;

            _roundTimerView = null;
            _boardResetView = null;
            _turnsCounterView = null;

            if (_selfObject != null) 
                MonoBehaviour.Destroy(_selfObject);
        }

        class RoundTimerView
        {
            private readonly RoundEventController _roundEventController;
            private readonly GameEventController _gameEventsController;
            private GameObject _selfObject;
            private TextMeshProUGUI _roundTimerText;
            private int _roundTimeValue;
            private int _currentRoundTime;

            public RoundTimerView(string roundTimerPath, Transform contentParent)
            {
                var loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                var objectByPath = loadObjectsManager.GetObjectByPath<GameObject>(roundTimerPath);
                _gameEventsController = GameClient.Get<IGameplayManager>().GetController<GameEventController>();
                _selfObject = MonoBehaviour.Instantiate(objectByPath, contentParent);
                _roundTimerText = _selfObject.transform.Find("Text_RoundTime").GetComponent<TextMeshProUGUI>();
                _roundTimeValue = GameClient.Get<IGameplayManager>().GameConfig.roundTime;
            }
            
            public void StartTimer()
            {
                _currentRoundTime = _roundTimeValue;
                _roundTimerText.gameObject.SetActive(true);

                DOTween.To(() => _currentRoundTime, x =>
                {
                    _currentRoundTime = x;
                    _roundTimerText.text = $"Time: {_currentRoundTime}";
                }, 0, _roundTimeValue).SetEase(Ease.Linear);
            }

            public void StopHideTimer()
            {
                _roundTimerText.gameObject.SetActive(false);
            }

            public void Dispose() => MonoBehaviour.Destroy(_selfObject);
        }

        class BoardResetView
        {
            private GameObject _selfObject;
            private Transform _piecesParent;
            private List<Image> _pieces =  new List<Image>();
            private bool _shouldUpdatePieces = false;  
            private int _currentActivePieceIndex = -1;
            public BoardResetView(string boardResetPath, Transform contentParent)
            {
                var loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                var objectByPath = loadObjectsManager.GetObjectByPath<GameObject>(boardResetPath);
                
                _selfObject = MonoBehaviour.Instantiate(objectByPath, contentParent);
                _piecesParent = _selfObject.transform.Find("RoundPieces");
                foreach (Transform child in _piecesParent)
                    _pieces.Add(child.transform.Find("Image_Sword").gameObject.GetComponent<Image>());
            }

            public void IncrementRound()
            {
                if (_currentActivePieceIndex + 1 < _pieces.Count)
                {
                    _currentActivePieceIndex++;
                    _pieces[_currentActivePieceIndex].enabled = true;
                }
            }
            
            public void ResetPieces()
            {
                foreach (var piece in _pieces) 
                    piece.enabled = false;

                _currentActivePieceIndex = -1;
            }

            public void Dispose() => MonoBehaviour.Destroy(_selfObject);
        }

        class TurnsCounterView
        {
            private GameObject _selfObject;
            private TextMeshProUGUI _counterText;
            public TurnsCounterView(string turnsCounterPath, Transform contentParent)
            {
                var loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
                var objectByPath = loadObjectsManager.GetObjectByPath<GameObject>(turnsCounterPath);
                var roundEventController = GameClient.Get<IGameplayManager>().GetController<RoundEventController>();
                _selfObject = MonoBehaviour.Instantiate(objectByPath, contentParent);
                _counterText = _selfObject.transform.Find("Text_TurnsCounter").GetComponent<TextMeshProUGUI>();
                roundEventController.OnTurnCounterChanged += TurnsCounterChanged;
            }

            private void TurnsCounterChanged(int value) => _counterText.text = "Turn: " + value;

            public void Dispose() => MonoBehaviour.Destroy(_selfObject);
        }
    }
}