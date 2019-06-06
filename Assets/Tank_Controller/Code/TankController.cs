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

        public TankState state;
        [Header("Movement Properties")] public float turnSpeed = 5f;

        [SerializeField] private float currSpeed, specialCounter;

        [Range(1, 10f)] private float maxSpeed = 8f;
        [Range(3, 10)] private const float MAX_SPECIAL = 5;
        [Range(700, 2000)] private const float MAX_SPEED = 1500;
        [Range(150, 500)] private const float START_SPEED = 200;

        private Rigidbody rb;
        private TankInputs input;
        private Thruster thrusters;


        public float rotationRate;
        public float turnRotationAngle, turnRotationSeekSpeed;

        private float rotationVelocity, groundAngleVelocity;

        #endregion

        #region Mono Methods

        void Start() {
            rb = GetComponent<Rigidbody>();
            input = GetComponent<TankInputs>();
            thrusters = GetComponent<Thruster>();
            state = TankState.NORMAL;
        }

        private void Update() {
            specialCounter = Mathf.Clamp(specialCounter + Time.deltaTime, 0, MAX_SPECIAL);
            if (state == TankState.NORMAL) {
                currSpeed = Mathf.Clamp(currSpeed + Mathf.Abs(input.ForwardInput) * currSpeed * Time.deltaTime, 1,
                    MAX_SPEED);

                if (input.Turbo) {
                    StartCoroutine(TurboBoost(specialCounter));
                }

                if (input.Block) {
                    StartCoroutine(BlockRoutine());
                }
            }


            if (state == TankState.COLLIDED) {
                currSpeed = START_SPEED;
            }
        }

        void FixedUpdate() {
            if (rb && input) {
                HandleMovement();
            }
        }

        #endregion


        #region Custom Code

        protected virtual void HandleMovement() {
            if (Physics.Raycast(transform.position, transform.up * -1, thrusters.distance + 3)) {
                rb.drag = 1;

                Vector3 forwardForce = currSpeed * input.ForwardInput * transform.forward;
                forwardForce = Time.deltaTime * rb.mass * forwardForce;
                rb.AddForce(forwardForce);
            }
            else {
                rb.drag = 0;
            }


            Vector3 turnTorque = rotationRate * input.RotationInput * Vector3.up;

            turnTorque = Time.deltaTime * rb.mass * turnTorque;
            rb.AddTorque(turnTorque);

            Vector3 newRotation = transform.eulerAngles;
            newRotation.z = Mathf.SmoothDampAngle(newRotation.z, input.ForwardInput * -turnRotationAngle,
                ref rotationVelocity, turnRotationSeekSpeed);
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
            rb.constraints = RigidbodyConstraints.FreezeAll;
            yield return new WaitUntil(BlockPredicate);

            state = TankState.NORMAL;
            rb.constraints = RigidbodyConstraints.None;
        }

        private bool BlockPredicate() {
            return !input.Block || specialCounter <= 0;
        }

        private IEnumerator TurboBoost(float currentSpecial) {
            float fraction = currentSpecial / MAX_SPECIAL;
            state = TankState.BOOST;
            currSpeed = currSpeed + (maxSpeed * fraction);
            yield return new WaitForSeconds(0.2f);

            state = TankState.NORMAL;
            specialCounter = 0;
            currSpeed = MAX_SPECIAL;
        }

        #endregion

        #endregion
    }
}