namespace RTSGame {

    public class Stats {

        public int Health;

        public int Attack;

        public int Defense;

        public float AttackRange;

        public float AttackSpeed; // Time between attacks in seconds

        public float CriticalChance;

        public long LastAttackTime = 0;

        public long LastHealTime = 0;

        public float FieldOfView;
    }
}
