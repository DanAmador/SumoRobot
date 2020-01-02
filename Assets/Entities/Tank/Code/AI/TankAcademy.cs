using MLAgents;
using UnityEngine;

namespace Tank.AI {
    public class TankAcademy : Academy {
        
        public override void InitializeAcademy() {
//            Monitor.SetActive(true);
            
            // We increase the Physics solver iterations in order to
            // make thruster calculations more accurate.
            Physics.defaultSolverIterations = 12;
            Physics.defaultSolverVelocityIterations = 12;
            Time.fixedDeltaTime = 0.01333f; //(75fps). default is .2 (60fps)
            Time.maximumDeltaTime = .15f; // Default is .33
        }
    }
}