using System.Collections.Generic;
using Tank;
using UnityEngine;

namespace GameSession {
    public class GameSessionManager : MonoBehaviour {
        public TankController[] players;

        private void Start() {
            players = GetComponentsInChildren<TankController>();
        }


        public TankController[] getEnemies(TankController player) {
            List<TankController> enemies = new List<TankController>();
            foreach (TankController tank in players) {
                if (!tank.Equals(player)) enemies.Add(tank);
            }

            return enemies.ToArray();
        }
    }
}