using System;
using MLAgents;

namespace Tank.AI {
    public class TankAgent : Agent {
        private TankController _tank;
        private void Start() {
            _tank = GetComponent<TankController>();
        }
    }
}