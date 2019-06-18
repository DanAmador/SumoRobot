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
        private bool collectReward;
        private float lastDistance;

        private float rayDistance = 50;
        private float[] rayAngles = {0f, 45f, 70f, 90f, 135f, 180f, 110f, 270};
        private String[] observables = {"Edge", "Player"};

        public override void InitializeAgent() {
            base.InitializeAgent();
            _tank = GetComponent<TankController>();
            _input = GetComponent<TankInputs>();
//            _input.playerControl = false;

            enemy = gs.getEnemy(_tank);
            enemyAgent = enemy.GetComponent<TankAgent>();
            _rayPerception = GetComponent<RayPerception3D>();
            _academy = FindObjectOfType<TankAcademy>();
            collectReward = true;
            lastDistance = Vector3.Distance(enemy.transform.position, _tank.transform.position);
        }

        public override void CollectObservations() {
            if (!enemy) {
                InitializeAgent();
            }

            Transform tankTransform = _tank.transform;
            Vector3 normalized = tankTransform.rotation.eulerAngles / 360.0f; // [0,1]


            AddVectorObs(_rayPerception.Perceive(rayDistance, rayAngles, observables, 0f, 0f));
            AddVectorObs(_rayPerception.Perceive(rayDistance / 2, rayAngles, observables, 0f, 0f));


            AddVectorObs(normalized);
            AddVectorObs((int) _tank.state, Enum.GetValues(typeof(TankState)).Length);
            AddVectorObs(_tank.getNormalizedSpecial());
            AddVectorObs(_tank.getNormalizedSpeed());
            AddVectorObs(_tank.onEdge);
            AddVectorObs(_tank.transform.forward);

            Transform enemyTransform = enemy.transform;
            Vector3 enemyPos = enemyTransform.position;

            AddVectorObs(1 - Vector3.Distance(enemyPos, tankTransform.position) / 60);
            AddVectorObs(enemy.transform.forward);
            Vector3 vecTo = (enemyPos - transform.position);
            AddVectorObs(vecTo.normalized);
            AddVectorObs((int) enemy.state, Enum.GetValues(typeof(TankState)).Length);
            AddVectorObs(Vector3.Dot(_tank.transform.forward.normalized, vecTo.normalized));
            AddVectorObs(enemy.getNormalizedSpecial());
        }

        public override void AgentAction(float[] vectorAction, string textAction) {
            if (enemy.state == TankState.DEAD || _tank.state == TankState.DEAD) {
                StartCoroutine(WaitBeforeReset(0));
            }

            int forward = Mathf.FloorToInt(vectorAction[0]);
            int rotation = Mathf.FloorToInt(vectorAction[1]);
            int button = Mathf.FloorToInt(vectorAction[2]);

            if (forward == 1) _input.ForwardInput = 1;
            if (forward == 2) _input.ForwardInput = -1;

            if (rotation == 1) _input.RotationInput = 1;
            if (rotation == 2) _input.RotationInput = -1;

            if (button == 1) _input.virtualInputSimulate(Buttons.BLOCK, time: 1f);
            if (button == 2) _input.virtualInputSimulate(Buttons.TURBO, time: 1f);
            if (button == 3) _input.virtualInputSimulate(Buttons.DRIFT);

            if (collectReward) {
                if (_academy.trainingTackle) TackleReward();
                else {
                    if (button != 0) {
                        AddReward(-.0001f);
                    }

                    NormalReward();
                }
            }


//            Vector3 vecTo = (enemy.transform.position - transform.position);
//            Debug.Log("Reward: " + GetReward() );
////            Debug.Log("Cumulative: " + GetCumulativeReward() );
////            Debug.Log(totalReward.ToString("0.##########"));
            Monitor.Log("Reward", GetCumulativeReward(), transform);
        }

        private void NormalReward() {
            float totalReward = 0;


            totalReward += -5f * .0000001f;

            if (_tank.state == TankState.COLLIDED) totalReward += -0.0001f;

            if (_tank.reflectedFlag) {
                AddReward(0.1f);
                _tank.reflectedFlag = false;
            }

            if (_tank.lastCollisionImpulse != Vector3.zero && _tank.getNormalizedSpeed() >= .7f) {
                //Facing forward
                float forwardTackle = Vector3.Dot(_tank.transform.forward.normalized,
                    (enemy.transform.position - transform.position).normalized);
                float side = Mathf.Abs(Vector3.Dot(_tank.transform.forward.normalized, enemy.transform.right));

                side = side >= .5f ? side : .5f;

                float mod = forwardTackle * side;


                totalReward += Math.Abs(_tank.lastCollisionImpulse.normalized.magnitude) * mod;
                _tank.lastCollisionImpulse = Vector3.zero;
            }

            if (_tank.state == TankState.DEAD) {
                totalReward -= 1;
                collectReward = false;
            }

            AddReward(totalReward);
        }

        private void TackleReward() {
            float mod = Vector3.Dot(_tank.transform.forward.normalized,
                (enemy.transform.position - transform.position).normalized);

            AddReward(-.0001f);

            if (_tank.state == TankState.DEAD) {
                AddReward(-1);
                StartCoroutine(WaitBeforeReset(.5f));
                collectReward = false;
            }

            if (_tank.lastCollisionImpulse != Vector3.zero) {
                _tank.lastCollisionImpulse = Vector3.zero;
                StartCoroutine(WaitBeforeReset(.5f));
                AddReward(1 * mod);
                collectReward = false;
            }
        }

        private IEnumerator WaitBeforeReset(float time) {
            yield return new WaitForSeconds(time);
            Done();
            if (enemyAgent) {
                enemyAgent.Done();
            }
        }

        public override void AgentReset() {
            gs.Reset();
            collectReward = true;

            if (_academy.spawnInMiddle) {
                Vector3 middle = gs.getMiddlePosition();
                float height = enemy.transform.position.y;
                middle.y = height;
                enemy.transform.position = middle;

                _tank.transform.position = middle;


                middle.z += 3;
                middle.x += 2;
                enemy.transform.position = middle;
            }

            if (_academy.trainingTackle) {
                Vector3 rand = UnityEngine.Random.onUnitSphere * _academy.enemySpawnVariance;
                rand.y = 0;
                enemy.transform.position += rand;
            }

            enemyAgent.enabled = _academy.pvp;
            lastDistance = Vector3.Distance(enemy.transform.position, _tank.transform.position);
        }
    }
}