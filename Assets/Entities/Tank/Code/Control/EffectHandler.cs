using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tank {
	[RequireComponent(typeof(CameraTracker))]
	
	[RequireComponent(typeof(TankController))]
	public class EffectHandler : MonoBehaviour {
		public GameObject forceField, turboTrail;
		public TrailRenderer[] trails;
		private TankController tc;
		private CameraTracker cam;

		private bool _boostFlag;
		void Start() {
			
			tc = GetComponent<TankController>();
			cam = GetComponentInChildren<CameraTracker>();
		}

		void Update() {
			if (tc.state == TankState.BOOST) {
				if (!_boostFlag) {
					StartCoroutine(CameraBackup());
					
				}
			}
			forceField.SetActive(tc.state == TankState.BLOCK);
		}
			
		private IEnumerator CameraBackup() {
			foreach (TrailRenderer trail in trails) {
				trail.emitting = true;
			}
			float currBack = cam.distanceBack;
			_boostFlag = true;
			cam.distanceBack = currBack * 2;
			
			yield return new WaitForSeconds(1);
			
			cam.distanceBack = currBack;

			yield return new WaitForSeconds(1);
			foreach (TrailRenderer trail in trails) {
				trail.emitting = false;
			}
			_boostFlag = false;

		}
		
	}
	

}