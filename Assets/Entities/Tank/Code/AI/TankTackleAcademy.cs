using MLAgents;

namespace Tank.AI {
    public class TankTackleAcademy : Academy {
        public float EnemySpawnVariance;

        public override void AcademyReset() {
            EnemySpawnVariance = resetParameters["EnemySpawnVariance"];
        }

        public override void InitializeAcademy() {
            Monitor.SetActive(true);
        }
    }
}