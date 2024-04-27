using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class InGameMenuManager : MonoBehaviour {

        public GameObject mainMenu;
        public GameObject quitConfirm;



        void Start () {
            gameObject.SetActive(false);
        }

        public void Open () {
            gameObject.SetActive(true);
            mainMenu.SetActive(true);
            quitConfirm.SetActive(false);
        }

        public void Close () {
            gameObject.SetActive(false);
        }

        public void Switch () {
            if (gameObject.activeSelf)
                Close();
            else
                Open();
        }

        public void AttemptToQuit () {
            mainMenu.SetActive(false);
            quitConfirm.SetActive(true);
        }

        public void NotToQuit () {
            mainMenu.SetActive(true);
            quitConfirm.SetActive(false);
        }

        public void ConfirmToQuit () {
            UnityEngine.SceneManagement.SceneManager.LoadScene(Global.SceneNames.START);
        }

    }
}
