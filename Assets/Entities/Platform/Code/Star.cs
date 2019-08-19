using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Platform {
    public class Star : MonoBehaviour {
        private Vector3 _direction;
        private Starfield _starfield;
        public int? ConstellationNum { get; private set; }
        private float _speed;

        void Start() {
            _speed = Random.Range(0.01f, .05f);
            _direction = Random.onUnitSphere;
        }

        void Update() {
            transform.RotateAround(transform.parent.position, _direction, _speed);
//            transform.Rotate(_direction, _speed * Time.deltaTime);
        }


        private void OnTriggerEnter(Collider other) {
            if (_starfield != null) {
                Star stother = other.gameObject.GetComponent<Star>();
                if (stother.ConstellationNum != null) {
                    ConstellationNum = stother.ConstellationNum;
                }
            }
        }

        private void OnTriggerExit(Collider other) {
            ConstellationNum = null;
        }

        public void AddStarfield(Starfield starfield) {
            _starfield = starfield;
        }
    }
}