using Enum = System.Enum;
using Math = System.Math;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;
using Photon.Pun;

using DoubleHeat.Animation;
using DoubleHeat.Utilities;

namespace DoubleHeat.SnowFightForDucksGame {

    public class PreparingRoom : MonoBehaviour {

        public PlayersNameDisplayManager  playersNameDisplayManager;
        public FlippableUIElementsManager flippableElementsManager;
        public Transform   duckShowingMainTrans;
        public Transform[] duckShowingTeammatesTrans;
        public Transform[] duckShowingOpponentsTrans;

        public Image    roomImage;
        public Sprite[] roomImageSprites;
        public Image    showingWordsImage;
        public Image    switchArrowLeftImage;
        public Image    switchArrowRightImage;
        public Sprite[] switchArrowAnimSprites;
        public Text     roomNameCopiedMessageText;

        public Button   startGameButton;

        [Header("Prefab")]
        public GameObject skinShowingUnitPrefab;

        [Header("Animation Properties")]
        public float showingAnimDuration;
        public float showingAnimShiftDistance;
        public Ease  showingEase;
        public SeqImgAnim.FixedFrameRateAnimProperties switchArrowAnimProps;
        public float roomNameCopiedMessageAnimDuration;
        public float roomNameCopiedMessageAnimShiftDistance;


        public Dictionary<DuckSkin, Sprite[]> showingAnimSpritesOfSkins = new Dictionary<DuckSkin, Sprite[]>();
        public Dictionary<DuckSkin, Sprite>   showingWordsSpriteOfSkins = new Dictionary<DuckSkin, Sprite>();

        DuckSkin[]    _duckSkins;
        Transform[]   _teamsMainTrans      = new Transform[2];
        Transform[][] _teamsTeammatesTrans = new Transform[2][];
        Transform[][] _teamsOpponentsTrans = new Transform[2][];

        Dictionary<int, SkinShowingUnit> _skinShowingUnitOfPlayers = new Dictionary<int, SkinShowingUnit>();

        Coroutine _currentSwitchArrowAnim;
        Sequence  _currentRoomNameCopiedMessageAnimSeq;
        bool      _hasShown = false;
        int       _localPlayerCurrentSkinIndex = 0;

        void Awake () {
            Global.startSceneManager.preparingRoom = this;

            _duckSkins = (DuckSkin[]) Enum.GetValues(typeof(DuckSkin));
            LoadSkinShowingSprites();

            roomNameCopiedMessageText.color = roomNameCopiedMessageText.color.GetAfterSetA(0f);
        }

        void LoadSkinShowingSprites () {

            Sprite[] wordsSprites = Resources.LoadAll<Sprite>("Sprites/UIs/Skin Showing Words");

            foreach (DuckSkin skin in _duckSkins) {

                foreach (Sprite sprite in wordsSprites) {
                    if (sprite.name.Contains(skin.ToString()))
                        showingWordsSpriteOfSkins.Add(skin, sprite);
                }


                Sprite[] duckSprites = Resources.LoadAll<Sprite>("Sprites/Players/" + PlayerAnimationManager._DuckSkinResourceFileName[skin]);

                List<Sprite> animSpritesList = new List<Sprite>();

                foreach (Sprite sprite in duckSprites) {
                    if (sprite.name.Contains("SkinShowing"))
                        animSpritesList.Add(sprite);
                }

                showingAnimSpritesOfSkins.Add(skin, animSpritesList.ToArray());
            }
        }

        void OnEnable () {

            // switch arrow
            for (int i = 0 ; i < 2 ; i++) {
                Image img = switchArrowLeftImage;
                if (i == 1)
                    img = switchArrowRightImage;

                _currentSwitchArrowAnim = StartCoroutine(SeqImgAnim.ImgAnim(img, switchArrowAnimSprites, false, null, switchArrowAnimProps.fps, switchArrowAnimProps.loop, switchArrowAnimProps.pingPong, null));
            }
        }

        void OnDisable () {
            _hasShown = false;
        }


        void ShowUp () {
            gameObject.GetComponent<RectTransform>().DOAnchorPosY(showingAnimShiftDistance, showingAnimDuration)
                .From()
                .SetEase(showingEase);

            _hasShown = true;
        }


        void SetLocalPlayerDuckSkin (int skinIndex) {
            PlayerPrefs.SetInt(Global.PrefKeys.DUCK_SKIN_INDEX, skinIndex);

            _localPlayerCurrentSkinIndex = skinIndex;

            UpdateLocalPlayerDuckSkin();
            NetEvent.SetLocalPlayerDuckSkin(_duckSkins[_localPlayerCurrentSkinIndex]);
        }



