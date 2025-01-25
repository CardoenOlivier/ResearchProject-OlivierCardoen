using UnityEngine;
using UnityEngine.InputSystem;

public class QuadcopterRotor3 : MonoBehaviour
{
    public GameObject[] rotors;
    public float maxRotationSpeed = 2000f;
    public MoveToTargetAgent3 moveToTargetAgent;
    public Rigidbody rb;
    public float thrustForce = 9.81f;
    public float hoverForce;
    public float ceilingHeight = 20f;
    public float minHeight = 1f;
    public float heightReward = 100f;
    private float previousDistanceToTarget;
    private Vector3 spawnPosition;
    [SerializeField] private Transform targetTransform;

    private float[] rotorSpeeds;

    void Start()
    {
        //get rigidbody component
        rb = GetComponent<Rigidbody>();
        //calculate hover force
        hoverForce = (rb.mass * Physics.gravity.magnitude) / rotors.Length;
        //initialize tracking variables
        previousDistanceToTarget = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        spawnPosition = transform.localPosition;
        rotorSpeeds = new float[rotors.Length];
    }

    void Update()
    {
        //rotate rotors
        for (int i = 0; i < rotors.Length; i++)
        {
            rotors[i].transform.Rotate(Vector3.forward, rotorSpeeds[i] * Time.deltaTime);
        }

        //reward for being at the right height
        float heightDifference = Mathf.Abs(transform.position.y - 10f);
        if (heightDifference < 1f)
        {
            moveToTargetAgent.AddReward(5f * (1f - heightDifference));
        }
        
        else
        {
            //penalize for being too high or too low
            moveToTargetAgent.AddReward(-heightDifference * 0.1f);
        }

        if (transform.position.y > ceilingHeight)
        {
            moveToTargetAgent.HitCeiling();
        }
        else if (transform.position.y < minHeight)
        {
            moveToTargetAgent.AddReward(-1000f);
        }
        //reward for not staying close to spawn
        float distanceFromSpawn = Vector3.Distance(
            new Vector3(transform.localPosition.x, 0, transform.localPosition.z),
            new Vector3(spawnPosition.x, 0, spawnPosition.z)
        );
        moveToTargetAgent.AddReward(distanceFromSpawn * 0.01f);
    }

    private void FixedUpdate()
    {
        float moveSpeed = 3f;
        float rotationSpeed = 200f;

        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing!");
            return;
        }

        float[] rotorForces = CalculateRotorForces();

        for (int i = 0; i < rotors.Length; i++)
        {
            ApplyForceToRotor(rotors[i], rotorForces[i], i);
        }

        // move the agent
        Vector3 localMovement = new Vector3(moveToTargetAgent.moveX, 0f, moveToTargetAgent.moveZ) * moveSpeed;
        Vector3 worldMovement = transform.TransformDirection(localMovement);
        Vector3 velocity = rb.velocity;
        velocity.x = worldMovement.x;
        velocity.z = worldMovement.z;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);

        Quaternion deltaRotation = Quaternion.Euler(0f, moveToTargetAgent.rotationY * rotationSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);

        CalculateRewards();
    }
    //calculate forces for each rotor
    private float[] CalculateRotorForces()
    {
        float[] forces = new float[rotors.Length];
        float baseHoverForce = hoverForce;
        float verticalInput = moveToTargetAgent.moveY;

        for (int i = 0; i < forces.Length; i++)
        {
            forces[i] = baseHoverForce + (verticalInput * thrustForce / rotors.Length);
        }

        return forces;
    }

    private void ApplyForceToRotor(GameObject rotor, float force, int index)
    {
        rb.AddForceAtPosition(Vector3.up * force, rotor.transform.position, ForceMode.Force);
        rotorSpeeds[index] = Mathf.Clamp(force / hoverForce, 0.1f, 1f) * maxRotationSpeed;
        Debug.Log($"Rotor {rotor.name}: Applied Force = {force}, Visual Rotar Speed = {rotorSpeeds[index]}");
    }

    private void CalculateRewards()
    {
        Vector2 currentXZPosition = new Vector2(transform.localPosition.x, transform.localPosition.z);
        Vector2 previousXZPosition = new Vector2(moveToTargetAgent.previousPosition.x, moveToTargetAgent.previousPosition.z);

        //penalize movement if not making progress
        if (Vector2.Distance(currentXZPosition, previousXZPosition) < 0.01f)
        {
            moveToTargetAgent.AddReward(-100f);
        }

        //calculate distance to target
        float distanceToTarget = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        float distanceDelta = previousDistanceToTarget - distanceToTarget;

        //reward for getting closer to the target
        if (distanceDelta > 0f && Vector2.Distance(currentXZPosition, previousXZPosition) > 0.01f)
        {
            moveToTargetAgent.AddReward(distanceDelta * 100f);
        }

        //mew height reward
        float heightError = Mathf.Abs(transform.localPosition.y - 10f);
        float heightReward = Mathf.Clamp(1f - (heightError / 5f), 0f, 1f);
        moveToTargetAgent.AddReward(heightReward * 50f);

        //update tracking variables
        moveToTargetAgent.previousPosition = transform.localPosition;
        previousDistanceToTarget = distanceToTarget;
    }
}
