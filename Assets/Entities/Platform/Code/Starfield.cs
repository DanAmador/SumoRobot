using System.Collections.Generic;
using UnityEngine;

namespace Platform {
    public class Starfield : MonoBehaviour {
        public int amountOfStars = 50;
        public GameObject starObject;
        private List<Star> _stars;

        public float radius = 25;

        void Start() {
            _stars = new List<Star>();

            for (int i = 0; i < amountOfStars; i++) {
                Vector3 spawnPos = Random.onUnitSphere * radius;
                GameObject st = Instantiate(starObject, spawnPos, Quaternion.identity);
                st.transform.parent = transform;
                Star star = st.GetComponent<Star>();
                star.AddStarfield(this);
                _stars.Add(star);
            }
        }

        // Update is called once per frame
        void Update() { }
    }
}