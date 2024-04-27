using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Photon.Realtime;
using Photon.Pun;

namespace DoubleHeat.SnowFightForDucksGame {

    [DisallowMultipleComponent]
    public class Statue: DamageableObject {

        public StatueAnimationManager animManager;
        public Color targetedTintColor;

        public byte team;


        public int  StatueNumber => _statueNumber;
        public bool IsRepairable => (_currentHP > 0 && _currentHP < Global.STATUE_MAX_HP);


        int  _statueNumber = -1;
        bool _isHighlighting = false;
        

        protected override void Awake () {
            base.Awake();
            _currentHP = Global.STATUE_MAX_HP;
            AnimPlayIdle(_currentHP);
        }

        public void SetNumber (int statueNumber) {
            _statueNumber = statueNumber;
        }


        protected override void Update () {
            base.Update();

            if (_isHighlighting) {

                animManager.statueSR.color = targetedTintColor;
                _isHighlighting = false;
            }
            else {
                animManager.statueSR.color = Color.white;
            }

        }


        protected override void ChangeHP (int value) {
            _currentHP = System.Math.Min(_currentHP + value, Global.STATUE_MAX_HP);
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
            Global.gameSceneManager.RefreshStatuesHP();

            // Statistics
            if (confirmedHit) {
                int shooter = log.snowballOwnerNumber;
                byte shooterTeam = NetEvent.GetPlayerTeam(NetEvent.GetCurrentPlayerInSeats(), shooter);

                PlayerStatistics shooterStats = Global.gameSceneManager.playersStatistics[shooter];

                if (shooterTeam == team) {
                    shooterStats.allyStatueHits++;
                }
                else {
                    shooterStats.oppoenetStatueHits++;
                }
            }
        }

        protected override void Destroyed () {
            obstacleBody.SetActive(false);
        }



        public override void GetHitFromLocal (int snowballOwnerNumber, int snowballIdByOwner) {
            base.GetHitFromLocal(snowballOwnerNumber, snowballIdByOwner);

            if (PhotonNetwork.IsMasterClient) {
                NetEvent.EmitStatueGetHitEvent(_statueNumber, snowballOwnerNumber, snowballIdByOwner);
            }
        }

        public void HighlightedForRepairableTarget () {
            _isHighlighting = true;
        }


        public void Repaired (int playerNumber = -2) {
            ChangeHP(1);
            AnimPlayIdle(_currentHP);

            Global.gameSceneManager.RefreshStatuesHP();


            // Statistics
            if (playerNumber != -2)
                Global.gameSceneManager.playersStatistics[playerNumber].repairedTimes++;
        }

        public void RepairedFromLocal (int playerNumber) {
            Repaired(playerNumber);
        }

        public void RepairedFromNetwork (int playerNumber) {
            Repaired(playerNumber);
        }


        public void SetHPFromNetwork (int hp) {
            CurrentHP = hp;
            animManager.CheckForCurrentHP(CurrentHP);
        }

    }

}
