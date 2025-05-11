using UnityEngine;

public class ObstacleMovement : MonoBehaviour // Zorg ervoor dat dit MonoBehaviour is
{
    public float minSpeed = 2f;
    public float maxSpeed = 8f;
    [HideInInspector] // currentSpeed wordt nu intern beheerd en extern gezet
    public float currentSpeed;

    // Deze wordt gezet door AgentJump.OnEpisodeBegin
    [HideInInspector]
    public Vector3 initialSpawnPosition;

    // Definieer een Z-waarde (wereldcoördinaten) waarachter het obstakel moet resetten
    // Pas deze waarde aan op basis van de grootte en positie van je plane
    public float despawnXBoundary = -10f;

    // Wordt aangeroepen door AgentJump.OnEpisodeBegin om het obstakel te positioneren en snelheid te geven
    public void InitializeObstacle(Vector3 spawnPos)
    {
        initialSpawnPosition = spawnPos;
        transform.position = initialSpawnPosition; // Gebruik wereldpositie voor consistentie
        SetRandomSpeed();
        // Debug.Log($"Obstacle Initialized at {transform.position} with speed {currentSpeed}");
    }

    void FixedUpdate()
    {
        // Beweeg het obstakel naar achteren (negatieve Z-as in wereldcoördinaten)
        transform.Translate(Vector3.left * currentSpeed * Time.fixedDeltaTime, Space.World);

        // Controleer of het obstakel de despawn grens heeft bereikt
        if (transform.position.x < despawnXBoundary)
        {
            // Debug.Log($"Obstacle at {transform.position.z} reached despawn boundary {despawnZBoundary}. Resetting.");
            // Reset het obstakel naar zijn startpositie.
            // In een scenario met één obstakel per episode, zou de episode meestal al geëindigd zijn
            // (agent gesprongen of geraakt) voordat dit punt bereikt wordt als de agent interactie heeft.
            // Dit is een vangnet of voor scenario's waar de episode langer duurt.
            ResetToSpawnAndGetNewSpeed();
        }
    }

    void SetRandomSpeed()
    {
        currentSpeed = Random.Range(minSpeed, maxSpeed);
    }

    // Reset het obstakel naar zijn initiële spawn positie en geef het een nieuwe willekeurige snelheid.
    public void ResetToSpawnAndGetNewSpeed()
    {
        transform.position = initialSpawnPosition; // Gebruik wereldpositie
        SetRandomSpeed(); // Geef het een nieuwe willekeurige snelheid
        // Debug.Log($"Obstacle Reset to {transform.position} with new speed {currentSpeed}");
    }
}