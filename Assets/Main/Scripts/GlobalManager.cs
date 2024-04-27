using System.Linq;

using UnityEngine;

using UniRx;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

namespace DoubleHeat.SnowFightForDucksGame {

    public class GlobalManager : MonoBehaviourPunCallbacks {

        public static GlobalManager current = null;


        public bool skipWelcomeStage;
        public bool enableInitMessage;

        public bool infiniteFiring;
        public bool infiniteBuilding;
        public bool infiniteRepairing;
        public bool permanentDuckSnowCake;

        [Range(1, 8)]
        public int   maxPlayersAmount;
        public float positionVerticalScale;
        public int   cardsAmountPerVoting;
        public float lagTimeTolerance;

        public OptionalRule[] bannedOptionalRules;


        void Awake () {

            // Singleton Keep Old
            if (current != null) {
                Destroy(gameObject);
                return;
            }
            current = this;

            if (Global.globalManager != null) {
                Destroy(gameObject);
                return;
            }
            Global.globalManager = this;


            DontDestroyOnLoad(gameObject);

        }

        void Start () {

            if (maxPlayersAmount > Global.PLAYERS_AMOUNT_LIMIT)
                maxPlayersAmount = Global.PLAYERS_AMOUNT_LIMIT;

            Global.InitInlineIcons();

            DOTween.Init();
            DOTween.defaultTimeScaleIndependent = true;
            DOTween.showUnityEditorReport = true;

            PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate = 0.5f;

            int keyboardMouseLayoutIndex = 0;

            if (PlayerPrefs.HasKey(Global.PrefKeys.KEYBOARD_MOUSE_LAYOUT_INDEX))
                keyboardMouseLayoutIndex = PlayerPrefs.GetInt(Global.PrefKeys.KEYBOARD_MOUSE_LAYOUT_INDEX);

            if (keyboardMouseLayoutIndex >= InputMethodKeys.keyboardMouseLayout.Length)
                keyboardMouseLayoutIndex = 0;

            Global.currentInputMethodKeys = InputMethodKeys.keyboardMouseLayout[keyboardMouseLayoutIndex];


            // -- Player Name --
            string defaultName = "Unknown Player";

            if (PlayerPrefs.HasKey(Global.PrefKeys.PLAYER_NAME)) {
                defaultName = PlayerPrefs.GetString(Global.PrefKeys.PLAYER_NAME);
            }

            PhotonNetwork.NickName = defaultName;
        }


        void Update () {
            if (Input.GetKeyDown(KeyCode.Keypad0)) {
                Screen.SetResolution(1920, 1080, true);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad1)) {
                Screen.SetResolution(1280, 720, false);
            }
        }



        /// ===== Callbacks =====

        // == General Connection ==
        public override void OnConnectedToMaster () {

            NetEvent.InitPlayerCustomProperties();

            if (PhotonNetwork.OfflineMode) {
                print("connected to offline master");

                if (Global.startSceneManager != null)
                    Global.startSceneManager.OnConnectedToMasterOffline();
            }
            else {
                print("Connected to Master");

                if (Global.startSceneManager != null)
                    Global.startSceneManager.OnConnectedToMaster();
            }
        }

        public override void OnDisconnected (DisconnectCause cause) {
            print("Disconnected");

            if (Global.startSceneManager != null)
                Global.startSceneManager.OnDisconnected();
        }

        public override void OnCreateRoomFailed (short returnCode, string message) {
            print("create room failed: " + message);

            if (Global.startSceneManager != null)
                Global.startSceneManager.OnCreateRoomFailed();
        }

        public override void OnJoinRoomFailed (short returnCode, string message) {
            print("joined room failed: " + message);

            if (Global.startSceneManager != null)
                Global.startSceneManager.OnJoinRoomFailed(message);
        }


        public override void OnJoinedRoom () {
            print("joined room: " + PhotonNetwork.CurrentRoom.Name);

            PhotonNetwork.AutomaticallySyncScene = true;

            if (Global.startSceneManager != null)
                Global.startSceneManager.OnJoinedRoom();
        }

        public override void OnLeftRoom () {
            print("left room");

            PhotonNetwork.AutomaticallySyncScene = false;

            if (Global.startSceneManager != null)
                Global.startSceneManager.OnLeftRoom();
        }


        // == In Room ==
        public override void OnPlayerEnteredRoom (Player player) {

        }

        public override void OnPlayerLeftRoom (Player otherPlayer) {
            NetEvent.RemoveLeftPlayersFromSeat();
        }

        public override void OnRoomPropertiesUpdate (Hashtable props) {

            foreach (object propKey in props.Keys) {

                if (propKey.ToString() == NetEvent.RoomCustomPropKeys.PLAYERS_IN_SEAT_WHEN_PREPARING) {

                    if (Global.startSceneManager != null && Global.startSceneManager.preparingRoom != null)
                        Global.startSceneManager.preparingRoom.UpdatePlayersSeat();
                }
                // else if (propKey.ToString() == NetEvent.RoomCustomPropKeys.STATUES_HP) {
                //
                //     if (Global.CurrentRoundInstance != null) {
                //         Global.gameSceneManager.UpdateStatuesHPFromNetwork(DataCompression.ByteArrayToIntArray((byte[]) props[propKey]));
                //     }
                // }
            }
        }

        public override void OnPlayerPropertiesUpdate (Player player, Hashtable props) {
            foreach (string propKey in props.Keys) {
                if (propKey == NetEvent.PlayerCustomPropKeys.DUCK_SKIN) {

                    if (Global.startSceneManager != null && Global.startSceneManager.preparingRoom != null)
                        Global.startSceneManager.preparingRoom.UpdatePlayerDuckSkin(player.ActorNumber);
                }
            }
        }




        public void ExitGame () {
            print("QUIT!!");
            Application.Quit();
        }

    }
}
