using System.Collections;

using UnityEngine;

using DG.Tweening;
using Photon.Realtime;
using Photon.Pun;

using DoubleHeat.Utilities;

namespace DoubleHeat.SnowFightForDucksGame {

    [DisallowMultipleComponent]
    public class Snowball : MonoBehaviour {

        public enum TargetType {
            Player,
            Statue,
            SnowWall,
            Immortal,
            DuckSnowCake,
            None
        }

        public const int OWNED_PROJECTILE_LAYER = 13;
        public const float START_BODY_HEIGHT = 0.3162f;
        public static readonly Quaternion SMASH_ON_GROUND_BODY_SPRITE_ROTATION = new Quaternion(0f, 0f, -0.7071068f, 0.7071068f);


        public SnowballAnimationManager animManager;
        public Transform                bodySpriteTrans;
        public Rigidbody2D              rb;

        [Range(0f, 1f)]
        public float droppingStartTravelingRate;


        public Vector2 FlyingDirection          => _currentFlyingDir;
        public int     OwnerNumber              => _ownerNumber;
        public int     IdByOwner                => _idByOwner;
        public bool    IsCurrentTargetPredicted => _isCurrentTargetPredicted;

        public TargetType CurrentTargetType {
            get => _currentTargetType;
            set => _currentTargetType = value;
        }
        public int[] CurrentTargetNumber {
            get => _currentTargetNumber;
            set => _currentTargetNumber = value;
        }


        int     _ownerNumber;
        int     _idByOwner;
        float   _startTime;
        Vector2 _startPos;
        Vector2 _startDir;
        float   _startBodyLocalHeight;
        float   _flyingDistance = 0f;
        float   _flyingDuration = 0f;
        Vector2 _curveDir = Vector2.zero;
        float[] _knuckleBallPathValues;

        Vector2 _positionOnGround;

        Vector2 _currentFlyingDir;
        bool    _isInited = false;
        bool    _isSmashing = false;

        TargetType _currentTargetType = TargetType.None;
        int[]      _currentTargetNumber = new int[0];
        bool       _isCurrentTargetPredicted = false;


        public static Vector2 GetCurveDir (Vector2 snowballDir, bool isCurveBallDirRight) {
            return Quaternion.AngleAxis(isCurveBallDirRight ? -90 : 90, Vector3.forward) * snowballDir;
        }


        public void Init (int playerNumber, int idByOwner, float spawnedTimeCompensation, Vector2 spawnedPos, Vector2 dir, Vector2 curveDir, float chargingRate = 1f, int knuckleBallSeed = 0) {

            float flyingSpeed = chargingRate * Global.CurrentRoundInstance.CurrentSnowballSpeed;
            _flyingDistance   = chargingRate * Global.CurrentRoundInstance.CurrentSnowballDistance;

            _ownerNumber          = playerNumber;
            _idByOwner            = idByOwner;
            _startTime            = Time.time + spawnedTimeCompensation;
            _startPos             = spawnedPos;
            _startDir             = dir;
            _startBodyLocalHeight = bodySpriteTrans.localPosition.y;
            _flyingDuration       = _flyingDistance / flyingSpeed;
            _curveDir             = curveDir;

            _currentFlyingDir = dir;

            if (_ownerNumber == PhotonNetwork.LocalPlayer.ActorNumber) {
                gameObject.layer = OWNED_PROJECTILE_LAYER;
                for (int i = 0 ; i < transform.childCount ; i++) {
                    transform.GetChild(i).gameObject.layer = OWNED_PROJECTILE_LAYER;
                }
            }

            if (_flyingDuration <= 0) {
                Destroy(gameObject);
            }

            if (Global.CurrentRoundInstance.activeRules.knuckleBall) {
                int pointAmount = Global.CurrentRoundInstance.snowballProps.knuckleBallShiftPointAmount;
                float range = Global.CurrentRoundInstance.snowballProps.knuckleBallShiftTotalRange / pointAmount;
                _knuckleBallPathValues = new float[pointAmount];

                Random.InitState(knuckleBallSeed);
                for (int i = 0 ; i < _knuckleBallPathValues.Length ; i++) {
                    _knuckleBallPathValues[i] = Random.Range(-range, range);
                    if (i > 0)
                        _knuckleBallPathValues[i] += _knuckleBallPathValues[i - 1];
                }
            }

            _isInited = true;


            // Statistics
            Global.gameSceneManager.playersStatistics[playerNumber].firedAmount++;
        }

