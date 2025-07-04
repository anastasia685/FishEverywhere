using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridCell
{
    public int x, y;
    public bool visited = false;
    public Vector2Int direction;
    public HashSet<GridCell> connections = new HashSet<GridCell>();
    public GameObject topWall, rightWall, bottomWall, leftWall;

    public GridCell(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void Connect(GridCell other)
    {
        connections.Add(other);
        other.connections.Add(this); // bi-directional
    }
    public void Disconnect(GridCell other)
    {
        if (other != null && IsConnected(other))
        {
            connections.Remove(other);
            other.connections.Remove(this); // Remove bidirectional connection
        }
    }

    public bool IsConnected(GridCell other)
    {
        return connections.Contains(other);
    }

    public override bool Equals(object obj)
    {
        if (obj is GridCell other)
        {
            return x == other.x && y == other.y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }
}

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] int size; // actual size/length of grid side in world-space
    [SerializeField] public int dimension; // amount of cell partitions in grid
    public float cellSize;

    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject goalPrefab;
    [SerializeField] GameObject enemyPrefab;

    [SerializeField] Transform player;

    GridCell[,] grid;
    public GridCell initialOrigin;
    GridCell origin, target;



    List<GridCell> GetValidNeighbors(GridCell cell)
    {
        List<GridCell> neighbors = new List<GridCell>();

        if (cell.y + 1 < dimension && cell.IsConnected(grid[cell.x, cell.y + 1]))
            neighbors.Add(grid[cell.x, cell.y + 1]); // Up
        if (cell.y - 1 >= 0 && cell.IsConnected(grid[cell.x, cell.y - 1]))
            neighbors.Add(grid[cell.x, cell.y - 1]); // Down
        if (cell.x + 1 < dimension && cell.IsConnected(grid[cell.x + 1, cell.y]))
            neighbors.Add(grid[cell.x + 1, cell.y]); // Right
        if (cell.x - 1 >= 0 && cell.IsConnected(grid[cell.x - 1, cell.y]))
            neighbors.Add(grid[cell.x - 1, cell.y]); // Left

        return neighbors;
    }



    void Awake()
    {
        cellSize = (float)size / (float)dimension;

        InitializeGrid();
        ChooseRandomEdgeOrigin();
        //ApplySidewinder();
        AssignInitialDirections();

        ApplyOriginShift();

        ConvertDirectionsToConnections();

        DrawGrid();

        target = FindTarget(out int maxDist, out List<GridCell> shortestPath);
        //Debug.Log($"Target chosen at ({target.x}, {target.y}) with depth {maxDist}");

        //Debug.Log($"Max dist: {maxDist}");

        //Debug.Log($"Target chosen at ({target.x}, {target.y})");

        Instantiate(goalPrefab, GetWorldPosition(target), Quaternion.identity);



        //FOR DEBUGGING - SHOW SHORTEST PATH TO TARGET
        /*Vector3 pathLineScale = new Vector3(0.1f, 0.3f, 0.1f);
        foreach (var cell in shortestPath)
        {
            var pathLine = Instantiate(wallPrefab, GetWorldPosition(cell), Quaternion.identity, transform);
            pathLine.transform.localScale = pathLineScale;
            Renderer renderer = pathLine.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.green;
            }
        }*/


        // spawn enemies
        SpawnEnemies(shortestPath);
    }
    public GridCell GetGridCell(int x, int y)
    {
        if (x >= 0 && x < dimension && y >= 0 && y < dimension)
            return grid[x, y];
        return null;
    }
    public Vector3 GetWorldPosition(GridCell cell)
    {
        Vector3 gridOffset = new Vector3((-size + cellSize) / 2, 0, (-size + cellSize) / 2);
        return new Vector3(cell.x * cellSize, 0, cell.y * cellSize) + gridOffset;
    }

    void InitializeGrid()
    {
        grid = new GridCell[dimension, dimension];

        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                grid[x, y] = new GridCell(x, y);
            }
        }
    }
    void ChooseRandomEdgeOrigin()
    {
        List<GridCell> edgeCells = new List<GridCell>();

        // Collect all edge cells (top, bottom, left, right borders)
        for (int x = 0; x < dimension; x++)
        {
            edgeCells.Add(grid[x, 0]); // Bottom edge
            edgeCells.Add(grid[x, dimension - 1]); // Top edge
        }
        for (int y = 0; y < dimension; y++)
        {
            edgeCells.Add(grid[0, y]); // Left edge
            edgeCells.Add(grid[dimension - 1, y]); // Right edge
        }

        // Choose a random edge cell
        initialOrigin = origin = edgeCells[Random.Range(0, edgeCells.Count)];
    }
    void DrawGrid()
    {
        //Vector3 gridOffset = new Vector3(-(dimension - 1) * cellSize / 2, 0, -(dimension - 1) * cellSize / 2);
        Vector3 gridOffset = new Vector3((-size + cellSize) / 2, 0, (-size + cellSize) / 2);
        Vector3 wallScale = new Vector3(cellSize + 0.2f, 1f, 0.2f);

        for (int y = 0; y < dimension; y++)
        {
            for (int x = 0; x < dimension; x++)
            {
                GridCell cell = grid[x, y];
                Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize) + gridOffset;

                // draw walls
                //top
                if (y == dimension - 1 || !cell.IsConnected(GetGridCell(x, y + 1)))
                {
                    cell.topWall = Instantiate(wallPrefab, pos + Vector3.forward * (cellSize / 2), Quaternion.identity, transform);
                    cell.topWall.transform.localScale = wallScale;
                }
                // bottom
                if (y == 0 || !cell.IsConnected(GetGridCell(x, y - 1)))
                {
                    cell.bottomWall = Instantiate(wallPrefab, pos - Vector3.forward * (cellSize / 2), Quaternion.identity, transform);
                    cell.bottomWall.transform.localScale = wallScale;
                }
                // left
                if (x == 0 || !cell.IsConnected(GetGridCell(x - 1, y)))
                {
                    cell.leftWall = Instantiate(wallPrefab, pos - Vector3.right * (cellSize / 2), Quaternion.Euler(0, 90, 0), transform);
                    cell.leftWall.transform.localScale = wallScale;
                }
                // right
                if (x == dimension - 1 || !cell.IsConnected(GetGridCell(x + 1, y)))
                {
                    cell.rightWall = Instantiate(wallPrefab, pos + Vector3.right * (cellSize / 2), Quaternion.Euler(0, 90, 0), transform);
                    cell.rightWall.transform.localScale = wallScale;
                }
            }
        }
    }
    void ApplySidewinder()
    {
        for (int y = 0; y < dimension; y++)
        {
            List<GridCell> runSet = new List<GridCell>();

            for (int x = 0; x < dimension; x++)
            {
                GridCell cell = grid[x, y];
                runSet.Add(cell);

                bool atEasternEdge = (x == dimension - 1);
                bool atNorthernEdge = (y == dimension - 1);

                bool shouldCloseOut = atEasternEdge || (!atNorthernEdge && Random.value > 0.5f);

                if (shouldCloseOut)
                {
                    GridCell chosenCell = runSet[Random.Range(0, runSet.Count)];
                    if (!atNorthernEdge)
                    {
                        GridCell northNeighbor = GetGridCell(chosenCell.x, chosenCell.y + 1);
                        if (northNeighbor != null)
                        {
                            chosenCell.Connect(northNeighbor);
                        }
                    }

                    runSet.Clear();
                }

                else if (!atEasternEdge)
                {
                    GridCell eastNeighbor = GetGridCell(x + 1, y);
                    if (eastNeighbor != null)
                    {
                        cell.Connect(eastNeighbor);
                    }
                }
            }
        }

        AssignDirectionsFromSidewinder();
    }
    void AssignDirectionsFromSidewinder()
    {
        Queue<GridCell> queue = new Queue<GridCell>();
        Dictionary<GridCell, GridCell> cameFrom = new Dictionary<GridCell, GridCell>();

        queue.Enqueue(origin);
        cameFrom[origin] = null;

        while (queue.Count > 0)
        {
            GridCell current = queue.Dequeue();
            List<GridCell> neighbors = new List<GridCell>();

            GridCell top = GetGridCell(current.x, current.y + 1);
            GridCell right = GetGridCell(current.x + 1, current.y);
            GridCell bottom = GetGridCell(current.x, current.y - 1);
            GridCell left = GetGridCell(current.x - 1, current.y);

            if (top != null && current.IsConnected(top) && !cameFrom.ContainsKey(top))
            {
                neighbors.Add(top);
                cameFrom[top] = current;
            }
            if (right != null && current.IsConnected(right) && !cameFrom.ContainsKey(right))
            {
                neighbors.Add(right);
                cameFrom[right] = current;
            }
            if (bottom != null && current.IsConnected(bottom) && !cameFrom.ContainsKey(bottom))
            {
                neighbors.Add(bottom);
                cameFrom[bottom] = current;
            }
            if (left != null && current.IsConnected(left) && !cameFrom.ContainsKey(left))
            {
                neighbors.Add(left);
                cameFrom[left] = current;
            }

            foreach (GridCell neighbor in neighbors)
            {
                queue.Enqueue(neighbor);
            }
        }

        foreach (var pair in cameFrom)
        {
            GridCell cell = pair.Key;
            GridCell parent = pair.Value;
            if (parent == null) continue;

            int dx = parent.x - cell.x;
            int dy = parent.y - cell.y;
            cell.direction = new Vector2Int(dx, dy);
        }
    }

    [ContextMenu("Apply Origin Shift")]
    void ApplyOriginShift()
    {
        for (int i = 0; i < dimension * dimension * 30; i++)
        {
            OriginShiftIteration();
        }
    }
    void OriginShiftIteration()
    {
        List<Vector2Int> possibleDirections = new List<Vector2Int>();

        GridCell top = GetGridCell(origin.x, origin.y + 1);
        GridCell right = GetGridCell(origin.x + 1, origin.y);
        GridCell bottom = GetGridCell(origin.x, origin.y - 1);
        GridCell left = GetGridCell(origin.x - 1, origin.y);

        if (top != null) possibleDirections.Add(new Vector2Int(0, 1));
        if (right != null) possibleDirections.Add(new Vector2Int(1, 0));
        if (bottom != null) possibleDirections.Add(new Vector2Int(0, -1));
        if (left != null) possibleDirections.Add(new Vector2Int(-1, 0));

        //if (possibleDirections.Count == 0) break;

        Vector2Int direction = possibleDirections[Random.Range(0, possibleDirections.Count)];
        int nextX = origin.x + direction.x;
        int nextY = origin.y + direction.y;

        GridCell nextOrigin = GetGridCell(nextX, nextY);
        origin.direction = direction;
        origin = nextOrigin;
        origin.direction = new Vector2Int(0, 1);
    }
    void AssignInitialDirections()
    {
        for (int y = 0; y < dimension; y++)
        {
            for (int x = 1; x < dimension; x++)
            {
                grid[x, y].direction = new Vector2Int(-1, 0);
            }
            grid[0, y].direction = new Vector2Int(0, -1);
        }

        grid[0, 0].direction = new Vector2Int(0, 0); // Bottom-left cell is stationary
    }
    void ConvertDirectionsToConnections()
    {
        for (int y = 0; y < dimension; y++)
        {
            for (int x = 0; x < dimension; x++)
            {
                GridCell cell = grid[x, y];
                cell.connections.Clear(); // Clear existing connections to avoid duplicates
            }
        }

        for (int y = 0; y < dimension; y++)
        {
            for (int x = 0; x < dimension; x++)
            {
                GridCell cell = grid[x, y];
                if (cell.direction == Vector2Int.zero)
                {
                    continue; // Skip if direction is not set
                }

                GridCell neighbor = GetGridCell(x + cell.direction.x, y + cell.direction.y);
                if (neighbor != null)
                {
                    cell.Connect(neighbor);
                    neighbor.Connect(cell); // Ensure the connection is bidirectional
                }
                else
                {
                    Debug.LogWarning($"Invalid direction at ({x},{y}): {cell.direction}");
                }
            }
        }
    }


    GridCell FindTarget(out int maxDepth, out List<GridCell> shortestPath)
    {
        //--- for bfs
        Queue<(GridCell, int)> queue = new Queue<(GridCell, int)>();
        Dictionary<GridCell, int> distances = new Dictionary<GridCell, int>();
        //---

        //--- for reconstructing shortest path
        Dictionary<GridCell, GridCell> parents = new Dictionary<GridCell, GridCell>();

        queue.Enqueue((initialOrigin, 0));
        distances[initialOrigin] = 0;

        GridCell farthest = initialOrigin;
        maxDepth = 0;

        while (queue.Count > 0)
        {
            (GridCell cell, int depth) = queue.Dequeue();

            if (depth > maxDepth)
            {
                maxDepth = depth;
                farthest = cell;
            }

            foreach (GridCell neighbor in cell.connections)
            {
                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = depth + 1;
                    parents[neighbor] = cell;
                    queue.Enqueue((neighbor, depth + 1));
                }
            }
        }

        // configure min/max depth
        int minDepth = Mathf.RoundToInt(maxDepth * 0.85f);
        int upperBoundDepth = Mathf.RoundToInt(maxDepth * 0.95f);

        // pick a target within minDepth to upperBoundDepth
        List<GridCell> validTargets = new List<GridCell>();
        foreach (var kvp in distances)
        {
            if (kvp.Value >= minDepth && kvp.Value <= upperBoundDepth)
            {
                validTargets.Add(kvp.Key);
            }
        }

        GridCell targetCell = validTargets.Count > 0 ? validTargets[Random.Range(0, validTargets.Count)] : initialOrigin;

        shortestPath = new List<GridCell>();
        GridCell current = targetCell;
        while (current != initialOrigin)
        {
            shortestPath.Add(current);
            current = parents[current];
        }
        shortestPath.Add(initialOrigin);
        shortestPath.Reverse();

        return targetCell;
    }
    void SpawnEnemies(List<GridCell> mainPath)
    {
        int midX = dimension / 2;
        int midY = dimension / 2;

        GridCell[] startCells = new GridCell[4]; // one per quadrant

        startCells[0] = GetRandomCellInArea(0, midX - 1, midY, dimension - 1);
        startCells[1] = GetRandomCellInArea(midX, dimension - 1, midY, dimension - 1);
        startCells[2] = GetRandomCellInArea(0, midX - 1, 0, midY - 1);
        startCells[3] = GetRandomCellInArea(midX, dimension - 1, 0, midY - 1);

        Vector3 pathLineScale = new Vector3(0.25f, 0.1f, 0.25f);

        foreach (var startCell in startCells)
        {
            if (startCell != null)
            {
                List<GridCell> patrolPath = GetSafePatrolPath(startCell, mainPath);
                if (patrolPath == null || patrolPath.Count == 0) continue;

                var enemyObj = Instantiate(enemyPrefab, GetWorldPosition(startCell), Quaternion.identity);
                var patrolScript = enemyObj.GetComponent<Patrol>();
                patrolScript.mazeGenerator = this;
                patrolScript.SetPatrolPath(patrolPath);

                // draw patrol path
                foreach(var cell in patrolPath)
                {
                    var pathLine = Instantiate(wallPrefab, GetWorldPosition(cell), Quaternion.identity, transform);
                    pathLine.transform.localScale = pathLineScale;
                }
            }
        }
    }
    GridCell GetRandomCellInArea(int minX, int maxX, int minY, int maxY)
    {
        List<GridCell> candidates = new List<GridCell>();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                GridCell cell = grid[x, y];

                if (GetValidNeighbors(cell).Count >= 2 && !IsTooClose(initialOrigin, cell) && !IsTooClose(target, cell))
                {
                    candidates.Add(cell);
                }
            }
        }

        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
    }
    List<GridCell> GetSafePatrolPath(GridCell startCell, List<GridCell> mainPath)
    {
        List<GridCell> patrolPath = new List<GridCell> { startCell };
        HashSet<GridCell> visited = new HashSet<GridCell> { startCell };
        GridCell current = startCell;

        while (true)
        {
            List<GridCell> neighbors = GetValidNeighbors(current);
            neighbors.RemoveAll(n => visited.Contains(n) || n.Equals(initialOrigin));

            // can't expand - check if path is valid; if not, discard/return null
            if (neighbors.Count == 0)
            {
                if (BlocksPlayerPath(patrolPath, mainPath)) return null;
                else break;
            }

            GridCell nextCell = neighbors[Random.Range(0, neighbors.Count)];
            patrolPath.Add(nextCell);
            visited.Add(nextCell);
            current = nextCell;

            // if desired lengh was reached and path is valid, return that path
            if (patrolPath.Count == 5 && !BlocksPlayerPath(patrolPath, mainPath))
            {
                break;
            }

            // the maximum limit
            if (patrolPath.Count == 7)
            {
                if (!BlocksPlayerPath(patrolPath, mainPath)) break;
                else return null;
            }
        }

        return patrolPath;
    }
    bool IsTooClose(GridCell targetCell, GridCell cell)
    {
        int safeDistance = 4;
        return Mathf.Abs(cell.x - initialOrigin.x) + Mathf.Abs(cell.y - initialOrigin.y) < safeDistance;
    }
    bool BlocksPlayerPath(List<GridCell> patrolPath, List<GridCell> playerPath)
    {
        int consecutiveSafe = 0;

        /*foreach (var cell in playerPath)
        {
            if (patrolPath.Contains(cell))
            {
                consecutiveSafe = 0; // reset when blocked
            }
            else
            {
                consecutiveSafe++;
                if (consecutiveSafe >= 3)
                    return false; // three consecutive save cells were found - path is valid
            }
        }*/
        foreach (var cell in patrolPath)
        {
            if (playerPath.Contains(cell))
            {
                consecutiveSafe = 0; // reset when blocked
            }
            else
            {
                consecutiveSafe++;
                if (consecutiveSafe >= 3)
                    return false; // three consecutive save cells were found - path is valid
            }
        }

        return true; // blocked by default
    }
}
