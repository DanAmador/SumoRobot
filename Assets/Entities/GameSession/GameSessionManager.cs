using System;
using System.Linq;
using Platform;
using Tank;
using TMPro;
using UnityEngine;

namespace GameSession {
    public class GameSessionManager : MonoBehaviour {
        [SerializeField] private TankController[] tanks;
        public TextMeshProUGUI txt;
        private PlatformMover _platform;
        private float _matchStart;
        public float roundDuration = 120; // in seconds 

        public float MatchPercentageRemaining => 1 - (Time.time - _matchStart) / roundDuration;

        private String SecondsRemaining =>
            (Mathf.Clamp(roundDuration - (Time.time - _matchStart), 0, roundDuration)).ToString("0.00");

        private bool _colorUpdated = false;


        public void Awake() {
            _platform = GetComponentInChildren<PlatformMover>();
            _matchStart = Time.time;
        }

        public void Update() {
            txt.text = SecondsRemaining;
            if (MatchPercentageRemaining < .3f && !_colorUpdated) {
                txt.color = new Color(1f, 0.49f, 0.73f);
                _colorUpdated = true;
            }
        }

        public TankController GetEnemy(TankController player) {
            return tanks.FirstOrDefault(tank => !tank.Equals(player));
        }

        public void Reset() {
            foreach (TankController tank in tanks) {
                tank.Reset();
            }

            _matchStart = Time.time;
            _platform.Reset();
            _colorUpdated = false;
            txt.color = Color.white;
        }
    }
}