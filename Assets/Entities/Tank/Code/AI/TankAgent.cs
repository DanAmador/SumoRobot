using System;
using System.Collections;
using GameSession;
using MLAgents;
using UnityEngine;

namespace Tank.AI {
    public class TankAgent : Agent {
        private TankController _tank, enemy;
        private TankAgent enemyAgent;
        private TankAcademy _academy;
        public GameSessionManager gs;
        private TankInputs _input;
        private RayPerception3D _rayPerception;

        public override void InitializeAgent() {
            base.InitializeAgent();
            _tank = GetComponent<TankController>();
            _input = GetComponent<TankInputs>();
//            _input.playerControl = false;

            enemy = gs.getEnemy(_tank);
            enemyAgent = enemy.GetComponent<TankAgent>();
            _rayPerception = GetComponent<RayPerception3D>();
            _academy = FindObjectOfType<TankAcademy>();
        }

        public override void CollectObservations() {
            if (!enemy) {
                InitializeAgent();
            }

            Transform tankTransform = _tank.transform;
            Vector3 normalized = tankTransform.rotation.eulerAngles / 360.0f; // [0,1]

            float rayDistance = 50;
            float[] rayAngles = {0f, 45f, 90f, 135f, 180f, 110f, 70f, 270};

            AddVectorObs(_rayPerception.Perceive(rayDistance, rayAngles, new[] {"Edge", "Player"}, 0f, 0f));


            AddVectorObs(normalized);
            AddVectorObs((int) _tank.state, Enum.GetValues(typeof(TankState)).Length);
            AddVectorObs(_tank.specialCounter);
            AddVectorObs(_tank.getNormalizedSpeed());
            AddVectorObs(_tank.onEdge);

            Transform enemyTransform = enemy.transform;
            Vector3 enemyPos = enemyTransform.position;

            AddVectorObs(1 - Vector3.Distance(enemyPos, tankTransform.position) / 100);
            Vector3 vecTo = (enemyPos - transform.position);
            AddVectorObs(vecTo.normalized);
            AddVectorObs((int) enemy.state, Enum.GetValues(typeof(TankState)).Length);
        }

        public override void AgentAction(float[] vectorAction, string textAction) {
            if (enemy.state == TankState.DEAD || _tank.state == TankState.DEAD) {
                StartCoroutine(WaitBeforeReset());
            }

            int forward = Mathf.FloorToInt(vectorAction[0]);
            int rotation = Mathf.FloorToInt(vectorAction[1]);
            int button = Mathf.FloorToInt(vectorAction[2]);

            if (forward == 1) _input.ForwardInput = 1;
            if (forward == 2) _input.ForwardInput = -1;
            if (forward == 3) _input.ForwardInput = .5f;
            if (forward == 4) _input.ForwardInput = -.5f;

            if (rotation == 1) _input.RotationInput = 1;
            if (rotation == 2) _input.RotationInput = -1;
            if (rotation == 3) _input.RotationInput = .5f;
            if (rotation == 4) _input.RotationInput = -.5f;

            _input.Drift = button == 1;
            _input.Turbo = button == 2;
            _input.Block = button == 3;


            float totalReward = _academy.trainingTackle ? TackleReward() : NormalReward();
            Vector3 vecTo = (enemy.transform.position - transform.position);
            Debug.Log(vecTo.normalized);
//            Debug.Log(totalReward.ToString("0.##########"));
            Monitor.Log("Reward", totalReward, transform);

            AddReward(totalReward);
        }

        private float NormalReward() {
            float totalReward = 0;
            float distance = Vector3.Distance(enemy.transform.position, _tank.transform.position);
            totalReward += -Mathf.Log((distance + .1f) / 10) / 10000;
//            if (_tank.state == TankState.NORMAL) totalReward += -0.001f;
            if (_tank.state == TankState.BLOCK) totalReward += 0.001f * _tank.specialCounter;
            if (_tank.state == TankState.BOOST) totalReward += 0.001f * _tank.specialCounter;
            if (_tank.state == TankState.COLLIDED) totalReward += -0.01f;
            if (_tank.onEdge) totalReward += -0.2f;

            if (_tank.lastCollisionImpulse != Vector3.zero) {
                totalReward += Math.Abs(_tank.lastCollisionImpulse.normalized.magnitude) *
                               (_tank.specialCounter * 3 + 2);
                _tank.lastCollisionImpulse = Vector3.zero;
            }

            if (enemy.state == TankState.DEAD) totalReward += 1;
            if (_tank.state == TankState.DEAD) totalReward -= 2;
            return totalReward;
        }

        private float TackleReward() {
            float totalReward = 0;

//            totalReward -= .0005f;
//            float distance = Vector3.Distance(enemy.transform.position, _tank.transform.position);

//            totalReward += -Mathf.Log((distance + .1f) / 10) / 10000;

//            if (_tank.onEdge) totalReward += -0.035f;
//            if (_tank.state == TankState.BOOST) totalReward += 0.001f * _tank.specialCounter;
            if (_tank.state == TankState.DEAD) totalReward -= 1;
            if (_tank.lastCollisionImpulse != Vector3.zero) {
                totalReward += 1;
                _tank.lastCollisionImpulse = Vector3.zero;
                StartCoroutine(WaitBeforeReset());
            }

            return totalReward;
        }

        private IEnumerator WaitBeforeReset() {
            yield return new WaitForSeconds(.5f);
            Done();
            if (enemyAgent) {
                enemyAgent.Done();
            }
        }

        public override void AgentReset() {
            gs.Reset();

            if (_academy.trainingTackle) {
                Vector3 rand = UnityEngine.Random.onUnitSphere * _academy.enemySpawnVariance;
                rand.y = 0;
                enemy.transform.position += rand;
            }
        }
    }
}