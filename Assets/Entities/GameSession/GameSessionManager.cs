using Platform;
using Tank;
using UnityEngine;

namespace GameSession {
    public class GameSessionManager : MonoBehaviour {
        private TankController[] tanks;
        private PlatformMover platform;

        public void Awake() {
            tanks = GetComponentsInChildren<TankController>();
            platform = GetComponentInChildren<PlatformMover>();
        }


        public Vector3 getMiddlePosition() {
            return platform.transform.position;
        }

        public TankController getEnemy(TankController player) {
            if (tanks == null)
                tanks =
                    GetComponentsInChildren<TankController>(); //Idk why it sometimes crashes thanks to the ml agents
            foreach (TankController tank in tanks) {
                if (!tank.Equals(player)) return tank;
            }

            return null;
        }

        public void Reset() {
            foreach (TankController tank in tanks) {
                tank.Reset();
            }

            platform.Reset();
        }
    }
}