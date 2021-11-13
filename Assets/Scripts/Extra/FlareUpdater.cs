using UnityEngine;
using System.Collections;

public class FlareUpdater : MonoBehaviour
{
    public float size = 3.0f;

    void Update()
    {
        float ratio = Mathf.Sqrt(Vector3.Distance(transform.position, Camera.main.transform.position));

        GetComponent<LensFlare>().brightness = size / ratio;
    }
}
