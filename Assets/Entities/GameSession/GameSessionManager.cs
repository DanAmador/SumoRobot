using System.Linq;
using Platform;
using Tank;
using UnityEngine;

namespace GameSession {
    public class GameSessionManager : MonoBehaviour {
        [SerializeField] private TankController[] tanks;
        private PlatformMover _platform;
        private float _matchStart;
        public float roundDuration = 30; // in seconds 

        public float MatchPercentageRemaining => 1 - (Time.time - _matchStart) / roundDuration;


        public void Awake() {
            _platform = GetComponentInChildren<PlatformMover>();
            _matchStart = Time.time;
        }


        public TankController GetEnemy(TankController player) {
            return tanks.FirstOrDefault(tank => !tank.Equals(player));
        }

        public void Reset() {
            foreach (TankController tank in tanks) {
                tank.Reset();
            }
            _matchStart = Time.time;
            _platform.Reset();
        }
    }
}