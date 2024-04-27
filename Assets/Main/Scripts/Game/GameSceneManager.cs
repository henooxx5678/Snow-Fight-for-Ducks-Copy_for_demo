using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Random = UnityEngine.Random;

using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

/*
NOTES
not done: handle no more card satuation
*/

namespace DoubleHeat.SnowFightForDucksGame {

    public class GameSceneManager : SingletonMonoBehaviour<GameSceneManager>, IOnEventCallback {

        public enum State {
            Starting,
            Round,
            Voting,
            Ending,
            Loading
        }

        public InGameMenuManager             inGameMenuManager;
        public ActiveRuleCardsShowingManager activeRuleCardsDisplay;
        public Transform[] magicalPlayerSpawnPoints;
        public Transform[] unicornPlayerSpawnPoints;
        public Statue[]  statues;
        public Transform snowWallsParent;


        [Header("Options")]
        public bool enableFixedRules;

        [Header("Round & Voting")]
        public float roundWaitForCountdownTime;
        public float roundCountDownToStartTime;
        public float roundDuration;
        public float votingDuration;
        public float votingEndingDuration;

        [Header("Timer In & Out")]
        public float timerWaitForGoOutTimeAfterRoundEnd;

        [Header("Player Building")]
        public float playerBuildPreviewOpacity;
        public float playerBuildingBaseOpacity;

        [Header("Refs")]
        public Camera mainCam;
        public GameSceneInputManager inputManager;
        public TimerDisplay timerDisplay;
        public TimerDisplayInOutAnimationManager timerDisplayInOutAnimationManager;

        [Header("Prefabs")]
        public GameObject roundInstancePrefab;
        public GameObject votingInstancePrefab;
        public GameObject gameResultHandlerPrefab;


        [HideInInspector]
        public RoundInstance  currentRoundInstance;
        [HideInInspector]
        public VotingInstance currentVotingInstance;

        public Dictionary<int, PlayerStatistics> playersStatistics = new Dictionary<int, PlayerStatistics>();



        public bool  IsRoundInited => _isRoundInited;
        public bool  IsVotingInited => _isVotingInited;
        public bool  IsInGameMenuOpened => inGameMenuManager.gameObject.activeSelf;
        public byte  LocalPlayerTeam => NetEvent.GetLocalPlayerTeam();
        public int   LocalPlayerTeamMemberAmount => NetEvent.GetLocalPlayerTeamCountOfMembers();
        public int   LocalPlayerOrderInTeam {
            get {
                int[,] playerInSeats = NetEvent.GetCurrentPlayerInSeats();

                for (byte team = 0 ; team < playerInSeats.GetLength(0) ; team++) {
                    for (int i = 0 ; i < playerInSeats.GetLength(1) ; i++) {
                        int order = 0;

                        if (playerInSeats[team, i] == PhotonNetwork.LocalPlayer.ActorNumber)
                            return order;

                        else if (playerInSeats[team, i] != -2)
                            order++;
                    }
                }
                return -1;
            }
        }

        public State CurrentState => _currentState;
        public float RoundReadyToStartTime => roundWaitForCountdownTime + roundCountDownToStartTime;

        public List<int> ActivePlayers {
            get {
                List<int> result = new List<int>();

                int[,] playerInSeats = NetEvent.GetCurrentPlayerInSeats();

                for (byte team = 0 ; team < playerInSeats.GetLength(0) ; team++) {
                    for (int i = 0 ; i < playerInSeats.GetLength(1) ; i++) {

                        int playerNumber = playerInSeats[team, i];
                        if (playerNumber != -2)
                            result.Add(playerNumber);
                    }
                }
                return result;
            }
        }

        public Transform[] LocalPlayerTeamSpawnPoints {
            get {
                byte team = LocalPlayerTeam;
                if (team == 0)
                    return magicalPlayerSpawnPoints;
                else if (team == 1)
                    return unicornPlayerSpawnPoints;

                return null;
            }
        }

        public string[] activeOptionalRulesName {
            get {
                OptionalRule[] rules = _optionalRulesUsing.ToArray();
                string[] result = new string[rules.Length];

                for (int i = 0 ; i < result.Length ; i++) {
                    result[i] = rules[i].ToString();
                }
                return result;
            }
        }



        List<FixedRule>    _currentFixedRules  = new List<FixedRule>();
        List<OptionalRule> _optionalRulesPool  = new List<OptionalRule>();
        List<OptionalRule> _optionalRulesUsed  = new List<OptionalRule>();
        List<OptionalRule> _optionalRulesUsing = new List<OptionalRule>();