        public void ReInitFromRebound (Vector2 startPos, Vector2 dir, float startTimeCompensation = 0f) {
            _startTime = Time.time + startTimeCompensation;
            _startPos  = startPos;
            _startDir  = dir;
            _curveDir  = Vector3.Cross(dir, Vector3.Cross(_startDir, _curveDir));

            _currentFlyingDir = dir;
        }

        void Update () {
            if (!_isInited || _isSmashing || Global.CurrentRoundInstance == null || !Global.CurrentRoundInstance.IsRunning)
                return;

            SetPosition(Time.time - _startTime);
        }

        void SetPosition (float passedTime) {

            float flyingProgressRate = System.Math.Min(passedTime / _flyingDuration, 1f);

            // set body sprite height
            if (flyingProgressRate > droppingStartTravelingRate) {

                float currentHeight = _startBodyLocalHeight;
                Tween tween = DOTween.To(() => currentHeight, x => currentHeight = x, 0f, _flyingDuration * (1f - droppingStartTravelingRate));
                tween.SetEase(Ease.InQuad);
                tween.Goto((flyingProgressRate - droppingStartTravelingRate) * _flyingDuration, true);
                tween.Kill(false);

                bodySpriteTrans.localPosition = bodySpriteTrans.localPosition.GetAfterSetY(currentHeight);
            }

            if (flyingProgressRate < 1) {

                Quaternion currentDirRot = Quaternion.identity;

                // == Curve Ball ==
                if (Global.CurrentRoundInstance.activeRules.curveBall) {

                    float radius      = Global.CurrentRoundInstance.snowballProps.curveBallRadius;
                    float passedAngle = Mathf.Lerp(0f, _flyingDistance, flyingProgressRate) / (Mathf.PI * radius) * 180f;

                    currentDirRot     = Quaternion.AngleAxis(passedAngle, Vector3.Cross(-_curveDir, _startDir));

                    _positionOnGround = _startPos + ((Vector2) (currentDirRot * -_curveDir) + _curveDir) * radius;
                    _currentFlyingDir = currentDirRot * _startDir;

                }
                else {
                    _positionOnGround = _startPos + _startDir * Mathf.Lerp(0f, _flyingDistance, flyingProgressRate);
                }

                // == Knuckle Ball ==
                if (Global.CurrentRoundInstance.activeRules.knuckleBall) {

                    int pointAmount = _knuckleBallPathValues.Length;

                    int currentKnuckleBallShiftPointIndex = (int) (flyingProgressRate / (1f / pointAmount));

                    float prevPointProgressRate = currentKnuckleBallShiftPointIndex * (1f / pointAmount);
                    float nextPointProgressRate = (currentKnuckleBallShiftPointIndex + 1) * (1f / pointAmount);

                    float currentShift = currentKnuckleBallShiftPointIndex > 0 ? _knuckleBallPathValues[currentKnuckleBallShiftPointIndex - 1] : 0f;
                    Tween tween = DOTween.To(() => currentShift, x => currentShift = x, _knuckleBallPathValues[currentKnuckleBallShiftPointIndex], 1f / pointAmount);
                    tween.Goto(flyingProgressRate - prevPointProgressRate, true);
                    tween.Kill(false);

                    _positionOnGround += (Vector2) (Quaternion.AngleAxis(90f, Vector3.forward) * currentDirRot * _startDir * currentShift);
                }

                transform.position = Global.GetActualWorldPosition(_positionOnGround);
            }
            else {
                // hit the ground
                DoSmashing(false);
            }
        }


