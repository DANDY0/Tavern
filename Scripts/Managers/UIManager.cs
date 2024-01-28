using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public class UIManager : IService, IUIManager
    {
        private IDataManager _dataManager;

        private List<IUIElement> _uiPages;
        private List<IUIPopup> _uiPopups;

        public IUIElement CurrentPage { get; set; }

        public CanvasScaler CanvasScaler { get; set; }
        public GameObject Canvas { get; set; }

        public void Dispose()
        {
            foreach (var page in _uiPages)
                page.Dispose();

            foreach (var popup in _uiPopups)
                popup.Dispose();

            _dataManager.DataLoadedEvent -= DataLoadedEventHandler;
        }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();

            _dataManager.DataLoadedEvent += DataLoadedEventHandler;
        }

        private void DataLoadedEventHandler()
        {
            Canvas = GameObject.Find("Canvas");

            CanvasScaler = Canvas.GetComponent<CanvasScaler>();

            _uiPages = new List<IUIElement>();
            _uiPages.Add(new MainPage());
            _uiPages.Add(new GamePage());
            _uiPages.Add(new InventoryPage());
            _uiPages.Add(new MapPage());
            
            foreach (var page in _uiPages)
                page.Init();

            _uiPopups = new List<IUIPopup>();
            _uiPopups.Add(new LoadingPopup());
            _uiPopups.Add(new MatchFoundedPopup());
            _uiPopups.Add(new GameFinishedPopup());
            _uiPopups.Add(new TravelResultPopup());
            
            foreach (var popup in _uiPopups)
                popup.Init();
        }

        public void Update()
        {
            if (!_dataManager.DataIsLoaded)
                return;

            foreach (var page in _uiPages)
                page.Update();

            foreach (var popup in _uiPopups)
                popup.Update();
        }

        public void HideAllPages()
        {
            foreach (var _page in _uiPages)
            {
                _page.Hide();
            }
        }

        public void HideAllPopups()
        {
            foreach (var _popup in _uiPopups)
            {
                _popup.Hide();
            }
        }

        public void SetPage<T>(bool hideAll = false) where T : IUIElement
        {
            IUIElement previousPage = null;

            if (hideAll)
            {
                HideAllPages();
            }
            else
            {
                if (CurrentPage != null)
                {
                    CurrentPage.Hide();
                    previousPage = CurrentPage;
                }
            }

            foreach (var _page in _uiPages)
            {
                if (_page is T)
                {
                    CurrentPage = _page;
                    break;
                }
            }
            CurrentPage.Show();
        }

        public void DrawPopup<T>(object message = null, bool setMainPriority = false) where T : IUIPopup
        {
            IUIPopup popup = null;
            foreach (var _popup in _uiPopups)
            {
                if (_popup is T)
                {
                    popup = _popup;
                    break;
                }
            }

            if (setMainPriority)
                popup.SetMainPriority();

            if (message == null)
                popup.Show();
            else
                popup.Show(message);
        }

        public void HidePopup<T>() where T : IUIPopup
        {
            foreach (var _popup in _uiPopups)
            {
                if (_popup is T)
                {
                    _popup.Hide();
                    break;
                }
            }
        }

        public T GetPopup<T>() where T : IUIPopup
        {
            IUIPopup popup = null;
            foreach (var _popup in _uiPopups)
            {
                if (_popup is T)
                {
                    popup = _popup;
                    break;
                }
            }

            return (T)popup;
        }

        public T GetPage<T>() where T : IUIElement
        {
            IUIElement page = null;
            foreach (var _page in _uiPages)
            {
                if (_page is T)
                {
                    page = _page;
                    break;
                }
            }

            return (T)page;
        }

        public void FadeScreen(GameObject screen, bool fadeIn, Action callback = null)
		{
            CanvasGroup canvasGroup = screen.GetComponent<CanvasGroup>();

            if(canvasGroup == null)
                canvasGroup = screen.AddComponent<CanvasGroup>();

            canvasGroup.interactable = false;

            canvasGroup.alpha = fadeIn ? 0f : 1f;
            //canvasGroup.DOFade(fadeIn ? 1f : 0f, 0.5f).OnComplete(() =>
            //{
                canvasGroup.interactable = true;
                MonoBehaviour.Destroy(canvasGroup);
                callback?.Invoke();
            //});
        }
    }
}