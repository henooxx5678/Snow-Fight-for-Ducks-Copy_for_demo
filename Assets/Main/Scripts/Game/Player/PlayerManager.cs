using System.Collections;
using System.Collections.Generic;

﻿using UnityEngine;

using DG.Tweening;
using Photon.Realtime;
using Photon.Pun;

using DoubleHeat;
using DoubleHeat.Utilities;

namespace DoubleHeat.SnowFightForDucksGame {

    public enum PlayerState {
        Idle,
        Walking,
        Turning,
        Collecting,
        Firing,
        Building,
        Repairing,
        KnockedOut,
        Downed,
        Recovering,
        Quacking,
        SwingingCake,
        MovingWall
    }

    [DisallowMultipleComponent]
    public class PlayerManager : MonoBehaviourPun {

        const float CARRIED_REL_POS_Z = -0.0001f;
        const float DUCK_SNOW_CAKE_REL_POS_Z = 0.0001f;

        public static PlayerManager LocalPlayerManager;


    // <editor-fold> ===== Public Variables =====
        // #region ===== SERIALIZED_VARIABLES =====
        public Transform   spritesTrans;
        public Transform   carriedSpriteTrans;
        public Transform   duckSnowCakeSpriteTrans;
        public Transform   nameDisplayPositionTrans;
        public Rigidbody2D rb;
        public GameObject  hitBox;
        public GameObject  duckSnowCakeHitBox;
        public LayerMask   movementCollisionLayerMask;
        public LayerMask   repairTargetDetectLayerMask;
        public LayerMask   moveWallTargetDetectLayerMask;

        public PlayerFireDirInstructorManager fireDirInstructorManager;
        public PlayerBuildPreviewManager buildPreviewManager;

        public float hitBoxMovingShiftTime;
        // #endregion ***********************************


        // #region ===== COMPONENTS =====
        PlayerMoveManager _moveManager;
        public PlayerMoveManager moveManager {
            get {
                if (_moveManager == null)
                    _moveManager = GetComponent<PlayerMoveManager>();
                return _moveManager;
            }
        }
        PlayerAnimationManager _animManager;
        public PlayerAnimationManager animManager {
            get {
                if (_animManager == null)
                    _animManager = GetComponent<PlayerAnimationManager>();
                return _animManager;
            }
        }
        // #endregion ********************


        public PlayerState CurrentState     => _currentState;
        public bool        IsActionable     => _currentState == PlayerState.Idle || _currentState == PlayerState.Walking || _currentState == PlayerState.Turning;
        public Vector2     FacingDirection  => _isFacingRight ? Vector2.right : Vector2.left;
        public int         CarriedAmount    => _carriedAmount;
        public Vector2     SwingDirection   => _thisSwingDir;
        public Vector2     DirectionToMouse => Global.GetPositionOnGround((Vector2) (Global.GetMouseWorldPosition() - (transform.position + Vector3.up * Snowball.START_BODY_HEIGHT) )).normalized;

        public Vector2 PositionOnGound {
            get => Global.GetPositionOnGround(transform.position);
            set => transform.position = Global.GetActualWorldPosition(value);
        }

    // </editor-fold>

    // <editor-fold> ===== Private Variables =====
        // Components
        GameObject _nameDisplayGO;

        int         _carriedAmount    = 0;
        // Vector2     _positionOnGround = Vector2.zero;
        Vector2     _walkVelocity     = Vector2.zero;
        Vector2     _thisSwingDir     = Vector2.zero;
        bool        _isFacingRight    = true;
        PlayerState _currentState     = PlayerState.Idle;
        Coroutine   _currentDoingAction;
        bool        _hasDuckSnowCake = false;
        bool        _isDuckSnowCakeHitBoxActive = false;

        // temps
        bool    _hasTurned = false;
        Vector2 _knockedOutDir = Vector2.zero;
        bool    _isCurrentSnowWallBuildFacingSet = false;
        SnowWall.FacingDirection _currentSnowWallBuildFacing;
        GameObject _currentTempBuiltShowing;
    // </editor-fold>


