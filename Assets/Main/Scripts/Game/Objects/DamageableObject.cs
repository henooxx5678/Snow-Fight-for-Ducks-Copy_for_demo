using Math = System.Math;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class DamageableObject: MonoBehaviour {

        protected class GetHitLog {
            public float time;
            public int snowballOwnerNumber;
            public int snowballIdByOwner;

            public GetHitLog (float time, int snowballOwnerNumber, int snowballIdByOwner) {
                this.time = time;
                this.snowballOwnerNumber = snowballOwnerNumber;
                this.snowballIdByOwner = snowballIdByOwner;
            }
        }

        public GameObject obstacleBody;
        public GameObject hitBox;


        public int  CurrentHP {
            get => _currentHP;
            set => Math.Max(value, 0);
        }

        protected int _currentHP = -1;
        protected List<GetHitLog> _getHitPendingLogs = new List<GetHitLog>();


        protected virtual void Awake () {
            transform.position = Global.GetPositionWithDepth(transform.position);
        }

        protected virtual void Update () {

            for (int i = _getHitPendingLogs.Count - 1 ; i >= 0 ; i--) {
                if (Time.time - _getHitPendingLogs[i].time > Global.globalManager.lagTimeTolerance) {
                    CancelTheHit(_getHitPendingLogs[i]);
                }
            }
        }


        protected virtual void ChangeHP (int value) {
            CurrentHP = _currentHP + value;
        }
        protected virtual void AnimPlayIdle (int hp) {}
        protected virtual void AnimPlayGetHit (int afterHP) {}

        protected virtual void HitChecked (GetHitLog log, bool confirmedHit = true) {
            _getHitPendingLogs.Remove(log);
        }
        protected virtual void Destroyed () {}


        void CancelTheHit (GetHitLog log) {
            if (log != null) {
                ChangeHP(1);
                obstacleBody.SetActive(true);
                hitBox.SetActive(true);
                AnimPlayIdle(_currentHP);
                HitChecked(log, false);
            }
        }

        GetHitLog GetGetHitLog (int snowballOwnerNumber, int snowballIdByOwner) {
            foreach (GetHitLog log in _getHitPendingLogs) {
                if (log.snowballOwnerNumber == snowballOwnerNumber && log.snowballIdByOwner == snowballIdByOwner)
                    return log;
            }
            return null;
        }


        public void Rot () {

            if (_currentHP > 0) {
                ChangeHP(-1);
                if (_currentHP == 0) {
                    hitBox.SetActive(false);
                }

                AnimPlayGetHit(_currentHP);
            }
        }

        public virtual void GetHitFromLocal (int snowballOwnerNumber, int snowballIdByOwner) {
            _getHitPendingLogs.Add(new GetHitLog(Time.time, snowballOwnerNumber, snowballIdByOwner));
            Rot();
        }

        public void GetHitFromNetwork (int snowballOwnerNumber, int snowballIdByOwner) {
            GetHitLog log = GetGetHitLog(snowballOwnerNumber, snowballIdByOwner);
            if (log != null)
                HitChecked(log);
        }


        public void AttemptToCancelTheHit (int snowballOwnerNumber, int snowballIdByOwner) {
            GetHitLog log = GetGetHitLog(snowballOwnerNumber, snowballIdByOwner);
            if (log != null && Time.time - log.time <= Global.globalManager.lagTimeTolerance) {
                CancelTheHit(log);
            }
        }

        public void OnGetHitAnimEnd () {
            if (_currentHP == 0)
                Destroyed();
        }

    }
}
