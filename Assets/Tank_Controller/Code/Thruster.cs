using UnityEngine;

namespace Tank_Controller {
    public class Thruster : MonoBehaviour {
        public float strength, distance;

        public Transform[] thrusters;
        private Rigidbody rb;

        public void Start() {
            rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate() {
            foreach (Transform thruster in thrusters) {
                RaycastHit hit;
                float distancePercentage;
                Vector3 downwardForce;
                Debug.DrawRay(thruster.position, transform.up * -1 , Color.yellow);
                if (Physics.Raycast(thruster.position, transform.up * -1, out hit, distance)) {
                    distancePercentage = 1 - (hit.distance / distance);
                    downwardForce = strength * distancePercentage * transform.up;
                    downwardForce = Time.deltaTime * rb.mass * downwardForce;
                    rb.AddForceAtPosition(downwardForce, thruster.position);
                }
            }
        }
    }
}

