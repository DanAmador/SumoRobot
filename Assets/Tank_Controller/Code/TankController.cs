using System.Collections;
using UnityEngine;

namespace Tank_Controller {
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TankInputs))]
    public class TankController : MonoBehaviour {
        #region Variables

        [Range(1, 10f)] private float maxSpeed = 8f;
        [Range(0, 0.1f)] public float turnSpeed = .1f;
        [Header("Movement Properties")] public float tankRotationSpeed = 20f;
        [SerializeField] private float currSpeed = 0f;
        [SerializeField] private float specialCounter = 0;


        private Rigidbody rb;
        private TankInputs input;

        #endregion

        #region Mono Methods

        void Start() {
            rb = GetComponent<Rigidbody>();
            input = GetComponent<TankInputs>();
        }

        private void Update() {
            currSpeed += (input.ForwardInput * 3) * Time.deltaTime -
                         ((1 - Mathf.Abs(input.ForwardInput)) * Time.deltaTime * 10);
            currSpeed = Mathf.Clamp(currSpeed, 3, maxSpeed);


            specialCounter = Mathf.Clamp(specialCounter + Time.deltaTime, 0, 5);
            if (input.Turbo) {
                StartCoroutine(turboBoost(specialCounter));
            }
        }

        void FixedUpdate() {
            if (rb && input) {
                HandleMovement();
            }
        }

        #endregion


        #region Custom Code

        private IEnumerator turboBoost(float currentSpecial) {
            currSpeed = currentSpecial * 6;
            yield return new WaitForSeconds(.2f);
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