        public void UpdatePlayersSeat () {

            if (PhotonNetwork.IsMasterClient)
                startGameButton.gameObject.SetActive(true);
            else
                startGameButton.gameObject.SetActive(false);


            int[,] playerInSeats = NetEvent.GetCurrentPlayerInSeats();

            byte currentTeam = NetEvent.GetPlayerTeam(playerInSeats, PhotonNetwork.LocalPlayer.ActorNumber);

            roomImage.sprite = roomImageSprites[currentTeam];

            if (currentTeam == 0)
                flippableElementsManager.transform.rotation = Quaternion.identity;
            else
                flippableElementsManager.transform.rotation = Global.horizontalFlipRotation;


            List<int> noLongerExistPlayersNumber = new List<int>();
            foreach (var playerNumber in _skinShowingUnitOfPlayers.Keys) {
                noLongerExistPlayersNumber.Add(playerNumber);
            }

            bool isAfterLocalPlayer = false;

            for (byte team = 0 ; team < playerInSeats.GetLength(0) ; team++) {
                for (int i = 0 ; i < playerInSeats.GetLength(1) ; i++) {

                    if (playerInSeats[team, i] != -2) {

                        int playerNumber = playerInSeats[team, i];
                        bool isLocalPlayer = playerNumber == PhotonNetwork.LocalPlayer.ActorNumber;

                        if (_skinShowingUnitOfPlayers.Keys.Contains(playerNumber)) {

                            noLongerExistPlayersNumber.Remove(playerNumber);

                        }
                        else {
                            SkinShowingUnit skinShowingUnit = Instantiate(skinShowingUnitPrefab).GetComponent<SkinShowingUnit>();
                            skinShowingUnit.Init(playerNumber, NetEvent.GetPlayerDuckSkin(playerNumber));
                            _skinShowingUnitOfPlayers.Add(playerNumber, skinShowingUnit);

                            if (isLocalPlayer)
                                SetLocalPlayerDuckSkin(PlayerPrefs.GetInt(Global.PrefKeys.DUCK_SKIN_INDEX));

                        }

                        if (isLocalPlayer) {
                            // local player
                            isAfterLocalPlayer = true;
                            _skinShowingUnitOfPlayers[playerNumber].transform.SetParent(duckShowingMainTrans, false);
                        }
                        else {
                            if (team == currentTeam) {
                                // teammate
                                int seatPos = isAfterLocalPlayer ? i - 1 : i;
                                _skinShowingUnitOfPlayers[playerNumber].transform.SetParent(duckShowingTeammatesTrans[seatPos], false);

                            }
                            else {
                                // opponent
                                _skinShowingUnitOfPlayers[playerNumber].transform.SetParent(duckShowingOpponentsTrans[i], false);
                            }
                        }

                        flippableElementsManager.CorrectRotation();
                        _skinShowingUnitOfPlayers[playerNumber].CorrectTransforms();
                    }

                }
            }

            foreach (int playerNumber in noLongerExistPlayersNumber) {

                Destroy(_skinShowingUnitOfPlayers[playerNumber].gameObject);
                _skinShowingUnitOfPlayers.Remove(playerNumber);
            }

            foreach (int playerNumber in _skinShowingUnitOfPlayers.Keys) {
                UpdatePlayerDuckSkin(playerNumber);
            }

            if (!_hasShown)
                ShowUp();
        }


        public void UpdateLocalPlayerDuckSkin () {
            showingWordsImage.sprite = showingWordsSpriteOfSkins[_duckSkins[_localPlayerCurrentSkinIndex]];
            _skinShowingUnitOfPlayers[PhotonNetwork.LocalPlayer.ActorNumber].SetDuckSkin(_duckSkins[_localPlayerCurrentSkinIndex]);
        }

        public void UpdatePlayerDuckSkin (int playerNumber) {
            if (playerNumber == PhotonNetwork.LocalPlayer.ActorNumber) {
                UpdateLocalPlayerDuckSkin();
            }
            else {
                DuckSkin skin = NetEvent.GetPlayerDuckSkin(playerNumber);
                _skinShowingUnitOfPlayers[playerNumber].SetDuckSkin(skin);
            }
        }


        // === Player Actions ===
        public void AttemptToLeaveRoom () {
            Global.startSceneManager.AttemptToLeaveRoom();
        }


        public void SwitchTeam () {
            NetEvent.LocalPlayerTryToSwitchTeam();
        }

        public void SwitchSkin (int dir) {
            SetLocalPlayerDuckSkin((_duckSkins.Length + _localPlayerCurrentSkinIndex + dir) % _duckSkins.Length);
        }


        public void AttemptToStartGame () {
            bool isUnbalanced = false;

            if (!PhotonNetwork.OfflineMode) {
                int[] counts = NetEvent.GetPlayersCountOfTeams(NetEvent.GetCurrentPlayerInSeats());
                isUnbalanced = counts[0] != counts[1];
            }

            Global.startSceneManager.AttemptToStartGame(isUnbalanced);
        }



        public void CopyRoomNameToClipboard () {
            GUIUtility.systemCopyBuffer = PhotonNetwork.CurrentRoom.Name;

            if (_currentRoomNameCopiedMessageAnimSeq == null) {

                _currentRoomNameCopiedMessageAnimSeq = DOTween.Sequence().Append( roomNameCopiedMessageText.DOFade(1f, roomNameCopiedMessageAnimDuration)
                    .From()
                    .SetEase(Ease.InQuint)
                )
                .Join( roomNameCopiedMessageText.transform.DOMoveY(-roomNameCopiedMessageAnimShiftDistance, roomNameCopiedMessageAnimDuration)
                    .From(true)
                )
                .OnKill(() => _currentRoomNameCopiedMessageAnimSeq = null);

            }
            else {
                _currentRoomNameCopiedMessageAnimSeq.Restart();
            }
        }

    }
}