    // <editor-fold> ===== MonoBehaviour Methods =====
        void Awake () {

            if (Global.CurrentRoundInstance == null) {
                Destroy(gameObject);
            }
            else {
                transform.SetParent(Global.CurrentRoundInstance.transform);
                photonView.Owner.TagObject = gameObject;

                int playerNumber = photonView.Owner.ActorNumber;
                Init( NetEvent.GetPlayerDuckSkin(playerNumber), NetEvent.GetPlayerTeam(NetEvent.GetCurrentPlayerInSeats(), playerNumber) );


                if (photonView.IsMine) {
                    LocalPlayerManager = this;
                }

                // _positionOnGround = Global.GetPositionOnGround(transform.position);
            }

        }

        void OnEnable () {
            if (_nameDisplayGO != null)
                _nameDisplayGO.SetActive(true);
        }

        void OnDisable () {
            if (_nameDisplayGO != null)
                _nameDisplayGO.SetActive(false);
        }

        public void Init (DuckSkin skin, byte team) {
            animManager.currentDuckSkin = skin;
            SetFacingRight(team == 0 ? true : false);
        }

        void Start () {

            if (!photonView.IsMine) {
                Destroy(fireDirInstructorManager.gameObject);
            }


            _nameDisplayGO = Global.CurrentRoundInstance.playersNameDisplayManager.Register(photonView.Owner.NickName, nameDisplayPositionTrans);


            duckSnowCakeSpriteTrans.gameObject.SetActive(false);
            duckSnowCakeHitBox.SetActive(false);

            if (Global.globalManager.permanentDuckSnowCake) {
                _hasDuckSnowCake = true;
                UpdateDuckSnowCakeSprite();
            }

        }


        void OnDestroy () {

            if (_nameDisplayGO != null)
                Destroy(_nameDisplayGO);

            if (photonView.IsMine)
                LocalPlayerManager = null;

        }

        void Update () {
            if (Global.CurrentRoundInstance == null || Global.CurrentRoundInstance.IsEnded)
                return;


            if (photonView.IsMine) {

                // facing
                if (moveManager != null) {
                    if (!Global.CurrentRoundInstance.activeRules.turnProhibited) {
                        if (_currentState == PlayerState.Turning) {
                            if (!_hasTurned && Vector2.Dot(FacingDirection, moveManager.MovingLeftOrRightDir) > 0) {
                                BackToIdle();
                            }
                            else if (_hasTurned && Vector2.Dot(FacingDirection, moveManager.MovingLeftOrRightDir) < 0) {
                                BackToIdle();
                            }
                        }
                        else if (IsActionable) {
                            if (Vector2.Dot(FacingDirection, moveManager.MovingLeftOrRightDir) < 0) {
                                Turn();
                                _hasTurned = false;
                            }
                            else if (_isFacingRight == true && Vector2.Dot(_walkVelocity, Vector2.left) > 0)
                                SetFacingRight(false);
                            else if (_isFacingRight == false && Vector2.Dot(_walkVelocity, Vector2.right) > 0)
                                SetFacingRight(true);
                        }
                    }
                }

            }
            else {
                // Vector2 currentPos = Global.GetPositionOnGround(transform.position);
                // _walkVelocity = (currentPos - _positionOnGround) / Time.deltaTime;  // not a precise value
                // _positionOnGround = currentPos;
            }


            // state
            if (_currentState == PlayerState.Walking && _walkVelocity == Vector2.zero)
                _currentState = PlayerState.Idle;
            else if (_currentState == PlayerState.Idle && _walkVelocity != Vector2.zero)
                _currentState = PlayerState.Walking;


            // Snow wall building base showing
            if (_currentState == PlayerState.Building) {
                if (_isCurrentSnowWallBuildFacingSet) {
                    buildPreviewManager.ShowBuildingBase(_currentSnowWallBuildFacing);
                }
            }
            else {
                _isCurrentSnowWallBuildFacingSet = false;
            }

            // Let carried sprite always in front ; Let duck snow cake sprite always in back
            carriedSpriteTrans.position = Global.ApplyZToVector(carriedSpriteTrans.position, spritesTrans.position.z + CARRIED_REL_POS_Z);
            duckSnowCakeSpriteTrans.position = Global.ApplyZToVector(duckSnowCakeSpriteTrans.position, spritesTrans.position.z + DUCK_SNOW_CAKE_REL_POS_Z);


            // Shift hit box when moving
            Vector2 shiftingVector = Vector2.zero;
            if (photonView.IsMine)
                shiftingVector = -_walkVelocity;
            else
                shiftingVector = _walkVelocity;

            hitBox.transform.localPosition = Vector2.MoveTowards(Vector2.zero, shiftingVector * hitBoxMovingShiftTime, Global.CurrentRoundInstance.playerProps.maxHitBoxMovingShiftDistance);
        }
    // </editor-fold>


