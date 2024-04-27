using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

namespace DoubleHeat.SnowFightForDucksGame {

    public enum FixedRule {
        StatuesRepairedOnceAtRoundEnd,
        RepairProhibited,
        BuildProhibited,
        StatuesRotting
    }

    public enum OptionalRule {
        // -- Player --
        KnockOutFarther,
        KnockDownProhibited,
        HighGravity,
        TurnProhibited,

        // -- Snowball --
        // FireNeedCharge,
        SlowerAndCollidableSnowball,
        KnuckleBall,
        CurveBall,
        Fire2GoesV,

        // -- Building --
        SnowWallBuildFastAndRotting,
        SnowWallCostDown,
        EggsWallMovable,
        TravelingStatue,

        // -- Special --
        // NarrowView,
        DuckSnowCake
    }

    public class RoundInstance : MonoBehaviour, IOnEventCallback {

        const int SNOWBALL_ID_CEILING = 16;

        [System.Serializable]
        public class ActiveRules {
            public bool statuesRepairedOnceAtRoundEnd;
            public bool repairProhibited;
            public bool buildProhibited;
            public bool statuesRotting;

            public bool knockOutFarther;
            public bool knockDownProhibited;
            public bool turnProhibited;
            public bool highGravity;
            public bool fireNeedCharge;
            public bool fire2GoesV;
            public bool slowerAndCollidableSnowball;
            public bool knuckleBall;
            public bool curveBall;
            public bool snowWallBuildFastAndRotting;
            public bool snowWallCostDown;
            public bool eggsWallMovable;
            public bool travelingStatue;
            public bool narrowView;
            public bool duckSnowCake;
        }

        [System.Serializable]
        public class PlayerProperties {
            public int   maxCarriedAmount;
            public float walkSpeed;
            public float walkSpeedSlower;
            public float walkBackSpeedRate;
            public float walkDirectionChangeSpeed;
            public float fire2GoesVDeflectionAngle;
            public float buildDistance;
            public float repairDistance;
            public float moveWallDistance;
            public float targetingDetectRadius;

            public float turnTime;
            public float collectTime;
            public float fireTime;
            public float snowballLaunchTime;
            public float buildTimeShorter;
            public float buildTime;
            public float repairTime;
            public float knockedOutFlyTime;
            public float knockedBackTime;
            public float downedTime;
            public float recoverTime;
            public float quackTime;
            public float swingCakeTime;
            public float cakeHitStartTime;
            public float cakeHitStopTime;

            public float knockedBackDistance;
            public float knockedOutDistance;
            public float knockedOutLongDistanceRate;
            public float knockedOutRotateDegree;

            public float maxHitBoxMovingShiftDistance;
        }
        [System.Serializable]
        public class SnowballProperties {
            public float flyingDistanceShorter;
            public float flyingDistance;
            public float flyingSpeed;
            public float flyingSpeedSlower;
            public float curveBallRadius;
            public int   knuckleBallShiftPointAmount;
            public float knuckleBallShiftTotalRange;
        }
        [System.Serializable]
        public class BuildingProperties {
            public int wallHP;
            public int wallHpCostDown;
            public int buildCost;
            public int buildCostCostDown;
            public int statueHP;
            public int repairCost;
        }


        /// ===== Public Variable ===== ///
        public PlayersNameDisplayManager playersNameDisplayManager;
        public RoundOverlayManager       overlayManager;
        public Transform                 snowballsParent;
        public GadgetsDisplay            gadgetsDisplay;

        public ActiveRules        activeRules;
        public PlayerProperties   playerProps;
        public SnowballProperties snowballProps;
        public BuildingProperties buildingProps;

        [Header("Prefabs")]
        public GameObject playerPrefab;
        public GameObject snowballPrefab;
        public GameObject snowWallRightUpPrefab;
        public GameObject snowWallRightPrefab;
        public GameObject snowWallRightDownPrefab;


        public bool           IsInited => _isInited;
        public bool           IsCountdownStarted => _isCountdownStarted;
        public bool           IsStarted => _isStarted;
        public bool           IsEnded => _isEnded;
        public bool           IsGameOver => _isGameOver;
        public bool           IsRunning => _isStarted && !_isEnded && !_isGameOver;
        public float          RoundStartTime => _roundStartTime;
        public FixedRule[]    CurrentFixedRules => _currentFixedRules;
        public OptionalRule[] CurrentRulesCards => _currentRuleCards;

