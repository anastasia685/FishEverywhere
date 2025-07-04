using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class Caster : MonoBehaviour
{
    [SerializeField] AudioClip WhooshSFX;
    [SerializeField] AudioClip SposhSFX;

    [SerializeField] Animator animator;


    [SerializeField] GameObject[] tileGrids;
    [SerializeField] LayerMask mask;
    Dictionary<int, List<Tilemap>> tilemaps = new();
    Vector3Int? highlightedTile = null;
    TilePicker activeTilePicker = null;
    Plane tileGridPlane;
    bool isCasting;

    private void Start()
    {
        foreach (var tileGrid in tileGrids)
        {
            var tilemapRenderers = tileGrid.GetComponentsInChildren<TilemapRenderer>().ToDictionary(i => i.GetInstanceID(), i => i.sortingOrder);
            
            var tilemaps = tileGrid.GetComponentsInChildren<Tilemap>().ToList();
            tilemaps.Sort((a, b) => {
                tilemapRenderers.TryGetValue(a.GetInstanceID(), out var sortOrderA);
                tilemapRenderers.TryGetValue(b.GetInstanceID(), out var sortOrderB);
                return sortOrderA - sortOrderB;
            });

            this.tilemaps.Add(tileGrid.GetInstanceID(), tilemaps);
        }
    }
    void Update()
    {
        if (!isCasting || activeTilePicker == null) return;

        int? gridId = GetCurrentGridId();
        if (!gridId.HasValue)
        {
            Debug.Log("No grid was found in the vicinity");
            return;
        }

        var currTilePicker = tilemaps[gridId.Value][4].GetComponent<TilePicker>();
        tileGridPlane = new Plane(Vector3.up, tilemaps[gridId.Value][0].transform.position);

        if(currTilePicker != activeTilePicker)
        {
            activeTilePicker.Clear();
            highlightedTile = null;

            activeTilePicker = currTilePicker;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (tileGridPlane.Raycast(ray, out float hitDist))
        {
            Vector3 hitPoint = ray.GetPoint(hitDist);
            Vector3Int cell = tilemaps[gridId.Value][0].WorldToCell(hitPoint);
            cell.z = 0;

            Vector3Int playerCell = tilemaps[gridId.Value][0].WorldToCell(transform.position);
            // chebyshev distance to include immediate diagonal neighbors
            int dx = Mathf.Abs(playerCell.x - cell.x);
            int dy = Mathf.Abs(playerCell.y - cell.y);
            int dist = Mathf.Max(dx, dy);

            int radius = 1;
            if (dist <= radius && tilemaps[gridId.Value][0].HasTile(cell) && cell != highlightedTile) // make sure the base layer has tile value
            {
                highlightedTile = cell;
                activeTilePicker.HighlightTile(cell);
            }
        }
        else if (highlightedTile.HasValue)
        {
            activeTilePicker.Clear();
            highlightedTile = null;
        }
    }
    private void OnEnable()
    {
        GameManager.PlayerControls.Player.Cast.performed += CastStarted;
        GameManager.PlayerControls.Player.Cast.canceled += CastEnded;
    }

    private void OnDisable()
    {
        GameManager.PlayerControls.Player.Cast.performed -= CastStarted;
        GameManager.PlayerControls.Player.Cast.canceled -= CastEnded;
    }

    public int? GetCurrentGridId()
    {
        float closest = float.MaxValue;
        GameObject closestGrid = null;

        foreach(var tileGrid in tileGrids)
        {
            float dist = transform.position.y - tileGrid.transform.position.y;
            if (Mathf.Abs(dist) > 0.1) continue;

            if(dist < closest)
            {
                closest = dist;
                closestGrid = tileGrid;
            }
        }
        return closestGrid != null ? closestGrid.GetInstanceID() : null;
    }

    void CastStarted(InputAction.CallbackContext ctx)
    {
        isCasting = true;

        int? gridId = GetCurrentGridId();
        if (!gridId.HasValue) return;

        activeTilePicker = tilemaps[gridId.Value][4].GetComponent<TilePicker>();

        if (highlightedTile.HasValue)
        {
            activeTilePicker.HighlightTile(highlightedTile.Value);
        }
    }
    void CastEnded(InputAction.CallbackContext ctx)
    {
        if (!highlightedTile.HasValue || activeTilePicker == null) return; //TODO: add some logs

        // this is to check if casting action was cancelled from something like ui input
        // technically, this should always fire, as cancel cleanup is done separately, but keep the check just in case
        if (isCasting) OnCast(highlightedTile.Value);

        activeTilePicker.Clear();
        highlightedTile = null;
        activeTilePicker = null;
        isCasting = false;
    }

    public void CancelCast()
    {
        if (!highlightedTile.HasValue || activeTilePicker == null) return;

        activeTilePicker.Clear();
        highlightedTile = null;
        activeTilePicker = null;
        isCasting = false;
    }

    void OnCast(Vector3Int tilePos)
    {
        // disable controls while processing cast request
        GameManager.PlayerControls.Player.Disable();
        GameManager.PlayerControls.UI.Disable();

        // check hidden tile
        var currentGridId = GetCurrentGridId();

        if(currentGridId == null)
        {
            Debug.Log("No close tile grid found.");

            // re-enable controls
            GameManager.PlayerControls.Player.Enable();
            GameManager.PlayerControls.UI.Enable();

            return;
        }

        CheckHiddenTiles(currentGridId.Value, tilePos);

        EnvironmentType currEnvironment = GetEnvironment(currentGridId.Value, tilePos);

        RewardManager.Instance.ChooseReward(currEnvironment);
        CatchableSO pendingReward = RewardManager.Instance.GetPendingItem();
        // this will be the same as currEnvironment, but just to be extra sure i guess :d
        // right now pending environment is used to determine which mini-game to open
        EnvironmentType pendingEnvironment = RewardManager.Instance.GetPendingEnvironment();
        if (pendingReward != null && pendingEnvironment != EnvironmentType.None && highlightedTile.HasValue)
        {
            //Vector3Int playerCell = tilemaps[currentGridId.Value][0].WorldToCell(transform.position);
            var tileWorldPos = tilemaps[currentGridId.Value][0].GetCellCenterWorld(highlightedTile.Value);
            var offset = tileWorldPos - transform.position;
            StartCoroutine(ThrowBobber(pendingReward, pendingEnvironment, offset));
        }
        else
        {
            Debug.Log("No reward found.");

            // re-enable controls
            GameManager.PlayerControls.Player.Enable();
            GameManager.PlayerControls.UI.Enable();
        }
    }

    void CheckHiddenTiles(int tilegridId, Vector3Int cellPos)
    {
        //Hidden Areas achivement
        var currTileMap = tilemaps.TryGetValue(tilegridId, out var currTilemaps) ? currTilemaps : null;
        if (currTileMap == null)
            return;

        TileBase hiddenTile = currTileMap[3].GetTile(cellPos); // (0-2 for main loot pool layers, 3 for hidden tile map, 4 for highlighting selected tile during casting, 
        if (hiddenTile != null)
        {
            HiddenAreasManager.Instance.MarkEntryCollected(cellPos);
        }
    }

    public EnvironmentType GetEnvironment(int tilegridId, Vector3Int position)
    {
        var currTileMap = tilemaps.TryGetValue(tilegridId, out var currTilemaps) ? currTilemaps : null;
        if (currTileMap == null)
            return EnvironmentType.None;

        // check layers top to bottom
        if (currTileMap[2].GetTile(position) != null) return EnvironmentType.Forest_Plants;
        if (currTileMap[1].GetTile(position) != null) return EnvironmentType.Forest_Undergrowth;
        if (currTileMap[0].GetTile(position) != null) return EnvironmentType.Forest_Ground;

        return EnvironmentType.None;
    }


    IEnumerator ThrowBobber(CatchableSO reward, EnvironmentType pendingEnvironment, Vector3 cellOffset)
    {
        //SFX
        AudioSource.PlayClipAtPoint(WhooshSFX, Camera.main.transform.position, 1f);

        //TODO: pick a cardinal direction to play different animations

        // play out the cast animation
        animator.SetTrigger(GetAnimationTriggerFromOffset(cellOffset));


        //SFX
        AudioSource.PlayClipAtPoint(SposhSFX, Camera.main.transform.position, 1f);

        yield return new WaitForSeconds(1.0f);


        MySceneManager.Instance.SetSpawnPosition(gameObject.transform.position);
        MySceneManager.Instance.SetSpawnRotation(gameObject.transform.rotation);
        switch (pendingEnvironment)
        {
            case EnvironmentType.Forest_Ground:
                MySceneManager.Instance.LoadMinigame("Maze");
                break;
            case EnvironmentType.Forest_Undergrowth:
            default:
                MySceneManager.Instance.LoadMinigame("FollowTarget");
                break;
        }
    }

    string GetAnimationTriggerFromOffset(Vector3 offset)
    {
        float absX = Mathf.Abs(offset.x);
        float absZ = Mathf.Abs(offset.z);

        if(absZ >= absX)
        {
            return offset.z <= 0 ? "fishFront" : "fishBack";
        }
        else
        {
            return offset.x <= 0 ? "fishLeft" : "fishRight";
        }
    }
}
