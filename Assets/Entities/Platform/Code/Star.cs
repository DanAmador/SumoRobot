using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Platform {
    public class Star : MonoBehaviour {
        private Vector3 _direction, _lookDirection;
        private float _speed;
        public List<LineRenderer> lines;
        public List<Star> _directNeighbors;
        public LineRenderer lineRendererPrefab;

        void Awake() {
            _speed = Random.Range(0.01f, .05f);
            _direction = Random.onUnitSphere;
            _lookDirection = Random.onUnitSphere * Random.Range(0,30);
        }

        void Update() {
//            if (transform.position.y < 20) {
//                _direction *= -1;
//            }
//            transform.LookAt(transform.parent.position + Random.onUnitSphere );

            transform.RotateAround(transform.parent.position, _direction, _speed);
//            transform.Rotate(_direction, _speed * Time.deltaTime);


            for (int i = 0; i < Mathf.Min(lines.Count, _directNeighbors.Count); i++) {
                LineRenderer line = lines[i];
                Star s = _directNeighbors[i];

                line.SetPosition(0, Vector3.zero);
                line.SetPosition(1, s.transform.position - transform.position);
            }
//            foreach (Star neighbor in directNeighbors) {

//                Debug.DrawLine(neighbor.transform.position, transform.position, Color.yellow);
        }


        private void OnTriggerEnter(Collider other) {
            if (_directNeighbors != null && other.gameObject.CompareTag("Star")) {
                Star stother = other.gameObject.GetComponent<Star>();
                if (!stother.HasDirectNeighor(this)) {
                    var lineRenderer = Instantiate(lineRendererPrefab, transform);
                    lineRenderer.transform.SetParent(transform);
                    lines.Add(lineRenderer);

                    _directNeighbors.Add(stother);
                }
            }
        }

        private void OnTriggerExit(Collider other) {
            if (lines.Count != 0 ) {
                var last = lines.Last();
                lines.Remove(last);
                Destroy(last);    
                _directNeighbors.Remove(other.gameObject.GetComponent<Star>());

            }
            
        }

        private bool HasDirectNeighor(Star star) {
            return _directNeighbors.Contains(star);
        }
    }
}