using System.Collections.Generic;
using UnityEngine;

namespace Tank {
	public class ThrusterManager : MonoBehaviour {
		public GameObject[] thrusterTransforms;
		private List<Thruster> thrusters;
		public bool working = true;
		public float strength, distance;
		public ParticleSystem ps;
		public void Awake() {
			Rigidbody rb = GetComponentInParent<Rigidbody>();

			thrusters = new List<Thruster>();
			foreach (GameObject obj in thrusterTransforms) {
				Thruster thruster = obj.AddComponent(typeof(Thruster)) as Thruster;
				thruster.InitValues(strength, distance, rb, ps);
				thrusters.Add(thruster);
			}
		}

		public void ToggleThrust() {
			working = !working;
		}

		private void FixedUpdate() {
			if (!working) return;
			
			foreach (Thruster thruster in thrusters) {
				thruster.Thrust();
			}
			
		}
	}
}