        float _gameStartRealTime = 0f;

        int   _currentRoundNumber = 1;
        int   _currentVotingNumber = 0;
        State _currentState = State.Starting;
        bool  _isRoundInited = false;
        bool  _isVotingInited = false;
        bool  _isVotingInstanceReadyToShowUp = false;






        //
        // FixedRule[] GetFixedRules (int roundNumber) {
        //     if (roundNumber == 1 || roundNumber == 2) {
        //         return new FixedRule[] {
        //             FixedRule.StatuesRepairedOnceAtRoundEnd
        //         };
        //     }
        //     else if (roundNumber == 5) {
        //         return new FixedRule[] {
        //             FixedRule.RepairProhibited
        //         };
        //     }
        //     else if (roundNumber == 6) {
        //         return new FixedRule[] {
        //             FixedRule.RepairProhibited,
        //             FixedRule.BuildProhibited
        //         };
        //     }
        //     else if (roundNumber == 7) {
        //         return new FixedRule[] {
        //             FixedRule.RepairProhibited,
        //             FixedRule.BuildProhibited,
        //             FixedRule.StatuesRotting
        //         };
        //     }
        //
        //     return new FixedRule[0];
        // }







        protected override void Awake () {
            base.Awake();

            Global.gameSceneManager = this;

            foreach (OptionalRule rule in Enum.GetValues(typeof(OptionalRule))) {

                if (!Global.globalManager.bannedOptionalRules.Contains(rule))
                    _optionalRulesPool.Add(rule);

            }

            TimerDisplay.LoadSpritesResources();
            PlayerAnimationManager.LoadSpritesResources();
            SnowWallAnimationManager.LoadSpritesResources();
            StatueAnimationManager.LoadSpritesResources();

            RuleCard.LoadRuleCardsProps();


            // Setup Statues Number
            for (int i = 0 ; i < statues.Length ; i++) {
                statues[i].SetNumber(i);
            }
        }

        protected override void OnDestroy () {
            base.OnDestroy();

            Global.gameSceneManager = null;

            TimerDisplay.ReleaseLoaded();
            PlayerAnimationManager.ReleaseLoaded();
            SnowWallAnimationManager.ReleaseLoaded();
            StatueAnimationManager.ReleaseLoaded();
        }

        void OnEnable () {
            PhotonNetwork.AddCallbackTarget(this);
        }

