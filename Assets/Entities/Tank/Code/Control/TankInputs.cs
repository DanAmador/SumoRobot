using UnityEngine;

namespace Tank {
    public class TankInputs : MonoBehaviour {
        public bool playerControl = true;
        private float forwardInput, rotationInput;
        private bool turbo, block, drift;
        private int playerNum = 1;

        private void Update() {
            HandleInputs();
        }


        public float ForwardInput {
            get => forwardInput;
            set => forwardInput = value;
        }

        public float RotationInput {
            get => rotationInput;
            set => rotationInput = value;
        }

        public bool Turbo {
            get => turbo;
            set => turbo = value;
        }

        public bool Block {
            get => block;
            set => block = value;
        }

        public bool Drift {
            get => drift;
            set => drift = value;
        }

        protected virtual void HandleInputs() {
            if (!playerControl) return;
            forwardInput = Input.GetAxis($"Vertical{playerNum}");
            rotationInput = Input.GetAxis($"Horizontal{playerNum}");
            turbo = Input.GetButtonDown($"Turbo{playerNum}");
            block = Input.GetButton($"Block{playerNum}");
            drift = Input.GetButton($"Drift{playerNum}");
        }
    }
}