using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Platform {
    public class Star : MonoBehaviour {
        private Vector3 _direction,_lookDirection;
        private float _speed;
        [SerializeField] private HashSet<Star> directNeighbors;
        void Awake() {
            directNeighbors = new HashSet<Star>();
            _speed = Random.Range(0.01f, .05f);
            _direction = Random.onUnitSphere;
            _lookDirection = Random.onUnitSphere * Random.Range(0,30);
        }

        void Update() {
            if (directNeighbors == null)
                directNeighbors = new HashSet<Star>(); //I have no fucking clue why it wasn't being initialized...
            if (transform.position.y < -2) {
                _direction *= -1;
            }

            transform.RotateAround(transform.parent.position, _direction, _speed);
            transform.LookAt(transform.parent.position + Random.onUnitSphere * Mathf.Sin(Time.deltaTime));
//            transform.Rotate(_direction, _speed * Time.deltaTime);

            foreach (Star neighbor in directNeighbors) {
                Debug.DrawLine(neighbor.transform.position, transform.position, Color.yellow);
            }
        }


        private void OnTriggerEnter(Collider other) {
            if (directNeighbors != null && other.gameObject.CompareTag("Star")) {
                Star stother = other.gameObject.GetComponent<Star>();
                if (!stother.HasDirectNeighor(this)) directNeighbors.Add(stother);
            }
        }

        private void OnTriggerExit(Collider other) {
            directNeighbors.Remove(other.gameObject.GetComponent<Star>());
        }

        public bool HasDirectNeighor(Star star) {
            return directNeighbors.Contains(star);
        }
    }
}