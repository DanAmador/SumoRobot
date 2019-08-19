using System;
using UnityEngine;

namespace Tank {
    public class Thruster : MonoBehaviour {
        Rigidbody rb;
        public float strength, distance;
        [SerializeField] private ParticleSystem.EmissionModule em;
        [SerializeField] private ParticleSystem ps;
        private int _layerMask;
        private bool _active;

        private void Start() {
            _active = true;
            _layerMask = LayerMask.GetMask("Player", "Platform");
        }

        public void Thrust() {
            if (!_active) return;
            RaycastHit hit;
            Debug.DrawRay(transform.position, transform.up * -distance, Color.yellow);

            if (!Physics.Raycast(transform.position, transform.up * -1, out hit, distance, _layerMask)) {
                if (ps.isPlaying) ps.Stop();
                return;
            }

            if (!hit.collider.gameObject.CompareTag("Platform")) return;

            if (!ps.isPlaying) ps.Play();
            float distancePercentage = 1 - (hit.distance / distance);
            Vector3 downwardForce = strength * distancePercentage * transform.up;
            downwardForce = Time.deltaTime * rb.mass * downwardForce;
            rb.AddForceAtPosition(downwardForce, transform.position);
        }


        public void OnOff() {
            _active = !_active;
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