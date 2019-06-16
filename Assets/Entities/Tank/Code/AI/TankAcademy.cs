using MLAgents;

namespace Tank.AI {
    public class TankAcademy : Academy {
        public float enemySpawnVariance;
        public bool trainingTackle = true;
        public bool spawnInMiddle = true;
        public bool pvp;

        public override void AcademyReset() {
//            enemySpawnVariance = resetParameters["EnemySpawnVariance"];
//            trainingTackle = Convert.ToBoolean(resetParameters["TrainingTackle"]);
//            spawnInMiddle= Convert.ToBoolean(resetParameters["SpawnInMiddle"]);
//            pvp= Convert.ToBoolean(resetParameters["PVP"]);
        }

        public override void InitializeAcademy() {
            Monitor.SetActive(true);
        }
    }
}