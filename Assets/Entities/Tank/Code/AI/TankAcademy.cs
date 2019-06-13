using MLAgents;

namespace Tank.AI {
    public class TankAcademy : Academy {
        public float enemySpawnVariance;
        public bool trainingTackle = true;
        public bool spawnInMiddle = true;

        public override void AcademyReset() {
            enemySpawnVariance = resetParameters["EnemySpawnVariance"];
        }

        public override void InitializeAcademy() {
            Monitor.SetActive(true);
        }
    }
}