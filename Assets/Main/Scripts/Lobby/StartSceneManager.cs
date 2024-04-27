using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;

namespace DoubleHeat.SnowFightForDucksGame {

    public class StartSceneManager : SingletonMonoBehaviour<StartSceneManager> {

        public enum Stage {
            Welcome,
            MainMenu,
            PreparingRoom,
            Loading
        }

        public Camera mainCam;

        public WelcomePanelManager  welcomePanel;
        public MainMenuPanelManager mainMenuPanel;
        public PreparingRoom preparingRoom;

        public GameObject forcedOverlayBackboard;
        public GameObject connectingPanel;
        public GameObject loadingPanel;

        public GameObject overlayBackboard;
        public GameObject initialMessagePanel;
        public GameObject exitGameComfirmPanel;
        public GameObject creditPanel;
        public GameObject controllingInstructPanel;
        public GameObject changePlayerNamePanel;
        public GameObject joinOnlineRoomPanel;
        public GameObject unbalancedStartCorfirmPanel;
        public GameObject leaveRoomConfirmPanel;
        public GameObject kickPlayerConfirmPanel;

        public Text gameVersionInfoText;
        public Text joinOnlineRoomErrorMessageText;


        public bool IsAnyAdditionalPanelActive {
            get {
                foreach (var panel in _additionalPanels) {
                    if (panel.activeSelf)
                        return true;
                }
                return false;
            }
        }


        GameObject[] _panels;
        GameObject[] _additionalPanels;
        GameObject[] _forcedOverlayPanels;

        Stage _currentStage = Stage.Loading;

        int _playerKickTarget = -1;


        protected override void Awake () {
            base.Awake();

            Global.startSceneManager = this;

            gameVersionInfoText.text = "Version: " + Application.version;
            gameVersionInfoText.enabled = false;

            _panels = new GameObject[] {
                welcomePanel.gameObject,
                mainMenuPanel.gameObject,
                preparingRoom.gameObject,
                connectingPanel,
                loadingPanel
            };

            _additionalPanels = new GameObject[] {
                initialMessagePanel,
                exitGameComfirmPanel,
                creditPanel,
                controllingInstructPanel,
                changePlayerNamePanel,
                joinOnlineRoomPanel,
                unbalancedStartCorfirmPanel,
                leaveRoomConfirmPanel,
                kickPlayerConfirmPanel
            };

            _forcedOverlayPanels = new GameObject[] {
                connectingPanel,
                loadingPanel
            };

            CloseAllAdditionalPanel();
            SetActiveForcedOverlayPanel(null);
        }

        protected override void OnDestroy () {
            base.OnDestroy();
            Global.startSceneManager = null;
        }

        void Start () {

            Init();

            if (PhotonNetwork.InRoom) {
                NetEvent.LeaveRoom();
            }

        }

        void Update () {

            if (Input.GetKeyDown(Global.currentInputMethodKeys.back)) {

                if (IsAnyAdditionalPanelActive)
                    CloseAllAdditionalPanel();

                else {
                    if (!forcedOverlayBackboard.activeSelf) {

                        if (_currentStage == Stage.MainMenu)
                            mainMenuPanel.Back();
                        else if (_currentStage == Stage.PreparingRoom)
                            AttemptToLeaveRoom();
                    }
                }
            }

        }


        void Init () {
            if (Global.isFirstInto && !Global.globalManager.skipWelcomeStage)
                SwitchStage(Stage.Welcome);
            else
                SwitchStage(Stage.MainMenu);
        }

        void SetActivePanel (GameObject activePanel) {
            Global.SetActiveOne(_panels, activePanel);
        }


        public void SetActiveAdditionalPanel (GameObject activePanel) {
            Global.SetActiveOne(_additionalPanels, activePanel);
            overlayBackboard.SetActive(activePanel != null);
        }

        public void SetActiveForcedOverlayPanel (GameObject activePanel) {
            Global.SetActiveOne(_forcedOverlayPanels, activePanel);
            forcedOverlayBackboard.SetActive(activePanel != null);
        }


