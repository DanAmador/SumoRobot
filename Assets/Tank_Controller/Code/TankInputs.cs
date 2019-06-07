using System;
using UnityEngine;

namespace Tank_Controller {
    public class TankInputs : MonoBehaviour {
        private float forwardInput, rotationInput;
        private bool turbo, block;
        private int playerNum = 1;

        private void Update() {
            HandleInputs();
        }

        public float ForwardInput => forwardInput;
        public float RotationInput => rotationInput;
        public bool Turbo => turbo;
        public bool Block => block;
        protected virtual void HandleInputs() {
            forwardInput = Input.GetAxis($"Vertical{playerNum}");
            rotationInput = Input.GetAxis($"Horizontal{playerNum}");
            turbo = Input.GetButtonDown($"Turbo{playerNum}");
            block = Input.GetButton($"Block{playerNum}");
        }
    }
}