using System.Collections;
using UnityEngine;

namespace Tank {
    public class TankInputs : MonoBehaviour {
        public bool playerControl = true;
        private float forwardInput, rotationInput;
        private bool turbo, block, drift;
        private bool blockFlag, turboFlag, driftFlag;
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

        public void VirtualInputSimulate(Buttons button, float time = .5f) {
            StartCoroutine(HoldValue(time, button));
        }

        //Please don't judge this ugly ass piece of code, I've been working for 5 days straight and I don't know what I'm doing
        private IEnumerator HoldValue(float time, Buttons option) {
            switch (option) {
            case Buttons.BLOCK:
                if (!blockFlag) {
                    blockFlag = true;
                    Block = true;

                    yield return new WaitForSecondsRealtime(time);
                    blockFlag = false;
                    Block = false;
                }

                break;
            case Buttons.TURBO:
                if (!turboFlag) {
                    turboFlag = true;
                    Turbo = true;

                    yield return new WaitForSecondsRealtime(time);
                    turboFlag = false;
                    Turbo = false;
                }

                break;

            case Buttons.DRIFT:
                if (!driftFlag) {
                    driftFlag = true;
                    Drift = true;

                    yield return new WaitForSecondsRealtime(time);
                    driftFlag = false;
                    Drift = false;
                }

                break;
            }
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