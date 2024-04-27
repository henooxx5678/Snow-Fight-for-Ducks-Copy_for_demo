using Math = System.Math;
using UnityEngine;

using Photon.Realtime;
using Photon.Pun;

namespace DoubleHeat.SnowFightForDucksGame {

    [DisallowMultipleComponent]
    public class SnowWall : DamageableObject {

        public enum FacingDirection {
            LeftUp,
            Left,
            LeftDown,
            RightUp,
            Right,
            RightDown
        }

        public struct NetProperties {
            public int     ownerNumber;
            public int     idByOwner;
            public Vector2 position;
            public int     hp;

            public static bool UnpackArray (NetProperties[] propss, out int[] ownersNumber, out int[] idsByOwner, out Vector2[] positions, out int[] hps) {

                if (propss == null) {
                    ownersNumber = null;
                    idsByOwner   = null;
                    positions    = null;
                    hps          = null;
                    return false;
                }

                ownersNumber = new int[propss.Length];
                idsByOwner   = new int[propss.Length];
                positions    = new Vector2[propss.Length];
                hps          = new int[propss.Length];

                for (int i = 0 ; i < propss.Length ; i++) {
                    ownersNumber[i] = propss[i].ownerNumber;
                    idsByOwner[i]   = propss[i].idByOwner;
                    positions[i]    = propss[i].position;
                    hps[i]          = propss[i].hp;
                }

                return true;
            }

            public static NetProperties[] PackUpArray (int[] ownersNumber, int[] idsByOwner, Vector2[] positions, int[] hps) {

                int[] arraysLength = new int[] { ownersNumber.Length, idsByOwner.Length, positions.Length, hps.Length};
                for (int i = 1 ; i < arraysLength.Length ; i++) {
                    if (arraysLength[0] != arraysLength[i])
                        return null;
                }

                NetProperties[] result = new NetProperties[arraysLength[0]];

                for (int i = 0 ; i < result.Length ; i++) {
                    result[i] = new NetProperties {
                        ownerNumber = ownersNumber[i],
                        idByOwner   = idsByOwner[i],
                        position    = positions[i],
                        hp          = hps[i]
                    };
                }

                return result;
            }

        }



        public SnowWallAnimationManager animManager;


        public int           OwnerNumber => _ownerNumber;
        public int           IdByOwner   => _idByOwner;
        public NetProperties NetProps    => new NetProperties {
            ownerNumber = _ownerNumber,
            idByOwner = _idByOwner,
            position = Global.GetPositionOnGround(transform.position),
            hp = _currentHP
        };


        int _ownerNumber = -2;
        int _idByOwner = -1;



        void Start () {
            _currentHP = (Global.CurrentRoundInstance != null) ? Global.CurrentRoundInstance.CurrentSnowWallMaxHP : Global.SNOW_WALL_MAX_HP;
            AnimPlayIdle(_currentHP);
        }


        public void InitDefault (int idByOwner) {
            _ownerNumber = -2;
            _idByOwner = idByOwner;
        }

        public void Init (int ownerNumber, int idByOwner, bool isFacingLeft) {
            _ownerNumber = ownerNumber;
            _idByOwner   = idByOwner;
            if (isFacingLeft) {
                transform.rotation = Global.horizontalFlipRotation;
            }

            // Statistics
            Global.gameSceneManager.playersStatistics[ownerNumber].builtAmount++;
        }


        public void RoundStartCheck (int currentMaxHP) {
            _currentHP = Math.Min(_currentHP, currentMaxHP);
        }


        protected override void ChangeHP (int value) {
            _currentHP = System.Math.Min(_currentHP + value, Global.CurrentRoundInstance.CurrentSnowWallMaxHP);
            _currentHP = System.Math.Max(_currentHP, 0);
        }

        protected override void AnimPlayIdle (int hp) {
            animManager.PlayIdle(hp);
        }

        protected override void AnimPlayGetHit (int afterHP) {
            animManager.PlayGetHitAnim(afterHP);
        }

