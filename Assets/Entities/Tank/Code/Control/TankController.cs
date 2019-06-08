using System.Collections;
using UnityEngine;

namespace Tank {
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TankInputs))]
    [RequireComponent(typeof(Thruster))]
    public class TankController : MonoBehaviour {
        #region Variables

        [Header("Movement Properties")] public float turnSpeed = 5f;
        public float rotationRate, turnRotationAngle, turnRotationSeekSpeed;

        public TankState state;

        [Range(3, 10)] private const float MAX_SPECIAL = 5;
        [Range(500, 1000)] private const float MAX_SPEED = 1000;
        [Range(150, 500)] private const float START_SPEED = 200;
        [SerializeField] private float specialCounter;
        private float _currSpeed, _rotationVelocity, _groundAngleVelocity;

        private Rigidbody _rb;
        private TankInputs _input;
        private Thruster _thrusters;



        #endregion

        #region Mono Methods

        void Start() {
            _rb = GetComponent<Rigidbody>();
            _input = GetComponent<TankInputs>();
            _thrusters = GetComponent<Thruster>();
            state = TankState.NORMAL;
            _currSpeed = START_SPEED;

        }

        private void Update() {
            specialCounter = Mathf.Clamp(specialCounter + Time.deltaTime, 0, MAX_SPECIAL);
            if (state == TankState.NORMAL) {
                _currSpeed = Mathf.Clamp(_currSpeed + Mathf.Abs(_input.ForwardInput) * _currSpeed * Time.deltaTime, 1,
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


            if (state == TankState.COLLIDED) {
                _currSpeed = START_SPEED;
            }
        }


        void FixedUpdate() {
            if (_rb && _input && _thrusters) {
                HandleMovement();
            }
        }

        #endregion


        #region Custom Code

        protected virtual void HandleMovement() {
            float drift_modifier = _input.Drift ? 0.5f : 1;

            if (Physics.Raycast(transform.position, transform.up * -1, _thrusters.distance + 3)) {
                _rb.drag = 1 * (1 / drift_modifier) ;
                Vector3 forwardForce = _currSpeed * _input.ForwardInput * drift_modifier*transform.forward;
                forwardForce = Time.deltaTime * _rb.mass * forwardForce; 
                _rb.AddForce(forwardForce);
            }
            else {
                _rb.drag = 0;
            }


            Vector3 turnTorque = rotationRate * _input.RotationInput  * (1 / drift_modifier) * Vector3.up;

            turnTorque = Time.deltaTime * _rb.mass * turnTorque;
            _rb.AddTorque(turnTorque);

            Vector3 newRotation = transform.eulerAngles;
            newRotation.z = Mathf.SmoothDampAngle(newRotation.z, _input.RotationInput* -turnRotationAngle,
                ref _rotationVelocity, turnRotationSeekSpeed);
            transform.eulerAngles = newRotation;
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.CompareTag("Death")) {
                Debug.Log("nani");
            }
        }

        void OnCollisionEnter(Collision collision) {
            if(state == TankState.BLOCK) return;
            if (collision.gameObject.CompareTag("Player")) {
                TankController collider = collision.gameObject.GetComponent<TankController>();
                if (collider.state == TankState.BLOCK) {
                    collision.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                    _rb.AddForce((-collision.impulse/Time.deltaTime) * 2);
                }

                _currSpeed = START_SPEED;
            }

        }

        private void OnCollisionExit(Collision collision) {
            if(state == TankState.BLOCK) return;
            if (collision.gameObject.CompareTag("Player")) {
                TankController collider = collision.gameObject.GetComponent<TankController>();
                if (collider.state == TankState.BLOCK) {
                    collision.rigidbody.constraints = RigidbodyConstraints.None;
                }
            }
        }


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
            _currSpeed = _currSpeed + MAX_SPEED * fraction;
            yield return new WaitForSeconds(1);

            state = TankState.NORMAL;
            specialCounter = 0;
            _currSpeed = MAX_SPEED;
        }

        #endregion

        #endregion
    }
}