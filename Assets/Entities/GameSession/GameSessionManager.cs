using Platform;
using Tank;
using UnityEngine;

namespace GameSession {
    public class GameSessionManager : MonoBehaviour {
        [SerializeField] private TankController[] tanks;
        private PlatformMover platform;

        public void Awake() {
            platform = GetComponentInChildren<PlatformMover>();
        }
        

        public TankController getEnemy(TankController player) {
            
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