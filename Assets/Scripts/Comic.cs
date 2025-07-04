using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Comic : MonoBehaviour
{

    [SerializeField] Sprite[] Panels;
    [SerializeField] Image image;
    int i = 1;

    // Start is called before the first frame update
    void Start()
    {
        image.sprite = Panels[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (i < Panels.Length)
            {
                image.sprite = Panels[i];
                i++;
            }
            else
            {
                MySceneManager.Instance.LoadScene(GameManager.Instance.startingScene);
            }
        }
    }
}
