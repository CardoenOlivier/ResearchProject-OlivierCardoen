using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class MoveToTargetAgent2 : Agent
{
    // Observation
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;
    [SerializeField] private Material floorNormal;
    // Movement penalty factor
    [SerializeField] public float timePenalty = -0.001f; // Small penalty for every step taken
    public Vector3 previousPosition;
    public float moveX = 0;
    public float moveY = 0;
    public float moveZ = 0;
    public float rotationY = 0;
    private bool isWinOrLoss = false;
    // Reset position after episode
    // Reset position after episode
    public override void OnEpisodeBegin()
    {
        // Reset agent's position and rotation
        transform.localPosition = new Vector3(5.42f, -0.645f, 3f);
        transform.localRotation = Quaternion.identity;
        previousPosition = transform.localPosition;

        // Randomize target position
        //targetTransform.localPosition = new Vector3(
        //    Random.Range(-6.5f, 6.5f), // Avoid walls on X-axis
        //    0,                          // Y remains fixed
        //    Random.Range(-5.6f, 6.5f) // Avoid walls on Z-axis
        //);
        targetTransform.localPosition = new Vector3(
            Random.Range(-20f, 20f), // Avoid walls on X-axis
            0,                          // Y remains fixed
            Random.Range(-20f, 20f) // Avoid walls on Z-axis
        );

        // Reset floor material if no win/loss occurred
        if (!isWinOrLoss)
        {
            floorMeshRenderer.material = floorNormal;
        }
        isWinOrLoss = false;
    }


    // Inputs for the AI, what does it need to solve the problem?
    public override void CollectObservations(VectorSensor sensor)
    {
        // Where am I?
        sensor.AddObservation(transform.localPosition);
        // Where is the target?
        //sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(transform.localRotation);
    }
    // Actions
    // What actions does the AI take?
    public override void OnActionReceived(ActionBuffers actions)
    {
        moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        //moveY = Mathf.Clamp(actions.ContinuousActions[2], 0f, 1f);
        rotationY = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<TargetGoal>(out TargetGoal goal))
        {
            SetReward(+10000f);
            floorMeshRenderer.material = winMaterial;
            isWinOrLoss = true;
            EndEpisode();
        }
        if (other.TryGetComponent<BrickWall>(out BrickWall wall))
        {
            SetReward(-100000f);
            floorMeshRenderer.material = loseMaterial;
            isWinOrLoss = true;
            EndEpisode();
        }
    }
}
