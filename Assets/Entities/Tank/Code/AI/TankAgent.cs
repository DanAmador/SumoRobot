using System;
using GameSession;
using MLAgents;
using UnityEngine;

namespace Tank.AI {
    public class TankAgent : Agent {
        private TankController _tank, enemy;
        public GameSessionManager gs;
        private TankInputs _input;

        public override void InitializeAgent() {
            base.InitializeAgent();
            _tank = GetComponent<TankController>();
            _input = GetComponent<TankInputs>();
            _input.playerControl = false;

            enemy = gs.getEnemy(_tank);
        }

        public override void CollectObservations() {
            if (!enemy) {
                InitializeAgent();
            }

            Transform tankTransform = _tank.transform;
            Vector3 normalized = tankTransform.rotation.eulerAngles / 360.0f; // [0,1]
            AddVectorObs(tankTransform.position);
            AddVectorObs(normalized);
            AddVectorObs((int) _tank.state, Enum.GetValues(typeof(TankState)).Length);
            AddVectorObs(_tank.specialCounter);
            AddVectorObs(_tank.getNormalizedSpeed());
            foreach (Vector3 distance in _tank.getRaycastDistance()) {
                AddVectorObs(distance);
            }

            Transform enemyTransform = enemy.transform;
            Vector3 enemyPos = enemyTransform.position;
            Vector3 vecTo = (enemyPos - transform.position).normalized;
            Vector3 rotNorm = enemyTransform.rotation.eulerAngles / 360.0f; // [0,1]
            AddVectorObs(Vector3.Distance(enemyPos, tankTransform.position));
            AddVectorObs(enemyPos);
            AddVectorObs(rotNorm);
            AddVectorObs(vecTo);
            AddVectorObs((int) enemy.state, Enum.GetValues(typeof(TankState)).Length);
            AddVectorObs(enemy.getNormalizedSpeed());
            AddVectorObs(enemy.specialCounter);
        }

        public override void AgentAction(float[] vectorAction, string textAction) {
            if (enemy.state == TankState.DEAD || _tank.state == TankState.DEAD) {
                Debug.Log("They be ded ");
                Done();
                return;
            }

            _input.ForwardInput = vectorAction[0];
            _input.RotationInput = vectorAction[1];
            _input.Drift = vectorAction[2] > .5f;
            _input.Turbo = vectorAction[3] > .8f;
            _input.Block = vectorAction[4] > .8f;

            float totalReward = 0;
            float distance = Vector3.Distance(enemy.transform.position, _tank.transform.position);
            totalReward += (distance / 50000);
            if (_tank.state == TankState.NORMAL) totalReward += -0.001f;
            if (_tank.state == TankState.BLOCK) totalReward += 0.01f * _tank.specialCounter;
            if (_tank.state == TankState.BOOST) totalReward += 0.001f * _tank.specialCounter;
            if (_tank.state == TankState.COLLIDED) totalReward += -0.1f;

            if (_tank.lastCollisionImpulse != Vector3.zero) {
                totalReward += Math.Abs(_tank.lastCollisionImpulse.normalized.magnitude);
                _tank.lastCollisionImpulse = Vector3.zero;
            }

            if (enemy.state == TankState.DEAD) totalReward += 1;
            if (_tank.state == TankState.DEAD) totalReward -= 1;

            AddReward(totalReward);
            if (_tank.state == TankState.DEAD) {
                Debug.Log("should end ");
                enemy.GetComponent<TankAgent>().Done();
                Done();
            }
        }

        public override void AgentReset() {
            gs.Reset();
        }
    }
}