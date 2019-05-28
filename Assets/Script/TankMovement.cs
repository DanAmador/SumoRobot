using System.Collections;
using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public int playerNum = 1;
    private float maxSpeed = 8f;                 
    public float turnSpeed = .1f;
    [SerializeField]
    private float currSpeed = 0f;
    [SerializeField]
    private float specialCounter = 0;
    

    private string movementAxisName, turnAxisName;
    private Rigidbody rb;
    private float movementInputValue, turnInputValue;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    private void OnEnable()
    {

        rb.isKinematic = false;


        movementInputValue = 0f;
        turnInputValue = 0f;
    }


    private void OnDisable()
    {

        rb.isKinematic = true;
    }


    private void Start()
    {
        movementAxisName = $"Vertical{playerNum}";
        turnAxisName = $"Horizontal{playerNum}";
        
    }


    private void Update()
    {
        movementInputValue = Input.GetAxis(movementAxisName);
        turnInputValue = Input.GetAxis(turnAxisName);

        currSpeed += (movementInputValue * 3) * Time.deltaTime  - ((1- Mathf.Abs(movementInputValue)) * Time.deltaTime * 10);
        currSpeed = Mathf.Clamp(currSpeed, 3, maxSpeed);


        specialCounter = Mathf.Clamp(specialCounter + Time.deltaTime, 0, 5);
        if (Input.GetButton("Turbo"))
        {
            StartCoroutine(turboBoost(specialCounter));
        }
    }


    private IEnumerator turboBoost(float currentSpecial)
    {
        currSpeed = currentSpecial * 6;
        yield return new WaitForSeconds(.2f);
        specialCounter = 0;

        currSpeed = maxSpeed;
    }


    private void FixedUpdate()
    {
        Move();
        Turn();
    }


    private void Move()
    {
        Vector3 movement = transform.forward * movementInputValue * currSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);
    }


    private void Turn() {
        float turn = turnInputValue * turnSpeed * Time.deltaTime;

        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        rb.MoveRotation(rb.rotation * turnRotation);
    }
}