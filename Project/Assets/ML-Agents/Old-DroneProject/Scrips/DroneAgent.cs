using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToTargetAgent : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private MeshRenderer floorMeshRenderer;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private Material floorNormal;
    [SerializeField] private Material warningMaterial;
    [SerializeField] private Material progressMaterial;
    [SerializeField] public float timePenalty = -0.01f;

    [SerializeField] private float rayLength = 10f;
    [SerializeField] private LayerMask perceptionLayer;

    public QuadcopterRotor quadCopterRotor;
    public Vector3 previousPosition;
    public float moveX = 0;
    public float moveY = 0;
    public float moveZ = 0;
    public float rotationY = 0;
    private bool isWinOrLoss = false;

    public Vector3 spawnAreaSize = new Vector3(50f, 1, 50f);
    public Vector3 targetAreaSize = new Vector3(50f, 1, 50f);

    // New variables for containment tracking
    private float containmentTimer = 0f;
    private const float CONTAINMENT_TIME_GOAL = 2.5f;
    private const float CONTAINMENT_REWARD_INTERVAL = 0.5f;
    private float lastContainmentRewardTime = 0f;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(
            Random.Range(-spawnAreaSize.x, spawnAreaSize.x),
            Random.Range(1, spawnAreaSize.y),
            Random.Range(-spawnAreaSize.z, spawnAreaSize.z)
        );
        transform.localRotation = Quaternion.identity;

        targetTransform.localPosition = new Vector3(
            Random.Range(-targetAreaSize.x / 2, targetAreaSize.x / 2),
            -1f,
            Random.Range(-targetAreaSize.z / 2, targetAreaSize.z / 2)
        );

        previousPosition = transform.localPosition;
        quadCopterRotor.rb.velocity = Vector3.zero;

        // Only reset material if not already in win or lose state
        if (!isWinOrLoss)
        {
            floorMeshRenderer.material = floorNormal;
        }
        isWinOrLoss = false;

        // Reset containment tracking
        containmentTimer = 0f;
        lastContainmentRewardTime = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(transform.localRotation);

        // Ray Perception
        Vector3[] rayDirections = { transform.forward, transform.right, -transform.right, transform.up };
        foreach (var direction in rayDirections)
        {
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, rayLength, perceptionLayer))
            {
                sensor.AddObservation(hit.distance / rayLength);
            }
            else
            {
                sensor.AddObservation(1f);
            }
        }

        // Add observations related to containment
        sensor.AddObservation(containmentTimer / CONTAINMENT_TIME_GOAL);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        moveY = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);
        rotationY = Mathf.Clamp(actions.ContinuousActions[3], -1f, 1f);

        AddReward(timePenalty);

        // Check if the drone is inside the target area
        CheckContainment();
    }

    private void CheckContainment()
    {
        Collider targetCollider = targetTransform.GetComponent<Collider>();

        if (targetCollider != null && targetCollider.bounds.Contains(transform.position))
        {
            containmentTimer += Time.deltaTime;

            // Always set to progress material when in target area
            if (floorMeshRenderer.material != winMaterial &&
                floorMeshRenderer.material != loseMaterial)
            {
                floorMeshRenderer.material = progressMaterial;
            }

            // Provide incremental rewards for staying in the area
            if (Time.time - lastContainmentRewardTime >= CONTAINMENT_REWARD_INTERVAL)
            {
                AddReward(50000); // Reward for staying in the area
                lastContainmentRewardTime = Time.time;
            }

            // Check if containment goal is reached
            if (containmentTimer >= CONTAINMENT_TIME_GOAL)
            {
                AddReward(800000); // Large reward for successful containment
                floorMeshRenderer.material = winMaterial;
                isWinOrLoss = true;
                EndEpisode();
            }
        }
        else
        {
            // Only reset to normal if not in win/lose state and currently in progress or warning material
            if (!isWinOrLoss &&
                (floorMeshRenderer.material == progressMaterial ||
                 floorMeshRenderer.material == warningMaterial))
            {
                floorMeshRenderer.material = floorNormal;
            }

            // Reset timer if outside the target area
            containmentTimer = 0f;
            AddReward(-1000f); // Penalty for leaving the area
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
        continuousActions[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<BrickWall>(out BrickWall wall))
        {
            SetReward(-100000f);
            floorMeshRenderer.material = loseMaterial;
            isWinOrLoss = true;
            EndEpisode();
        }
        else if (other.TryGetComponent<Person>(out Person person))
        {
            SetReward(-8000f);

            // Always set to warning material when entering person's area
            if (floorMeshRenderer.material != winMaterial &&
                floorMeshRenderer.material != loseMaterial)
            {
                floorMeshRenderer.material = warningMaterial;
            }
        }
    }

    public void HitCeiling()
    {
        AddReward(-15000);
        floorMeshRenderer.material = loseMaterial;
        isWinOrLoss = true;
        EndEpisode();
    }
}
