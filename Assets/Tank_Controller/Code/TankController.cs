using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Tank_Controller {
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TankInputs))]
    [RequireComponent(typeof(Thruster))]
    public class TankController : MonoBehaviour {
        #region Variables

        [Header("Movement Properties")] public float turnSpeed = 5f;
        public float rotationRate, turnRotationAngle, turnRotationSeekSpeed;

        public TankState state;

        [Range(3, 10)] private const float MAX_SPECIAL = 5;
        [Range(700, 2000)] private const float MAX_SPEED = 1500;
        [Range(150, 500)] private const float START_SPEED = 200;
        [SerializeField] private float currSpeed, specialCounter;
        private float _rotationVelocity, _groundAngleVelocity;

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
                    StartCoroutine(BlockRoutine());
                }
            }


            if (state == TankState.COLLIDED) {
                currSpeed = START_SPEED;
            }
        }

        void FixedUpdate() {
            if (_rb && _input) {
                HandleMovement();
            }
        }

        #endregion


        #region Custom Code

        protected virtual void HandleMovement() {
            if (Physics.Raycast(transform.position, transform.up * -1, _thrusters.distance + 3)) {
                _rb.drag = 1;

                Vector3 forwardForce = currSpeed * _input.ForwardInput * transform.forward;
                forwardForce = Time.deltaTime * _rb.mass * forwardForce;
                _rb.AddForce(forwardForce);
            }
            else {
                _rb.drag = 0;
            }


            Vector3 turnTorque = rotationRate * _input.RotationInput * Vector3.up;

            turnTorque = Time.deltaTime * _rb.mass * turnTorque;
            _rb.AddTorque(turnTorque);

            Vector3 newRotation = transform.eulerAngles;
            newRotation.z = Mathf.SmoothDampAngle(newRotation.z, _input.ForwardInput * -turnRotationAngle,
                ref _rotationVelocity, turnRotationSeekSpeed);
            transform.eulerAngles = newRotation;
        }


        void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.CompareTag("Player")) {
                TankController collider = collision.gameObject.GetComponent<TankController>();
            }
        }


        #region Routines

        private IEnumerator BlockRoutine() {
            state = TankState.BLOCK;
            _rb.constraints = RigidbodyConstraints.FreezeAll;
            yield return new WaitUntil(BlockPredicate);

            state = TankState.NORMAL;
            _rb.constraints = RigidbodyConstraints.None;
        }

        private bool BlockPredicate() {
            return !_input.Block || specialCounter <= 0;
        }

        private IEnumerator TurboBoost(float currentSpecial) {
            float fraction = currentSpecial / MAX_SPECIAL;
            state = TankState.BOOST;
            currSpeed = currSpeed + (MAX_SPEED * fraction);
            yield return new WaitForSeconds(0.2f);

            state = TankState.NORMAL;
            specialCounter = 0;
            currSpeed = MAX_SPECIAL;
        }

        #endregion

        #endregion
    }
}