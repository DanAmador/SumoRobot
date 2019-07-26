using UnityEngine;

public class Thruster : MonoBehaviour {
	[SerializeField] Rigidbody rb;
	public float strength, distance;
	
	public void Thrust() {
		RaycastHit hit;
		float distancePercentage;
		Vector3 downwardForce;
		Debug.DrawRay(transform.position, transform.up * -1, Color.yellow);
		
		if (!Physics.Raycast(transform.position, transform.up * -1, out hit, distance)) return;
		
		distancePercentage = 1 - (hit.distance / distance);
		downwardForce = strength * distancePercentage * transform.up;
		downwardForce = Time.deltaTime * rb.mass * downwardForce;
		rb.AddForceAtPosition(downwardForce, transform.position);
	}

	public void InitValues(float strength, float distance,Rigidbody rb1) {
		if (rb) return;
		this.distance = distance;
		this.strength = strength;
		rb = rb1;
	}
}