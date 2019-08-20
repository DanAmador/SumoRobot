using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class ObserverationDebug : MonoBehaviour {
    public Transform end;
    private RayPerception3D _rayPerception;
    private readonly float[] _rayAngles = {0f};//, 45f, 70f, 90f, 135f, 180f, 110f, 270};
    private readonly string[] _observables = { "Player"};
    public float rayDistance = 100;
    public List<float> views;
    void Start() {
        _rayPerception = GetComponent<RayPerception3D>();

    }

    // Update is called once per frame
    void Update() {

        views = _rayPerception.Perceive(rayDistance, _rayAngles, _observables, 0f, 0f);

    }
}