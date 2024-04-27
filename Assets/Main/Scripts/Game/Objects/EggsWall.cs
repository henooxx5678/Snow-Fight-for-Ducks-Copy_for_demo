using UnityEngine;

using Photon.Pun;

namespace DoubleHeat.SnowFightForDucksGame {

    public class EggsWall : MonoBehaviour {

        public Transform hitBoxTrans;


        public bool IsMovedByPlayer => _isMovedByPlayer;
        public int  MoverNumber => _moverNumber;


        Vector2 _prevPositionOnGround;
        Vector2 _positionOnGround;
        bool _isMovedByPlayer = false;
        int  _moverNumber = -1;


        void Awake () {
            _positionOnGround = Global.GetPositionOnGround(transform.position);
            transform.position = Global.GetActualWorldPosition(_positionOnGround);

            _prevPositionOnGround = _positionOnGround;
        }

        void Update () {

            // shift hit box when moving
            if (_isMovedByPlayer) {

                Vector2 velocity = (_prevPositionOnGround - _positionOnGround) / Time.deltaTime; // not a percise velocity

                Vector2 shiftingVector = Vector2.zero;

                if (_moverNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                    shiftingVector = -velocity;
                else
                    shiftingVector = velocity;

                // hitBoxTrans.localPosition = Vector2.MoveTowards(Vector2.zero, shiftingVector * ??time, maxDistanceDelta);

                _prevPositionOnGround = _positionOnGround;
            }
        }


        public void HandleByPlayer (int playerNumber) {
            _isMovedByPlayer = true;
            _moverNumber = playerNumber;
        }

    }
}
