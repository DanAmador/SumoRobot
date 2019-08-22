using System;
using System.Collections;
using Tank.AI;
using UnityEngine;

namespace Tank {
    [RequireComponent(typeof(ThrusterManager))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TankInputs))]
    [RequireComponent(typeof(TankAgent))]
    public class TankController : MonoBehaviour {
        #region Variables

        [Header("Movement Properties")] public float rotationRate;
        public float turnRotationAngle;
        public float turnRotationSeekSpeed;

        public TankState state;


        [field: Header("Gameplay Values")]
        [field: Range(5, 20)]
        [field: SerializeField]
        public float MAX_SPECIAL { get; } = 15;

        private float CurrentSpeed { get; set; }
        private float SpecialCounter { get; set; }

        private float TimeSinceLastCollision => lastCollisionPos == _initialPos ? 0 : Time.time - _lastColTime;

        public bool MustFleeFromCollision => TimeSinceLastCollision < 3;
        public bool TooCloseFlag => Vector3.Distance(lastCollisionPos, transform.position) < tooCloseLimit;


        [Range(800, 3000), SerializeField] private float MAX_SPEED = 2000;
        [Range(150, 500), SerializeField] private float START_SPEED = 300;
        [Range(1, 5), SerializeField] private float timeToMaxSpeed = 2.5f;
        [Range(4, 10), SerializeField] private float timeToMAX_SPECIAL = 5f;
        [Range(0, 1)] public float special4Block = 0.4f;
        [Range(0, 1)] public float special4Boost = 0.2f;

        private float _rotationVelocity, _groundAngleVelocity, _accelRatePerSec, _lastColTime, specialRatePerSec;

        [Header("Internal variables")] public bool onEdge;

        [NonSerialized] public Vector3 lastCollisionPos;
        [NonSerialized] private TankInputs _input;
        private Rigidbody _rb;
        private ThrusterManager _tm;
        private Vector3 _initialPos;
        private Quaternion _initialRot;
        [SerializeField] private TankAgent _agent;
        public float tooCloseLimit = 15;

        #region Getters

        public float GetNormalizedSpeed() {
            if (state == TankState.BLOCK) return 0;

            return Mathf.Clamp01((CurrentSpeed - START_SPEED) / (MAX_SPEED - START_SPEED));
        }

        public float GetNormalizedSpecial() {
            return Mathf.Clamp01(SpecialCounter / MAX_SPECIAL);
        }

        #endregion

        #endregion


        #region Mono Methods

        void Start() {
            _agent = gameObject.GetComponent<TankAgent>();
            _accelRatePerSec = (MAX_SPEED - START_SPEED) / timeToMaxSpeed;
            specialRatePerSec = MAX_SPECIAL / timeToMAX_SPECIAL;
            _rb = GetComponent<Rigidbody>();
            _tm = GetComponent<ThrusterManager>();
            _input = GetComponent<TankInputs>();
            CurrentSpeed = START_SPEED;
            state = TankState.NORMAL;
            var transform1 = transform;
            _initialPos = transform1.position;
            _initialRot = transform1.rotation;
        }


        public void Reset() {
            _tm.ToggleThrust();
            lastCollisionPos = _initialPos;


            transform.position = _initialPos;
            transform.rotation = _initialRot;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
            state = TankState.NORMAL;
            CurrentSpeed = START_SPEED;
            SpecialCounter = 0;

            _rb.constraints = RigidbodyConstraints.None;
            StartCoroutine(ResetWait());
        }

        private void Update() {
            Debug.DrawLine(lastCollisionPos, lastCollisionPos + transform.up * 10);

            if (state == TankState.NORMAL) {
                SpecialCounter =
                    Mathf.Clamp(
                        SpecialCounter + (Mathf.Log(1 + GetNormalizedSpeed() * 5) + 1) * specialRatePerSec *
                        Time.deltaTime,
                        0, MAX_SPECIAL);

                CurrentSpeed = Mathf.Clamp(
                    CurrentSpeed + Mathf.Abs(_input.ForwardInput) * _accelRatePerSec * Time.deltaTime,
                    START_SPEED, MAX_SPEED);


                if (_input.Turbo) {
                    if (SpecialCounter > MAX_SPECIAL * special4Boost) {
                        StartCoroutine(TurboBoost());
                    }
                }

                if (_input.Block) {
                    if (SpecialCounter >= MAX_SPECIAL * special4Block) {
                        StartCoroutine(BlockRoutine());
                    }
                }
            }

            if (Mathf.Abs(_input.ForwardInput) < .50f) {
                CurrentSpeed = Mathf.Clamp(
                    CurrentSpeed - _accelRatePerSec * Time.deltaTime * .25f,
                    START_SPEED, MAX_SPEED);
            }
        }


        void FixedUpdate() {
            if (_rb && _input && _tm) {
                HandleMovement();
            }
        }

        #region Collider Events

        //I really should've used a State or Observer Design pattern ... this is a fucking mess, but it's too late now, sorry :( 
        private void OnCollisionEnter(Collision collision) {
            if (!collision.gameObject.CompareTag("Player")) return;
            if (state == TankState.BLOCK) {
                _rb.constraints = RigidbodyConstraints.FreezeAll;
                _agent.AddReward(.7f * GetNormalizedSpecial());
                return;
            }


            TankController other = collision.gameObject.GetComponent<TankController>();
            float otherDot = other.ForwardDot(transform.position);
            if (Mathf.Abs(ForwardDot(other.transform.position)) < Mathf.Abs(otherDot)) {
                if (Mathf.Abs(otherDot) >= .5f) {
                    Vector3 impulseForce = collision.impulse;
                    switch (other.state) {
                    case TankState.BLOCK:
                        StartCoroutine(CollisionStateHandler());
                        impulseForce *= 7;
                        break;
                    case TankState.BOOST:
                        StartCoroutine(CollisionStateHandler());
                        SpecialCounter += MAX_SPECIAL * .3f;
                        impulseForce *= 3 ;
                        break;
                    }


                    _rb.AddForce(impulseForce, ForceMode.Impulse);
                }
            }
            else {
                if (!TooCloseFlag) {
                    var position = collision.transform.position;
                    _agent.TackleReward(position);

                    lastCollisionPos = position;
                    _lastColTime = Time.time;
                }
            }
        }

        private void OnCollisionExit(Collision collision) {
            if (collision.gameObject.CompareTag("Player")) {
                if (state == TankState.BLOCK) {
                    _rb.constraints = RigidbodyConstraints.None;
                }
            }
        }


        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.CompareTag("Edge")) {
                onEdge = true;
            }


            if (other.gameObject.CompareTag("Death")) {
                state = TankState.DEAD;
                _agent.Dead();
            }
        }

        private void OnTriggerStay(Collider other) {
            if (other.gameObject.CompareTag("Edge")) {
                onEdge = true;
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.gameObject.CompareTag("Edge")) {
                onEdge = false;
            }
        }

        #endregion

        #endregion


        #region Custom Code

        #region Game Logic 

        public float ForwardDot(Vector3 c) {
            var transform1 = transform;
            return Vector3.Dot(transform1.forward, (c - transform1.position).normalized);
        }

        private void HandleMovement() {
            float driftModifier = _input.Drift ? 0.5f : 1;

            if (Physics.Raycast(transform.position, transform.up * -1, _tm.distance)) {
                _rb.drag = 1 * (1 / driftModifier);
                Vector3 forwardForce = CurrentSpeed * _input.ForwardInput * driftModifier * transform.forward;
                forwardForce = Time.deltaTime * _rb.mass * forwardForce;
                _rb.AddForce(forwardForce);
            }
            else {
                _rb.drag = 0;
            }


            Vector3 turnTorque = rotationRate * _input.RotationInput * (1 / driftModifier) * Vector3.up;

            turnTorque = Time.deltaTime * _rb.mass * turnTorque;
            _rb.AddTorque(turnTorque);

            Vector3 newRotation = transform.eulerAngles;
            newRotation.z = Mathf.SmoothDampAngle(newRotation.z, _input.RotationInput * -turnRotationAngle,
                ref _rotationVelocity, turnRotationSeekSpeed);
            transform.eulerAngles = newRotation;


            SpecialCounter -= Mathf.Clamp(Mathf.Abs(_input.RotationInput) * 5, 0, MAX_SPECIAL) * Time.deltaTime;
        }

        #endregion

        #region ObservationHelper

        #endregion

        #region Routines

        private IEnumerator BlockRoutine() {
            state = TankState.BLOCK;
            SpecialCounter -= Mathf.Clamp(MAX_SPECIAL * .3f, 0, MAX_SPECIAL);

            yield return new WaitUntil(BlockPredicate);

            state = TankState.NORMAL;
            _rb.constraints = RigidbodyConstraints.None;
        }

        private bool BlockPredicate() {
            _rb.AddForce(-_rb.velocity * .05f);
//            _rb.velocity = Vector3.zero;
//            _rb.position = transform.position;
            SpecialCounter -= specialRatePerSec * Time.deltaTime * 5;
            return !_input.Block || SpecialCounter <= 0;
        }

        private IEnumerator TurboBoost() {
            state = TankState.BOOST;
            float oldSpeed = CurrentSpeed;
            CurrentSpeed = Mathf.Clamp(CurrentSpeed + CurrentSpeed * GetNormalizedSpecial() * 2, CurrentSpeed,
                MAX_SPEED * 1.5f);
            yield return new WaitForSeconds(1);

            SpecialCounter = Mathf.Clamp(SpecialCounter - MAX_SPECIAL * special4Boost, 0, MAX_SPECIAL);
            CurrentSpeed = oldSpeed;

            yield return new WaitForSeconds(.5f);

            state = TankState.NORMAL;
        }

        private IEnumerator CollisionStateHandler() {
            state = TankState.COLLIDED;
            CurrentSpeed = START_SPEED;
            yield return new WaitForSeconds(1);

            state = TankState.NORMAL;
        }

        private IEnumerator ResetWait() {
            yield return new WaitForSeconds(.5f);
            _tm.ToggleThrust();
            _rb.isKinematic = false;
        }

        #endregion

        #endregion
    }
}