namespace DoubleHeat.SnowFightForDucksGame {

    public class PlayerStatistics {

        public int opponentHits = 0;
        public int allyHits = 0;
        public int oppoenetStatueHits = 0;
        public int allyStatueHits = 0;
        public int snowWallHits = 0;

        public int firedAmount = 0;
        public int builtAmount = 0;
        public int repairedTimes = 0;
        public int getHitTimes = 0;

        public int votedTimes = 0;
        public int votedElectedTimes = 0;

        public float VotedElectedRate => (float) votedElectedTimes / votedTimes;



        public float GetVoteRate (int totalVotingPhaseTimes) {
            return (float) votedTimes / totalVotingPhaseTimes;
        }


        public static PlayerStatistics Clone (PlayerStatistics stats) {

            PlayerStatistics result = new PlayerStatistics();

            result.opponentHits = stats.opponentHits;
            result.allyHits = stats.allyHits;
            result.oppoenetStatueHits = stats.oppoenetStatueHits;
            result.allyStatueHits = stats.allyStatueHits;
            result.snowWallHits = stats.snowWallHits;

            result.firedAmount = stats.firedAmount;
            result.builtAmount = stats.builtAmount;
            result.repairedTimes = stats.repairedTimes;
            result.getHitTimes = stats.getHitTimes;

            result.votedTimes = stats.votedTimes;
            result.votedElectedTimes = stats.votedElectedTimes;

            return result;
        }



        public static PlayerStatistics GetTotal (PlayerStatistics[] playersStats) {
            PlayerStatistics total = new PlayerStatistics();

            foreach (var stats in playersStats) {
                total.opponentHits       += stats.opponentHits;
                total.allyHits           += stats.allyHits;
                total.oppoenetStatueHits += stats.oppoenetStatueHits;
                total.allyStatueHits     += stats.allyStatueHits;
                total.snowWallHits       += stats.snowWallHits;

                total.firedAmount        += stats.firedAmount;
                total.builtAmount        += stats.builtAmount;
                total.repairedTimes      += stats.repairedTimes;
                total.getHitTimes        += stats.getHitTimes;

                total.votedTimes         += stats.votedTimes;
                total.votedElectedTimes  += stats.votedElectedTimes;
            }

            return total;
        }

    }
}