        public float RoundTimeElapsed => Time.time - _roundStartTime;
        public float RoundTimeRemaining => _roundDuration - RoundTimeElapsed;
        public float CurrentPlayerWalkSpeed => activeRules.highGravity ? playerProps.walkSpeedSlower : playerProps.walkSpeed;
        public int   CurrentPlayerMaxCarriedAmount => playerProps.maxCarriedAmount;
        public int   CurrentCollectAmount => 1;
        public int   CurrentFireCost  => activeRules.fire2GoesV ? 2 : 1;
        public int   CurrentBuildCost => activeRules.snowWallCostDown ? buildingProps.buildCostCostDown : buildingProps.buildCost;
        public int   CurrentRepairCost => buildingProps.repairCost;
        public float CurrentKnockedOutDistance => (activeRules.knockDownProhibited ? playerProps.knockedBackDistance : playerProps.knockedOutDistance) * (activeRules.knockOutFarther ? playerProps.knockedOutLongDistanceRate : 1f);

        public float CurrentSnowballDistance => activeRules.highGravity ? snowballProps.flyingDistanceShorter : snowballProps.flyingDistance;
        public float CurrentSnowballSpeed => activeRules.slowerAndCollidableSnowball ? snowballProps.flyingSpeedSlower : snowballProps.flyingSpeed;
        public int   CurrentSnowWallMaxHP => activeRules.snowWallCostDown ? buildingProps.wallHpCostDown : buildingProps.wallHP;


        /// ===== Private Variable ===== ///
        FixedRule[] _currentFixedRules = new FixedRule[0];
        OptionalRule[] _currentRuleCards = new OptionalRule[0];

        int[] _playerNumbers;
        Dictionary<int, int> _snowballIdByOwnerCounters = new Dictionary<int, int>();
        Dictionary<int, int> _snowWallIdByOwnerCounters = new Dictionary<int, int>();

        bool  _isInited = false;
        float _roundDuration = Mathf.Infinity;
        float _roundStartTime = 0f;
        bool  _isCountdownStarted = false;
        bool  _isStarted  = false;
        bool  _isEnded    = false;
        bool  _isGameOver = false;


        /// ===== Methods ===== ///
        void Awake () {

            if (Global.gameSceneManager != null) {
                if (Global.gameSceneManager.currentRoundInstance != null)
                    Destroy(Global.gameSceneManager.currentRoundInstance.gameObject);
            }
            Global.gameSceneManager.currentRoundInstance = this;



            Physics2D.IgnoreLayerCollision(Global.PROJECTILE_LAYER, Global.PROJECTILE_LAYER, true);

            _playerNumbers = new int[PhotonNetwork.CurrentRoom.Players.Keys.Count];
            PhotonNetwork.CurrentRoom.Players.Keys.CopyTo(_playerNumbers, 0);

            foreach (int playerNumber in _playerNumbers) {
                _snowballIdByOwnerCounters.Add(playerNumber, 0);
                _snowWallIdByOwnerCounters.Add(playerNumber, 0);
            }
        }


        public void Init (FixedRule[] fRules, OptionalRule[] oRules, float roundStartTime, float roundDuration) {
            SetFixedRules(fRules);
            SetOptionalRules(oRules);
            _roundStartTime = roundStartTime;
            _roundDuration = roundDuration;
            Global.gameSceneManager.timerDisplay.UpdateDisplay(_roundDuration);
            _isInited = true;
        }

        void OnEnable () {
            PhotonNetwork.AddCallbackTarget(this);
        }

