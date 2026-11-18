using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Sprite spriteApagado;
    [SerializeField] private Sprite spriteActivado;
    [SerializeField] private AudioClip sonidoActivacion;
    [SerializeField] private AudioSource fuenteAudio;

    private SpriteRenderer spriteRenderer;
    private bool activado;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        BoxCollider2D trigger = GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        if (spriteApagado != null)
        {
            spriteRenderer.sprite = spriteApagado;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activado || !other.CompareTag("Player"))
        {
            return;
        }

        GameManager manager = GameManager.Instancia;
        if (manager == null)
        {
            Debug.LogError("No hay GameManager para registrar el checkpoint.", this);
            return;
        }

        manager.RegistrarCheckpoint(this);
        activado = true;
        spriteRenderer.sprite = spriteActivado != null ? spriteActivado : spriteRenderer.sprite;
        if (sonidoActivacion != null)
        {
            if (fuenteAudio != null)
            {
                fuenteAudio.PlayOneShot(sonidoActivacion);
            }
            else
            {
                AudioSource.PlayClipAtPoint(sonidoActivacion, transform.position);
            }
        }
    }

    public void Desactivar()
    {
        activado = false;
        if (spriteRenderer != null && spriteApagado != null)
        {
            spriteRenderer.sprite = spriteApagado;
        }
    }
}
