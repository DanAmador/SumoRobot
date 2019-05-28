using System;
using UnityEngine;

namespace Tank_Controller {
    public class TankInputs : MonoBehaviour {
        private float forwardInput, rotationInput;
        private bool turbo;
        private int playerNum = 1;
        void Start() { }

        private void Update() {
            HandleInputs();
        }

        public float ForwardInput => forwardInput;
        public float RotationInput => rotationInput;
        public bool Turbo => turbo; 
        protected virtual void HandleInputs() {
            forwardInput = Input.GetAxis($"Vertical{playerNum}");
            rotationInput = Input.GetAxis($"Horizontal{playerNum}");
            turbo = Input.GetButton($"Turbo{playerNum}");
        }
    }
}