    // <editor-fold> ===== Public Methods =====
        public void TryToCollect () {
            if (IsActionable)
                if (_carriedAmount < Global.CurrentRoundInstance.CurrentPlayerMaxCarriedAmount ) {
                    Collect();
                }
        }
        public void TryToFire (bool isCurveBallDirRight = true) {
            if (IsActionable)
                if (_carriedAmount >= Global.CurrentRoundInstance.CurrentFireCost || Global.globalManager.infiniteFiring) {
                    Vector2 dir = Global.GetActualWorldDirection(DirectionToMouse);
                    if (IsLegalDirection(dir)) {
                        Fire(dir, isCurveBallDirRight);
                    }
                }
        }
        public void TryToReleaseChargeToFire (float chargingRate) {
            // do it later

        }
        public void TryToBuild () {
            if (IsActionable)
                if (_carriedAmount >= Global.CurrentRoundInstance.CurrentBuildCost || Global.globalManager.infiniteBuilding) {
                   Vector2 dir = Global.GetActualWorldDirection(DirectionToMouse);
                   if (IsLegalDirection(dir) && buildPreviewManager.IsBuildLegal) {
                       Build(dir);
                   }
                }
        }
        public void TryToRepair () {
            if (IsActionable)
                if (_carriedAmount >= Global.CurrentRoundInstance.CurrentRepairCost || Global.globalManager.infiniteRepairing) {
                    Statue target = TryToGetRepairTarget();
                    if (target != null && target.IsRepairable)
                        Repair(target);
                }
        }
        public void TryToQuack () {
            if (IsActionable)
                Quack();
        }
        public void TryToSwingCake () {
            if (IsActionable)
                if (_hasDuckSnowCake) {
                    Vector2 dir = Global.GetActualWorldDirection(DirectionToMouse);
                    if (IsLegalDirection(dir)) {
                        SwingCake(dir);
                    }
                }
        }
        public void TryToMoveWall () {
            if (IsActionable) {
                EggsWall target = TryToGetMoveWallTarget();
                if (target != null && !target.IsMovedByPlayer)
                    StartToMoveWall(target);
            }

        }



        public void ShowFireDirInstructor (Color color) {
            if (fireDirInstructorManager != null)
                fireDirInstructorManager.Show(color, DirectionToMouse);
        }

        public void ShowBuildPreview () {
            buildPreviewManager.ShowPreview(SnowWall.GetFacingType(DirectionToMouse));
        }

        public void RevealRepairableTarget () {
            Statue statue = TryToGetRepairTarget();
            if (statue != null && statue.IsRepairable)
                statue.HighlightedForRepairableTarget();
        }

        public void SetBuildingBase (SnowWall.FacingDirection facing) {
            _currentSnowWallBuildFacing = facing;
            _isCurrentSnowWallBuildFacingSet = true;
        }

        public void SetCarriedAmount (int amount) {
            _carriedAmount = amount;
            _carriedAmount = System.Math.Max(_carriedAmount, 0);
            _carriedAmount = System.Math.Min(_carriedAmount, Global.CurrentRoundInstance.CurrentPlayerMaxCarriedAmount);
        }

        public void TurnByAnim () {
            if (_currentState == PlayerState.Turning) {
                SetFacingRight(!_isFacingRight);

                if (photonView.IsMine)
                    _hasTurned = true;
            }
        }


        public void GetHit (int snowballOwnerNumber, int snowballIdByOwner, Vector2 snowballDir, float eventTimeCompensation = 0f) {

            KnockedOut(snowballDir, eventTimeCompensation);

            if (Global.CurrentRoundInstance.activeRules.duckSnowCake) {
                _hasDuckSnowCake = true;
                UpdateDuckSnowCakeSprite();
            }

            // Statistics
            int shooter = snowballOwnerNumber;

            int[,] playerInSeats = NetEvent.GetCurrentPlayerInSeats();
            byte shooterTeam = NetEvent.GetPlayerTeam(playerInSeats, shooter);

            Dictionary<int, PlayerStatistics> playersStats = Global.gameSceneManager.playersStatistics;

            if (shooterTeam == NetEvent.GetPlayerTeam(playerInSeats, photonView.Owner.ActorNumber))
                playersStats[shooter].allyHits++;
            else
                playersStats[shooter].opponentHits++;

            playersStats[photonView.Owner.ActorNumber].getHitTimes++;
        }

