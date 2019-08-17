using System;
using UnityEngine;

namespace Tank {
    public class Thruster : MonoBehaviour {
        Rigidbody rb;
        public float strength, distance;
        [SerializeField] private ParticleSystem.EmissionModule em;
        [SerializeField] private ParticleSystem ps;
        private bool active;

        private void Start() {
            active = true;
        }

        public void Thrust() {
            if (!active) return;
            RaycastHit hit;
            float distancePercentage;
            Vector3 downwardForce;
            Debug.DrawRay(transform.position, transform.up * -distance, Color.yellow);

            if (!Physics.Raycast(transform.position, transform.up * -1, out hit, distance)) {
                if (ps.isPlaying) ps.Stop();
                return;
            }

            if (!hit.collider.gameObject.CompareTag("Platform")) return;
        
            if (!ps.isPlaying) ps.Play();
            distancePercentage = 1 - (hit.distance / distance);
            downwardForce = strength * distancePercentage * transform.up;
            downwardForce = Time.deltaTime * rb.mass * downwardForce;
            rb.AddForceAtPosition(downwardForce, transform.position);
        }


        public void OnOff() {
            active = !active;
        }
        public void InitValues(float strength, float distance, Rigidbody rb, ParticleSystem ps) {
            if (this.rb) return;

            this.distance = distance;
            this.strength = strength;
            this.rb = rb;


            this.ps = Instantiate(ps, transform);
            em = this.ps.emission;
            em.enabled = true;
        }
    }
}