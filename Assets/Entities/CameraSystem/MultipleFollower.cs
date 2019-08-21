using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CameraSystem {
    [RequireComponent(typeof(Camera))]
    public class MultipleFollower : MonoBehaviour {
        private Camera cam;
        public Vector3 offset;
        public List<Transform> targets;

        public float minZoom = 40f;
        public float maxZoom = 40f;
        public float zoomLimiter = 40f;
        public float smoothTime = .5f;


        private Vector3 velocity;

        void Start() {
            cam = GetComponent<Camera>();
        }

        void LateUpdate() {
            if (targets.Count == 0) return;

//            Move();
            Zoom();
        }

        private void Zoom() {
            float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
            cam.fieldOfView = newZoom;
        }


        private void Move() {
            transform.position =
                Vector3.SmoothDamp(transform.position, GetCenterPoint() + offset, ref velocity, smoothTime);
        }

        private float GetGreatestDistance() {
            var bounds = new Bounds(targets[0].position, Vector3.zero);
            foreach (var t in targets) {
                bounds.Encapsulate(t.position);
            }

            return bounds.size.magnitude;
        }


        private Vector3 GetCenterPoint() {
            if (targets.Count == 1) {
                return targets.First().position;
            }

            Bounds bounds = new Bounds(targets.First().position, Vector3.zero);
            targets.ForEach(t => bounds.Encapsulate(t.position));
            return bounds.center;
        }
    }
}