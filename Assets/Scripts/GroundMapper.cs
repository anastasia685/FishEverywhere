using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class GroundMapper : MonoBehaviour
{
    Texture2D regionTexture;
    //Material groundMaterial;

    // temporary
    Color GrassColor = new Color(0.043f, 0.27f, 0.09f);
    Color DirtColor = new Color(0.76f, 0.53f, 0.27f);
    Color RockColor = new Color(0.5f, 0.5f, 0.5f);

    /*void OnEnable()
    {
        if (groundMaterial == null)
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                groundMaterial = renderer.sharedMaterial; // Use sharedMaterial in Edit Mode
            }
        }

        CreateEnvironmentTexture();
    }

    void OnValidate()
    {
        CreateEnvironmentTexture();
    }*/

    void Start()
    {
        //groundMaterial = GetComponent<MeshRenderer>().sharedMaterial;

        // Create a region-based texture mask
        CreateEnvironmentTexture();
    }

    void CreateEnvironmentTexture()
    {
        int gridSize = Mathf.RoundToInt(transform.localScale.x * 10f); // Match plane size dynamically
        regionTexture = new Texture2D(gridSize, gridSize, TextureFormat.RGB24, false);
        regionTexture.filterMode = FilterMode.Point;
        regionTexture.wrapMode = TextureWrapMode.Clamp;

        float perlinScale = 10f; // Controls region variation (larger = more gradual changes)
        float offsetX = 10, offsetY = 15;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                // Generate Perlin noise value (normalized 0 - 1)
                float noiseValue = Mathf.PerlinNoise((x + offsetX) / perlinScale, (y + offsetY) / perlinScale);

                Color regionColor;

                // Assign regions based on noise value
                if (noiseValue < 0.55f)
                    regionColor = GrassColor;
                else if (noiseValue < 0.77f)
                    regionColor = DirtColor;
                else
                    regionColor = RockColor;

                regionTexture.SetPixel(x, y, regionColor);
            }
        }

        regionTexture.Apply();

        /*if (groundMaterial != null)
        {
            groundMaterial.SetTexture("_RegionTex", regionTexture);
        }*/
    }

    public EnvironmentType GetEnvironment(Vector3 worldPosition)
    {
        Quaternion inverseRotation = Quaternion.Inverse(transform.rotation);
        Vector3 localPos = inverseRotation * (worldPosition - transform.position);

        float uvX = (localPos.x / (transform.localScale.x * 10f)) + 0.5f;
        float uvY = (localPos.z / (transform.localScale.z * 10f)) + 0.5f;

        int x = Mathf.Clamp(Mathf.FloorToInt(uvX * regionTexture.width), 0, regionTexture.width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(uvY * regionTexture.height), 0, regionTexture.height - 1);

        Color sampledColor = regionTexture.GetPixel(x, y);

        
        if (ColorsAreClose(sampledColor, DirtColor))
            return EnvironmentType.Forest_Ground;
        if (ColorsAreClose(sampledColor, RockColor))
            return EnvironmentType.Forest_Plants;
        return EnvironmentType.Forest_Undergrowth;
    }


    bool ColorsAreClose(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }
}
