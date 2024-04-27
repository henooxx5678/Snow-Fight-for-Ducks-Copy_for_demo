
namespace DoubleHeat {

    public struct TimerTimeDisplay {

        public int min;
        public int sec;

        public string MinSecDisplay => min.ToString("00") + " : " + sec.ToString("00");

        public TimerTimeDisplay (int min, int sec) {
            this.min = min;
            this.sec = sec;
        }

        public static TimerTimeDisplay FromSeconds (float time) {

            if (time < 0)
                time = 0f;
            else if (time > 3600)
                time = 87 * 60f;

            int min = (int) (time / 60);
            int sec = (int) time % 60;

            return new TimerTimeDisplay(min, sec);
        }
    }
}