        void OnDisable () {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        void Start () {

            PhotonNetwork.AutomaticallySyncScene = false;

            _gameStartRealTime = Time.realtimeSinceStartup;


            // Init Snow Walls
            SnowWall[] snowWalls = GetCurrentSnowWalls();
            for (int i = 0 ; i < snowWalls.Length ; i++) {
                snowWalls[i].InitDefault(i);
            }

            // Init Players Statistics
            foreach (int playerNumber in ActivePlayers) {
                playersStatistics.Add(playerNumber, new PlayerStatistics());
            }

            SwtichToRound();
        }






        void SwitchToVoting () {
            _currentState = State.Voting;

            if (PhotonNetwork.IsMasterClient) {
                PhotonNetwork.FetchServerTimestamp();
                // NetEvent.SetCandaidatesAndStartTimestamp(DrawCards(), PhotonNetwork.ServerTimestamp);
                NetEvent.EmitInitVotingEvent(DrawCards(), PhotonNetwork.ServerTimestamp);
            }
        }

        void SwtichToRound () {
            _currentState = State.Round;

            if (PhotonNetwork.IsMasterClient) {
                PhotonNetwork.FetchServerTimestamp();
                // NetEvent.SetActiveRulesAndInitTimestamp(_optionalRulesUsing.ToArray(), PhotonNetwork.ServerTimestamp);
                NetEvent.EmitInitRoundEvent(_optionalRulesUsing.ToArray(), PhotonNetwork.ServerTimestamp);
            }
        }


        void InitRound (OptionalRule[] optionalRules, int initTimestamp) {

            FixedRule[] fixedRules = new FixedRule[0];

            if (enableFixedRules) {
                fixedRules = _currentFixedRules.ToArray();
            }



            PhotonNetwork.FetchServerTimestamp();
            float initTimeCompensation = (initTimestamp - PhotonNetwork.ServerTimestamp) / 1000f;

            float waitForStartTime = RoundReadyToStartTime;
            if (_currentVotingNumber > 0)
                waitForStartTime += votingEndingDuration;

            Instantiate(roundInstancePrefab).GetComponent<RoundInstance>().Init(fixedRules, optionalRules, Time.time + initTimeCompensation + waitForStartTime, roundDuration);

            inputManager.InitForRound();

            if (currentVotingInstance != null)
                currentVotingInstance.PrepareToGoOut(-currentRoundInstance.RoundTimeElapsed - RoundReadyToStartTime);


            activeRuleCardsDisplay.Refresh( Global.GetRulesName(fixedRules, optionalRules) );


            _isRoundInited = true;
        }

        void InitVoting (OptionalRule[] candidates, int startTimestamp) {

            PhotonNetwork.FetchServerTimestamp();
            float startTimeCompensation = (startTimestamp - PhotonNetwork.ServerTimestamp) / 1000f;

            VotingInstance votingInstance = Instantiate(votingInstancePrefab).GetComponent<VotingInstance>();
            votingInstance.Init(_currentRoundNumber, candidates, Time.realtimeSinceStartup + startTimeCompensation);

            foreach (OptionalRule rule in candidates) {
                if (_optionalRulesPool.Count == 0) {
                    _optionalRulesPool.AddRange(_optionalRulesUsed);
                    _optionalRulesUsed.Clear();
                }

                if (_optionalRulesPool.Contains(rule)) {
                    _optionalRulesPool.Remove(rule);
                }
            }

            inputManager.InitForVoting();

            _currentVotingNumber++;
            _isVotingInited = true;

            if (_isVotingInstanceReadyToShowUp) {
                votingInstance.gameObject.SetActive(true);
                _isVotingInstanceReadyToShowUp = false;
            }
        }



        OptionalRule[] DrawCards () {

            int resultCardsAmount = Math.Min(Global.globalManager.cardsAmountPerVoting, _optionalRulesPool.Count + _optionalRulesUsed.Count);

            OptionalRule[] candidates = new OptionalRule[resultCardsAmount];

            for (int i = 0 ; i < candidates.Length ; i++) {

                if (_optionalRulesPool.Count == 0) {
                    _optionalRulesPool.AddRange(_optionalRulesUsed);
                    _optionalRulesUsed.Clear();
                }

                if (_optionalRulesPool.Count > 0) {

                    int randInt = Random.Range(0, _optionalRulesPool.Count);

                    candidates[i] = _optionalRulesPool[randInt];
                    _optionalRulesPool.RemoveAt(randInt);
                }
            }

            return candidates;
        }


        // ===== Public Methods ====
        public Vector3 GetLocalPlayerSpawnPointThisRound () {
            int index = (LocalPlayerOrderInTeam + _currentRoundNumber - 1) % LocalPlayerTeamMemberAmount;
print("Order: " + LocalPlayerOrderInTeam + "  , Amount: " + LocalPlayerTeamMemberAmount);
print("index: " + index);
            if (index != -1)
                return LocalPlayerTeamSpawnPoints[index].position;

            return Vector2.zero;
        }

        public SnowWall[] GetCurrentSnowWalls () {
            SnowWall[] result = new SnowWall[snowWallsParent.childCount];

            for (int i = 0 ; i < result.Length ; i++) {
                result[i] = snowWallsParent.GetChild(i).gameObject.GetComponent<SnowWall>();
            }

            return result;
        }


        public SnowWall GetSnowWall (int ownerNumber, int idByOwner) {

            for (int i = 0 ; i < snowWallsParent.childCount ; i++) {
                SnowWall snowWall = snowWallsParent.GetChild(i).gameObject.GetComponent<SnowWall>();

                if (snowWall.OwnerNumber == ownerNumber && snowWall.IdByOwner == idByOwner)
                    return snowWall;
            }
            return null;
        }



        public void OpenInGameInfoPanel () {
            activeRuleCardsDisplay.Open();
        }

        public void CloseInGameInfoPanel () {
            activeRuleCardsDisplay.Close();
        }

        public void SwitchInGameMenu () {
            inGameMenuManager.Switch();
        }





        public void RoundPhaseEnded () {
            _isRoundInited = false;

            DOTween.Sequence()
                .AppendInterval( timerWaitForGoOutTimeAfterRoundEnd )
                .AppendCallback( timerDisplayInOutAnimationManager.GoOut );

            _currentRoundNumber++;
            SwitchToVoting();
        }

        public void ShowVotingInstance () {
            if (_isVotingInited) {
                currentVotingInstance.gameObject.SetActive(true);
            }
            else {
                _isVotingInstanceReadyToShowUp = true;
            }
        }




        public void RuleElected (OptionalRule electedRule, List<OptionalRule> restRules) {
            _optionalRulesUsing.Add(electedRule);
            _optionalRulesUsed.AddRange(restRules);
        }


        public void OnRoundInstanceRemoved () {
            timerDisplayInOutAnimationManager.GoIn();

            if (currentVotingInstance != null)
                currentVotingInstance.StartToPlayCandidatesShowingAnim();
        }

        public void OnVotingTimeEnded () {
            timerDisplayInOutAnimationManager.GoOut();
        }

        public void OnPlayElectedCard () {
            SwtichToRound();
        }

        public void OnVotingInstanceRemoved () {
            _isVotingInited = false;
            timerDisplayInOutAnimationManager.GoIn();
        }

        public void GoToGameOverScene (byte winTeam) {
            GameResultHandler gameResultHandler = Instantiate(gameResultHandlerPrefab).GetComponent<GameResultHandler>();
            gameResultHandler.Init(winTeam, _currentRoundNumber, Time.realtimeSinceStartup - _gameStartRealTime, playersStatistics);

            UnityEngine.SceneManagement.SceneManager.LoadScene(Global.SceneNames.GAME_OVER);
        }




        // Update Objects Data
        public void RefreshStatuesHP () {
            if (!PhotonNetwork.IsMasterClient)
                return;

            int[] statuesHP = new int[statues.Length];
            for (int i = 0 ; i < statuesHP.Length ; i++) {
                statuesHP[i] = statues[i].CurrentHP;
            }
            NetEvent.EmitUpdateStatuesHP(statuesHP);

            if (currentRoundInstance != null)
                currentRoundInstance.CheckIfGameOver(statuesHP);
        }

        public void RefreshSnowWallsData () {
            if (!PhotonNetwork.IsMasterClient)
                return;

            SnowWall[] snowWalls = GetCurrentSnowWalls();
            SnowWall.NetProperties[] propss = new SnowWall.NetProperties[snowWalls.Length];

            for (int i = 0 ; i < propss.Length ; i++) {
                propss[i] = snowWalls[i].NetProps;
            }
            NetEvent.EmitUpdateSnowWallsData(propss);
        }




        // Pun Raise EventTrigger
        public void OnEvent (EventData photonEvent) {

            byte eventCode = photonEvent.Code;
            object[] data;

            try {
                data = (object[]) photonEvent.CustomData;

                if (eventCode == (byte) PunRaiseEventCode.InitRound) {

                    OptionalRule[] activeRules        = NetEvent.ByteArrayToOptionalRules((byte[]) data[0]);
                    int            roundInitTimestamp = (int)                                      data[1];

                    InitRound(activeRules, roundInitTimestamp);

                }
                else if (eventCode == (byte) PunRaiseEventCode.InitVoting) {

                    OptionalRule[] candidates     = NetEvent.ByteArrayToOptionalRules((byte[]) data[0]);
                    int            votingStartTimestamp = (int)                                      data[1];

                    InitVoting(candidates, votingStartTimestamp);

                }
                else if (eventCode == (byte) PunRaiseEventCode.UpdateStatuesData) {

                    int[] statuesHP = (int[]) data[0];

                    for (int i = 0 ; i < statues.Length ; i++) {
                        statues[i].CurrentHP = statuesHP[i];
                    }

                    if (currentRoundInstance != null)
                        currentRoundInstance.CheckIfGameOver(statuesHP);

                }
                else if (eventCode == (byte) PunRaiseEventCode.UpdateSnowWallsData) {

                    int[]     ownersNumber = (int[])     data[0];
                    int[]     idsByOwner   = (int[])     data[1];
                    Vector2[] positions    = (Vector2[]) data[2];
                    int[]     hps          = (int[])     data[3];

                    SnowWall.NetProperties[] propss = SnowWall.NetProperties.PackUpArray(ownersNumber, idsByOwner, positions, hps);

                    foreach (SnowWall snowWall in GetCurrentSnowWalls()) {
                        snowWall.SetByNetPropertiesArray(propss);
                    }
                }

            }
            catch (System.InvalidCastException e) {
                print(e);
            }

        }

    }
}