        void OnTriggerEnter2D (Collider2D other) {
            if (!_isInited || _isSmashing)
                return;

            if (other.tag == "Snowball") {
                if (!Global.CurrentRoundInstance.activeRules.slowerAndCollidableSnowball)
                    return;

                if (other.gameObject.GetComponent<Snowball>().OwnerNumber == _ownerNumber)
                    return;
            }
            else {
                GetHitDetector getHitDetector = other.gameObject.GetComponent<GetHitDetector>();

                if (getHitDetector != null) {
                    _currentTargetType = getHitDetector.Type;

                    if (_currentTargetType == TargetType.Player) {
                        PlayerManager targetPlayerManager = ((PlayerGetHitDetector) getHitDetector).playerManager;
                        _currentTargetNumber = new int[1] { targetPlayerManager.photonView.Owner.ActorNumber };

                        if (targetPlayerManager.photonView.Owner.ActorNumber == _ownerNumber)
                            return;

                        if (targetPlayerManager.photonView.Owner.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                            // hit local player
                            NetEvent.EmitPlayerGetHitEvent(PhotonNetwork.LocalPlayer.ActorNumber, PlayerManager.LocalPlayerManager.transform.position, _ownerNumber, _idByOwner);
                        else
                            // prediected hit
                            _isCurrentTargetPredicted = true;

                    }
                    else if (_currentTargetType == TargetType.Statue) {
                        Statue targetStatue = ((StatueGetHitDetector) getHitDetector).statue;
                        _currentTargetNumber = new int[1] { targetStatue.StatueNumber };

                        targetStatue.GetHitFromLocal(_ownerNumber, _idByOwner);
                    }
                    else if (_currentTargetType == TargetType.SnowWall) {
                        SnowWall targetSnowWall = ((SnowWallGetHitDetector) getHitDetector).snowWall;
                        _currentTargetNumber = new int[2] { targetSnowWall.OwnerNumber, targetSnowWall.IdByOwner };

                        targetSnowWall.GetHitFromLocal(_ownerNumber, _idByOwner);
                    }
                    else if (_currentTargetType == TargetType.DuckSnowCake) {
                        PlayerManager targetCakePlayerManager = ((DuckSnowCakeGetHitDetector) getHitDetector).playerManager;

                        Vector2 reboundNormal = targetCakePlayerManager.SwingDirection;
                        if (reboundNormal != Vector2.zero) {
                            Vector2 newDir = Vector2.Reflect(FlyingDirection, targetCakePlayerManager.SwingDirection);

                            ReInitFromRebound(_positionOnGround, newDir);
                            if (targetCakePlayerManager.photonView.IsMine)
                                NetEvent.EmitSnowballReboundEvent(_ownerNumber, _idByOwner, _positionOnGround, newDir);
                        }

                        return;
                    }
                    else if (_currentTargetType == TargetType.Immortal) {
                        // do nothing
                    }
                }
            }

            DoSmashing(true);
        }

        void DoSmashing (bool isHitSomething, float eventTimeCompensation = 0f) {

            _isSmashing = true;
            rb.simulated = false;

            if (isHitSomething) {
                if (Vector2.Dot(FlyingDirection, Vector2.right) < 0) {
                    transform.rotation = Global.horizontalFlipRotation;
                }
            }
            else {
                bodySpriteTrans.rotation = SMASH_ON_GROUND_BODY_SPRITE_ROTATION;
            }

            animManager.PlaySmashing(eventTimeCompensation);
        }


        public void HitTargetEventFromNetwork (TargetType targetType, int[] targetNumber, float eventTimeCompensation) {
            _currentTargetType = targetType;
            _currentTargetNumber = targetNumber;
            _isCurrentTargetPredicted = false;

            SetPosition(Time.time - _startTime + eventTimeCompensation);
            DoSmashing(true, eventTimeCompensation);
        }

    }
}
