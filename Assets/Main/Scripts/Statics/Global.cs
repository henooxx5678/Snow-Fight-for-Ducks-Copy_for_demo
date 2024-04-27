using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

namespace DoubleHeat.SnowFightForDucksGame {

    public enum InputDevice {
        KeyboardMouse,
        XboxOneController,
        PS4Controller,
        UnknownController
    }

    public static class Global {


        public static class SceneNames {
            public const string START     = "StartScene";
            public const string GAME      = "GameScene";
            public const string GAME_OVER = "GameOverScene";
        }


        public static class PrefKeys {
            public const string PLAYER_NAME = "PlayerName";
            public const string KEYBOARD_MOUSE_LAYOUT_INDEX = "KeyboardMouseLayoutIndex";
            public const string DUCK_SKIN_INDEX = "DuckSkinIndex";
        }

        public const int PROJECTILE_LAYER = 12;
        public const int OWNED_PROJECTILE_LAYER = 13;


        public const byte PLAYERS_AMOUNT_LIMIT = 8;
        public const int PLAYER_MAX_CARRIED_AMOUNT = 3;
        public const int STATUE_AMOUNT_PER_TEAM = 3;
        public const int STATUE_MAX_HP = 3;
        public const int SNOW_WALL_MAX_HP = 2;

        public static readonly Quaternion horizontalFlipRotation = new Quaternion(0f, 1f, 0f, 0f);


        public static GlobalManager        globalManager;
        public static StartSceneManager    startSceneManager;
        public static GameSceneManager     gameSceneManager;
        public static GameOverSceneManager gameOverSceneManager;
        public static GameResultHandler    gameResultHandler;

        public static string currentLanguageMark = "ZH";
        public static InputDevice currentInputDevice = InputDevice.KeyboardMouse;

        public static Dictionary<KeyCode, string> inlineIconKeyboardMouseCorrespondence = new Dictionary<KeyCode, string>();
        public static Dictionary<KeyCode, string> inlineIconXboxOneControllerCorrespondence = new Dictionary<KeyCode, string>();


        public static RoundInstance CurrentRoundInstance => gameSceneManager.currentRoundInstance;
        public static VotingInstance CurrentVotingInstance => gameSceneManager.currentVotingInstance;


        public static Camera mainCam;


        public static InputMethodKeys currentInputMethodKeys;
        public static bool isFirstInto = true;


        static bool _inlineIconInited = false;

        public static void InitInlineIcons () {
            if (_inlineIconInited)
                return;

            inlineIconKeyboardMouseCorrespondence = new Dictionary<KeyCode, string>() {
                { KeyCode.Alpha1, "1" },
                { KeyCode.Alpha2, "2" },
                { KeyCode.Alpha3, "3" },
                { KeyCode.Alpha4, "4" },
                { KeyCode.W, "W" },
                { KeyCode.A, "A" },
                { KeyCode.S, "S" },
                { KeyCode.D, "D" },
                { KeyCode.Q, "Q" },
                { KeyCode.E, "E" },
                { KeyCode.Space, "Space" },
                { KeyCode.LeftShift, "LeftShift" },
                { KeyCode.Return, "Enter" },
                { KeyCode.Tab, "Tab" },
                { KeyCode.Mouse0, "MouseLB" },
                { KeyCode.Mouse1, "MouseRB" }
            };

            // inlineIconXboxOneControllerCorrespondence = new Dictionary<KeyCode, string>() {
            //     { KeyCode. }
            // };

            _inlineIconInited = true;
        }


        public static Vector3 ApplyZToVector (Vector3 v3, float z) {
            return new Vector3(v3.x, v3.y, z);
        }

        public static Vector3 GetActualWorldPosition (Vector2 positionOnGround) {
            Vector3 pos = positionOnGround;
            pos.y *= globalManager.positionVerticalScale;
            pos.z = pos.y;
            return pos;
        }
        public static Vector2 GetActualWorldDirection (Vector2 directionOnGround) {
            Vector2 dir = directionOnGround;
            dir.y *= globalManager.positionVerticalScale;
            return dir;
        }

        public static Vector2 GetPositionOnGround (Vector3 worldPosition) {
            Vector2 pos = worldPosition;
            pos.y /= globalManager.positionVerticalScale;
            return pos;
        }

        public static Vector3 GetPositionWithDepth (Vector3 pos) {
            pos.z = pos.y;
            return pos;
        }

        public static Vector3 GetMouseWorldPosition () {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0f;
            return pos;
        }

        public static GameObject GetPlayerInstanceInRoomByNumber (int playerNumber) {

            return (GameObject) (PhotonNetwork.CurrentRoom.GetPlayer(playerNumber).TagObject);
        }

        public static GameObject[] GetAllPlayersInstanceInRoom () {
            List<GameObject> resultList = new List<GameObject>();
            foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values) {
                resultList.Add((GameObject) player.TagObject);
            }
            return resultList.ToArray();
        }





        public static List<string> GetListOfRuleNames () {
            List<string> result = new List<string>();

            foreach (var rule in System.Enum.GetValues(typeof(FixedRule))) {
                result.Add(rule.ToString());
            }

            foreach (var rule in System.Enum.GetValues(typeof(OptionalRule))) {
                result.Add(rule.ToString());
            }

            return result;
        }

        public static string[] GetRulesName (FixedRule[] rules) {
            string[] result = new string[rules.Length];

            for (int i = 0 ; i < result.Length ; i++) {
                result[i] = rules[i].ToString();
            }

            return result;
        }

        public static string[] GetRulesName (OptionalRule[] rules) {
            string[] result = new string[rules.Length];

            for (int i = 0 ; i < result.Length ; i++) {
                result[i] = rules[i].ToString();
            }

            return result;
        }

        public static string[] GetRulesName (FixedRule[] fixedRules, OptionalRule[] optionalRules) {
            string[] result = new string[fixedRules.Length + optionalRules.Length];
            GetRulesName(fixedRules).CopyTo(result, 0);
            GetRulesName(optionalRules).CopyTo(result, fixedRules.Length);

            return result;
        }


        public static void SetActiveOne (GameObject[] all, GameObject activeOne) {
            foreach (var panel in all) {
                if (activeOne != null && panel == activeOne)
                    panel.SetActive(true);
                else
                    panel.SetActive(false);
            }
        }

        public static void SetActiveOnes (GameObject[] all, GameObject[] activeOnes) {
            foreach (var panel in all) {
                if (activeOnes.Contains(panel))
                    panel.SetActive(true);
                else
                    panel.SetActive(false);
            }
        }


    }


}
