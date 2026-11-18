using UnityEngine;

[DefaultExecutionOrder(-100)]
public sealed class HeadStepCollisionSetup : MonoBehaviour
{
    private void Awake()
    {
        Configurar("Chispa", "Roca");
    }

    private static void Configurar(string chispaNombre, string rocaNombre)
    {
        GameObject chispa = GameObject.Find(chispaNombre);
        GameObject roca = GameObject.Find(rocaNombre);
        if (chispa == null || roca == null)
        {
            return;
        }

        Collider2D chispaBody = chispa.GetComponent<Collider2D>();
        Collider2D rocaBody = roca.GetComponent<Collider2D>();
        Collider2D chispaHead = ObtenerCabeza(chispa);
        Collider2D rocaHead = ObtenerCabeza(roca);
        if (chispaBody == null || rocaBody == null || chispaHead == null || rocaHead == null)
        {
            Debug.LogWarning("HeadStepCollisionSetup requiere colliders de cuerpo y CabezaEscalon en Chispa y Roca.");
            return;
        }

        Physics2D.IgnoreCollision(chispaBody, rocaBody, true);
        Physics2D.IgnoreCollision(chispaBody, chispaHead, true);
        Physics2D.IgnoreCollision(rocaBody, rocaHead, true);
        Physics2D.IgnoreCollision(chispaBody, rocaHead, false);
        Physics2D.IgnoreCollision(rocaBody, chispaHead, false);
    }

    private static Collider2D ObtenerCabeza(GameObject jugador)
    {
        Transform cabeza = jugador.transform.Find("CabezaEscalon");
        return cabeza == null ? null : cabeza.GetComponent<Collider2D>();
    }
}
