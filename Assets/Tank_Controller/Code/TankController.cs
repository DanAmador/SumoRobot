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
        [Header("Movement Properties")] public float tankRotationSpeed = 20f;
        [SerializeField] private float currSpeed, specialCounter;

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
                currSpeed += input.ForwardInput * 3 * Time.deltaTime -
                             (1 - Mathf.Abs(input.ForwardInput)) * Time.deltaTime * 10;
                currSpeed = Mathf.Clamp(currSpeed, 3, maxSpeed);

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
            rb.MovePosition(transform.position + (currSpeed * Time.deltaTime * input.ForwardInput * transform.forward));
            Quaternion wantedRotation = transform.rotation *
                                        Quaternion.Euler(input.RotationInput * Time.deltaTime * tankRotationSpeed *
                                                         Vector3.up);
            rb.MoveRotation(wantedRotation);
        }


        void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.tag == "Player") {
                Debug.Log("Ye");
            }
        }

        #endregion
    }
}