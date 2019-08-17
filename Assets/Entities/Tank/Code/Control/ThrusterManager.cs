using System.Collections.Generic;
using UnityEngine;

namespace Tank {
	public class ThrusterManager : MonoBehaviour {
		public GameObject[] thrusterTransforms;
		private List<Thruster> _thrusters;
		public float strength, distance;
		public ParticleSystem ps;
		public void Awake() {
			Rigidbody rb = GetComponentInParent<Rigidbody>();

			_thrusters = new List<Thruster>();
			foreach (GameObject obj in thrusterTransforms) {
				Thruster thruster = obj.AddComponent(typeof(Thruster)) as Thruster;
				thruster.InitValues(strength, distance, rb, ps);
				_thrusters.Add(thruster);
			}
		}

		public void ToggleThrust() {
			foreach (Thruster thruster in _thrusters) {
					thruster.OnOff();
			}
		}

		private void FixedUpdate() {
			foreach (Thruster thruster in _thrusters) {
				thruster.Thrust();
			}
			
		}
	}
}

