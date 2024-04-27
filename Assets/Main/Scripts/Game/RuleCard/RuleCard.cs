using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

using DoubleHeat.Utilities;

namespace DoubleHeat.SnowFightForDucksGame {

    public class RuleCard : MonoBehaviour {

        public enum Genre {
            Fixed,
            Player,
            Snowball,
            Building,
            Special
        }

        public static readonly Vector3 bodyFrameScaleSelected = new Vector3(1.04f, 1.028f, 1f);
        public static readonly Vector3 bodyFrameScaleHover    = new Vector3(1.025f, 1.0167f, 1f);


        public static List<RuleCard> exitsInstances = new List<RuleCard>();

        private static Dictionary<string, RuleCardProps> _RulePropsOfRuleNames = new Dictionary<string, RuleCardProps>();
        private static bool _IsRulePropsLoaded = false;


        public SpriteRenderer bodySR;
        public SpriteRenderer bodyFrameSR;
        public SpriteRenderer baseSR;
        public SpriteRenderer showingFrameSR;
        public Canvas         contentCanvas;
        public Text titleText;
        public TMPro.TextMeshProUGUI descriptionText;
        public RuleCardShowingAnimationManager showingAnimManager;
        public RuleCardCanvasEventHandler canvasEventHandler;
        public Sprite fixedRuleBase;
        public Sprite optionalRuleBase;
        public Sprite goldShowingFrameSprite;
        public Sprite silverShowingFrameSprite;

        public Transform  voteStampStartTrans;
        public GameObject voteStampPrefab;
        public float      voteStampMaxIntervalDistance;

        public bool isHoverToAnimEnabled = true;

        [Header("Card Base Colors")]
        public Color playerTypeColor;
        public Color snowballTypeColor;
        public Color buildingTypeColor;
        public Color specialTypeColor;

        [Header("Body Frame Colors")]
        public Color selectedFrameColor;
        public Color hoverFrameColor;


        public delegate void EventCall (int identifierNumber);

        public string RuleName => _ruleName;
        public RuleCardProps Props => _RulePropsOfRuleNames[_ruleName];
        public bool   IsVoted {
            get => _isVoted;
            set {
                _isVoted = value;
                if (_isVoted) {
                    bodyFrameSR.enabled = true;
                    bodyFrameSR.color = selectedFrameColor;
                    bodyFrameSR.transform.localScale = bodyFrameScaleSelected;
                }
                else {
                    bodyFrameSR.enabled = false;
                }
            }
        }
        public float  VoteStampsAreaWidth => Vector3.Dot(voteStampStartTrans.position - transform.position, Vector3.left) * 2;
        public EventCall ClickCall { set => _clickCall = value; }

        // Showing Anims
        public bool  AlwaysPlayShowingAnim {
            get => showingAnimManager.AlwaysPlayAnim;
            set => showingAnimManager.AlwaysPlayAnim = value;
        }


        RuleCardAnimationManager _animManager;

        Dictionary<Genre, Color> _colorOfTypes;

        string    _ruleName;
        int       _identifierNumber = -1;
        Sprite[]  _showingAnimSprites;

        bool      _isVoted = false;
        EventCall _clickCall;

        List<VoteStamp> _currentVoteStamps = new List<VoteStamp>();


        public static void LoadRuleCardsProps () {
            if (_IsRulePropsLoaded)
                return;

            System.Type[] ruleTypes = new System.Type[] { typeof(FixedRule), typeof(OptionalRule) };

            string allText = Resources.Load<TextAsset>(string.Format("Texts/{0}/RuleCardText_{0}", Global.currentLanguageMark)).text;
            string[] lines = allText.Split(new char[] {'\n'}, System.StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0 ; i < lines.Length - 3 ; i++) {

                foreach (var ruleType in ruleTypes) {

                    foreach (var rule in System.Enum.GetValues(ruleType)) {
                        if (lines[i].Contains(rule.ToString())) {
                            _RulePropsOfRuleNames.Add(rule.ToString(), GenerateRuleCardProps(lines[i + 1], lines[i + 2], lines[i + 3]));
                        }

                    }

                }
            }
            _IsRulePropsLoaded = true;
        }


        private static RuleCardProps GenerateRuleCardProps (string appearanceProps, string title, string description) {
            string[] props = appearanceProps.Split(new char[] {','});
            return new RuleCardProps((RuleCard.Genre) System.Enum.Parse(typeof(RuleCard.Genre), props[0]), (props[1].Contains("Gold")), title, description);
        }

