using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class AgentJump : Agent
{
    Rigidbody rb;
    public float jumpForce = 4f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    bool isGrounded; 
    public Transform obstacleTransform;
    public ObstacleMovement obstacleScript;
    public Transform agentSpawnPoint;
    public Transform obstacleSpawnPoint;

    private bool obstacleClearedThisSpecificObstacle;

    public float jumpCooldown = 1f;
    private float lastJumpTime = -10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (obstacleTransform == null || obstacleScript == null || agentSpawnPoint == null || obstacleSpawnPoint == null)
        {
            Debug.LogError("Een of meerdere belangrijke objecten zijn niet toegewezen in de Inspector voor AgentJump!");
        }
        lastJumpTime = Time.time - jumpCooldown;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = agentSpawnPoint.position;
        transform.rotation = agentSpawnPoint.rotation;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (obstacleScript != null)
        {
            obstacleScript.InitializeObstacle(obstacleSpawnPoint.position);
        }
        
        obstacleClearedThisSpecificObstacle = false;
        lastJumpTime = Time.time - jumpCooldown;
        // Debug.Log("Nieuwe EPISODE gestart. Agent en obstakel gereset.");
    }

    void FixedUpdate()
    {
        // Waarde toekennen aan de nu correct gedeclareerde isGrounded variabele
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (obstacleTransform == null || !obstacleTransform.gameObject.activeInHierarchy || obstacleScript == null)
        {
            return;
        }

        if (obstacleClearedThisSpecificObstacle) { // Als dit obstakel al is afgehandeld (door succes of botsing die tot EndEpisode leidde)
             // En we zijn hier nog steeds, dan is OnEpisodeBegin nog niet aangeroepen, wat vreemd is.
             // Dit zou echter niet mogen gebeuren als EndEpisode() correct werkt.
            return;
        }

        float agentX = transform.position.x;
        float agentY = transform.position.y;
        float obstacleX = obstacleTransform.position.x;
        float obstacleY = obstacleTransform.position.y;

        bool obstacleIsJustPassed = obstacleX < (agentX - 0.5f) && obstacleX > (agentX - 2.5f);
        bool agentIsSufficientlyHigh = agentY > (obstacleY + 0.5f);

        // Geen check meer voor !obstacleClearedThisSpecificObstacle hier, want als het true was, hadden we hierboven een return.
        if (obstacleIsJustPassed && agentIsSufficientlyHigh)
        {
            SetReward(1.0f);
            obstacleClearedThisSpecificObstacle = true; 
            Debug.Log("Obstakel succesvol ontweken! +1 Reward. EPISODE EINDIGT.");
            EndEpisode();
            return;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.y);
        sensor.AddObservation(rb.linearVelocity.y);
        sensor.AddObservation(isGrounded); // Nu correct

        if (obstacleTransform != null && obstacleTransform.gameObject.activeInHierarchy)
        {
            Vector3 dirToObstacle = obstacleTransform.position - transform.position;
            sensor.AddObservation(dirToObstacle.x);
            sensor.AddObservation(dirToObstacle.z);
            sensor.AddObservation(obstacleScript != null ? obstacleScript.currentSpeed : 0f);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int jumpAction = actions.DiscreteActions[0];

        if (jumpAction == 1 && isGrounded && (Time.time >= lastJumpTime + jumpCooldown)) // Nu correct
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.Space))
        {
            discreteActions[0] = 1;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ObstacleTag"))
        {
            if (!obstacleClearedThisSpecificObstacle)
            {
                SetReward(-1.0f);
                Debug.Log("Botsing met obstakel! -1 Reward. EPISODE EINDIGT.");
            }
            EndEpisode();
        }
    }
}