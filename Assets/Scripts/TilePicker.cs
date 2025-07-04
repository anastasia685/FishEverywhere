using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePicker : MonoBehaviour
{
    [SerializeField] LayerMask mask;
    [SerializeField] Tile highlightTile;
    Caster caster;
    Tilemap tilemap;

    Vector3Int? currentTile = null;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }
    void Start()
    {
        caster = FindAnyObjectByType<Caster>(); // there's only gonna be one player rig in the scene
    }

    /*void Update()
    {
        if (!caster.isCasting || transform.parent.gameObject.GetInstanceID() != caster.GetCurrentGridId())
        {
            if(prevPos.HasValue)
            {
                tilemap.SetTile(prevPos.Value, null);
                prevPos = null;
            }
            return;
        }
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100, mask))
        {
            Vector3Int cellPos = tilemap.WorldToCell(hit.point);
            cellPos.z = 0;
            //Debug.Log(cellPos);

            //TODO: limit selection to neighboring cells

            if (cellPos != prevPos)
            {
                if (prevPos.HasValue)
                {
                    tilemap.SetTile(prevPos.Value, null);
                }

                tilemap.SetTile(cellPos, highlightTile);
                prevPos = cellPos;
            }
        }
        else if (prevPos.HasValue)
        {
            tilemap.SetTile(prevPos.Value, null);
            prevPos = null;
        }
    }*/

    public void HighlightTile(Vector3Int cellPos)
    {
        if (currentTile.HasValue && currentTile.Value != cellPos)
        {
            tilemap.SetTile(currentTile.Value, null);
        }

        tilemap.SetTile(cellPos, highlightTile);
        currentTile = cellPos;
    }

    public void Clear()
    {
        if (currentTile.HasValue)
        {
            tilemap.SetTile(currentTile.Value, null);
            currentTile = null;
        }
    }

    public Vector3Int? GetCurrentTile()
    {
        return currentTile;
    }
}
