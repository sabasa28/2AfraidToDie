using UnityEngine;

public class FlareUpdater : MonoBehaviour
{
    public float size = 3.0f;

    void Update()
    {
        float ratio = 0.0f;
        if (Camera.main) ratio = Mathf.Sqrt(Vector3.Distance(transform.position, Camera.main.transform.position));

        if (TryGetComponent(out LensFlare lensFlare)) lensFlare.brightness = size / ratio;
    }
}