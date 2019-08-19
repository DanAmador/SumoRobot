using System.Collections.Generic;
using UnityEngine;

namespace Platform {
    public class Starfield : MonoBehaviour {
        public int amountOfStars = 50;
        public GameObject starObject;
        private List<Star> _stars;
        public float radius = 25;
        private int _counter;

        void Start() {
            _stars = new List<Star>();

            for (int i = 0; i < amountOfStars; i++) {
                Vector3 spawnPos = Random.onUnitSphere * radius;
//                spawnPos.y = Mathf.Abs(spawnPos.y);
                GameObject st = Instantiate(starObject, spawnPos, Quaternion.identity);
                st.transform.parent = transform;
                Star star = st.GetComponent<Star>();
                _stars.Add(star);
            }
        }
    }
}