        public static void UpdateAllDescriptionsText () {
            foreach (RuleCard card in exitsInstances) {
                if (card != null)
                    card.descriptionText.text = card.Props.GetDescriptionText(Global.currentInputMethodKeys);
            }
        }


        void Awake () {
            _animManager = gameObject.GetComponent<RuleCardAnimationManager>();

            bodyFrameSR.enabled = false;
            Collider2D collider = gameObject.GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = false;
        }


        public void Init (string ruleName, int identifierNumber = -1, bool alwaysPlayAnim = false) {

            _colorOfTypes = new Dictionary<Genre, Color>() {
                { Genre.Fixed, Color.white},
                { Genre.Player,   playerTypeColor },
                { Genre.Snowball, snowballTypeColor },
                { Genre.Building, buildingTypeColor },
                { Genre.Special,  specialTypeColor }
            };
            LoadRuleCardsProps();


            _ruleName = ruleName;
            _identifierNumber = identifierNumber;

            if (!Global.GetListOfRuleNames().Contains(ruleName))
                return;


            RuleCardProps props = _RulePropsOfRuleNames[ruleName];

            // sprites
            baseSR.sprite = (props.genre == Genre.Fixed) ? fixedRuleBase : optionalRuleBase;
            baseSR.color = _colorOfTypes[props.genre];
            showingFrameSR.sprite = props.isGolden ? goldShowingFrameSprite : silverShowingFrameSprite;

            // texts
            titleText.text = props.title;
            descriptionText.text = props.GetDescriptionText(Global.currentInputMethodKeys);

            showingAnimManager.Init(ruleName, alwaysPlayAnim);

            RuleCard.exitsInstances.Add(this);
        }

        void OnDestroy () {
            RuleCard.exitsInstances.Remove(this);
        }


        public void Click () {
            if (_clickCall != null)
                _clickCall(_identifierNumber);
        }

        public void PointerEnter () {
            if (isHoverToAnimEnabled) {

                if (!_isVoted) {
                    bodyFrameSR.enabled = true;
                    bodyFrameSR.color = hoverFrameColor;
                    bodyFrameSR.transform.localScale = bodyFrameScaleHover;
                }

                showingAnimManager.PlayAnim();
            }
        }

        public void PointerExit () {

            if (!_isVoted)
                bodyFrameSR.enabled = false;

            if (isHoverToAnimEnabled)
                showingAnimManager.PlayTitleImage();
        }



        // == At Voting Phase ==
        void TryToShowNextVoteStamp (int[] playersWhoVoteThis, int completedVoteStampIndex, TweenCallback allCompletedCallback) {

            int nextIndex = completedVoteStampIndex + 1;

            if (nextIndex < playersWhoVoteThis.Length) {

                VoteStamp voteStamp = Instantiate(voteStampPrefab, voteStampStartTrans.position + Vector3.right * VoteStampsAreaWidth, Quaternion.identity, voteStampStartTrans).GetComponent<VoteStamp>();
                voteStamp.ShowUp( NetEvent.GetPlayerDuckSkin(playersWhoVoteThis[nextIndex]), () => TryToShowNextVoteStamp(playersWhoVoteThis, nextIndex, allCompletedCallback) );

                _currentVoteStamps.Add(voteStamp);
            }
            else {
                allCompletedCallback();
            }
        }

        public void UpdateVoteStampsPositions (float movingDuration) {
            if (_currentVoteStamps.Count > 1) {
                float intervalDistance = VoteStampsAreaWidth / (_currentVoteStamps.Count - 1);

                for (int i = 0 ; i < _currentVoteStamps.Count ; i++) {

                    _currentVoteStamps[i].transform.position = _currentVoteStamps[i].transform.position.GetAfterSetZ(i * -0.01f);

                    if (intervalDistance > voteStampMaxIntervalDistance) {
                        // no squeeze
                        _currentVoteStamps[i].transform.position = voteStampStartTrans.position + Vector3.right * voteStampMaxIntervalDistance * i;
                    }
                    else {
                        // squeeze
                        Vector3 destination = voteStampStartTrans.position + Vector3.right * voteStampMaxIntervalDistance * i;
                        _currentVoteStamps[i].transform.DOMove(destination, movingDuration).SetEase(Ease.InOutSine);
                    }
                }

            }
            else if (_currentVoteStamps.Count == 1) {
                _currentVoteStamps[0].transform.position = voteStampStartTrans.position;
            }
        }



