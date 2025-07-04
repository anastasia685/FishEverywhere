using UnityEngine;

public class FollowTargetSpawner : MonoBehaviour
{
    [SerializeField] GameObject obstaclePrefab1, obstaclePrefab2, obstaclePrefab3;
    GameObject obstacle;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject targetPrefab;
    [SerializeField] Transform progressBarFill;

    [SerializeField] Vector2 mapSize = new Vector2(12, 12); // Total area size
    [SerializeField] float padding = 1f; // Space between obstacles and borders
    [SerializeField] Vector2 obstacleSizeRange = new Vector2(1f, 2f); // (min, max)
    [SerializeField] Vector2 targetSpawnRange = new Vector2(3f, 5f); // (min, max)

    [SerializeField] GameObject Leaves;

    Vector3 playerSpawnPosition;

    void Start()
    {
        SetBackground();
        GenerateObstacles();
        SpawnPlayer();
        SpawnTarget();
    }

    void GenerateObstacles()
    {
        // Step 1: Define quadrant size
        float halfX = mapSize.x / 2;
        float halfZ = mapSize.y / 2;

        // Step 2: Define quadrant centers (offset by padding)
        Vector2[] quadrantCenters = new Vector2[]
        {
            new Vector2(-halfX / 2, halfZ / 2),  // Top Left
            new Vector2(halfX / 2, halfZ / 2),   // Top Right
            new Vector2(-halfX / 2, -halfZ / 2), // Bottom Left
            new Vector2(halfX / 2, -halfZ / 2)   // Bottom Right
        };

        foreach (Vector2 center in quadrantCenters)
        {
            //float width = Random.Range(obstacleSizeRange.x, obstacleSizeRange.y);
            //float height = Random.Range(obstacleSizeRange.x, width / 2);
            float scale = Random.Range(obstacleSizeRange.x, obstacleSizeRange.y);


            int rand = Random.Range(1, 4);
            if (rand == 1) { obstacle = obstaclePrefab1; }
            else if (rand == 2) { obstacle = obstaclePrefab2; }
            else { obstacle = obstaclePrefab3; }

            /*/
            if (Random.value > 0.5f)
            {
                // swap x & y
                float tmp = width;
                width = height;
                height = tmp;
            }
            /*/

            
            // Compute valid spawn range (ensuring full obstacle fits)
            float xMin = center.x - (halfX / 2) + padding + (scale / 2);
            float xMax = center.x + (halfX / 2) - padding - (scale / 2);
            float zMin = center.y - (halfZ / 2) + padding + (scale / 2);
            float zMax = center.y + (halfZ / 2) - padding - (scale / 2);
            

            // Generate a valid random position
            float xPos = Random.Range(xMin, xMax);
            float zPos = Random.Range(zMin, zMax);

            // Instantiate obstacle
            GameObject newObj = Instantiate(obstacle, new Vector3(xPos, 0.5f, zPos), Quaternion.Euler(0, Random.Range(0,360),0), transform);
            newObj.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    void SpawnPlayer()
    {
        float halfX = mapSize.x / 2;
        float halfZ = mapSize.y / 2;

        // Randomly choose an edge (0: Left, 1: Right, 2: Top, 3: Bottom)
        int edge = Random.Range(0, 4);

        float xPos = 0, zPos = 0;

        switch (edge)
        {
            case 0: // Left edge
                xPos = -halfX + padding;
                zPos = Random.Range(-halfZ + padding, halfZ - padding);
                break;
            case 1: // Right edge
                xPos = halfX - padding;
                zPos = Random.Range(-halfZ + padding, halfZ - padding);
                break;
            case 2: // Top edge
                xPos = Random.Range(-halfX + padding, halfX - padding);
                zPos = halfZ - padding;
                break;
            case 3: // Bottom edge
                xPos = Random.Range(-halfX + padding, halfX - padding);
                zPos = -halfZ + padding;
                break;
        }

        playerSpawnPosition = new Vector3(xPos, 0, zPos);
        Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
    }

    void SpawnTarget()
    {
        float halfX = mapSize.x / 2;
        float halfZ = mapSize.y / 2;

        // Generate an offset within the allowed distance range
        float angle = Random.Range(0f, Mathf.PI * 2);
        float radius = Random.Range(targetSpawnRange.x, targetSpawnRange.y);
        float xOffset = Mathf.Cos(angle) * radius;
        float zOffset = Mathf.Sin(angle) * radius;

        // Compute raw target position
        float xPos = playerSpawnPosition.x + xOffset;
        float zPos = playerSpawnPosition.z + zOffset;

        // **Clamp within grid bounds**
        xPos = Mathf.Clamp(xPos, -halfX + padding, halfX - padding);
        zPos = Mathf.Clamp(zPos, -halfZ + padding, halfZ - padding);

        // Instantiate the target
        var targetObj = Instantiate(targetPrefab, new Vector3(xPos, 0, zPos), Quaternion.identity);
        targetObj.GetComponent<FolllowTargetMovement>().progressBarFill = progressBarFill;
    }

    void SetBackground()
    {
        if (RewardManager.Instance.GetPendingEnvironment() == EnvironmentType.Forest_Plants)
        {
            Leaves.SetActive(true);
        }
    }
}
