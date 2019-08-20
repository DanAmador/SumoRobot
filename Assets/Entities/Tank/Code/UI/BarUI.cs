using UnityEngine;
using UnityEngine.UI;

namespace Tank.UI {
    public class BarUI : MonoBehaviour {
        private TankController _tank;
        public Image speed, special;

        void Start() {
            _tank = GetComponentInParent<TankController>();

        }

        void Update() {
            special.fillAmount = _tank.GetNormalizedSpecial();
            speed.fillAmount = Mathf.Abs(_tank.GetNormalizedSpeed());
        }
    }
}