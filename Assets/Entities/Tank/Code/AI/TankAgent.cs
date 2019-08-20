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
        [SerializeField] private bool _collectReward;

        private float _rayDistance;
        private readonly float[] _rayAngles = {0f, 45f, 70f, 90f, 135f, 180f, 110f, 270};
        private readonly string[] _playerObs = {"Player"};
        private readonly string[] _edgeObs = {"Edge"};

        public override void InitializeAgent() {
            base.InitializeAgent();
            _tank = gameObject.GetComponent<TankController>();
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

            Transform enemyTransform = _enemy.transform;
            Vector3 enemyPos = enemyTransform.position;
            Vector3 vecTo = (enemyPos - transform.position).normalized;


            AddVectorObs(_rayPerception.Perceive(_rayDistance, _rayAngles, _playerObs, 0f, 0f));
            AddVectorObs(_rayPerception.Perceive(_rayDistance * 0.6666667F, _rayAngles, _edgeObs, 0f, 0f));

            AddVectorObs(gs.MatchPercentageRemaining);

            AddVectorObs((int) _tank.state, Enum.GetValues(typeof(TankState)).Length);
            AddVectorObs((int) _enemy.state, Enum.GetValues(typeof(TankState)).Length);

            AddVectorObs(Mathf.Clamp(1 - (_tank.GetNormalizedSpecial() / _tank.special4Block), 0f, 1f));
            AddVectorObs(Mathf.Clamp(1 - (_tank.GetNormalizedSpecial() / (_tank.special4Boost )), 0f, 1));
            AddVectorObs(Mathf.Clamp(1 - (_enemy.GetNormalizedSpecial() / _enemy.special4Block), 0f, 1f));

            
            AddVectorObs(ForwardDot());
            AddVectorObs(_enemyAgent.ForwardDot());
            
            AddVectorObs(_tank.GetNormalizedSpeed());
            AddVectorObs(_tank.GetNormalizedSpecial());
            

            AddVectorObs(_enemy.GetNormalizedSpeed());
            AddVectorObs(_enemy.GetNormalizedSpecial());

            AddVectorObs(_tank.onEdge);
            AddVectorObs(_enemy.onEdge);


            AddVectorObs(Distance2Target());

            AddVectorObs(vecTo);

            AddVectorObs(1 - Vector2.Distance(_tank.lastCollisionPos, _tank.transform.position) / _tank.tooCloseLimit);
            AddVectorObs(1 - Vector2.Distance(_enemy.lastCollisionPos, _enemy.transform.position) / _enemy.tooCloseLimit);
        }

        public override void AgentAction(float[] vectorAction, string textAction) {
            if (_input.simulating) {
                _input.ForwardInput = vectorAction[0];
                _input.RotationInput = vectorAction[1];

                if (vectorAction[2] > .6)
                    _input.VirtualInputSimulate(Buttons.BLOCK, ((vectorAction[2] - .6f) / .4f) * 2);
                if (vectorAction[3] > .8)
                    _input.VirtualInputSimulate(Buttons.TURBO, ((vectorAction[3] - .8f) / .4f) * 2);
                if (vectorAction[4] > .6) _input.VirtualInputSimulate(Buttons.DRIFT);
            }

            if (_collectReward) {
                NormalReward();
                if (gs.MatchPercentageRemaining <= 0) {
                    AddReward(-.7f);
                    _collectReward = false;
                    Done();
                }
            }

//            Vector3 vecTo = (enemy.transform.position - transform.position);
//            Debug.Log("Reward: " + GetReward() );
//            Debug.Log($"{brain.name} cumulative: {GetCumulativeReward()}");
//            Debug.Log(totalReward.ToString("0.##########"));


            Monitor.Log($"{gameObject.name} reward: ", GetCumulativeReward(), _enemy.transform);
            Monitor.Log($"{gameObject.name} dot: ", ForwardDot(), _enemy.transform);
        }

        private void NormalReward() {
//            Debug.Log(_tank.GetNormalizedSpecial());
            float totalReward = 0;
//            
            totalReward += .0005f * _tank.GetNormalizedSpeed();


            if (_tank.TooCloseFlag && _tank.MustFleeFromCollision) {
                totalReward -= .0003f * (1 - Vector2.Distance(_tank.transform.position, _tank.lastCollisionPos) /
                                         _tank.tooCloseLimit);
            }

            if (_tank.onEdge) {
                totalReward -= .0005f;
            }


            AddReward(totalReward);
        }

        public void TackleReward(Vector3 col) {
            float totalReward = 0;

            float forwardTackle = ForwardDot(col);

            totalReward += .05f * gs.MatchPercentageRemaining;

            // Is it facing the collision? 
            if (forwardTackle < .5f) return;

            // Is it attacking the enemy from the side?
            float side = Mathf.Abs(
                Vector3.Dot(_tank.transform.forward.normalized, _enemy.transform.right.normalized));

//            side = side >= .5f ? side : .5f;
            totalReward += side * .02f;
            totalReward += forwardTackle * (_tank.state == TankState.BOOST ? 1 : .5f);

            AddReward(Mathf.Clamp(totalReward, 0, 1));
        }


        public void Dead() {
            float reward = gs.MatchPercentageRemaining;
            AddReward(-reward);
//            _enemyAgent.AddReward(reward);
            _collectReward = false;
            StartCoroutine(WaitBeforeReset(2));
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
            return Vector3.Distance(_enemy.transform.position, _tank.transform.position) / 60;
        }

        // Is it facing the enemy ?
        private float ForwardDot() {
            return ForwardDot(Vector3.zero);
        }

        private float ForwardDot(Vector3 c) {
            var transform1 = _tank.transform;
            Vector3 toCheck = c == Vector3.zero ? _enemy.transform.position : c;
            return Vector3.Dot(transform1.forward, (toCheck - transform1.position).normalized);
        }
    }
}