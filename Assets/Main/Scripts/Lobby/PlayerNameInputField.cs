using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using DG.Tweening;

using Photon.Pun;
using System.Collections;

namespace DoubleHeat.SnowFightForDucksGame {

    [RequireComponent(typeof(InputField))]
    public class PlayerNameInputField : MonoBehaviour {

        public Text errorMessage;

        InputField _inputField;

        void Awake () {

            errorMessage.text = "";

            _inputField = gameObject.GetComponent<InputField>();
            _inputField.text = PhotonNetwork.NickName;
        }



        public void SetPlayerName (string name) {

            if (string.IsNullOrEmpty(name)) {

                print("Invalid Name");
                return;
            }

            errorMessage.text = "";
            PhotonNetwork.NickName = name;

            PlayerPrefs.SetString(Global.PrefKeys.PLAYER_NAME, name);

            Global.startSceneManager.MultiplayerNameSuccessfullySet();
        }

    }
}
