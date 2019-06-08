using System;
using UnityEngine;

namespace Tank {
    public class CameraTracker : MonoBehaviour {
        public Transform target;

        public float distanceUp, distanceBack, minimumHeight;

        private Vector3 positionVelocity;

        private void Start() {
            if (target != null) return;
            TankController tank = GetComponentInParent<TankController>();
            target = tank.transform;

        }

        private void FixedUpdate() {
            Vector3 newPosition = target.position + (target.forward * distanceBack);
            newPosition.y = Mathf.Max(newPosition.y + distanceUp, minimumHeight);

            transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref positionVelocity, 0.18f);

            Vector3 focalPoint = target.position + (target.forward * 5);
            transform.LookAt(focalPoint);
        }
    }
}