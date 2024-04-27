using System.Collections.Generic;

using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class ActiveRuleCardsShowingManager : MonoBehaviour {

        public GameObject underlayGO;
        public RuleCardListDisplay activeRuleCardsDisplay;


        void Start () {
            Close();
        }

        public void Open () {
            underlayGO.SetActive(true);
        }

        public void Close () {
            underlayGO.SetActive(false);

        }

        public void Refresh (string[] ruleNames) {

            print("Active Rules: " + ruleNames.Length);
            activeRuleCardsDisplay.Refresh(ruleNames);

        }

    }
}
