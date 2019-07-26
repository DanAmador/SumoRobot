using System;
using System.Collections;
using UnityEngine;

namespace Tank {
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(TankInputs))]
	public class TankController : MonoBehaviour {
		#region Variables

		[Header("Movement Properties")] public float rotationRate, turnRotationAngle, turnRotationSeekSpeed;

		public TankState state;

		[Header("Player Parameters")] 
		[Range(3, 10), SerializeField] private float MAX_SPECIAL = 5 ;
		[Range(800, 3000), SerializeField] private float MAX_SPEED = 2000;
		[Range(150, 500), SerializeField] private  float START_SPEED = 300;
		[Range(1, 5), SerializeField] private float timeToStartMax = 2.5f;

		private float _rotationVelocity, _groundAngleVelocity, _accelRatePerSec;
		public bool onEdge, tooCloseFlag;
		[NonSerialized] public Vector3 lastCollisionImpulse, lastCollisionPos;
		public TankInputs _input;
		public float CurrSpeed { get; private set; }
		public float SpecialCounter { get; private set; }

		private Rigidbody _rb;
		private Thruster _thrusters;

		private Vector3 _initialPos;
		private Quaternion _initialRot;

		#region Getters

		public float GetNormalizedSpeed() {
			return (CurrSpeed - START_SPEED) / (MAX_SPEED - START_SPEED);
		}

		public float GetNormalizedSpecial() {
			return SpecialCounter / MAX_SPECIAL;
		}

		#endregion

		#endregion


		#region Mono Methods

		void Start() {
			_accelRatePerSec = (MAX_SPEED - START_SPEED) / timeToStartMax;
			_rb = GetComponent<Rigidbody>();
			_input = GetComponent<TankInputs>();
			_thrusters = GetComponentInChildren<Thruster>();
			CurrSpeed = START_SPEED;
			state = TankState.NORMAL;
			_initialPos = transform.position;
			_initialRot = transform.rotation;
		}


		public void Reset() {
			lastCollisionImpulse = Vector3.zero;
			lastCollisionPos = _initialPos;
			_rb.velocity = Vector3.zero;
			_rb.angularVelocity = Vector3.zero;

			transform.position = _initialPos;
			transform.rotation = _initialRot;
			state = TankState.NORMAL;
			CurrSpeed = START_SPEED;
			SpecialCounter = 0;

			_rb.constraints = RigidbodyConstraints.None;
		}

		private void Update() {
			Debug.DrawLine(lastCollisionPos, lastCollisionPos + transform.up * 10);
			SpecialCounter = Mathf.Clamp(SpecialCounter + Time.deltaTime, 0, MAX_SPECIAL);

			if (state == TankState.NORMAL) {
				CurrSpeed = Mathf.Clamp(CurrSpeed + Mathf.Abs(_input.ForwardInput) * _accelRatePerSec * Time.deltaTime,
					START_SPEED, MAX_SPEED);


				if (_input.Turbo) {
					if (SpecialCounter > 3) {
						StartCoroutine(TurboBoost(SpecialCounter));
					}
				}

				if (_input.Block) {
					if (SpecialCounter > 3) {
						StartCoroutine(BlockRoutine());
					}
				}


				if (Mathf.Abs(_input.ForwardInput) < .50f) {
					CurrSpeed = Mathf.Clamp(CurrSpeed - (CurrSpeed * Time.deltaTime) * .5f , START_SPEED,
						MAX_SPEED);
				}
			}
		}


		void FixedUpdate() {
			if (state == TankState.COLLIDED) {
				state = TankState.NORMAL;
				CurrSpeed = START_SPEED;
			}

			if (_rb && _input && _thrusters) {
				HandleMovement();
			}
		}

		#region Collider Events

		void OnCollisionEnter(Collision collision) {
			if (state == TankState.BLOCK) {
				lastCollisionImpulse = collision.impulse;
				return;
			}

			if (collision.gameObject.CompareTag("Player")) {
				if (Vector3.Distance(lastCollisionPos, collision.transform.position) > 9 &&
				    GetNormalizedSpeed() > .9f) {
					tooCloseFlag = false;
					lastCollisionPos = collision.transform.position;
				}
				else {
					tooCloseFlag = true;
				}

				TankController collider = collision.gameObject.GetComponent<TankController>();
				if (collider.state == TankState.BLOCK) {
					state = TankState.COLLIDED;
					collision.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
					_rb.AddForce(-2 * collision.impulse / Time.deltaTime);
				}
				else {
					lastCollisionImpulse = collision.impulse;
					collider.state = TankState.COLLIDED;
				}

				CurrSpeed = START_SPEED;
			}
		}

		private void OnCollisionExit(Collision collision) {
			if (state == TankState.BLOCK) return;
			if (collision.gameObject.CompareTag("Player")) {
				TankController collider = collision.gameObject.GetComponent<TankController>();
				if (collider.state == TankState.BLOCK) {
					collision.rigidbody.constraints = RigidbodyConstraints.None;
				}
			}
		}


		private void OnTriggerEnter(Collider other) {
			if (other.gameObject.CompareTag("Edge")) {
				onEdge = true;
			}


			if (other.gameObject.CompareTag("Death")) {
				state = TankState.DEAD;
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

		protected virtual void HandleMovement() {
			float drift_modifier = _input.Drift ? 0.5f : 1;

			if (Physics.Raycast(transform.position, transform.up * -1, _thrusters.distance)) {
				_rb.drag = 1 * (1 / drift_modifier);
				Vector3 forwardForce = CurrSpeed * _input.ForwardInput * drift_modifier * transform.forward;
				forwardForce = Time.deltaTime * _rb.mass * forwardForce;
				_rb.AddForce(forwardForce);
			}
			else {
				_rb.drag = 0;
			}


			Vector3 turnTorque = rotationRate * _input.RotationInput * (1 / drift_modifier) * Vector3.up;

			turnTorque = Time.deltaTime * _rb.mass * turnTorque;
			_rb.AddTorque(turnTorque);

			Vector3 newRotation = transform.eulerAngles;
			newRotation.z = Mathf.SmoothDampAngle(newRotation.z, _input.RotationInput * -turnRotationAngle,
				ref _rotationVelocity, turnRotationSeekSpeed);
			transform.eulerAngles = newRotation;
		}

		#endregion

		#region ObservationHelper

		#endregion

		#region Routines

		private IEnumerator BlockRoutine() {
			state = TankState.BLOCK;

			yield return new WaitUntil(BlockPredicate);

			state = TankState.NORMAL;
		}

		private bool BlockPredicate() {
			_rb.AddForce(-_rb.velocity);
			_rb.velocity = Vector3.zero;
			_rb.position = transform.position;
			SpecialCounter -= Time.deltaTime * 5;
			return !_input.Block || SpecialCounter <= 0;
		}

		private IEnumerator TurboBoost(float currentSpecial) {
			float fraction = (currentSpecial * currentSpecial) / MAX_SPECIAL;
			state = TankState.BOOST;
			CurrSpeed += MAX_SPEED * fraction;
			yield return new WaitForSeconds(1);

			state = TankState.NORMAL;
			SpecialCounter = 0;
			CurrSpeed = MAX_SPEED;
		}

		#endregion

		#endregion
	}
}