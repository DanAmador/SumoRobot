using System;
using System.Collections;
using GameSession;
using MLAgents;
using UnityEngine;

namespace Tank.AI {
    public class TankAgent : Agent {
        private TankController _tank, enemy;
        private TankAgent enemyAgent;
        public GameSessionManager gs;
        private TankInputs _input;
        private RayPerception3D _rayPerception;
        private bool collectReward;

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
            collectReward = true;
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
            AddVectorObs(Mathf.Clamp(_tank.GetNormalizedSpecial() / _tank.special4Block, 0f, _tank.MaxSpecial / _tank.special4Block));
            AddVectorObs(Mathf.Clamp(_tank.GetNormalizedSpecial() / _tank.special4Boost, 0f, _tank.MaxSpecial / _tank.special4Boost));
            AddVectorObs(_tank.MaxSpecial);
            AddVectorObs(_tank.GetNormalizedSpeed());
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
            AddVectorObs(enemy.GetNormalizedSpecial());
            AddVectorObs(1 - Vector2.Distance(_tank.lastCollisionPos, _tank.transform.position) / _tank.tooCloseLimit);
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

            if (button == 1) _input.VirtualInputSimulate(Buttons.BLOCK, 1f);
            if (button == 2) _input.VirtualInputSimulate(Buttons.TURBO, 1f);
            if (button == 3) _input.VirtualInputSimulate(Buttons.DRIFT);


            if (collectReward) NormalReward();
//            Vector3 vecTo = (enemy.transform.position - transform.position);
//            Debug.Log("Reward: " + GetReward() );
////            Debug.Log("Cumulative: " + GetCumulativeReward() );
////            Debug.Log(totalReward.ToString("0.##########"));


            Monitor.Log("Reward", GetCumulativeReward(), transform);
        }

        private void NormalReward() {
            float totalReward = 0;
            totalReward -= .0005f;

            if (_tank.GetNormalizedSpeed() <= .4f) {
                totalReward -= .0002f;
            }

            if (_tank.tooCloseFlag) {
                totalReward -= .001f;
            }

            if (_tank.onEdge) {
                totalReward -= .0003f;
            }
            if (_tank.state == TankState.COLLIDED) {
                totalReward -= 0.1f;
            }

            AddReward(totalReward);
            if (_tank.state == TankState.DEAD) {
                SetReward(-1f);
                collectReward = false;
            }
        }

        public void TackleReward() {
            float totalReward = 0;


            //Facing forward
            var transform1 = _tank.transform;
            float forwardTackle = Mathf.Abs(Vector3.Dot(transform1.forward.normalized,
                (enemy.transform.position - transform1.position).normalized)); // Is it facing the enemy ?

            if (forwardTackle < 0.5f || _tank.GetNormalizedSpeed() < 0.5f ||
                _tank._input.ForwardInput < 0.5f) return;


            float side =
                Mathf.Abs(Vector3.Dot(_tank.transform.forward.normalized,
                    enemy.transform.right.normalized)); // Is it attacking the enemy from the side?

            side = side >= .5f ? side : .5f;

            totalReward += forwardTackle * side;
            AddReward(totalReward);
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
        }
    }
}