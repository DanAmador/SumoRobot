using UnityEngine;

namespace Tank_Controller {
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TankInputs))]
    public class TankController : MonoBehaviour {
        #region Variables
        
        [Header("Movement Properties")]
        public float tankSpeed = 15f;
        public float tankRotationSpeed = 20f;
        
        private Rigidbody  rb;
        private TankInputs input;

        #endregion

        #region Mono Methods

        void Start() {
            rb = GetComponent<Rigidbody>();
            input = GetComponent<TankInputs>();
        }

        void FixedUpdate() {
            if (rb && input) {
                HandleMovement();
            }
        }

        #endregion

        #region Custom Code

        protected virtual void HandleMovement() {
            rb.MovePosition(transform.position + (tankSpeed * Time.deltaTime * transform.forward * input.ForwardInput));
        }

        #endregion
    }
}