        void SwitchStage (Stage stage) {
            _currentStage = stage;

            if (stage == Stage.Welcome) {
                SetActivePanel(welcomePanel.gameObject);

            }
            else if (stage == Stage.MainMenu) {
                SetActivePanel(mainMenuPanel.gameObject);

                gameVersionInfoText.enabled = true;

                if (Global.isFirstInto) {

                    if (Global.globalManager.enableInitMessage)
                        SetActiveAdditionalPanel(initialMessagePanel);

                    Global.isFirstInto = false;
                }
            }
            else if (stage == Stage.PreparingRoom) {
                SetActivePanel(preparingRoom.gameObject);

            }
            else if (stage == Stage.Loading) {
                SetActivePanel(loadingPanel);

            }

        }


        // === Public Methods ===
        public void CloseAllAdditionalPanel () {
            SetActiveAdditionalPanel(null);
        }

        public void PassWelcomeStage() {
            SwitchStage(Stage.MainMenu);
        }



        public void AttemptToExitGame () {
            SetActiveAdditionalPanel(exitGameComfirmPanel);
        }

        public void ConfirmToExitGame () {
            Global.globalManager.ExitGame();
        }

        public void OpenCreditPanel () {
            SetActiveAdditionalPanel(creditPanel);
        }

        public void OpenControllingInstructPanel () {
            SetActiveAdditionalPanel(controllingInstructPanel);
        }

        public void GoSinglePlayerFreeMode () {
            NetEvent.GoOfflineMode();
        }


        public void ConnectOnline () {
            SetActiveForcedOverlayPanel(connectingPanel);
            NetEvent.Connect();
        }

        public void CreateRoom () {
            SetActiveAdditionalPanel(connectingPanel);
            NetEvent.CreateRoom();
        }

        public void JoinRoom (string roomName) {
            NetEvent.JoinRoom(roomName);
        }

        public void AttemptToLeaveRoom () {
            SetActiveAdditionalPanel(leaveRoomConfirmPanel);
        }

        public void ConfirmLeaveRoom () {
            CloseAllAdditionalPanel();
            SetActivePanel(null);
            SetActiveForcedOverlayPanel(loadingPanel);
            NetEvent.LeaveRoom();
        }


        public void AttemptToKickPlayer (int playerNumber) {
            _playerKickTarget = playerNumber;
            SetActiveAdditionalPanel(kickPlayerConfirmPanel);
        }

        public void ComfirmToKickPlayer () {
            if (_playerKickTarget != -1) {
                NetEvent.KickPlayer(_playerKickTarget);
                _playerKickTarget = -1;
            }
            CloseAllAdditionalPanel();
        }

        public void AttemptToStartGame (bool isUnbalanced) {
            if (isUnbalanced) {
                SetActiveAdditionalPanel(unbalancedStartCorfirmPanel);
            }
            else {
                ConfirmToStartGame();
            }
        }

        public void ConfirmToStartGame () {
            NetEvent.StartGame();
        }


        public void MultiplayerNameSuccessfullySet () {
            CloseAllAdditionalPanel();
            mainMenuPanel.UpdateMultiplayerNameDisplay();
        }



        // == OnEvents ==
        public void OnConnectedToMasterOffline () {
            SetActiveForcedOverlayPanel(null);
            CreateRoom();
        }

        public void OnConnectedToMaster () {
            SetActiveForcedOverlayPanel(null);
            mainMenuPanel.OnConnectedToMaster();
        }

        public void OnCreateRoomFailed () {
            CloseAllAdditionalPanel();
        }

        public void OnJoinRoomFailed (string message) {
            SetActiveForcedOverlayPanel(null);
            joinOnlineRoomErrorMessageText.gameObject.SetActive(true);
            joinOnlineRoomErrorMessageText.enabled = true;
            joinOnlineRoomErrorMessageText.text = message;
        }

        public void OnJoinedRoom () {
            CloseAllAdditionalPanel();
            SetActiveForcedOverlayPanel(null);
            SwitchStage(Stage.PreparingRoom);

            NetEvent.SetInitTeamForJoinedLocalPlayer();

        }

        public void OnLeftRoom () {
            if (PhotonNetwork.OfflineMode)
                PhotonNetwork.OfflineMode = false;

            SetActiveForcedOverlayPanel(null);
            Init();

            mainMenuPanel.OnLeftRoom();
        }

        public void OnDisconnected () {
            joinOnlineRoomPanel.SetActive(false);
            SetActiveForcedOverlayPanel(null);
            SwitchStage(Stage.MainMenu);
            mainMenuPanel.OnDisconnected();
        }

    }
}
