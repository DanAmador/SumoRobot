using UnityEngine;

namespace Tank_Controller {
    public class TankInputs : MonoBehaviour {
        private float forwardInput, rotationInput;
        // Start is called before the first frame update
        void Start() { }

        // Update is called once per frame
        void FixedUpdate() {
        }

        public float ForwardInput => forwardInput;
        public float RotationInput => rotationInput;
        
        protected virtual void HandleInputs() {
            forwardInput = Input.GetAxis("Vertical");
            rotationInput = Input.GetAxis("Horizontal");
        }
    }
}