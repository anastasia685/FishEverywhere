using UnityEngine;

public class Gate : MonoBehaviour
{
    [SerializeField] CardinalDirection direction;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            MySceneManager.Instance.SwitchScene(direction);
        }
    }
}
