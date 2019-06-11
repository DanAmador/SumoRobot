using System.Collections;
using UnityEngine;

namespace Tank {
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TankInputs))]
    public class TankController : MonoBehaviour {
        #region Variables

        [Header("Movement Properties")] public float rotationRate, turnRotationAngle, turnRotationSeekSpeed;

        public TankState state;

        [Range(3, 10)] private const float MAX_SPECIAL = 5;
        [Range(500, 1000)] private const float MAX_SPEED = 1000;
        [Range(150, 500)] private const float START_SPEED = 200;
        private float _rotationVelocity, _groundAngleVelocity;
        public bool onEdge;

        public Vector3 lastCollisionImpulse;
        public TankInputs _input;
        public float currSpeed { get; private set; }
        public float specialCounter { get; private set; }

        private Rigidbody _rb;
        private Thruster _thrusters;

        private Vector3 initialPos;
        private Quaternion initialRot;

        #region Getters

        public float getNormalizedSpeed() {
            return (currSpeed - START_SPEED) / (MAX_SPEED - START_SPEED);
        }

        #endregion

        #endregion


        #region Mono Methods

        void Start() {
            _rb = GetComponent<Rigidbody>();
            _input = GetComponent<TankInputs>();
            _thrusters = GetComponentInChildren<Thruster>();
            currSpeed = START_SPEED;
            state = TankState.NORMAL;
            initialPos = transform.position;
            initialRot = transform.rotation;
        }


        public void Reset() {
            transform.position = initialPos;
            transform.rotation = initialRot;
            state = TankState.NORMAL;
            currSpeed = START_SPEED;
            specialCounter = 0;
            lastCollisionImpulse = Vector3.zero;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.position = initialPos;
            _rb.rotation = initialRot;
        }

        private void Update() {
            specialCounter = Mathf.Clamp(specialCounter + Time.deltaTime, 0, MAX_SPECIAL);
            if (state == TankState.NORMAL) {
                currSpeed = Mathf.Clamp(currSpeed + Mathf.Abs(_input.ForwardInput) * currSpeed * Time.deltaTime, 1,
                    MAX_SPEED);

                if (_input.Turbo) {
                    StartCoroutine(TurboBoost(specialCounter));
                }

                if (_input.Block) {
                    if (specialCounter > 3) {
                        StartCoroutine(BlockRoutine());
                    }
                }
            }
        }


        void FixedUpdate() {
            if (state == TankState.COLLIDED) {
                state = TankState.NORMAL;
                currSpeed = START_SPEED;
            }

            if (_rb && _input && _thrusters) {
                HandleMovement();
            }
        }

        #region Collider Events

        void OnCollisionEnter(Collision collision) {
            if (state == TankState.BLOCK) {
                lastCollisionImpulse = collision.impulse;
                return;
            }

            if (collision.gameObject.CompareTag("Player")) {
                TankController collider = collision.gameObject.GetComponent<TankController>();
                if (collider.state == TankState.BLOCK) {
                    state = TankState.COLLIDED;
                    collision.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                    _rb.AddForce((-collision.impulse / Time.deltaTime) * 2);
                }
                else {
                    lastCollisionImpulse = collision.impulse;
                    collider.state = TankState.COLLIDED;
                }

                currSpeed = START_SPEED;
            }

            if (collision.gameObject.CompareTag("Death")) {
                state = TankState.DEAD;
            }
        }

        private void OnCollisionExit(Collision collision) {
            if (state == TankState.BLOCK) return;
            if (collision.gameObject.CompareTag("Player")) {
                TankController collider = collision.gameObject.GetComponent<TankController>();
                if (collider.state == TankState.BLOCK) {
                    collision.rigidbody.constraints = RigidbodyConstraints.None;
                }
            }
        }


        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.CompareTag("Edge")) {
                onEdge = true;
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
            float drift_modifier = _input.Drift ? 0.5f : 1;

            if (Physics.Raycast(transform.position, transform.up * -1, _thrusters.distance)) {
                _rb.drag = 1 * (1 / drift_modifier);
                Vector3 forwardForce = currSpeed * _input.ForwardInput * drift_modifier * transform.forward;
                forwardForce = Time.deltaTime * _rb.mass * forwardForce;
                _rb.AddForce(forwardForce);
            }
            else {
                _rb.drag = 0;
            }


            Vector3 turnTorque = rotationRate * _input.RotationInput * (1 / drift_modifier) * Vector3.up;

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
        }

        private bool BlockPredicate() {
            _rb.AddForce(-_rb.velocity);
            _rb.velocity = Vector3.zero;
            _rb.position = transform.position;
            specialCounter -= Time.deltaTime * 5;
            return !_input.Block || specialCounter <= 0;
        }

        private IEnumerator TurboBoost(float currentSpecial) {
            float fraction = (currentSpecial * currentSpecial) / MAX_SPECIAL;
            state = TankState.BOOST;
            currSpeed = currSpeed + MAX_SPEED * fraction;
            yield return new WaitForSeconds(1);

            state = TankState.NORMAL;
            specialCounter = 0;
            currSpeed = MAX_SPEED;
        }

        #endregion

        #endregion
    }
}