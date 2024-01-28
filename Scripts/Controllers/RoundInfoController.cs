using System;
using DG.Tweening;
using GrandDevs.Networking;
using GrandDevs.Tavern.Helpers;
using TMPro;
using Enumerators = GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class RoundInfoController: IController
    {
        private RoundEventController _roundEventController;
        private GameEventController _gameEventsController;
        
        private Tweener _timerTweener;
        private TextMeshProUGUI _roundCounterText;
        private TextMeshProUGUI _roundStateText;
        private TextMeshProUGUI _roundTimeText;

        private int _roundCounterValue = 1;
        private int _roundTimeValue;
        private int _currentRoundTime;
        
        private readonly string _processingStateText = "Combat \n stage";
        private readonly string _startStateText = "Preparation \n stage";
        
        public void AssignUIElements(TextMeshProUGUI roundCounterText, TextMeshProUGUI roundStateText, TextMeshProUGUI roundTimeText)
        {
            _roundCounterText = roundCounterText;
            _roundStateText = roundStateText;
            _roundTimeText = roundTimeText;
        }

        public void Init()
        {
            var gameplayManager = GameClient.Get<IGameplayManager>();
            _roundEventController = gameplayManager.GetController<RoundEventController>();
            _gameEventsController = gameplayManager.GetController<GameEventController>();
            
            _roundEventController.OnRoundStarted += RoundStartedEventHandler;
            _roundEventController.OnRoundProcessing += RoundProcessingEventHandler;
            _roundEventController.OnRoundEnded += RoundEndedEventHandler;
            _gameEventsController.OnGetGameConfig += GetGameConfigEventHandler;
        }

        public void Update()
        {
        }

        public void ResetAll()
        {
            _timerTweener?.Kill();
        }

        public void Dispose()
        {
            _timerTweener?.Kill();
            _roundEventController.OnRoundStarted -= RoundStartedEventHandler;
            _roundEventController.OnRoundProcessing -= RoundProcessingEventHandler;
            _roundEventController.OnRoundEnded -= RoundEndedEventHandler;
        }
        
        private void GetGameConfigEventHandler(GameConfig data)
        {
            _roundTimeValue = data.roundTime;
        }

        private void RoundStartedEventHandler(RoundStartedData data)
        {
            /*IncrementRoundCounter();
            StartTimer();
            SetRoundState(Enumerators.RoundState.RoundStart);*/
        }

        private void IncrementRoundCounter()
        {
            _roundCounterText.text = $"Round: {_roundCounterValue++}";
        }

        private void RoundProcessingEventHandler(RoundProcessingData data)
        {
            /*StopHideTimer();
            SetRoundState(Enumerators.RoundState.RoundProcessing);*/
        }

        private void RoundEndedEventHandler()
        {
        }

        private void SetRoundState(Enumerators.RoundState roundState)
        {
            switch (roundState)
            {
                case Enumerators.RoundState.RoundStart:
                    _roundStateText.text = _startStateText;
                    break;
                case Enumerators.RoundState.RoundProcessing:
                    _roundStateText.text = _processingStateText;
                    break;
                case Enumerators.RoundState.RoundEnd:
                    break;
                case Enumerators.RoundState.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(roundState), roundState, null);
            }
        }

        private void StartTimer()
        {
            _currentRoundTime = _roundTimeValue;
            _roundTimeText.gameObject.SetActive(true);

            DOTween.To(() => _currentRoundTime, x =>
            {
                _currentRoundTime = x;
                _roundTimeText.text = $"Time: {_currentRoundTime}";
            }, 0, _roundTimeValue).SetEase(Ease.Linear);
        }

        private void StopHideTimer()
        {
            _timerTweener?.Pause();
            _roundTimeText.gameObject.SetActive(false);
        }
        
    }
}