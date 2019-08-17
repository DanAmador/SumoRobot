using System;
using System.Collections;
using Tank.AI;
using UnityEngine;

namespace Tank {
    [RequireComponent(typeof(ThrusterManager))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TankInputs))]
    [RequireComponent(typeof(TankAgent))]
    public class TankController : MonoBehaviour {
        #region Variables

        [Header("Movement Properties")] public float rotationRate;
        public float turnRotationAngle;
        public float turnRotationSeekSpeed;

        public TankState state;
    
        public float TimeSinceLastCollision {
            get => lastCollisionPos == _initialPos ? 0 : Time.time - lastColTime;
        }
        public float MaxSpecial {
            get => MAX_SPECIAL;
        }

        [Header("Gameplay Values"), Range(5, 20), SerializeField]
        private float MAX_SPECIAL = 15;


        [Range(800, 3000), SerializeField] private float MAX_SPEED = 2000;
        [Range(150, 500), SerializeField] private float START_SPEED = 300;
        [Range(1, 5), SerializeField] private float timeToMaxSpeed = 2.5f;
        [Range(0, 1)] public float special4Block = 0.4f;
        [Range(0, 1)] public float special4Boost = 0.1f;

        private float _rotationVelocity, _groundAngleVelocity, _accelRatePerSec, lastColTime;
        
        [Header("Internal variables")] public bool onEdge;

        public bool tooCloseFlag => Vector3.Distance(lastCollisionPos, transform.position) < tooCloseLimit;
        [NonSerialized] public Vector3 lastCollisionPos;
        [NonSerialized] public TankInputs _input;
        public float current_speed { get; private set; }
        public float specialCounter { get; private set; }

        private Rigidbody _rb;
        private ThrusterManager _tm;
        private Vector3 _initialPos;
        private Quaternion _initialRot;
        [SerializeField] private TankAgent _agent;
        public float tooCloseLimit = 10;

        #region Getters

        public float GetNormalizedSpeed() {
            return (current_speed - START_SPEED) / (MAX_SPEED - START_SPEED);
        }

        public float GetNormalizedSpecial() {
            return specialCounter / MAX_SPECIAL;
        }

        #endregion

        #endregion


        #region Mono Methods

        void Start() {
            _agent = GetComponent<TankAgent>();
            _accelRatePerSec = (MAX_SPEED - START_SPEED) / timeToMaxSpeed;
            _rb = GetComponent<Rigidbody>();
            _tm = GetComponent<ThrusterManager>();
            _input = GetComponent<TankInputs>();
            current_speed = START_SPEED;
            state = TankState.NORMAL;
            _initialPos = transform.position;
            _initialRot = transform.rotation;
        }


        public void Reset() {
            _tm.ToggleThrust();
            lastCollisionPos = _initialPos;


            transform.position = _initialPos;
            transform.rotation = _initialRot;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
            state = TankState.NORMAL;
            current_speed = START_SPEED;
            specialCounter = 0;

            _rb.constraints = RigidbodyConstraints.None;
            StartCoroutine(ResetWait());
        }

        private void Update() {
            Debug.DrawLine(lastCollisionPos, lastCollisionPos + transform.up * 10);
            specialCounter = Mathf.Clamp(specialCounter + Time.deltaTime, 0, MAX_SPECIAL);


            if (state == TankState.NORMAL) {
                current_speed = Mathf.Clamp(current_speed + Mathf.Abs(_input.ForwardInput) * _accelRatePerSec * Time.deltaTime,
                    START_SPEED, MAX_SPEED);


                if (_input.Turbo) {
                    if (specialCounter > MAX_SPECIAL * special4Boost) {
                        StartCoroutine(TurboBoost(specialCounter));
                    }
                }

                if (_input.Block) {
                    if (specialCounter >= MAX_SPECIAL * special4Block) {
                        StartCoroutine(BlockRoutine());
                    }
                }


                if (Mathf.Abs(_input.ForwardInput) < .50f) {
                    current_speed = Mathf.Clamp(
                        current_speed - Mathf.Abs(_input.ForwardInput) * _accelRatePerSec * Time.deltaTime * .5f,
                        START_SPEED, MAX_SPEED);
                }
            }
        }


        void FixedUpdate() {
            if (_rb && _input && _tm) {
                HandleMovement();
            }
        }

        #region Collider Events

        //I really should've used a State or Observer Design pattern ... this is a fucking mess, but it's too late now, sorry :( 
        private void OnCollisionEnter(Collision collision) {
            if (!collision.gameObject.CompareTag("Player")) return;
            if (state == TankState.BLOCK) {
                _rb.constraints = RigidbodyConstraints.FreezeAll;
                _agent.AddReward(1);
                return;
            }

            TankController collider = collision.gameObject.GetComponent<TankController>();
            if (collider.state == TankState.BLOCK) {
                StartCoroutine(CollisionStateHandler());
                _rb.AddForceAtPosition(collider.transform.position, -2 * collision.impulse / Time.deltaTime);
            }

            if (collider.state == TankState.BOOST) {
                StartCoroutine(CollisionStateHandler());
            }


            if (!tooCloseFlag) {
                var position = collision.transform.position;
                _agent.TackleReward(position);
                lastCollisionPos = position;
                lastColTime = Time.time;
            }


            if (state != TankState.BOOST) {
                current_speed = START_SPEED;
            }
        }

        private void OnCollisionExit(Collision collision) {
            if (collision.gameObject.CompareTag("Player")) {
                if (state == TankState.BLOCK) {
                    _rb.constraints = RigidbodyConstraints.None;
                }
            }
        }


        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.CompareTag("Edge")) {
                onEdge = true;
            }


            if (other.gameObject.CompareTag("Death")) {
                state = TankState.DEAD;
            }
        }

        private void OnTriggerStay(Collider other) {
            if (other.gameObject.CompareTag("Edge")) {
                onEdge = true;
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.gameObject.CompareTag("Edge")) {
                onEdge = false;
            }
        }

        #endregion

        #endregion


        #region Custom Code

        #region Game Logic 

        protected virtual void HandleMovement() {
            float driftModifier = _input.Drift ? 0.5f : 1;

            if (Physics.Raycast(transform.position, transform.up * -1, _tm.distance)) {
                _rb.drag = 1 * (1 / driftModifier);
                Vector3 forwardForce = current_speed * _input.ForwardInput * driftModifier * transform.forward;
                forwardForce = Time.deltaTime * _rb.mass * forwardForce;
                _rb.AddForce(forwardForce);
            }
            else {
                _rb.drag = 0;
            }


            Vector3 turnTorque = rotationRate * _input.RotationInput * (1 / driftModifier) * Vector3.up;

            turnTorque = Time.deltaTime * _rb.mass * turnTorque;
            _rb.AddTorque(turnTorque);

            Vector3 newRotation = transform.eulerAngles;
            newRotation.z = Mathf.SmoothDampAngle(newRotation.z, _input.RotationInput * -turnRotationAngle,
                ref _rotationVelocity, turnRotationSeekSpeed);
            transform.eulerAngles = newRotation;
        }

        #endregion

        #region ObservationHelper

        #endregion

        #region Routines

        private IEnumerator BlockRoutine() {
            state = TankState.BLOCK;

            yield return new WaitUntil(BlockPredicate);

            state = TankState.NORMAL;
            _rb.constraints = RigidbodyConstraints.None;
        }

        private bool BlockPredicate() {
            _rb.AddForce(-_rb.velocity);
            _rb.velocity = Vector3.zero;
            _rb.position = transform.position;
            specialCounter -= Time.deltaTime * 5;
            return !_input.Block || specialCounter <= 0;
        }

        private IEnumerator TurboBoost(float currentSpecial) {
            float fraction = currentSpecial * currentSpecial / MAX_SPECIAL;
            state = TankState.BOOST;
            current_speed += MAX_SPEED * fraction;
            yield return new WaitForSeconds(1);

            specialCounter = 0;
            current_speed = MAX_SPEED;

            yield return new WaitForSeconds(.5f);

            state = TankState.NORMAL;
        }

        private IEnumerator CollisionStateHandler() {
            state = TankState.COLLIDED;
            current_speed = START_SPEED;
            _agent.AddReward(-.005f);
            yield return new WaitForSeconds(1);

            state = TankState.NORMAL;
        }

        private IEnumerator ResetWait() {
            yield return new WaitForSeconds(1);
            _tm.ToggleThrust();
            _rb.isKinematic = false;

        }

        #endregion

        #endregion
    }
}