        public void VotingTimeEnded () {
            isHoverToAnimEnabled = false;
            if (!_isVoted)
                bodyFrameSR.enabled = false;

            showingAnimManager.PlayTitleImage();
            _clickCall = null;
        }

        public void StartToShowVoteStamps (Dictionary<int, int> playersVoted, TweenCallback completedCallback) {

            List<int> playersWhoVoteThis = new List<int>();

            foreach (int playerNumber in playersVoted.Keys) {
                if (playersVoted[playerNumber] == _identifierNumber) {
                    playersWhoVoteThis.Add(playerNumber);
                }
            }

            TryToShowNextVoteStamp(playersWhoVoteThis.ToArray(), -1, completedCallback);
        }

        public void ShowElectedResult (bool isElected) {

            if (isElected) {
                showingAnimManager.PlayAnimFromStart();

                if (_animManager != null)
                    _animManager.PlayElectedAnim();
            }
            else {
                if (_animManager != null)
                    _animManager.NonElectedGoOut();
            }

        }

        public void ElectedPrepareToGoOut (float remainedTime) {
            if (_animManager != null)
                _animManager.ElectedPrepareToGoOut(remainedTime);
        }

        public void OnOutOfViewport () {
            Destroy(gameObject);
        }


        public void SetSpritesSortingLayer (string layerName) {
            bodySR.SetSortingLayer(layerName);
            baseSR.SetSortingLayer(layerName);
            showingFrameSR.SetSortingLayer(layerName);
            contentCanvas.SetSortingLayer(layerName);
            showingAnimManager.showingSR.SetSortingLayer(layerName);
        }
    }


    public struct RuleCardProps {
        public RuleCard.Genre genre;
        public bool isGolden;
        public string title;
        public string rawDescription;

        public RuleCardProps (RuleCard.Genre genre, bool isGolden, string title, string description) {
            this.genre = genre;
            this.isGolden = isGolden;
            this.title = title;
            this.rawDescription = description;
        }

        public string GetDescriptionText (InputMethodKeys inputMethodKeys) {

            if (inputMethodKeys == null)
                return rawDescription;

            string result = rawDescription;

            result = result.Replace("[icon: advance]", inputMethodKeys.GetKeyIconLabel(inputMethodKeys.advance));
            result = result.Replace("[icon: back]", inputMethodKeys.GetKeyIconLabel(inputMethodKeys.back));
            result = result.Replace("[icno: menu]", inputMethodKeys.GetKeyIconLabel(inputMethodKeys.menu));
            result = result.Replace("[icon: in game info]", inputMethodKeys.GetKeyIconLabel(inputMethodKeys.inGameInfo));

            result = result.Replace("[icon: fire]", inputMethodKeys.GetKeyIconLabel(inputMethodKeys.fire));
            result = result.Replace("[icon: 2nd fire]", inputMethodKeys.GetKeyIconLabel(inputMethodKeys.secondaryFire));
            result = result.Replace("[icon: collect]", inputMethodKeys.GetKeyIconLabel(inputMethodKeys.collect));
            result = result.Replace("[icon: quack]", inputMethodKeys.GetKeyIconLabel(inputMethodKeys.quack));
            result = result.Replace("[icon: curve ball switch dir]", inputMethodKeys.GetKeyIconLabel(inputMethodKeys.curveBallSwitchDirection));

            for (int i = 0 ; i < inputMethodKeys.gadgets.Length ; i++) {
                if (inputMethodKeys.gadgetSwitchMethods[0] == InputMethodKeys.GadgetSwitchMethod.CorrespondingKey) {
                    result = result.Replace(string.Format("[icon: switch gadget to {0}]", i), inputMethodKeys.GetKeyIconLabel(inputMethodKeys.gadgets[i]));
                }
                else if (inputMethodKeys.gadgetSwitchMethods[0] == InputMethodKeys.GadgetSwitchMethod.LeftRightSwitch) {
                    result = result.Replace(string.Format("[icon: switch gadget to {0}]", i), inputMethodKeys.GetKeyIconLabel(inputMethodKeys.switchGadgetLeft) + inputMethodKeys.GetKeyIconLabel(inputMethodKeys.switchGadgetRight));
                }
                else if (inputMethodKeys.gadgetSwitchMethods[0] == InputMethodKeys.GadgetSwitchMethod.SelectionsRing) {
                    // result = result.Replace(string.Format("[icon: switch gadget to {0}]", i), inputMethodKeys.GetKeyIconLabel(inputMethodKeys.holdToSwitchGadget) + <right stick>);
                }
            }

            return result;
        }
    }
}
