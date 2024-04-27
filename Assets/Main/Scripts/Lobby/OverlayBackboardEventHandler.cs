using UnityEngine;
using UnityEngine.EventSystems;

namespace DoubleHeat.SnowFightForDucksGame {

    public class OverlayBackboardEventHandler : MonoBehaviour, IPointerClickHandler {

        public void OnPointerClick (PointerEventData evetData) {
            Global.startSceneManager.CloseAllAdditionalPanel();
        }

    }
}
