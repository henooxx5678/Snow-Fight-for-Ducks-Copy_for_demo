using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UniRx;
using DG.Tweening;
using Photon.Pun;

using DoubleHeat.Utilities;

namespace DoubleHeat.SnowFightForDucksGame {

    public class MainMenuPanelManager : MonoBehaviour {

        public enum Page {
            Main,
            Multiplayer,
            OnlineMultiplayer
        }


        public RectTransform panelTrans;
        public CanvasGroup   canvasGroup;

        public GameObject mainPage;
        public GameObject multiplayerPage;
        public GameObject onlineMultiplayerPage;

        public Text multiplayerNameDisplay;

        [Header("Animation Properties")]
        public float showingAnimDuration;
        public float showingAnimShiftDistance;


        Dictionary<Page, GameObject> _panelOfPages;
        Page _currentPage;


        void Awake () {

            _panelOfPages = new Dictionary<Page, GameObject>() {
                { Page.Main, mainPage }  ,
                { Page.Multiplayer, multiplayerPage },
                { Page.OnlineMultiplayer, onlineMultiplayerPage }
            };
        }

        void OnEnable () {
            ShowUp();
        }

        void ShowUp () {

            if (PhotonNetwork.IsConnected)
                SwitchPage(Page.OnlineMultiplayer);
            else
                SwitchPage(Page.Main);
                

            DOTween.Sequence().Append( panelTrans.DOAnchorPosY(-showingAnimShiftDistance, showingAnimDuration)
                .From(true)
                .SetEase(Ease.OutCubic)
            )
            .Join( canvasGroup.DOFade(0f, showingAnimDuration)
                .From()
                .SetEase(Ease.OutQuint)
            );

        }


        void SwitchPage (Page page) {
            _currentPage = page;
            Global.SetActiveOne(_panelOfPages.Values.ToArray(), _panelOfPages[page]);
        }

        public void Back () {
            if (_currentPage == Page.Main) {
                Global.startSceneManager.AttemptToExitGame();
            }
            else if (_currentPage == Page.Multiplayer) {
                SwitchPage(Page.Main);
            }
            else if (_currentPage == Page.OnlineMultiplayer) {
                NetEvent.Disconnect();
                SwitchPage(Page.Multiplayer);
            }
        }


        // == Main Page ==
        public void GoMultiplayer () {
            UpdateMultiplayerNameDisplay();
            SwitchPage(Page.Multiplayer);
        }

        public void GoSinglePlayer () {
            Global.startSceneManager.GoSinglePlayerFreeMode();
        }

        // == Multiplayer Page ==
        public void GoOnlineMultiplayer () {
            Global.startSceneManager.ConnectOnline();
        }


        // == Online Multiplayer Page ==
        public void GoChangePlayerName () {
            Global.startSceneManager.SetActiveAdditionalPanel(Global.startSceneManager.changePlayerNamePanel);
        }

        public void GoCreateOnlineRoom () {
            Global.startSceneManager.CreateRoom();
        }

        public void GoJoinOnlineRoom () {
            Global.startSceneManager.SetActiveAdditionalPanel(Global.startSceneManager.joinOnlineRoomPanel);
            Global.startSceneManager.joinOnlineRoomErrorMessageText.text = "";
        }



        public void OnConnectedToMaster () {
            SwitchPage(Page.OnlineMultiplayer);
        }

        public void OnLeftRoom () {
            if (!PhotonNetwork.OfflineMode && PhotonNetwork.IsConnected)
                SwitchPage(Page.OnlineMultiplayer);
            else {
                SwitchPage(Page.Main);
            }
        }

        public void OnDisconnected () {
            if (!PhotonNetwork.OfflineMode)
                SwitchPage(Page.Multiplayer);
        }



        public void UpdateMultiplayerNameDisplay () {
            multiplayerNameDisplay.text = PhotonNetwork.NickName;
        }

    }
}
