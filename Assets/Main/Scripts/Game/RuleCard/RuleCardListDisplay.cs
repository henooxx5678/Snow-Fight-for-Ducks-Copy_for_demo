using System.Collections.Generic;

using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class RuleCardListDisplay : MonoBehaviour {

        public enum ShowingCardsMode {
            All,
            Custom
        }

        public enum RefreshMode {
            EveryFrame,
            OnCall
        }


        public ShowingCardsMode showingCardsMode = ShowingCardsMode.Custom;
        public RefreshMode      refreshMode      = RefreshMode.OnCall;

        public float horizontalInterval;
        public float verticalInterval;
        public int maxInRow;
        public bool alwaysPlayCardsAnim;

        public string[] rulesName = new string[0];


        [Header("Prefabs")]
        public RuleCard ruleCardPrefab;


        List<RuleCard> _cards = new List<RuleCard>();


        void Start () {
            RuleCard.LoadRuleCardsProps();
        }


        void Udpate () {
            if (refreshMode == RefreshMode.EveryFrame) {
                Refresh();
            }
        }



        public void Refresh (string[] rulesName = null) {

            if (showingCardsMode == ShowingCardsMode.All)
                this.rulesName = Global.GetListOfRuleNames().ToArray();
            else if (rulesName != null)
                this.rulesName = rulesName;




            ReLoadCards(this.rulesName);

            // Set Positions
            int     rowsAmount         = maxInRow > 0 ? (_cards.Count - 1) / maxInRow + 1       : 1;
            int     columnsAmount      = maxInRow > 0 ? System.Math.Min(_cards.Count, maxInRow) : _cards.Count;
            Vector2 toCenterCorrection = (Vector2.left * horizontalInterval * (columnsAmount - 1) + Vector2.up * verticalInterval * (rowsAmount -1)) / 2;

            for (int i = 0 ; i < _cards.Count ; i++) {

                Vector2Int rowColumnNumber = GetRowColumnNumber(i, columnsAmount);
                _cards[i].transform.localPosition = Vector2.right * horizontalInterval * rowColumnNumber.x + Vector2.down * verticalInterval * rowColumnNumber.y + toCenterCorrection;
                _cards[i].AlwaysPlayShowingAnim = alwaysPlayCardsAnim;
            }
        }

        void ReLoadCards (string[] rulesName) {

            _cards = new List<RuleCard>();

            List<RuleCard> oldCards = new List<RuleCard>();
            for (int i = 0 ; i < transform.childCount ; i++) {
                RuleCard card = transform.GetChild(i).gameObject.GetComponent<RuleCard>();
                if (card != null) {
                    oldCards.Add(card);
                }
            }

            foreach (string ruleName in rulesName) {

                RuleCard existCard = null;

                foreach (RuleCard card in oldCards) {

                    if (card.RuleName == ruleName) {
                        existCard = card;
                        break;
                    }
                }

                if (existCard != null) {
                    oldCards.Remove(existCard);
                    _cards.Add(existCard);
                }
                else {
                    RuleCard newCard = Instantiate(ruleCardPrefab).GetComponent<RuleCard>();
                    newCard.transform.SetParent(transform, false);

                    if (newCard != null) {
                        newCard.Init(ruleName);
                        newCard.SetSpritesSortingLayer("Overlay");
                        _cards.Add(newCard);
                    }
                }
            }

            // remove unused cards
            foreach (RuleCard restCard in oldCards) {
                Destroy(restCard.gameObject);
            }

        }

        Vector2Int GetRowColumnNumber (int index, int amountPerRow) {
            return new Vector2Int(index % amountPerRow, index / amountPerRow);
        }

    }
}
