using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Photon.Pun;

using DoubleHeat.Animation;
using DoubleHeat.Utilities;

namespace DoubleHeat.SnowFightForDucksGame {

    public class SkinShowingUnit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        static readonly Vector2 nonWhiteDuckShiftCompensation    = new Vector2(0f, -40.5f);
        static readonly Vector2 animShiftCompensationFacingRight = new Vector2(-3.7f, -5.6f);
        static readonly Vector2 animShiftCompensationFacingLeft  = new Vector2(3.7f, -5.6f);

        public Transform playerNameDisplayPositionTrans;
        public FlippableUIElementsManager flippableElementsManager;
        public Image image;
        public GameObject switchTeamButtonGO;
        public GameObject kickButtonGO;

        public SeqImgAnim.FixedFrameRateAnimProperties animProps;

        public string   DisplayName     => _displayName;
        public DuckSkin CurrentDuckSkin => _currentDuckSkin;
        public bool     IsLocalPlayer   => (_playerNumber == PhotonNetwork.LocalPlayer.ActorNumber);


        public RectTransform ImageRectTrans => (RectTransform) image.transform;


        GameObject _nameDisplayGO;

        bool      _inited = false;
        int       _playerNumber = -1;
        string    _displayName = "";
        DuckSkin  _currentDuckSkin;
        Coroutine _currentPlayingAnim;
        Vector2   _currentAnimShiftCompensation = Vector2.zero;

        void Awake () {
            kickButtonGO.SetActive(false);
            switchTeamButtonGO.SetActive(false);
        }

        public void Init (int playerNumber, DuckSkin duckSkin) {
            _playerNumber = playerNumber;

            _displayName = PhotonNetwork.CurrentRoom.Players[playerNumber].NickName;

            SetDuckSkin(duckSkin);

            if (!IsLocalPlayer) {
                _nameDisplayGO = Global.startSceneManager.preparingRoom.playersNameDisplayManager.Register(_displayName, playerNameDisplayPositionTrans);
            }

            _inited = true;
        }

        void OnEnable () {
            if (_nameDisplayGO != null)
                _nameDisplayGO.SetActive(true);
        }

        void OnDisable () {
            if (_nameDisplayGO != null)
                _nameDisplayGO.SetActive(false);
        }


        void OnDestroy () {
            if (_currentPlayingAnim != null)
                StopCoroutine(_currentPlayingAnim);

            if (_nameDisplayGO != null)
                Destroy(_nameDisplayGO);
        }


        public void CorrectTransforms () {

            flippableElementsManager.CorrectRotation();

            if (transform.localRotation == Quaternion.identity)
                _currentAnimShiftCompensation = animShiftCompensationFacingRight;
            else
                _currentAnimShiftCompensation = animShiftCompensationFacingLeft;

            SetDuckSkin(_currentDuckSkin, true);
        }



        public void SetDuckSkin (DuckSkin duckSkin, bool forceReplayAnim = false) {
            if (!forceReplayAnim && _inited && duckSkin == _currentDuckSkin && _currentPlayingAnim != null)
                return;

            _currentDuckSkin = duckSkin;

            if (_currentPlayingAnim != null)
                StopCoroutine(_currentPlayingAnim);

            if (duckSkin == DuckSkin.White)
                ImageRectTrans.anchoredPosition = Vector2.zero;
            else
                ImageRectTrans.anchoredPosition = nonWhiteDuckShiftCompensation;

            Sprite[] animSprites = Global.startSceneManager.preparingRoom.showingAnimSpritesOfSkins[duckSkin];
            Vector2[] anchoredPositions = new Vector2[] { ImageRectTrans.anchoredPosition, ImageRectTrans.anchoredPosition + _currentAnimShiftCompensation };
            _currentPlayingAnim = StartCoroutine(SeqImgAnim.ImgAnim(image, animSprites, true, anchoredPositions, animProps.fps, animProps.loop, animProps.pingPong, null));
        }


        public void LocalPlayerSwitchTeam () {
            if (IsLocalPlayer)
                Global.startSceneManager.preparingRoom.SwitchTeam();
        }

        public void PlayerKicked () {
            Global.startSceneManager.AttemptToKickPlayer(_playerNumber);
        }





        public void OnPointerEnter (PointerEventData eventData) {
            if (IsLocalPlayer) {
                switchTeamButtonGO.SetActive(true);
            }
            else if (PhotonNetwork.IsMasterClient) {
                kickButtonGO.SetActive(true);
            }
        }

        public void OnPointerExit (PointerEventData eventData) {
            if (IsLocalPlayer) {
                switchTeamButtonGO.SetActive(false);
            }
            else if (PhotonNetwork.IsMasterClient) {
                kickButtonGO.SetActive(false);
            }
        }

    }
}