        public void DuckSnowCakeHit () {
            if (!Global.globalManager.permanentDuckSnowCake) {
                _hasDuckSnowCake = false;
                UpdateDuckSnowCakeSprite();
            }
        }

        // #region ===== FROM_NETWORK =====
        public void SetStateFromNetwork (PlayerState state, bool isFacingRight) {
            if (!photonView.IsMine) {
                _currentState = state;

                if (state == PlayerState.Turning || state == PlayerState.Firing || state == PlayerState.Building || state == PlayerState.Repairing || state == PlayerState.SwingingCake)
                    SetFacingRight(isFacingRight);

                animManager.PlayCurrentFromStart();
            }
        }

        public void GetHitFromNetwork (int snowballOwnerNumber, int snowballIdByOwner, Vector2 snowballDir, float eventTimeCompensation) {
            if (_currentState == PlayerState.KnockedOut) {
                KnockedOut(snowballDir, eventTimeCompensation);
            }
            else {
                GetHit(snowballOwnerNumber, snowballIdByOwner, snowballDir, eventTimeCompensation);
            }
        }

        public void HasBuilt () {
            if (_currentTempBuiltShowing!= null)
                Destroy(_currentTempBuiltShowing);

            _currentTempBuiltShowing = null;
        }
        // #endregion

    // </editor-fold>


    // <editor-fold> ===== Private Methods =====
        Statue TryToGetRepairTarget () {
            TargetedByPlayerDetector targetedByPlayerDetector = TryToTargetObject(DirectionToMouse, Global.CurrentRoundInstance.playerProps.repairDistance, repairTargetDetectLayerMask);

            if (targetedByPlayerDetector != null && targetedByPlayerDetector.Type == TargetableByPlayer.Statue) {
                return ((StatueTargetedByPlayerDetector) targetedByPlayerDetector).statue;
            }
            return null;
        }

        EggsWall TryToGetMoveWallTarget () {
            TargetedByPlayerDetector targetedByPlayerDetector = TryToTargetObject(DirectionToMouse, Global.CurrentRoundInstance.playerProps.moveWallDistance, moveWallTargetDetectLayerMask);

            if (targetedByPlayerDetector != null && targetedByPlayerDetector.Type == TargetableByPlayer.EggsWall) {
                return ((EggsWallTargetedByPlayerDetector) targetedByPlayerDetector).eggsWall;
            }
            return null;
        }

        TargetedByPlayerDetector TryToTargetObject (Vector2 dir, float distance, LayerMask layerMask) {

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, Global.CurrentRoundInstance.playerProps.targetingDetectRadius, dir, distance, layerMask);
            if (hit.collider != null)
                return hit.collider.gameObject.GetComponent<TargetedByPlayerDetector>();
            return null;
        }


        void DoAction (IEnumerator action) {
            if (_currentDoingAction != null) {
                StopCoroutine(_currentDoingAction);
            }
            _currentDoingAction = StartCoroutine(action);
        }


        // == Actions ==
        delegate void ActionUpdate ();
        delegate void ActionComplete ();

        IEnumerator Action (ActionUpdate update, ActionComplete complete, float actionTimeCompensation = 0f) {

            NetEvent.EmitSetPlayerStateEvent(PhotonNetwork.LocalPlayer.ActorNumber, _currentState, _isFacingRight);

            float duration = Global.CurrentRoundInstance.GetPlayerActionTime(_currentState);
            float startTime = Time.time + actionTimeCompensation;

            while (Time.time - startTime < duration) {
                update();
                yield return null;
            }

            complete();
        }

        void BackToIdle () {
            _currentState = PlayerState.Idle;
            if (_currentDoingAction != null)
                StopCoroutine(_currentDoingAction);

            _currentDoingAction = null;

            hitBox.SetActive(true);
            duckSnowCakeHitBox.SetActive(false);

            NetEvent.EmitSetPlayerStateEvent(PhotonNetwork.LocalPlayer.ActorNumber, _currentState, _isFacingRight);
        }