        protected override void HitChecked (GetHitLog log, bool confirmedHit = true) {
            base.HitChecked(log);
            Global.gameSceneManager.RefreshSnowWallsData();

            // Statistics
            if (confirmedHit) {
                int shooter = log.snowballOwnerNumber;
                Global.gameSceneManager.playersStatistics[shooter].snowWallHits++;
            }
        }

        protected override void Destroyed () {
            Destroy(gameObject);
        }


        public override void GetHitFromLocal (int snowballOwnerNumber, int snowballIdByOwner) {
            base.GetHitFromLocal(snowballOwnerNumber, snowballIdByOwner);

            if (PhotonNetwork.IsMasterClient) {
                NetEvent.EmitSnowWallGetHitEvent(_ownerNumber, _idByOwner, snowballOwnerNumber, snowballIdByOwner);
            }
        }


        public bool SetByNetPropertiesArray (NetProperties[] propss) {

            foreach (NetProperties props in propss) {
                if (props.ownerNumber == _ownerNumber && props.idByOwner == _idByOwner) {

                    transform.position = Global.GetActualWorldPosition(props.position);
                    CurrentHP = props.hp;

                    animManager.CheckForCurrentHP(CurrentHP);

                    return true;
                }
            }

            Destroy(gameObject);
            return false;
        }




        public static FacingDirection GetFacingType (Vector2 dir) {
            float dirAngle = DataCompression.Direction2DToAngleDegree(dir);

            if (dirAngle > 157.5f || dirAngle < -157.5f) {
                return SnowWall.FacingDirection.Left;
            }
            else if (dirAngle < -90) {
                return SnowWall.FacingDirection.LeftDown;
            }
            else if (dirAngle < -22.5f) {
                return SnowWall.FacingDirection.RightDown;
            }
            else if (dirAngle < 22.5f) {
                return SnowWall.FacingDirection.Right;
            }
            else if (dirAngle < 90) {
                return SnowWall.FacingDirection.RightUp;
            }
            else {
                return SnowWall.FacingDirection.LeftUp;
            }
        }

        public static Vector2 GetVectorByFacingDir (FacingDirection facing) {
            if (facing == FacingDirection.LeftUp)
                return Quaternion.AngleAxis(-45, Vector3.forward) * Vector3.left;
                // return (new Vector2(-1, 1)).normalized;

            else if (facing == FacingDirection.Left)
                return Vector2.left;

            else if (facing == FacingDirection.LeftDown)
                return Quaternion.AngleAxis(45, Vector3.forward) * Vector3.left;
                // return (new Vector2(-1, -1)).normalized;

            else if (facing == FacingDirection.RightUp)
                return Quaternion.AngleAxis(45, Vector3.forward) * Vector3.right;
                // return (new Vector2(1, 1)).normalized;

            else if (facing == FacingDirection.Right)
                return Vector2.right;

            else if (facing == FacingDirection.RightDown)
                return Quaternion.AngleAxis(-45, Vector3.forward) * Vector3.right;
                // return (new Vector2(1, -1)).normalized;

            return Vector2.zero;
        }

        public static Quaternion GetRotationByFacingDir (FacingDirection facing) {
            if (facing == FacingDirection.LeftUp)
                return Quaternion.AngleAxis(135, Vector3.forward);

            else if (facing == FacingDirection.Left)
                return Quaternion.AngleAxis(180, Vector3.forward);

            else if (facing == FacingDirection.LeftDown)
                return Quaternion.AngleAxis(-135, Vector3.forward);

            else if (facing == FacingDirection.RightUp)
                return Quaternion.AngleAxis(45, Vector3.forward);

            else if (facing == FacingDirection.Right)
                return Quaternion.identity;

            else if (facing == FacingDirection.RightDown)
                return Quaternion.AngleAxis(-45, Vector3.forward);

            return Quaternion.identity;
        }



    }
}
