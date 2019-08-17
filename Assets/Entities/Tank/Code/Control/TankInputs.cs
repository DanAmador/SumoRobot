using System.Collections;
using UnityEngine;

namespace Tank {
    public class TankInputs : MonoBehaviour {
        public bool playerControl = true;
        private float _forwardInput, _rotationInput;
        private bool _turbo, _block, _drift;
        private bool _blockFlag, _turboFlag, _driftFlag;
        private int playerNum = 1;
        public bool simulating = true;
        private void Update() {
            HandleInputs();
        }


        public float ForwardInput {
            get => _forwardInput;
            set => _forwardInput = value;
        }

        public float RotationInput {
            get => _rotationInput;
            set => _rotationInput = value;
        }

        public bool Turbo {
            get => _turbo;
            set => _turbo = value;
        }

        public bool Block {
            get => _block;
            set => _block = value;
        }

        public void VirtualInputSimulate(Buttons button, float time = .5f) {
            StartCoroutine(HoldValue(time, button));
        }

        //Please don't judge this ugly ass piece of code, I've been working for 5 days straight and I don't know what I'm doing
        private IEnumerator HoldValue(float time, Buttons option) {
            switch (option) {
            case Buttons.BLOCK:
                if (!_blockFlag) {
                    _blockFlag = true;
                    Block = true;

                    yield return new WaitForSecondsRealtime(time);
                    _blockFlag = false;
                    Block = false;
                }

                break;
            case Buttons.TURBO:
                if (!_turboFlag) {
                    _turboFlag = true;
                    Turbo = true;

                    yield return new WaitForSecondsRealtime(time);
                    _turboFlag = false;
                    Turbo = false;
                }

                break;

            case Buttons.DRIFT:
                if (!_driftFlag) {
                    _driftFlag = true;
                    Drift = true;

                    yield return new WaitForSecondsRealtime(time);
                    _driftFlag = false;
                    Drift = false;
                }

                break;
            }
        }

        public bool Drift {
            get => _drift;
            set => _drift = value;
        }

        protected virtual void HandleInputs() {
            if (!playerControl) return;
            _forwardInput = Input.GetAxis($"Vertical{playerNum}");
            _rotationInput = Input.GetAxis($"Horizontal{playerNum}");
            _turbo = Input.GetButtonDown($"Turbo{playerNum}");
            _block = Input.GetButton($"Block{playerNum}");
            _drift = Input.GetButton($"Drift{playerNum}");
        }
    }
}