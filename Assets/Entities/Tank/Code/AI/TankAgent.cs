using System;
using System.Collections;
using GameSession;
using MLAgents;
using UnityEngine;

namespace Tank.AI {
    public class TankAgent : Agent {
        private TankController _tank, _enemy;
        private TankAgent _enemyAgent;
        public GameSessionManager gs;
        private TankInputs _input;
        private RayPerception3D _rayPerception;
        private bool _collectReward;

        private float _rayDistance;
        private readonly float[] _rayAngles = {0f, 45f, 70f, 90f, 135f, 180f, 110f, 270};
        private readonly string[] _observables = {"Edge", "Player"};

        public override void InitializeAgent() {
            base.InitializeAgent();
            _tank = GetComponent<TankController>();
            _input = GetComponent<TankInputs>();
//            _input.playerControl = false;

            _rayDistance = _tank.tooCloseLimit * 2;
            _enemy = gs.GetEnemy(_tank);
            _enemyAgent = _enemy.GetComponent<TankAgent>();


            _rayPerception = GetComponent<RayPerception3D>();
            _collectReward = true;
        }

        public override void CollectObservations() {
            if (!_enemy) {
                InitializeAgent();
            }

            Transform tankTransform = _tank.transform;
            Vector3 normalized = tankTransform.rotation.eulerAngles / 360.0f; // [0,1]


            AddVectorObs(_rayPerception.Perceive(_rayDistance, _rayAngles, _observables, 0f, 0f));
            AddVectorObs(_rayPerception.Perceive(_rayDistance / 2, _rayAngles, _observables, 0f, 0f));


            AddVectorObs(normalized);
            AddVectorObs(gs.MatchPercentageRemaining);

            AddVectorObs((int) _tank.state, Enum.GetValues(typeof(TankState)).Length);
            AddVectorObs(Mathf.Clamp(1 - (_tank.GetNormalizedSpecial() / _tank.special4Block), 0f, 1f));
            AddVectorObs(Mathf.Clamp(1 - (_tank.GetNormalizedSpecial() / _tank.special4Boost) * 5, 0f, 1));

            AddVectorObs(Mathf.Abs(ForwardDot()));
            AddVectorObs(_tank.MaxSpecial);
            AddVectorObs(_tank.GetNormalizedSpeed());
            AddVectorObs(_tank.onEdge);
            AddVectorObs(_tank.transform.forward);

            Transform enemyTransform = _enemy.transform;
            Vector3 enemyPos = enemyTransform.position;

            AddVectorObs(Distance2Target());
            AddVectorObs(_enemy.transform.forward);
            Vector3 vecTo = (enemyPos - transform.position);

            AddVectorObs(vecTo.normalized);

            AddVectorObs((int) _enemy.state, Enum.GetValues(typeof(TankState)).Length);
            AddVectorObs(Vector3.Dot(_tank.transform.forward.normalized, vecTo.normalized));
            AddVectorObs(_enemy.GetNormalizedSpecial());
            AddVectorObs(1 - Vector2.Distance(_tank.lastCollisionPos, _tank.transform.position) / _tank.tooCloseLimit);
        }

        public override void AgentAction(float[] vectorAction, string textAction) {
            if (gs.MatchPercentageRemaining <= 0) {
                AddReward(-1f);
                _collectReward = false;
                StartCoroutine(WaitBeforeReset(1));
            }

            if (_input.simulating) {
                int forward = Mathf.FloorToInt(vectorAction[0]);
                int rotation = Mathf.FloorToInt(vectorAction[1]);
                int button = Mathf.FloorToInt(vectorAction[2]);

                if (forward == 1) {
                    _input.ForwardInput = 1;
                    AddReward(.0005f);
                }

                if (forward == 2) _input.ForwardInput = -1;

                if (rotation == 1) _input.RotationInput = 1;
                if (rotation == 2) _input.RotationInput = -1;

                if (button == 1) _input.VirtualInputSimulate(Buttons.BLOCK, 1f);
                if (button == 2) _input.VirtualInputSimulate(Buttons.TURBO, 1f);
                if (button == 3) _input.VirtualInputSimulate(Buttons.DRIFT);


                if (_collectReward) NormalReward();
            }

//            Vector3 vecTo = (enemy.transform.position - transform.position);
//            Debug.Log("Reward: " + GetReward() );
//            Debug.Log($"Cumulative: {GetCumulativeReward()}");
//            Debug.Log(totalReward.ToString("0.##########"));


            Monitor.Log("Reward", GetCumulativeReward(), transform);
        }

        private void NormalReward() {
            float totalReward = 0;

            totalReward -= .00005f * (1 - _tank.GetNormalizedSpecial() );


            if (_tank.TooCloseFlag && _tank.MustFleeFromCollision) {
                totalReward -= .0003f * (1 - Vector2.Distance(_tank.transform.position, _tank.lastCollisionPos) /
                                         _tank.tooCloseLimit);
            }

            if (_tank.onEdge) {
                totalReward -= .0003f;
            }


            AddReward(totalReward);
            if (_tank.state == TankState.DEAD) {
                SetReward(-5f);

                StartCoroutine(WaitBeforeReset(0));

                _collectReward = false;
            }

            if (_enemy.state == TankState.DEAD) {
                if (_tank.TimeSinceLastCollision < 4){
                    AddReward(1 + 5f * gs.MatchPercentageRemaining);
                    _collectReward = false;
                }

                StartCoroutine(WaitBeforeReset(0));
            }
        }

        public void TackleReward(Vector3 col) {
            float totalReward = 0;
            AddReward(.05f * gs.MatchPercentageRemaining);
            if (_tank.state != TankState.BOOST) return;

            // Is it facing the collision? 
            float forwardTackle = ForwardDot(col);


            // Is it attacking the enemy from the side?
            float side = Mathf.Abs(
                Vector3.Dot(_tank.transform.forward.normalized, _enemy.transform.right.normalized));

            side = side >= .5f ? side : .5f;
            totalReward += forwardTackle * side;
            AddReward(totalReward);
        }


        private IEnumerator WaitBeforeReset(float time) {
            yield return new WaitForSeconds(time);

            Done();
            _enemyAgent.Done();
        }

        public override void AgentReset() {
            gs.Reset();
            _collectReward = true;
        }


        private float Distance2Target() {
            return Vector3.Distance(_enemy.transform.position, _tank.transform.position) / 50;
        }

        // Is it facing the enemy ?
        private float ForwardDot() {
            return ForwardDot(Vector3.zero);
        }

        private float ForwardDot(Vector3 c) {
            var transform1 = _tank.transform;
            Vector3 toCheck = c == Vector3.zero ? _enemy.transform.position : c;
            return Mathf.Abs(Vector3.Dot(transform1.forward,
                (toCheck - transform1.position).normalized));
        }
    }
}