using UnityEngine;
public class QuadcopterRotor2 : MonoBehaviour
{
    public GameObject[] rotors;
    public float rotationSpeed = 1000f;
    public MoveToTargetAgent2 moveToTargetAgent;
    public Rigidbody rb;
    void Update()
    {
        // Rotate each rotor around the forward axis (z-axis) for horizontal rotation
        foreach (GameObject rotor in rotors)
        {
            rotor.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }
    private void Start()
    {
        // Ensure Rigidbody reference is set
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        float moveSpeed = 10f;
        float rotationSpeed = 200f;
        //float thrustForce = 100f;
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing!");
            return;
        }
        // Create the movement vector in local space
        Vector3 localMovement = new Vector3(moveToTargetAgent.moveX, 0f, moveToTargetAgent.moveZ) * moveSpeed;
        // Convert the local movement vector to world space based on the agent's rotation
        Vector3 worldMovement = transform.TransformDirection(localMovement);
        // Apply horizontal movement using velocity
        Vector3 velocity = rb.velocity;
        velocity.x = worldMovement.x;
        velocity.z = worldMovement.z;
        rb.velocity = velocity;
        // Apply vertical thrust
        //float verticalThrust = moveToTargetAgent.moveY * thrustForce;
        //rb.AddForce(Vector3.up * verticalThrust, ForceMode.Force);
        // Apply rotation around the Y-axis using Rigidbody
        Quaternion deltaRotation = Quaternion.Euler(0f, moveToTargetAgent.rotationY * rotationSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);
        // Penalize the agent for taking time
        moveToTargetAgent.AddReward(moveToTargetAgent.timePenalty);
        // Penalize further if the agent stays in the same spot
        if (Vector3.Distance(transform.localPosition, moveToTargetAgent.previousPosition) < 0.01f)
        {
            moveToTargetAgent.AddReward(-0.1f); // Additional penalty for not moving
        }
        // Update the previous position
        moveToTargetAgent.previousPosition = transform.localPosition;
    }
}

