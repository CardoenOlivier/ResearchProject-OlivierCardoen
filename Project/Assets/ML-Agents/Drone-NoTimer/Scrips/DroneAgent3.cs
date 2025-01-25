using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToTargetAgent3 : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;
    [SerializeField] private Material floorNormal;
    [SerializeField] public float timePenalty = -0.01f;
    [SerializeField] private float rayLength = 10f;
    [SerializeField] private LayerMask perceptionLayer;
    [SerializeField] private BrickWall3[] walls;

    public QuadcopterRotor3 quadCopterRotor;
    public Vector3 previousPosition;
    public float moveX = 0;
    public float moveY = 0;
    public float moveZ = 0;
    public float rotationY = 0;
    private bool isWinOrLoss = false;

    //size of plane
    public Vector3 spawnAreaSize = new Vector3(4, 1, 4);
    public Vector3 targetAreaSize = new Vector3(4, 1, 4);

    public override void OnEpisodeBegin()
    {
        float halfPlaneWidth = spawnAreaSize.x / 2f;
        float halfPlaneDepth = spawnAreaSize.z / 2f;

        //randomize wall positions and rotations
        foreach (BrickWall3 wall in walls)
        {
            //fixed Y position at -8.7, allowing some penetration into ground
            Vector3 wallPosition = new Vector3(
                Random.Range(-halfPlaneWidth * 0.5f, halfPlaneWidth * 0.5f),
                -8.7f,
                Random.Range(-halfPlaneDepth * 0.5f, halfPlaneDepth * 0.5f)
            );
            wall.transform.localPosition = wallPosition;

            //random Y rotation
            wall.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        }

        //spawn agent with checks to avoid walls
        Vector3 agentPosition;
        do
        {
            agentPosition = new Vector3(
                Random.Range(-halfPlaneWidth * 0.5f, halfPlaneWidth * 0.5f),
                Random.Range(1f, spawnAreaSize.y),
                Random.Range(-halfPlaneDepth * 0.5f, halfPlaneDepth * 0.5f)
            );
        } while (IsPositionInWalls(agentPosition));
        transform.localPosition = agentPosition;

        //spawn target with checks to avoid walls
        Vector3 targetPosition;
        do
        {
            targetPosition = new Vector3(
                Random.Range(-halfPlaneWidth * 0.5f, halfPlaneWidth * 0.5f),
                -1f,
                Random.Range(-halfPlaneDepth * 0.5f, halfPlaneDepth * 0.5f)
            );
        } while (IsPositionInWalls(targetPosition));
        targetTransform.localPosition = targetPosition;

        //reset agent
        transform.localRotation = Quaternion.identity;
        previousPosition = transform.localPosition;
        quadCopterRotor.rb.velocity = Vector3.zero;

        if (!isWinOrLoss)
        {
            floorMeshRenderer.material = floorNormal;
        }
        isWinOrLoss = false;
    }

    private bool IsPositionInWalls(Vector3 position)
    {
        foreach (BrickWall3 wall in walls)
        {
            Collider wallCollider = wall.GetComponent<Collider>();
            if (wallCollider.bounds.Contains(position))
            {
                return true;
            }
        }
        return false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position and rotation, target position
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
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        moveY = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);
        rotationY = Mathf.Clamp(actions.ContinuousActions[3], -1f, 1f);

        AddReward(timePenalty);
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
        if (other.TryGetComponent<TargetGoal3>(out TargetGoal3 goal))
        {
            AddReward(10000f);
            floorMeshRenderer.material = winMaterial;
            isWinOrLoss = true;
            EndEpisode();
        }
        if (other.TryGetComponent<BrickWall3>(out BrickWall3 wall))
        {
            SetReward(-10000f);
            floorMeshRenderer.material = loseMaterial;
            isWinOrLoss = true;
            EndEpisode();
        }
    }

    public void HitCeiling()
    {
        AddReward(-10000);
        floorMeshRenderer.material = loseMaterial;
        isWinOrLoss = true;
        EndEpisode();
    }
}
