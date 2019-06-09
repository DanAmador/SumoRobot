using UnityEngine;

namespace Platform {
    public class PlatformMover : MonoBehaviour {
        [SerializeField, Range(0, 2)] private float xAmplitude, zAmplitude;
        [SerializeField, Range(1, 15)] private float xRotation = 1, zRotation = 1;
        private Transform initialTrans;
        private Vector3 initialPos;
        private Quaternion initialRot;

        private void Start() {
            initialTrans = transform;
            initialPos = transform.position;
            initialRot = transform.rotation;
        }

        void Update() {
            float pingPong = Mathf.PingPong(Time.time, Mathf.PI * 2);
            float x = xAmplitude * xRotation == 0 ? 0 : xRotation - Mathf.Sin(pingPong * xAmplitude) * xRotation;
            float z = zAmplitude * zRotation == 0 ? 0 : zRotation - Mathf.Cos(pingPong * zAmplitude) * zRotation;
            Vector3 rotationVector = new Vector3(x, 0, z);
            transform.rotation = Quaternion.Euler(rotationVector);
        }

        public void Reset() {
            transform.position = initialPos;
            transform.rotation = initialRot;
        }
    }
}