using System;
using UnityEngine;

namespace Platform {
    public class PlatformMover : MonoBehaviour {
        [SerializeField, Range(0, 2)] private float xAmplitude, zAmplitude;
        [SerializeField, Range(1, 15)] private float xRotation = 1, zRotation = 1;

        void Update() {
            float pingPong = Mathf.PingPong(Time.time, Mathf.PI * 2);
            float x = xAmplitude == 0 || xRotation == 0 ? 0 : xRotation - Mathf.Sin(pingPong * xAmplitude) * xRotation;
            float z = zAmplitude == 0 || zRotation == 0 ? 0 : zRotation - Mathf.Cos(pingPong * zAmplitude) * zRotation;
            Vector3 rotationVector = new Vector3(x, 0, z);
            transform.rotation = Quaternion.Euler(rotationVector);
        }
    }
}