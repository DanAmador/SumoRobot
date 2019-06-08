using System;
using GameSession;
using MLAgents;
using UnityEngine;

namespace Tank.AI {
    public class TankAgent : Agent {
        private TankController _tank;
        private GameSessionManager gs;
        private TankController[] enemies;

        private void Start() {
            _tank = GetComponent<TankController>();
            enemies = gs.getEnemies(_tank);
        }

        public override void CollectObservations() {
            Transform tankTransform = _tank.transform;
            Vector3 normalized = tankTransform.rotation.eulerAngles / 360.0f; // [0,1]
            AddVectorObs(tankTransform.position);
            AddVectorObs(normalized);
            AddVectorObs((int) _tank.state, Enum.GetValues(typeof(TankState)).Length);
            AddVectorObs(_tank.currSpeed);
            AddVectorObs(_tank.specialCounter);

            foreach (Vector3 distance in _tank.getRaycastDistance()) {
                AddVectorObs(distance);
            }

            foreach (TankController enemy in enemies) {
                Transform enemyTransform = enemy.transform;
                Vector3 enemyPos = enemyTransform.position;
                Vector3 vecTo = (tankTransform.position - enemyPos).normalized;
                Vector3 rotNorm = enemyTransform.rotation.eulerAngles / 360.0f; // [0,1]
                AddVectorObs(enemyPos);
                AddVectorObs(rotNorm);
                AddVectorObs(vecTo);
                AddVectorObs((int) enemy.state, Enum.GetValues(typeof(TankState)).Length);
                AddVectorObs(enemy.currSpeed);
                AddVectorObs(enemy.specialCounter);
            }
        }
    }
}