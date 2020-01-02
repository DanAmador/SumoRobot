using System;
using System.Collections;
using GameSession;
using MLAgents;
using UnityEngine;
using Random = System.Random;

namespace Tank.AI {
    public class TankAgent : Agent {
        private TankController _tank, _enemy;
        [SerializeField] private TankAgent _enemyAgent;
        public GameSessionManager gs;
        private TankInputs _input;
        private RayPerception3D _rayPerception;
        private bool _collectReward;
        private float _rayDistance;

        private ResetParameters _resetParameters;

        private readonly float[] _rayAngles = {
            0f, 15f, 30f, 45f, 60f, 75f, 90f, 105f, 120f, 135f, 150, 165, 180f, 195, 210, 225, 240, 255, 270, 285, 300,
            315, 330, 345
        };

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


            TankAcademy academy = FindObjectOfType<TankAcademy>();
            _resetParameters = academy.resetParameters;
        }

        public override void CollectObservations() {
            if (!_enemy) {
                InitializeAgent();
            }

            Transform enemyTransform = _enemy.transform;
            Vector3 enemyPos = enemyTransform.position;
            Vector3 vecTo = (enemyPos - transform.position).normalized;


            AddVectorObs(_rayPerception.Perceive(_rayDistance, _rayAngles, _playerObs, 0f, 0f));
            AddVectorObs(_rayPerception.Perceive(_rayDistance * 1.5f, _rayAngles, _edgeObs, 0f, 0f));

            AddVectorObs(gs.MatchPercentageRemaining);

            AddVectorObs((int) _tank.state, Enum.GetValues(typeof(TankState)).Length);
            AddVectorObs((int) _enemy.state, Enum.GetValues(typeof(TankState)).Length);

            AddVectorObs(Mathf.Clamp01(1 - _tank.GetNormalizedSpecial() / _tank.special4Block));
            AddVectorObs(Mathf.Clamp01(1 - _tank.GetNormalizedSpecial() / _tank.special4Boost));
            AddVectorObs(Mathf.Clamp01(1 - _enemy.GetNormalizedSpecial() / _enemy.special4Block));


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

            AddVectorObs(1 - Mathf.Clamp01(Vector2.Distance(_tank.lastCollisionPos, _tank.transform.position) /
                                           _tank.tooCloseLimit));
            AddVectorObs(1 - Mathf.Clamp01(Vector2.Distance(_enemy.lastCollisionPos, _enemy.transform.position) /
                                           _enemy.tooCloseLimit));

            AddVectorObs(_tank.MustFleeFromCollision);
            AddVectorObs(_tank.TooCloseFlag);

            AddVectorObs(Mathf.Clamp01(_tank.TimeSinceLastCollision / (gs.roundDuration * .10f)));

            AddVectorObs(_enemy.MustFleeFromCollision);
            AddVectorObs(_enemy.TooCloseFlag);
        }

        public override void AgentAction(float[] vectorAction, string textAction) {
            if (_input.simulating) {
                _input.ForwardInput = vectorAction[0];
                _input.RotationInput = vectorAction[1];

                if (vectorAction[2] > .5)
                    _input.VirtualInputSimulate(Buttons.BLOCK, Mathf.Abs(vectorAction[2]) * 2);
                if (vectorAction[3] > .3)
                    _input.VirtualInputSimulate(Buttons.TURBO, Mathf.Abs(vectorAction[3]) * 2);
                if (vectorAction[4] > .3) _input.VirtualInputSimulate(Buttons.DRIFT);
            }

            if (_collectReward) {
                NormalReward();
                if (gs.MatchPercentageRemaining <= 0) {
                    AddReward(-.7f);
                    _collectReward = false;
                    Done();
                }
            }


        }

        private void NormalReward() {
            float totalReward = 0;


            //Optimize being inside of circle and being outside for enemy
            totalReward += (1 - gs.UnitDistance2Center(_tank.transform)) * .0002f;
            totalReward += gs.UnitDistance2Center(_enemy.transform) * .0005f;
          
            //incentivize constant hitting
            if (_tank.TimeSinceLastCollision / (gs.roundDuration * .10f) >= .9f) {
                totalReward += -.003f;
            }

            // KEEP MOVING BRUUUH 
            if (_input.ForwardInput > 0) {
                totalReward += _input.ForwardInput * _tank.GetNormalizedSpeed() * .001f;
            }
            else {
                //Velocity needs to be maintained 
                totalReward += .0005f *
                               (_tank.GetNormalizedSpeed() > .5f
                                   ? 0
                                   : Mathf.Clamp01(1 - _tank.GetNormalizedSpeed() * 3) * -1);
            }



            if (_tank.onEdge) totalReward -= .0006f;
            if (_tank.TooCloseFlag && _tank.MustFleeFromCollision && _tank.numCollisions != 0)
                totalReward -= .0003f * (1 - Vector2.Distance(_tank.transform.position, _tank.lastCollisionPos) /
                                         _tank.tooCloseLimit);

            AddReward(totalReward);
        }

        public void TackleReward(Vector3 col) {
            float totalReward = 0;

            float forwardTackle = Mathf.Abs(ForwardDot(col));


            // Is it facing the collision? 

            AddReward(1f * gs.MatchPercentageRemaining);

            if (forwardTackle < .5f) return;

            // Is it attacking the enemy from the side?
            float side = Mathf.Abs(
                Vector3.Dot(_tank.transform.forward.normalized, _enemy.transform.right.normalized));
            
            if (_tank.state == TankState.BLOCK && _enemy.state == TankState.COLLIDED) {
                totalReward += .01f;
            }
            totalReward += side * .2f;
            totalReward += forwardTackle * (_tank.state == TankState.BOOST ? 2 : .8f) * _tank.GetNormalizedSpeed();
            AddReward(Mathf.Clamp01(totalReward));
        }


        public void Dead() {
            float reward = Mathf.Clamp01(2 * gs.MatchPercentageRemaining);
            AddReward(-reward);
            if (_tank.MustFleeFromCollision) _enemyAgent.AddReward(reward);
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
            SetResetParameters();
        }


        private float Distance2Target() {
            return Vector3.Distance(_enemy.transform.position, _tank.transform.position) / 60;
        }

        // Is it facing the enemy ?
        private float ForwardDot() {
            return ForwardDot(Vector3.zero);
        }

        private float ForwardDot(Vector3 c) {
            Vector3 toCheck = (c == Vector3.zero ? _enemy.transform.position : c);
            return _tank.ForwardDot(toCheck);
        }

        private void SetResetParameters() {
            Vector3 rand = UnityEngine.Random.onUnitSphere * _resetParameters["spawnVariance"];
            rand.y = 0;
            _tank.transform.position += rand;
            _tank.special4Block = _resetParameters["special4Block"];
            _tank.special4Boost = _resetParameters["special4Boost"];
        }
    }
}