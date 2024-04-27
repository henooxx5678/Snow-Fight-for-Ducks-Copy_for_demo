using UnityEngine;

using DG.Tweening;

namespace DoubleHeat.SnowFightForDucksGame {

    public class FixedRuleCardsChangingAnimationManager : MonoBehaviour {

        [System.Serializable]
        public class DuckTheRemoverAnimRrops {
            public float waitorForGoInTime;
            public float duration;
            public float distance;
        }

        [System.Serializable]
        public class RemovedCardAnimProps {
            public Ease  goInEase;
            public float goInDuration;
            public float goInDistance;
        }

        [System.Serializable]
        public class AddedCardAnimProps {
            public Ease  goInEase;
            public Ease  goOutEase;
            public float inOutDuration;
            public float inOutDistance;
            public float stayDuration;
            public float stayDistance;
        }

        public Transform duckTheRemoverTrans;

        public DuckTheRemoverAnimRrops duckTheRemoverAnimProps;
        public RemovedCardAnimProps    removedCardAnimProps;
        public AddedCardAnimProps      addedCardAnimProps;

        [Header("Prefabs")]
        public GameObject ruleCardPrefab;


        public void PlayCardRemoved (string ruleName, TweenCallback endCallback) {

            RuleCard card = GenerateCard(ruleName);
            card.transform.position = Vector3.zero;

            RemovedCardAnimProps cardProps = removedCardAnimProps;
            DuckTheRemoverAnimRrops duckProps = duckTheRemoverAnimProps;

            DOTween.Sequence()
                .Append( card.transform.DOMoveY(cardProps.goInDistance, cardProps.goInDuration)
                    .From(true)
                    .SetEase(cardProps.goInEase)
                )
                .AppendInterval( duckProps.waitorForGoInTime )
                .Append( duckTheRemoverTrans.transform.DOMoveX(duckProps.distance, duckProps.duration)
                    .From(-duckProps.distance, true)
                    .SetEase(Ease.Linear)
                )
                .OnComplete(endCallback);

        }

        public void PlayCardAdded (string ruleName, TweenCallback endCallback) {

            RuleCard card = GenerateCard(ruleName);
            card.transform.position = Vector3.zero;

            AddedCardAnimProps props = addedCardAnimProps;

            DOTween.Sequence()
                .Append( card.transform.DOMoveY(-props.stayDistance, props.inOutDuration)
                    .From(-props.inOutDistance, true)
                    .SetEase(props.goInEase)
                )
                .Append( card.transform.DOMoveY(props.stayDistance, props.stayDuration)
                    .SetEase(Ease.Linear)
                )
                .Append( card.transform.DOMoveY(props.inOutDistance, props.inOutDuration)
                    .SetEase(props.goOutEase)
                )
                .OnComplete(endCallback);

        }

        RuleCard GenerateCard (string ruleName) {

            RuleCard card = Instantiate(ruleCardPrefab).GetComponent<RuleCard>();
            card.transform.SetParent(transform);

            return card;
        }

    }
}
