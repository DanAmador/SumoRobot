using System;
using System.Collections;
using GameSession;
using MLAgents;
using UnityEngine;

namespace Tank.AI {
    public class TankAgent : Agent {
        private TankController _tank, enemy;
        private TankAgent enemyAgent;
        private Academy _academy;
        public GameSessionManager gs;
        private TankInputs _input;
        private RayPerception3D _rayPerception;

        public override void InitializeAgent() {
            base.InitializeAgent();
            _tank = GetComponent<TankController>();
            _input = GetComponent<TankInputs>();
            _input.playerControl = false;

            enemy = gs.getEnemy(_tank);
            enemyAgent = enemy.GetComponent<TankAgent>();
            _rayPerception = GetComponent<RayPerception3D>();
            _academy = FindObjectOfType<Academy>();
        }

        public override void CollectObservations() {
            if (!enemy) {
                InitializeAgent();
            }

            Transform tankTransform = _tank.transform;
            Vector3 normalized = tankTransform.rotation.eulerAngles / 360.0f; // [0,1]

            float rayDistance = 25;
            float[] rayAngles = {0f, 45f, 90f, 135f, 180f, 110f, 70f};

            AddVectorObs(_rayPerception.Perceive(rayDistance, rayAngles, new[] {"Death", "Player"}, 0f, 0f));


            AddVectorObs(tankTransform.position);
            AddVectorObs(normalized);
            AddVectorObs((int) _tank.state, Enum.GetValues(typeof(TankState)).Length);
            AddVectorObs(_tank.specialCounter);
            AddVectorObs(_tank.getNormalizedSpeed());


            Transform enemyTransform = enemy.transform;
            Vector3 enemyPos = enemyTransform.position;
            Vector3 vecTo = (enemyPos - transform.position).normalized;

            AddVectorObs((1 - Vector3.Distance(enemyPos, tankTransform.position)) / 100);
            AddVectorObs(enemyPos);
            AddVectorObs(vecTo);
            AddVectorObs((int) enemy.state, Enum.GetValues(typeof(TankState)).Length);
        }

        public override void AgentAction(float[] vectorAction, string textAction) {
            if (enemy.state == TankState.DEAD || _tank.state == TankState.DEAD) {
                StartCoroutine(WaitBeforeReset());
            }

            _input.ForwardInput = vectorAction[0];
            _input.RotationInput = vectorAction[1];
            _input.Drift = vectorAction[2] > .6f;
            _input.Turbo = vectorAction[3] > .6f;
            _input.Block = vectorAction[4] > .6f;

            float totalReward = 0;
            float distance = Vector3.Distance(enemy.transform.position, _tank.transform.position);
            totalReward += distance > 20 ? -(Mathf.Abs(distance / 100)) / 100000 : (Mathf.Abs(distance / 100)) / 100000;
//            if (_tank.state == TankState.NORMAL) totalReward += -0.001f;
            if (_tank.state == TankState.BLOCK) totalReward += 0.0001f * _tank.specialCounter;
            if (_tank.state == TankState.BOOST) totalReward += 0.0001f * _tank.specialCounter;
            if (_tank.state == TankState.COLLIDED) totalReward += -0.01f;

            if (_tank.lastCollisionImpulse != Vector3.zero) {
                totalReward += Math.Abs(_tank.lastCollisionImpulse.normalized.magnitude) *
                               ((_tank.specialCounter * 3) + 2);
                _tank.lastCollisionImpulse = Vector3.zero;
            }

            if (enemy.state == TankState.DEAD) totalReward += 1;
            if (_tank.state == TankState.DEAD) totalReward -= .5f;


            Monitor.Log("Reward", totalReward, transform);

            AddReward(totalReward);
        }

        private IEnumerator WaitBeforeReset() {
            yield return new WaitForSeconds(.5f);
            enemyAgent.Done();
            Done();
        }

        public override void AgentReset() {
            gs.Reset();

            if (_academy.GetType() == typeof(TankTackleAcademy)) {
                Vector3 rand = UnityEngine.Random.onUnitSphere * ((TankTackleAcademy) _academy).EnemySpawnVariance;
                rand.y = 0;
                enemy.transform.position += rand;
            }
        }
    }
}