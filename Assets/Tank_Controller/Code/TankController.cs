using System;
using System.Collections;
using UnityEngine;

namespace Tank_Controller {
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TankInputs))]
    public class TankController : MonoBehaviour {
        #region Variables

        public TankState state;
        [Range(1, 10f)] private float maxSpeed = 8f;
        [Header("Movement Properties")] public float turnSpeed = 5f;
        [SerializeField] private float currSpeed, specialCounter;
        public float hoverForce = 65f;
        public float hoverHeight = 2f;
        private Rigidbody rb;
        private TankInputs input;

        #endregion

        #region Mono Methods

        void Start() {
            rb = GetComponent<Rigidbody>();
            input = GetComponent<TankInputs>();
            state = TankState.NORMAL;
        }

        private void Update() {
            specialCounter = Mathf.Clamp(specialCounter + Time.deltaTime, 0, 5);
            if (state == TankState.NORMAL) {
//                currSpeed += input.ForwardInput * 3 * Time.deltaTime -
//                             (1 - Mathf.Abs(input.ForwardInput)) * Time.deltaTime * 10;
//                currSpeed = Mathf.Clamp(currSpeed, 3, maxSpeed);

                if (input.Turbo) {
                    StartCoroutine(TurboBoost(specialCounter));
                }

                if (input.Block) {
                    StartCoroutine(BlockRoutine());
                }
            }
        }

        void FixedUpdate() {
            if (rb && input) {
                HandleMovement();
            }
        }

        #endregion


        #region Custom Code

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
            state = TankState.BOOST;
            currSpeed = currentSpecial * 6;
            yield return new WaitForSeconds(0.2f);

            state = TankState.NORMAL;
            specialCounter = 0;
            currSpeed = maxSpeed;
        }

        protected virtual void HandleMovement() {
//            rb.MovePosition(transform.position + (currSpeed * Time.deltaTime * input.ForwardInput * transform.forward));
//            Quaternion wantedRotation = transform.rotation *
//                                        Quaternion.Euler(input.RotationInput * Time.deltaTime * turnSpeed *
//                                                         Vector3.up);
//            rb.MoveRotation(wantedRotation);
//            
            
            Ray ray = new Ray(transform.position, -transform.up);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, hoverHeight)) {
                float proportionalHeight = (hoverHeight - hit.distance) / hoverHeight;
                Vector3 appliedHoverForce = Vector3.up * proportionalHeight * hoverForce;
                rb.AddForce(appliedHoverForce, ForceMode.Acceleration);
            }

            rb.AddRelativeForce(0f, 0f, input.ForwardInput * currSpeed);
            rb.AddRelativeTorque(0f, input.RotationInput * turnSpeed, 0f);
        }


        void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.tag == "Player") {
                TankController collider = collision.gameObject.GetComponent<TankController>();
            }
        }

        #endregion
    }
}