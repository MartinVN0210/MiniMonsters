using UnityEngine;

public class CamaraSigue : MonoBehaviour
{
    [SerializeField] private Transform chispa;
    [SerializeField] private Transform roca;
    [SerializeField] private float suavizado = 5f;
    [SerializeField] private Vector3 desfase = new Vector3(0f, 1f, -10f);

    private void LateUpdate()
    {
        if (chispa == null || roca == null) return;

        Vector3 puntoMedio = (chispa.position + roca.position) * 0.5f;
        Vector3 posicionDeseada = puntoMedio + desfase;
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavizado * Time.deltaTime);
    }
}