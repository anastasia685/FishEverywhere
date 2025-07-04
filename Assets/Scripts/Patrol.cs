using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    public float speed = 1f;
    private List<GridCell> patrolPath;
    private int currentTargetIndex = 0;
    private bool movingForward = true;

    public MazeGenerator mazeGenerator;

    public void SetPatrolPath(List<GridCell> path)
    {
        patrolPath = path;
        currentTargetIndex = 0;
    }

    void Update()
    {
        if (patrolPath == null || patrolPath.Count == 0) return;

        // Move towards the target
        GridCell targetCell = patrolPath[currentTargetIndex];
        Vector3 targetPosition = mazeGenerator.GetWorldPosition(targetCell);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Check if reached the target
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (movingForward)
            {
                currentTargetIndex++;
                if (currentTargetIndex >= patrolPath.Count)
                {
                    movingForward = false;
                    currentTargetIndex = patrolPath.Count - 1;
                }
            }
            else
            {
                currentTargetIndex--;
                if (currentTargetIndex < 0)
                {
                    movingForward = true;
                    currentTargetIndex = 0;
                }
            }
        }
    }
}
