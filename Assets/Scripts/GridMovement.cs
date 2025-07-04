using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridMovement : MinigamePlayerMovement
{
    [SerializeField] MazeGenerator mazeGenerator;
    [SerializeField] TMP_Text countdownText;

    GridCell currentCell, targetCell;
    Vector3 targetPosition;

    Vector3 queuedDirection;

    bool isMoving = false;

    float countDown;

    Vector2Int prevDirection = Vector2Int.zero;

    void Start()
    {
        SetParams();

        countdownText.text = countDown.ToString("F0");

        currentCell = mazeGenerator.initialOrigin;
        if (currentCell != null)
        {
            transform.position = mazeGenerator.GetWorldPosition(currentCell);
        }
    }

    void Update()
    {
        // update countdown
        countDown -= Time.deltaTime;
        if (countDown <= 0)
        {
            OnLose();
        }
        else
        {
            countdownText.text = countDown.ToString("F0");
        }


        if (!isMoving) ProcessInput();
        MoveToTarget();
    }

    void ProcessInput()
    {
        if (moveInput == Vector2.zero) return;

        // Get grid coordinates
        float cellSize = mazeGenerator.cellSize;
        int dimension = mazeGenerator.dimension;

        Vector3 gridOffset = new Vector3(-(dimension) * cellSize / 2, 0, -(dimension) * cellSize / 2);
        int currentX = Mathf.FloorToInt((transform.position.x - gridOffset.x) / cellSize);
        int currentY = Mathf.FloorToInt((transform.position.z - gridOffset.z) / cellSize);

        // Determine movement direction (no diagonals)
        Vector2Int direction = Vector2Int.zero;

        // if moveInput has both horizontal and vertical components - pick the latest one
        if (Mathf.Abs(moveInput.x) == Mathf.Abs(moveInput.y))
        {
            // follow previous direction if any, otherwise prefer horizontal
            direction = prevDirection.x != 0
                ? new Vector2Int((int)Mathf.Sign(moveInput.x), 0)
                : new Vector2Int(0, (int)Mathf.Sign(moveInput.y));
        }
        else if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            direction = new Vector2Int((int)Mathf.Sign(moveInput.x), 0); // move horizontally
        }
        else
        {
            direction = new Vector2Int(0, (int)Mathf.Sign(moveInput.y)); // move vertically
        }

        // Calculate next cell
        int targetX = currentX + direction.x;
        int targetY = currentY + direction.y;

        // Get next cell and validate
        GridCell nextCell = mazeGenerator.GetGridCell(targetX, targetY);
        if (nextCell == null || !currentCell.IsConnected(nextCell)) return; // Blocked move

        // Set new movement target
        targetCell = nextCell;
        targetPosition = mazeGenerator.GetWorldPosition(targetCell);
        isMoving = true;
        prevDirection = direction;
    }

    void MoveToTarget()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Check if reached target
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            currentCell = targetCell;
            isMoving = false; // Allow new movement input
        }
    }

    void OnLose()
    {
        //SFX
        AudioManager.Instance.playSFX(FailSFX);

        Debug.Log("The fish got away!");
        RewardManager.Instance.ClearPendingReward();
        MySceneManager.Instance.LoadScene(MySceneManager.Instance.GetCurrentScene());
    }

    void SetParams()
    {
        RarityType pendingRarity = RewardManager.Instance.GetPendingRarity();

        switch (pendingRarity)
        {
            case RarityType.Common:
            default:
                countDown = 30;
                break;
            case RarityType.Rare:
                countDown = 25;
                break;
            case RarityType.Epic:
                countDown = 20;
                break;
            case RarityType.Legendary:
                countDown = 15;
                break;
            case RarityType.Mythic:
                countDown = 10;
                break;
        }

        countDown *= PlayerStatsManager.Instance.GetTimeScaleMultiplier();
    }
}
