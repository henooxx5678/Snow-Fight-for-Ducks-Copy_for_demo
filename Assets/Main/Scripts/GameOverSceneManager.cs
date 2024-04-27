using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;

using DoubleHeat;
using DoubleHeat.Utilities;

namespace DoubleHeat.SnowFightForDucksGame {

    public class GameOverSceneManager : SingletonMonoBehaviour<GameOverSceneManager> {


        public Image backgroundImage;
        public Text  winLoseText;

        public Sprite winBackgroundMagical;
        public Sprite winBackgroundUnicorn;
        public Sprite drawBackground;

        public float backgroundXMagical;
        public float backgroundXUnicorn;
        public float backgroundXDraw;

        public string winWords;
        public string loseWords;
        public string drawWords;

        [Header("Result Items")]
        public GameObject resultItemPrefab;
        public Transform resultItemsParent;
        public float resultItemsIntervalDistance;


        protected override void Awake () {
            base.Awake();
            
            Global.gameOverSceneManager = this;
        }

        void Start () {

            NetEvent.LeaveRoom();

            if (Global.gameResultHandler != null) {

                Global.gameResultHandler.transform.SetParent(transform);  // Destroy Together


                byte winTeam = Global.gameResultHandler.WinTeam;
                byte playerTeam = NetEvent.GetPlayerTeam(NetEvent.GetCurrentPlayerInSeats(), PhotonNetwork.LocalPlayer.ActorNumber);

                if (winTeam == 0) {
                    InitShowing(winBackgroundMagical, backgroundXMagical, (winTeam == playerTeam) ? winWords : loseWords);
                }
                else if (winTeam == 1) {
                    InitShowing(winBackgroundUnicorn, backgroundXUnicorn, (winTeam == playerTeam) ? winWords : loseWords);
                }
                else {
                    // draw
                    InitShowing(drawBackground, backgroundXDraw, drawWords);
                }



            }
        }

        void InitShowing (Sprite backgroundSprite, float backgroundX, string winLoseWords) {

            backgroundImage.sprite = backgroundSprite;
            ((RectTransform) backgroundImage.transform).SetAnchoredPosX(backgroundX);
            backgroundImage.SetNativeSize();

            winLoseText.text = winLoseWords;

            SetResultTexts(PhotonNetwork.LocalPlayer.ActorNumber);
        }

        void SetResultTexts (int playerNumber) {

            Dictionary<string, string> resultsStringOfName = Global.gameResultHandler.GetResultsStringOfName(playerNumber);

            string[] itemsName = resultsStringOfName.Keys.ToArray();

            for (int i = 0 ; i < itemsName.Length ; i++) {

                GameObject resultItem = Instantiate(resultItemPrefab, resultItemsParent);
                resultItem.GetComponent<RectTransform>().anchoredPosition = i * resultItemsIntervalDistance * Vector2.down;

                resultItem.GetComponent<TextMeshProUGUI>().text = string.Format("<align=left>{0}<line-height=0>\n<align=right>{1}<line-height=1em>", itemsName[i], resultsStringOfName[itemsName[i]]);
            }
        }


        public void Exit () {
            UnityEngine.SceneManagement.SceneManager.LoadScene(Global.SceneNames.START);
        }

    }
}
