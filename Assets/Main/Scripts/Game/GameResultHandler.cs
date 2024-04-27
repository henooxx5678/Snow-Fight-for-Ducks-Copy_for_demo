using TimeSpan = System.TimeSpan;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace DoubleHeat.SnowFightForDucksGame {

    public class GameResultHandler : MonoBehaviour {

        [System.Serializable]
        public class ItemNames {
            // game
            public string totalRounds;
            public string totalGameTime;

            // local player
            public string opponentHits;
            public string allyHits;
            public string oppoenetStatueHits;
            public string allyStatueHits;
            public string snowWallHits;

            public string firedAmount;
            public string builtAmount;
            public string repairedTimes;
            public string getHitTimes;

            public string votedTimes;
            public string votedElectedTimes;
            public string votedElectedRate;
        }


        public ItemNames itemNames;

        public Dictionary<int, PlayerStatistics> playersStats;

        public byte  WinTeam       => _winTeam;
        public int   TotalRounds   => _totalRounds;
        public float TotalGameTime => _totalGameTime;

        public PlayerStatistics TotalStatsOfPlayers => PlayerStatistics.GetTotal(playersStats.Values.ToArray());

        public Dictionary<string, string> GetResultsStringOfName (int playerNumber) {

            TimeSpan gameTimeSpan = TimeSpan.FromSeconds(_totalGameTime);

            return new Dictionary<string, string>() {
                { itemNames.totalRounds, _totalRounds.ToString() },
                { itemNames.totalGameTime, string.Format("{0:D2}:{1:D2}:{2:D2}", gameTimeSpan.Hours, gameTimeSpan.Minutes, gameTimeSpan.Seconds) },

                { itemNames.opponentHits, playersStats[playerNumber].opponentHits.ToString() },
                { itemNames.allyHits, playersStats[playerNumber].allyHits.ToString() },
                { itemNames.oppoenetStatueHits, playersStats[playerNumber].oppoenetStatueHits.ToString() },
                { itemNames.allyStatueHits, playersStats[playerNumber].allyStatueHits.ToString() },
                { itemNames.snowWallHits, playersStats[playerNumber].snowWallHits.ToString() },

                { itemNames.firedAmount, playersStats[playerNumber].firedAmount.ToString() },
                { itemNames.builtAmount, playersStats[playerNumber].builtAmount.ToString() },
                { itemNames.repairedTimes, playersStats[playerNumber].repairedTimes.ToString() },
                { itemNames.getHitTimes, playersStats[playerNumber].getHitTimes.ToString() },

                // { itemNames.votedTimes, playersStats[playerNumber].votedTimes.ToString() },
                { itemNames.votedElectedTimes, playersStats[playerNumber].votedElectedTimes.ToString() }
                // { itemNames.votedElectedRate, playersStats[playerNumber].VotedElectedRate.ToString("P") }
            };
        }


        byte  _winTeam = 255;
        int   _totalRounds = 0;
        float _totalGameTime = 0f;


        void Awake () {
            Global.gameResultHandler = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Init (byte winTeam, int totalRounds, float totalGameTime, Dictionary<int, PlayerStatistics> playersStatistics) {
            _winTeam = winTeam;
            _totalRounds = totalRounds;
            _totalGameTime = totalGameTime;

            playersStats = new Dictionary<int, PlayerStatistics>();

            foreach (int playerNumber in playersStatistics.Keys) {
                playersStats.Add(playerNumber, PlayerStatistics.Clone(playersStatistics[playerNumber]));
            }
        }


    }
}
