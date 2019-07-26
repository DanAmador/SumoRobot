using System.Collections.Generic;
using UnityEngine;

namespace Tank {
	public class ThrusterManager : MonoBehaviour {
		public GameObject[] thrusterTransforms;
		private List<Thruster> thrusters;
		public bool working = true;
		public float strength, distance;

		public void Start() {
			Rigidbody _rb = GetComponent<Rigidbody>();

			thrusters = new List<Thruster>();
			foreach (GameObject obj in thrusterTransforms) {
				Thruster thruster = obj.AddComponent(typeof(Thruster)) as Thruster;
				thruster.InitValues(strength, distance, _rb);
				thrusters.Add(thruster);
			}
		}

		public void ToggleThrust() {
			working = !working;
		}

		void FixedUpdate() {
			if (!working) return;
			
			foreach (Thruster thruster in thrusters) {
				thruster.Thrust();
			}
			
		}
	}
}

