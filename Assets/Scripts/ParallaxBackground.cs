using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float distanceFromCamera = 10f;
    [Min(0f)]
    [SerializeField] private float coveragePadding = 1.5f;
    [Min(0.01f)]
    [SerializeField] private float smoothTime = 0.2f;

    private SpriteRenderer spriteRenderer;
    private Vector3 followVelocity;

    private void Awake()
    {
        if (!TryGetComponent(out spriteRenderer))
        {
            Debug.LogError("ParallaxBackground requires a SpriteRenderer.", this);
            enabled = false;
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError("ParallaxBackground requires a target camera.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null || spriteRenderer.sprite == null)
        {
            return;
        }

        float viewHeight = targetCamera.orthographicSize * 2f;
        float viewWidth = viewHeight * targetCamera.aspect;
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        float scale = Mathf.Max(
            (viewWidth + coveragePadding * 2f) / spriteSize.x,
            (viewHeight + coveragePadding * 2f) / spriteSize.y);

        transform.localScale = Vector3.one * scale;
        transform.rotation = targetCamera.transform.rotation;

        Vector3 cameraPosition = targetCamera.transform.position
                                 + targetCamera.transform.forward * distanceFromCamera;
        Vector3 position = Vector3.SmoothDamp(
            transform.position,
            cameraPosition,
            ref followVelocity,
            smoothTime);

        float horizontalMargin = Mathf.Max(0f, (spriteRenderer.bounds.size.x - viewWidth) * 0.5f);
        float verticalMargin = Mathf.Max(0f, (spriteRenderer.bounds.size.y - viewHeight) * 0.5f);
        position.x = Mathf.Clamp(position.x, cameraPosition.x - horizontalMargin, cameraPosition.x + horizontalMargin);
        position.y = Mathf.Clamp(position.y, cameraPosition.y - verticalMargin, cameraPosition.y + verticalMargin);
        position.z = cameraPosition.z;
        transform.position = position;
    }
}
