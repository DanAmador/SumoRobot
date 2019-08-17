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

        private float rayDistance;
        private float[] rayAngles = {0f, 45f, 70f, 90f, 135f, 180f, 110f, 270};
        private String[] observables = {"Edge", "Player"};

        public override void InitializeAgent() {
            base.InitializeAgent();
            _tank = GetComponent<TankController>();
            _input = GetComponent<TankInputs>();
//            _input.playerControl = false;

            rayDistance = _tank.tooCloseLimit * 3.6f;
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
            AddVectorObs(Mathf.Clamp(1 - (_tank.GetNormalizedSpecial() / _tank.special4Block) * 5, 0f, 1f));
            AddVectorObs(Mathf.Clamp(1 - _tank.GetNormalizedSpecial() / _tank.special4Boost, 0f, _tank.special4Boost));
            AddVectorObs(_tank.MaxSpecial);
            AddVectorObs(_tank.GetNormalizedSpeed());
            AddVectorObs(_tank.onEdge);
            AddVectorObs(_tank.transform.forward);

            Transform enemyTransform = enemy.transform;
            Vector3 enemyPos = enemyTransform.position;

            AddVectorObs(Distance2Target());
            AddVectorObs(enemy.transform.forward);
            Vector3 vecTo = (enemyPos - transform.position);
            AddVectorObs(vecTo.normalized);
            AddVectorObs((int) enemy.state, Enum.GetValues(typeof(TankState)).Length);
            AddVectorObs(Vector3.Dot(_tank.transform.forward.normalized, vecTo.normalized));
            AddVectorObs(enemy.GetNormalizedSpecial());
            AddVectorObs(1 - Vector2.Distance(_tank.lastCollisionPos, _tank.transform.position) / _tank.tooCloseLimit);
        }

        public override void AgentAction(float[] vectorAction, string textAction) {
            if (_input.simulating) {
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
            }

//            Vector3 vecTo = (enemy.transform.position - transform.position);
//            Debug.Log("Reward: " + GetReward() );
//            Debug.Log($"Cumulative: {GetCumulativeReward()}");
//            Debug.Log(totalReward.ToString("0.##########"));


            Monitor.Log("Reward", GetCumulativeReward(), transform);
        }

        private void NormalReward() {
            float totalReward = 0;

            totalReward -= .00005f * (1 - Distance2Target());

            if (_tank.GetNormalizedSpeed() <= .4f) {
                totalReward -= .0002f;
            }

            
            if (_tank.GetNormalizedSpecial() <= .35f) {
                totalReward -= .0002f;
            }
            
            if (_tank.tooCloseFlag) {
                totalReward -= .0003f * (1 - Vector2.Distance(_tank.transform.position, _tank.lastCollisionPos) /
                                         _tank.tooCloseLimit);
            }

            if (_tank.onEdge) {
                totalReward -= .0003f;
            }


            AddReward(totalReward);
            if (_tank.state == TankState.DEAD) {
                if (GetCumulativeReward() > 0) {
                    SetReward(-1f);
                }
                else {
                    AddReward(-1f);
                }

                StartCoroutine(WaitBeforeReset(0));

                collectReward = false;
            }

            if (enemy.state == TankState.DEAD) {
                if (enemy.TimeSinceLastCollision < 5) {
                    if (GetCumulativeReward() < 0) {
                        SetReward(1f);
                    }
                    else {
                        AddReward(1f);
                    }

                    collectReward = false;
                }

                StartCoroutine(WaitBeforeReset(0));
            }
        }

        public void TackleReward(Vector3 col) {
            float totalReward = 0;


            //Facing forward
            float forwardTackle = ForwardDot(col);

            if (forwardTackle < 0.5f || _tank.GetNormalizedSpeed() < 0.5f) return;


            float side =
                Mathf.Abs(Vector3.Dot(_tank.transform.forward.normalized,
                    enemy.transform.right.normalized)); // Is it attacking the enemy from the side?

            side = side >= .5f ? side : .5f;
            totalReward += forwardTackle * side * (_tank.state == TankState.BOOST ? 1 : .3f);
            AddReward(totalReward);
        }


        private IEnumerator WaitBeforeReset(float time) {
            yield return new WaitForSeconds(time);

            Done();
            enemyAgent.Done();
        }

        public override void AgentReset() {
            gs.Reset();
            collectReward = true;
        }


        private float Distance2Target() {
            return Vector3.Distance(enemy.transform.position, _tank.transform.position) / 50;
        }

        // Is it facing the enemy ?
        private float ForwardDot() {
            return ForwardDot(Vector3.zero);
        }

        private float ForwardDot(Vector3 c) {
            var transform1 = _tank.transform;
            Vector3 toCheck = c == Vector3.zero ? enemy.transform.position : c;
            return Mathf.Abs(Vector3.Dot(transform1.forward,
                (toCheck - transform1.position).normalized));
        }
    }
}