        void OnDisable () {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        void OnDestroy () {
            Time.timeScale = 1f;
        }

        void Start () {
            // check snow walls
            for (int i = 0 ; i < Global.gameSceneManager.snowWallsParent.childCount ; i++) {
                Global.gameSceneManager.snowWallsParent.GetChild(i).gameObject.GetComponent<SnowWall>().RoundStartCheck(CurrentSnowWallMaxHP);
            }


            // init local player
            if (playerPrefab == null) {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else {
                if (PlayerManager.LocalPlayerManager == null) {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);

                    PhotonNetwork.Instantiate(playerPrefab.name, Global.gameSceneManager.GetLocalPlayerSpawnPointThisRound(), Quaternion.identity, 0);
                }
            }

            overlayManager.SetupReadyMessage();
        }


        void Update () {
            if (!_isInited)
                return;

            if (!_isStarted) {

                if (!_isCountdownStarted && RoundTimeElapsed > -Global.gameSceneManager.roundCountDownToStartTime)  {
                    overlayManager.PlayCountdownAnim(-RoundTimeElapsed);
                    _isCountdownStarted = true;
                }

                if (Time.time - _roundStartTime > 0)
                    _isStarted = true;
            }
            else {
                Global.gameSceneManager.timerDisplay.UpdateDisplay(RoundTimeRemaining);
            }

            if (!IsEnded && RoundTimeRemaining < 0) {
                EndRound();
            }
        }


        void EndRound () {
            if (_isGameOver)
                return;

            _isEnded = true;
            Time.timeScale = 0f;

            if (activeRules.statuesRepairedOnceAtRoundEnd) {
                foreach (Statue statue in Global.gameSceneManager.statues) {
                    statue.Repaired();
                }
            }
            if (activeRules.statuesRotting) {
                foreach (Statue statue in Global.gameSceneManager.statues) {
                    statue.Rot();
                }
            }
            if (activeRules.snowWallBuildFastAndRotting) {
                for (int i = 0 ; i < Global.gameSceneManager.snowWallsParent.childCount ; i++) {
                    Global.gameSceneManager.snowWallsParent.GetChild(i).gameObject.GetComponent<SnowWall>().Rot();
                }
            }

            Global.gameSceneManager.RoundPhaseEnded();
            overlayManager.PlayEndRoundAnim(Global.gameSceneManager.ShowVotingInstance, () => {
                Global.gameSceneManager.currentRoundInstance = null;
                Global.gameSceneManager.OnRoundInstanceRemoved();
                Destroy(gameObject);
            } );
        }


        public void CheckIfGameOver (int[] statuesHP) {

            int[] aliveStatuesCountOfTeams = new int[2] {0, 0};

            for (byte team = 0 ; team < aliveStatuesCountOfTeams.Length ; team++) {
                for (int i = 0 ; i < Global.STATUE_AMOUNT_PER_TEAM ; i++) {

                    if (statuesHP[i + team * Global.STATUE_AMOUNT_PER_TEAM] > 0)
                        aliveStatuesCountOfTeams[team]++;

                }
            }

            if (aliveStatuesCountOfTeams[0] == 0 || aliveStatuesCountOfTeams[1] == 0) {

                if (aliveStatuesCountOfTeams[0] == aliveStatuesCountOfTeams[1]) {
                    GameOver(255);
                }
                else {
                    for (byte team = 0 ; team < aliveStatuesCountOfTeams.Length ; team++) {
                        if (aliveStatuesCountOfTeams[team] > 0)
                            GameOver(team);
                    }
                }
            }
        }

        void GameOver (byte winTeam) {
            _isGameOver = true;
            overlayManager.PlayGameOverAnim(() => Global.gameSceneManager.GoToGameOverScene(winTeam));
        }



        // PUN RaiseEvent
        public void OnEvent (EventData photonEvent) {

            byte eventCode = photonEvent.Code;
            object[] data;

            try {
                data = (object[]) photonEvent.CustomData;

                if (eventCode == (byte) PunRaiseEventCode.SetPlayerState) {

                    int         playerNumber  = (int)                data[0];
                    PlayerState state         = (PlayerState) (byte) data[1];
                    bool        isFacingRight = (bool)               data[2];

                    Global.GetPlayerInstanceInRoomByNumber(playerNumber).GetComponent<PlayerManager>().SetStateFromNetwork(state, isFacingRight);
                }
                if (eventCode == (byte) PunRaiseEventCode.SetPlayerCarriedAmout) {

                    int playerNumber  = (int) data[0];
                    int carriedAmount = (int) data[1];

                    Global.GetPlayerInstanceInRoomByNumber(playerNumber).GetComponent<PlayerManager>().SetCarriedAmount(carriedAmount);
                }
                else if (eventCode == (byte) PunRaiseEventCode.PlayerFiring) {

                    int     eventTimeStamp      = (int)                                            data[0];
                    int     playerNumber        = (int)                                            data[1];
                    Vector2 position            = (Vector2)                                        data[2];
                    Vector2 fireDir             = DataCompression.AngleDegreeToDirection2D((float) data[3]);
                    float   chargingRate        = (float)                                          data[4];
                    bool    isCurveBallDirRight = (bool)                                           data[5];

                    PhotonNetwork.FetchServerTimestamp();
                    float eventTimeCompensation = (eventTimeStamp - PhotonNetwork.ServerTimestamp) / 1000f;

                    StartCoroutine(PlayerFiring(playerNumber, position, fireDir, chargingRate, isCurveBallDirRight, eventTimeCompensation, eventTimeStamp));

                }
                else if (eventCode == (byte) PunRaiseEventCode.PlayerGetHit) {

                    int     eventTimeStamp      = (int)     data[0];
                    int     playerNumber        = (int)     data[1];
                    Vector2 playerPosition      = (Vector2) data[2];
                    int     snowballOwnerNumber = (int)     data[3];
                    int     snowballIdByOwner   = (int)     data[4];

                    Snowball snowball = GetSnowball(snowballOwnerNumber, snowballIdByOwner);

                    if (snowball != null) {
                        PhotonNetwork.FetchServerTimestamp();
                        float eventTimeCompensation = (eventTimeStamp - PhotonNetwork.ServerTimestamp) / 1000f;

                        Global.GetPlayerInstanceInRoomByNumber(playerNumber).GetComponent<PlayerManager>().GetHitFromNetwork(snowballOwnerNumber, snowballIdByOwner, snowball.FlyingDirection, eventTimeCompensation);

                        // if a net player blocks the snowball from a hitted building, cancel the hit to that building.
                        if (snowball.CurrentTargetType == Snowball.TargetType.Statue) {
                            Global.gameSceneManager.statues[snowball.CurrentTargetNumber[0]].AttemptToCancelTheHit(snowball.OwnerNumber, snowball.IdByOwner);
                            snowball.CurrentTargetType = Snowball.TargetType.Player;
                            snowball.CurrentTargetNumber = new int[1] {playerNumber};
                        }
                        snowball.HitTargetEventFromNetwork(Snowball.TargetType.Player, new int[] { playerNumber }, eventTimeCompensation);
                    }
                }
                else if (eventCode == (byte) PunRaiseEventCode.SnowballRebound) {

                    int     eventTimeStamp = (int)                                            data[0];
                    int     ownerNumber    = (int)                                            data[1];
                    int     idByOwner      = (int)                                            data[2];
                    Vector2 pos            = (Vector2)                                        data[3];
                    Vector2 newDir         = DataCompression.AngleDegreeToDirection2D((float) data[4]);

                    PhotonNetwork.FetchServerTimestamp();
                    float eventTimeCompensation = (eventTimeStamp - PhotonNetwork.ServerTimestamp) / 1000f;

                    Snowball snowball = GetSnowball(ownerNumber, idByOwner);
                    if (snowball != null)
                        snowball.ReInitFromRebound(pos, newDir, eventTimeCompensation);

                }
                else if (eventCode == (byte) PunRaiseEventCode.SnowWallStartBuilding) {

                    int                      playerNumber = (int)                              data[0];
                    SnowWall.FacingDirection facing       = (SnowWall.FacingDirection) (byte) (data[1]);

                    Global.GetPlayerInstanceInRoomByNumber(playerNumber).GetComponent<PlayerManager>().SetBuildingBase(facing);

                }
                else if (eventCode == (byte) PunRaiseEventCode.SnowWallBuilt) {

                    int                      playerNumber = (int)                              data[0];
                    Vector2                  position     = (Vector2)                          data[1];
                    SnowWall.FacingDirection facingDir    = (SnowWall.FacingDirection) (byte) (data[2]);

                    GameObject snowWallPrefab = null;
                    bool isFacingLeft = false;

                    if (facingDir == SnowWall.FacingDirection.LeftUp || facingDir == SnowWall.FacingDirection.RightUp) {
                        snowWallPrefab = snowWallRightUpPrefab;
                        if (facingDir == SnowWall.FacingDirection.LeftUp)
                            isFacingLeft = true;
                    }
                    else if (facingDir == SnowWall.FacingDirection.Left || facingDir == SnowWall.FacingDirection.Right) {
                        snowWallPrefab = snowWallRightPrefab;
                        if (facingDir == SnowWall.FacingDirection.Left)
                            isFacingLeft = true;
                    }
                    else if (facingDir == SnowWall.FacingDirection.LeftDown || facingDir == SnowWall.FacingDirection.RightDown) {
                        snowWallPrefab = snowWallRightDownPrefab;
                        if (facingDir == SnowWall.FacingDirection.LeftDown)
                            isFacingLeft = true;
                    }

                    GameObject newSnowWall = Instantiate(snowWallPrefab, Global.GetActualWorldPosition(position), Quaternion.identity, Global.gameSceneManager.snowWallsParent);
                    newSnowWall.GetComponent<SnowWall>().Init(playerNumber, _snowWallIdByOwnerCounters[playerNumber], isFacingLeft);
                    _snowWallIdByOwnerCounters[playerNumber]++;

                    Global.GetPlayerInstanceInRoomByNumber(playerNumber).GetComponent<PlayerManager>().HasBuilt();

                }
                else if (eventCode == (byte) PunRaiseEventCode.StatueGetHit) {

                    int eventTimeStamp      = (int) data[0];
                    int statueNumber        = (int) data[1];
                    int snowballOwnerNumber = (int) data[2];
                    int snowballIdByOwner   = (int) data[3];

                    PhotonNetwork.FetchServerTimestamp();
                    float elapsedTime = (PhotonNetwork.ServerTimestamp - eventTimeStamp) / 1000f;

                    if (elapsedTime <= Global.globalManager.lagTimeTolerance) {
                        Global.gameSceneManager.statues[statueNumber].GetHitFromNetwork(snowballOwnerNumber, snowballIdByOwner);
                    }

                }
                else if (eventCode == (byte) PunRaiseEventCode.StatueRepaired) {

                    int eventTimeStamp = (int) data[0];
                    int playerNumber   = (int) data[1];
                    int statueNumber   = (int) data[2];

                    PhotonNetwork.FetchServerTimestamp();
                    float elapsedTime = (PhotonNetwork.ServerTimestamp - eventTimeStamp) / 1000f;

                    if (elapsedTime <= Global.globalManager.lagTimeTolerance) {
                        Global.gameSceneManager.statues[statueNumber].RepairedFromNetwork(playerNumber);
                    }
                }
                else if (eventCode == (byte) PunRaiseEventCode.SnowWallGetHit) {

                    int eventTimeStamp      = (int) data[0];
                    int snowWallOwnerNumber = (int) data[1];
                    int snowWallIdByOwner   = (int) data[2];
                    int snowballOwnerNumber = (int) data[3];
                    int snowballIdByOwner   = (int) data[4];

                    PhotonNetwork.FetchServerTimestamp();
                    float elapsedTime = (PhotonNetwork.ServerTimestamp - eventTimeStamp) / 1000f;

                    if (elapsedTime <= Global.globalManager.lagTimeTolerance) {
                        SnowWall snowWall = Global.gameSceneManager.GetSnowWall(snowWallOwnerNumber, snowWallIdByOwner);

                        if (snowWall != null)
                            snowWall.GetHitFromNetwork(snowballOwnerNumber, snowballIdByOwner);
                    }

                }

            }
            catch (System.InvalidCastException e) {
                print(e);
            }

        }


        void SetFixedRules (FixedRule[] rules) {
            foreach (var rule in rules) {
                if      (rule == FixedRule.StatuesRepairedOnceAtRoundEnd) activeRules.statuesRepairedOnceAtRoundEnd = true;
                else if (rule == FixedRule.RepairProhibited)              activeRules.repairProhibited              = true;
                else if (rule == FixedRule.BuildProhibited)               activeRules.buildProhibited               = true;
                else if (rule == FixedRule.StatuesRotting)                activeRules.statuesRotting                = true;
            }
        }

        void SetOptionalRules (OptionalRule[] rules) {
            foreach (var rule in rules) {
                // Player
                if      (rule == OptionalRule.KnockOutFarther)             activeRules.knockOutFarther     = true;
                else if (rule == OptionalRule.KnockDownProhibited)         activeRules.knockDownProhibited = true;
                else if (rule == OptionalRule.TurnProhibited)              activeRules.turnProhibited      = true;
                else if (rule == OptionalRule.HighGravity)                 activeRules.highGravity         = true;

                // Snowball)
                // else if (rule == OptionalRule.FireNeedCharge)              activeRules.fireNeedCharge = true;
                else if (rule == OptionalRule.Fire2GoesV)                  activeRules.fire2GoesV = true;
                else if (rule == OptionalRule.SlowerAndCollidableSnowball) activeRules.slowerAndCollidableSnowball = true;
                else if (rule == OptionalRule.KnuckleBall)                 activeRules.knuckleBall = true;
                else if (rule == OptionalRule.CurveBall)                   activeRules.curveBall = true;

                // Building)
                else if (rule == OptionalRule.SnowWallBuildFastAndRotting) activeRules.snowWallBuildFastAndRotting = true;
                else if (rule == OptionalRule.SnowWallCostDown)            activeRules.snowWallCostDown = true;
                else if (rule == OptionalRule.EggsWallMovable)             activeRules.eggsWallMovable = true;
                else if (rule == OptionalRule.TravelingStatue)             activeRules.travelingStatue = true;

                // Special)
                // else if (rule == OptionalRule.NarrowView)                  activeRules.narrowView = true;
                else if (rule == OptionalRule.DuckSnowCake)                activeRules.duckSnowCake = true;
            }

            if (activeRules.slowerAndCollidableSnowball) {
                Physics2D.IgnoreLayerCollision(Global.OWNED_PROJECTILE_LAYER, Global.PROJECTILE_LAYER, false);
                Physics2D.IgnoreLayerCollision(Global.PROJECTILE_LAYER, Global.PROJECTILE_LAYER, false);
            }
        }


        IEnumerator PlayerFiring (int playerNumber, Vector2 position, Vector2 direction, float chargingRate, bool isCurveBallDirRight, float eventTimeCompensation, int knuckleBallSeed = 0) {

            float spawnTimeCompensation = 0f;
            float waitingTime = playerProps.snowballLaunchTime + eventTimeCompensation;
            if (waitingTime > 0)
                yield return new WaitForSeconds(waitingTime);
            else
                spawnTimeCompensation = waitingTime;


            Vector2 curveDir = Vector2.zero;

            if (activeRules.fire2GoesV) {
                for (int i = -1 ; i <= 1 ; i += 2) {
                    Vector2 actualDir = Quaternion.AngleAxis(playerProps.fire2GoesVDeflectionAngle, Vector3.forward * i) * direction;
                    if (activeRules.curveBall)
                        curveDir = Snowball.GetCurveDir(actualDir, isCurveBallDirRight);

                    SpawnSnowball(playerNumber, spawnTimeCompensation, position, actualDir, curveDir, chargingRate, knuckleBallSeed);
                }
            }
            else {
                if (activeRules.curveBall)
                    curveDir = Snowball.GetCurveDir(direction, isCurveBallDirRight);

                SpawnSnowball(playerNumber, spawnTimeCompensation, position, direction, curveDir, chargingRate, knuckleBallSeed);
            }
        }

        void SpawnSnowball (int playerNumber, float spawnTimeCompensation, Vector2 position, Vector2 direction, Vector2 curveDir, float chargingRate = 1f, int knuckBallSeed = 0) {

            GameObject snowball = Instantiate(snowballPrefab, Global.GetActualWorldPosition(position), Quaternion.identity, snowballsParent);
            snowball.GetComponent<Snowball>().Init(playerNumber, _snowballIdByOwnerCounters[playerNumber], spawnTimeCompensation, position, direction, curveDir, chargingRate, knuckBallSeed);
            _snowballIdByOwnerCounters[playerNumber] = (_snowballIdByOwnerCounters[playerNumber] + 1) % SNOWBALL_ID_CEILING;
        }




        public float GetPlayerActionTime (PlayerState actionState) {
            if      (actionState == PlayerState.Turning)               return playerProps.turnTime;
            else if (actionState == PlayerState.Collecting)            return playerProps.collectTime;
            else if (actionState == PlayerState.Firing)                return playerProps.fireTime;

            else if (actionState == PlayerState.Building)
                if (activeRules.snowWallBuildFastAndRotting)           return playerProps.buildTimeShorter;
                else                                                   return playerProps.buildTime;

            else if (actionState == PlayerState.Repairing)             return playerProps.repairTime;

            else if (actionState == PlayerState.KnockedOut)
                if (activeRules.knockDownProhibited)                   return playerProps.knockedBackTime;
                else                                                   return playerProps.knockedOutFlyTime;

            else if (actionState == PlayerState.Downed)
                if (activeRules.knockDownProhibited)                   return 0f;
                else                                                   return playerProps.downedTime;

            else if (actionState == PlayerState.Recovering)
                if (activeRules.knockDownProhibited)                   return 0f;
                else                                                   return playerProps.recoverTime;

            else if (actionState == PlayerState.Quacking)              return playerProps.quackTime;
            else if (actionState == PlayerState.SwingingCake)          return playerProps.swingCakeTime;
            else                                                       return 0f;
        }

        public Snowball GetSnowball (int ownerNumber, int idByOwner) {

            for (int i = 0 ; i < snowballsParent.childCount ; i++) {
                Snowball snowball = snowballsParent.GetChild(i).gameObject.GetComponent<Snowball>();

                if (snowball.OwnerNumber == ownerNumber && snowball.IdByOwner == idByOwner)
                    return snowball;
            }
            return null;
        }




    }
}
