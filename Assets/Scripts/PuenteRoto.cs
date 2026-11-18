using UnityEngine;

public class PuenteRoto : MonoBehaviour
{
public void Activar()
    {
        gameObject.SetActive(true);
        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>(true))
        {
            collider.enabled = true;
        }
    }
}
