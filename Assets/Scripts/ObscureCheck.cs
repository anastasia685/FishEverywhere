using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObscureCheck : MonoBehaviour
{
    [SerializeField] GameObject player;
    float fadeAlpha = 0.4f;

    HashSet<GameObject> currentObstructions = new HashSet<GameObject>();
    List<GameObject> newHits = new List<GameObject>();

    Vector3[] offsets = new Vector3[]
    {
        new Vector3(0, 1.5f, 0),         // center
        new Vector3(-0.2f, 2.6f, 0),     // top-left
        new Vector3(0.2f, 2.6f, 0),      // top-right
        new Vector3(-0.35f, 0.5f, 0),    // bottom-left
        new Vector3(0.35f, 0.5f, 0),     // bottom-right
    };

    void Update()
    {
        newHits.Clear();

        foreach (var offset in offsets)
        {
            Vector3 target = player.transform.position + offset;
            Vector3 dir = target - transform.position;

            //Debug.DrawRay(transform.position, dir, Color.red, 0.1f);

            Ray ray = new Ray(transform.position, dir);
            RaycastHit[] hits;


            hits = Physics.RaycastAll(ray, 22.0f);
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                GameObject hitObj = hit.collider.gameObject;
                if (hitObj != player && !newHits.Contains(hitObj))
                {
                    newHits.Add(hitObj);
                }
            }
        }

        // Fade in objects that were obstructing before but aren't anymore
        foreach (GameObject obj in currentObstructions)
        {
            if (!newHits.Contains(obj))
            {
                var fader = obj.GetComponent<Fader>();
                if (fader) fader.SetAlpha(1f);
            }
        }

        // Fade out newly obstructing objects
        foreach (GameObject obj in newHits)
        {
            if (!currentObstructions.Contains(obj))
            {
                var fader = obj.GetComponent<Fader>();
                if (fader) fader.SetAlpha(fadeAlpha / newHits.Count);
            }
        }

        // Update the set for next frame
        currentObstructions.Clear();
        currentObstructions.UnionWith(newHits);
    }
}
