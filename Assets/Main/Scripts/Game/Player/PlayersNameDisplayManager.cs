using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class PlayersNameDisplayManager : MonoBehaviour {

        [Header("Prefabs")]
        public GameObject playerNameDisplayPrefab;

        public GameObject Register (string nameDisplay, Transform following) {
            GameObject instance = Instantiate(playerNameDisplayPrefab, transform);
            instance.GetComponent<PlayerNameDisplayManager>().Init(nameDisplay, following);

            return instance;
        }

    }
}