        void Turn () {
            _currentState = PlayerState.Turning;

            DoAction( Action(
                () => {},
                () => { BackToIdle(); }
            ) );
        }

        // #region ===== ACTIVE_ACTIONS =====
        void Collect () {
            _currentState = PlayerState.Collecting;

            DoAction( Action(
                () => {},
                () => {
                    ChangeCarriedAmount(Global.CurrentRoundInstance.CurrentCollectAmount);
                    BackToIdle();
                }
            ) );
        }
        void Fire (Vector2 fireDir, bool isCurveBallDirRight) {
            _currentState = PlayerState.Firing;
            _walkVelocity = Vector2.zero;
            AwareFacing(fireDir);
            ChangeCarriedAmount(-Global.CurrentRoundInstance.CurrentFireCost);

            NetEvent.EmitPlayerFiringEvent(PhotonNetwork.LocalPlayer.ActorNumber, PositionOnGound, fireDir, 1f, isCurveBallDirRight);

            DoAction( Action(
                () => {},
                () => { BackToIdle(); }
            ) );
        }
        void Build (Vector2 dir) {
            _currentState = PlayerState.Building;
            _walkVelocity = Vector2.zero;
            AwareFacing(dir);

            SnowWall.FacingDirection snowWallfacing = SnowWall.GetFacingType(dir);
            SetBuildingBase(snowWallfacing);

            DoAction( Action(
                () => {
                    if (!Global.gameSceneManager.inputManager.FireButton)
                        BackToIdle();
                },
                () => {
                    ChangeCarriedAmount(-Global.CurrentRoundInstance.CurrentBuildCost);

                    _currentTempBuiltShowing = buildPreviewManager.GenerateTempBuiltShowing(snowWallfacing);

                    Vector2 pos = PositionOnGound + SnowWall.GetVectorByFacingDir(snowWallfacing) * Global.CurrentRoundInstance.playerProps.buildDistance;
                    NetEvent.EmitSnowWallBuilt(PhotonNetwork.LocalPlayer.ActorNumber, pos, snowWallfacing);
                    BackToIdle();
                }
            ) );
        }
        void Repair (Statue target) {
            _currentState = PlayerState.Repairing;
            _walkVelocity = Vector2.zero;
            AwareFacing(target.transform.position - transform.position);

            DoAction( Action(
                () => {
                    if (!Global.gameSceneManager.inputManager.FireButton)
                        BackToIdle();
                    else
                        target.HighlightedForRepairableTarget();
                },
                () => {
                    ChangeCarriedAmount(-Global.CurrentRoundInstance.CurrentRepairCost);
                    target.RepairedFromLocal(photonView.Owner.ActorNumber);
                    NetEvent.EmitStatueRepairedEvent(photonView.Owner.ActorNumber, target.StatueNumber);
                    BackToIdle();
                }
            ) );
        }
        void Quack () {
            _currentState = PlayerState.Quacking;

            DoAction( Action(
                () => {},
                () => { BackToIdle(); }
            ) );
        }
        void SwingCake (Vector2 swingDir) {
            _currentState = PlayerState.SwingingCake;
            _thisSwingDir = swingDir;
            AwareFacing(swingDir);
            duckSnowCakeHitBox.transform.rotation = Quaternion.FromToRotation(Vector2.right, swingDir);
            float startTime = Time.time;

            DoAction( Action(
                () => {
                    float passedTime = Time.time - startTime;
                    if (passedTime > Global.CurrentRoundInstance.playerProps.cakeHitStartTime) {

                        if (passedTime < Global.CurrentRoundInstance.playerProps.cakeHitStopTime) {
                            if (!_isDuckSnowCakeHitBoxActive) {
                                duckSnowCakeHitBox.SetActive(true);
                                _isDuckSnowCakeHitBoxActive = true;
                            }
                        }
                        else {
                            if (_isDuckSnowCakeHitBoxActive) {
                                duckSnowCakeHitBox.SetActive(false);
                                _isDuckSnowCakeHitBoxActive = false;
                            }
                        }
                    }
                },
                () => {
                    _thisSwingDir = Vector2.zero;
                    BackToIdle();
                }
            ) );
        }
        void StartToMoveWall (EggsWall target) {
            _currentState = PlayerState.MovingWall;
        }
        // #endregion

