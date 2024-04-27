using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Random = UnityEngine.Random;

using UniRx;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

namespace DoubleHeat.SnowFightForDucksGame {

    public class VotingInstance : MonoBehaviour, IOnEventCallback {

        public VotingInstanceAnimationManager animationManager;
        public Transform ruleCardsParent;

        public float cardsIntervalDistance;
        public float candidatesWaitForShowUpTime;
        public float voteResultWaitForShowUpTime;

        [Header("Prefabs")]
        public GameObject ruleCardPrefab;


        public float VotingTimeElapsed => Time.realtimeSinceStartup - _votingStartTimeInRealtime;
        public float VotingTimeRemaining => (Global.gameSceneManager == null ? Mathf.Infinity : Global.gameSceneManager.votingDuration - VotingTimeElapsed);
        public float TimeElapsedAfterVotingEnded => Time.realtimeSinceStartup - (_votingStartTimeInRealtime + Global.gameSceneManager.votingDuration);
        public int   CurrentVoted => _currentVoted;
        public bool  IsVotingTime => (_isInited && !_isVotingEnded);

        public bool AllPlayersVoted {
            get {
                foreach (int playerNumber in Global.gameSceneManager.ActivePlayers) {
                    if (!_playersVoted.Keys.Contains(playerNumber))
                        return false;
                }
                return true;
            }
        }


        OptionalRule[] _candidateRules;
        RuleCard[]     _candidateCards;
        Dictionary<int, int> _playersVoted = new Dictionary<int, int>();
        int _electedIndex = -1;

        bool  _isInited = false;
        bool  _isCandidatesShowed = false;
        bool  _isVotingEnded = false;
        bool  _isElectedConfirmed = false;
        bool  _isVoteResultShowingStarted = false;

        float _votingStartTimeInRealtime = Mathf.Infinity;
        int   _currentVoted = -1;



        void Awake () {

            if (Global.gameSceneManager != null) {
                if (Global.gameSceneManager.currentVotingInstance != null)
                    Destroy(Global.gameSceneManager.currentVotingInstance.gameObject);

                Global.gameSceneManager.currentVotingInstance = this;
            }

            gameObject.SetActive(false);
        }


        public void Init (int nextRoundNumber, OptionalRule[] candidates, float votingStartTimeInRealtime) {
            _candidateRules = candidates;
            _votingStartTimeInRealtime = votingStartTimeInRealtime;
            GenerateCandidatesCard();


            if (nextRoundNumber == 3) {

            }
            else if (nextRoundNumber == 5) {

            }
            else if (nextRoundNumber == 6) {

            }
            else if (nextRoundNumber == 7) {

            }

            _isInited = true;

        }


        void OnEnable () {
            PhotonNetwork.AddCallbackTarget(this);
            animationManager.PlayShowUp();
        }

        void OnDisable () {
            PhotonNetwork.RemoveCallbackTarget(this);
        }


        void Update () {
            if (!_isInited)
                return;

            if (!_isVotingEnded) {
                Global.gameSceneManager.timerDisplay.UpdateDisplay(VotingTimeRemaining);

                if (!_isCandidatesShowed && VotingTimeElapsed > candidatesWaitForShowUpTime) {
                    animationManager.PlayShowAllCandidates(_candidateCards);
                    _isCandidatesShowed = true;
                }

                if (VotingTimeRemaining < 0) {
                    EndVoting();
                }
            }
            else {
                // Voting ended
                if (_isElectedConfirmed) {

                    if (!_isVoteResultShowingStarted) {

                        if (TimeElapsedAfterVotingEnded > voteResultWaitForShowUpTime) {
                            TryToShowVoteStampsOfNextCard(-1);
                            _isVoteResultShowingStarted = true;
                        }
                    }

                }
            }

        }


        RuleCard GenerateCard (string ruleName, int identifierNumber = -1) {
            RuleCard card = Instantiate(ruleCardPrefab, ruleCardsParent).GetComponent<RuleCard>();
            card.Init(ruleName, identifierNumber);

            return card;
        }

        void GenerateCandidatesCard () {

            _candidateCards = new RuleCard[_candidateRules.Length];
            for (int i = 0 ; i < _candidateCards.Length ; i++) {
                _candidateCards[i] = GenerateCard(_candidateRules[i].ToString(), i);
                _candidateCards[i].transform.position = ruleCardsParent.position + Vector3.right * (i - 1) * cardsIntervalDistance;
                _candidateCards[i].gameObject.SetActive(false);
                _candidateCards[i].ClickCall = RuleCardClicked;
            }
        }

        void EndVoting () {

            foreach (RuleCard card in _candidateCards) {
                card.VotingTimeEnded();
            }


            NetEvent.EmitPlayerVoteEvent(PhotonNetwork.LocalPlayer.ActorNumber, _currentVoted);

            _isVotingEnded = true;

            Global.gameSceneManager.OnVotingTimeEnded();
        }


