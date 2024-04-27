using System.Collections;
using System.Collections.Generic;

﻿using UnityEngine;

using DG.Tweening;
using Photon.Realtime;
using Photon.Pun;

using DoubleHeat;
using DoubleHeat.Utilities;

namespace DoubleHeat.SnowFightForDucksGame {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerManager))]
    public class PlayerMoveManager : MonoBehaviour {


        // #region ===== SERIALIZED_VARIABLES ======
        public Rigidbody2D rb;
        public LayerMask   moveLayerMask;
        // #endregion ***********************************


        PlayerManager _playerManager;
        public PlayerManager playerManager {
            get {
                if (_playerManager == null)
                    _playerManager = GetComponent<PlayerManager>();
                return _playerManager;
            }
        }

        public Vector2 MoveVelocity {get; private set;} = Vector2.zero;


        public bool    IsMovable => playerManager.IsActionable;
        public Vector2 MovingLeftOrRightDir {
            get {
                if (Vector2.Dot(MoveVelocity, Vector2.right) > 0)
                    return Vector2.right;
                else if (Vector2.Dot(MoveVelocity, Vector2.left) > 0)
                    return Vector2.left;
                else
                    return Vector2.zero;
            }
        }



        Vector2Log _prevPositionOnGroundLog = Vector2Log.zero;

        void Update () {

            if (playerManager == null)
                return;

            if (playerManager.photonView.IsMine) {

                // set walk velocity
                Vector2 moveDir = GetTargetWalkDir();
                float   speed   = Global.CurrentRoundInstance.CurrentPlayerWalkSpeed;

                Vector2 differenceDir = moveDir - MoveVelocity / speed;
                float stepMagn = Global.CurrentRoundInstance.playerProps.walkDirectionChangeSpeed * Time.deltaTime;

                if (differenceDir.sqrMagnitude > Mathf.Pow(stepMagn, 2)) {
                    MoveVelocity += differenceDir.normalized * stepMagn * speed;
                    moveDir = MoveVelocity.normalized;
                }
                else {
                    MoveVelocity = moveDir * speed;
                }

                if (Global.CurrentRoundInstance.activeRules.turnProhibited) {
                    Vector2 actualVelocity = MoveVelocity;

                    // is move backward?
                    if (Vector2.Dot(MoveVelocity, playerManager.FacingDirection) < 0) {
                        actualVelocity.x = MoveVelocity.x * Global.CurrentRoundInstance.playerProps.walkBackSpeedRate;
                        actualVelocity.y = MoveVelocity.y;
                    }

                    moveDir = actualVelocity.normalized;
                    speed = Vector2.Dot(moveDir, actualVelocity);
                }

                TryToMoveAndWatchObstacle(moveDir, speed * Time.deltaTime);

            }
            else {
                MoveVelocity = (playerManager.PositionOnGound - _prevPositionOnGroundLog.v2) / (Time.time - _prevPositionOnGroundLog.time);  // not a precise value
                _prevPositionOnGroundLog = new Vector2Log(playerManager.PositionOnGound, Time.time);
            }

        }



        Vector2 GetTargetWalkDir () {
            return IsMovable ? Global.gameSceneManager.inputManager.CurrentMoveInput : Vector2.zero;
        }

        public void TryToMoveAndWatchObstacle (Vector2 dir, float distance) {

            Vector2 finalDeltaPos = PhysicsTools2D.GetFinalDeltaPosAwaringObstacle(rb, dir, distance, moveLayerMask);

            // Move
            playerManager.PositionOnGound += finalDeltaPos;
        }

    }
}