        // #region ===== PASSIVE_ACTIONS =====
        void KnockedOut (Vector2 knockedDir, float actionTimeCompensation = 0f) {
            _currentState = PlayerState.KnockedOut;
            _walkVelocity = Vector2.zero;
            hitBox.SetActive(false);

            Vector2 startPos = PositionOnGound;
            float knockOutDistance = Global.CurrentRoundInstance.CurrentKnockedOutDistance;
            float bodyRotateAngle = ((Vector2.Dot(FacingDirection, knockedDir) > 0) ? -1 : 1) * Global.CurrentRoundInstance.playerProps.knockedOutRotateDegree;
            float prevKnockedOutDistance = 0f;
            float currentKnockedOutDistance = 0f;
            DOTween.To(() => currentKnockedOutDistance, x => currentKnockedOutDistance = x, knockOutDistance, Global.CurrentRoundInstance.playerProps.knockedOutFlyTime)
                .SetEase(Ease.OutCirc);

            photonView.Synchronization = ViewSynchronization.Off;

            DoAction( Action(
                () => {
                    if (!Global.CurrentRoundInstance.activeRules.knockDownProhibited) {
                        float distanceRate = currentKnockedOutDistance / knockOutDistance;
                        SetBodyRotation(distanceRate * bodyRotateAngle);
                    }

                    float deltaDistance = currentKnockedOutDistance - prevKnockedOutDistance;
                    moveManager.TryToMoveAndWatchObstacle(knockedDir, deltaDistance);
                    prevKnockedOutDistance = currentKnockedOutDistance;
                },
                () => {
                    if (!Global.CurrentRoundInstance.activeRules.knockDownProhibited) {
                        SetBodyRotation(bodyRotateAngle);
                        Downed(knockedDir);
                    }
                    else {
                        BackToIdle();
                    }
                },
                actionTimeCompensation
            ) );
        }

        void Downed (Vector2 knockedDir) {
            _currentState = PlayerState.Downed;

            photonView.Synchronization = ViewSynchronization.UnreliableOnChange;

            DoAction( Action(
                () => {},
                () => {
                    Recover(knockedDir);
                }
            ) );
        }

        void Recover (Vector2 knockedDir) {

            _currentState = PlayerState.Recovering;
            float bodyRotateAngle = ((Vector2.Dot(FacingDirection, knockedDir) > 0) ? -1 : 1) * Global.CurrentRoundInstance.playerProps.knockedOutRotateDegree;
            float rotationRate = 1f;
            DOTween.To(() => rotationRate, x => rotationRate = x, 0f, Global.CurrentRoundInstance.playerProps.recoverTime)
                .SetEase(Ease.OutSine);

            DoAction( Action(
                () => {
                    SetBodyRotation(rotationRate * bodyRotateAngle);
                },
                () => {
                    SetBodyRotation(0f);
                    BackToIdle();
                }
            ) );
        }
        // #endregion


        void AwareFacing (Vector2 fireDir) {
            if (Vector2.Dot(fireDir, FacingDirection) < 0)
                SetFacingRight(!_isFacingRight);
        }


        void ChangeCarriedAmount (int deltaAmount) {
            if (photonView.IsMine) {
                SetCarriedAmount(_carriedAmount + deltaAmount);
                NetEvent.EmitSetPlayerCarriedAmountEvent(PhotonNetwork.LocalPlayer.ActorNumber, _carriedAmount);
            }
        }

        bool IsLegalDirection (Vector2 actionDir) {
            return !Global.CurrentRoundInstance.activeRules.turnProhibited || Vector2.Dot(actionDir, FacingDirection) >= 0;
        }

        void SetFacingRight (bool facingRight) {
            _isFacingRight = facingRight;
            spritesTrans.rotation = (_isFacingRight ? Quaternion.identity : Global.horizontalFlipRotation);
            animManager.quackWordSR.flipX = !facingRight;
        }

        void SetBodyRotation (float angle) {
            spritesTrans.rotation = (_isFacingRight ? Quaternion.identity : Global.horizontalFlipRotation) * Quaternion.AngleAxis(angle, Vector3.forward);
        }

        void UpdateDuckSnowCakeSprite () {
            duckSnowCakeSpriteTrans.gameObject.SetActive(_hasDuckSnowCake);
        }
    // </editor-fold>

    }
}