        void ElectedConfirmed (int electedIndex) {
            if (_isElectedConfirmed)
                return;


            List<OptionalRule> restRules = new List<OptionalRule>();
            for (int i = 0 ; i < _candidateRules.Length ; i++) {
                if (i != electedIndex)
                    restRules.Add(_candidateRules[i]);
            }

            _electedIndex = electedIndex;

            Global.gameSceneManager.RuleElected(_candidateRules[electedIndex], restRules);

            _isElectedConfirmed = true;


            // Statistics
            foreach (int playerNumber in _playersVoted.Keys) {
                if (_playersVoted[playerNumber] >= 0) {

                    PlayerStatistics playerStats = Global.gameSceneManager.playersStatistics[playerNumber];

                    playerStats.votedTimes++;

                    if (_playersVoted[playerNumber] == electedIndex)
                        playerStats.votedElectedTimes++;

                }
            }

        }


        int GetElectedIndex (Dictionary<int, int> playersVoted) {

            int[] candidatesVotedCounter = new int[_candidateRules.Length];

            // init
            for (int i = 0 ; i < candidatesVotedCounter.Length ; i++) {
                candidatesVotedCounter[i] = 0;
            }

            // count
            foreach (int voted in playersVoted.Values) {
                if (voted >= 0 && voted < candidatesVotedCounter.Length)
                    candidatesVotedCounter[voted]++;
            }

            // find elected
            List<int> indicesOfHighestVoted = new List<int>();
            indicesOfHighestVoted.Add(0);

            for (int i = 1 ; i < candidatesVotedCounter.Length ; i++) {
                if (candidatesVotedCounter[i] > candidatesVotedCounter[indicesOfHighestVoted[0]]) {
                    indicesOfHighestVoted.Clear();
                    indicesOfHighestVoted.Add(i);
                }
                else if (candidatesVotedCounter[i] == candidatesVotedCounter[indicesOfHighestVoted[0]]) {
                    indicesOfHighestVoted.Add(i);
                }
            }

            return indicesOfHighestVoted[Random.Range(0, indicesOfHighestVoted.Count)];
        }




        // == RuleCards Anims ==
        void TryToShowVoteStampsOfNextCard (int completedCandidateIndex) {

            int nextIndex = completedCandidateIndex + 1;

            if (nextIndex < _candidateCards.Length) {

                _candidateCards[nextIndex].StartToShowVoteStamps( _playersVoted, () => TryToShowVoteStampsOfNextCard(nextIndex) );
            }
            else {
                Global.gameSceneManager.OnPlayElectedCard();
                animationManager.PlayElectedAnim(_candidateCards, _electedIndex);
            }
        }



        // ===== Public Methods =====
        public void StartToPlayCandidatesShowingAnim () {
            foreach (RuleCard card in _candidateCards) {
                card.AlwaysPlayShowingAnim = true;
            }
        }

        public void RuleCardClicked (int identifierNumber) {
            if (_isVotingEnded)
                return;

            if (IsVotingTime && identifierNumber >= 0 && identifierNumber < _candidateCards.Length) {

                _currentVoted = identifierNumber;
                for (int i = 0 ; i < _candidateCards.Length ; i++) {
                    _candidateCards[i].IsVoted = (i == identifierNumber);
                }

            }
        }




        public void PrepareToGoOut (float remainedTime) {
            animationManager.PrepareToGoOut(remainedTime, OnGoOutEnded);

            if (_electedIndex >= 0 && _electedIndex < _candidateCards.Length)
                _candidateCards[_electedIndex].ElectedPrepareToGoOut(remainedTime - animationManager.GoOutDuration);
        }

        void OnGoOutEnded () {
            Global.gameSceneManager.currentVotingInstance = null;
            Global.gameSceneManager.OnVotingInstanceRemoved();
            Destroy(gameObject);
        }



        // Pun Raise EventTrigger
        public void OnEvent (EventData photonEvent) {

            byte eventCode = photonEvent.Code;
            object[] data;

            try {
                data = (object[]) photonEvent.CustomData;

                if (eventCode == (byte) PunRaiseEventCode.PlayerVote) {

                    int playerNumber   = (int) data[0];
                    int candidateIndex = (int) data[1];

                    _playersVoted.Add(playerNumber, candidateIndex);


                    if (PhotonNetwork.IsMasterClient) {
                        if (AllPlayersVoted) {
                            int electedIndex = GetElectedIndex(_playersVoted);
                            NetEvent.EmitElectedEvent(electedIndex);

                            ElectedConfirmed(electedIndex);
                        }
                    }

                }
                else if (eventCode == (byte) PunRaiseEventCode.Elected) {

                    int electedIndex = (int) data[0];

                    ElectedConfirmed(electedIndex);

                }

            }
            catch (System.InvalidCastException e) {
                print(e);
            }

        }


    }
}
