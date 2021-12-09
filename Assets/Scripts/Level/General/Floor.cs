using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    [SerializeField] float openAngle = 90.0f;
    public float timeToOpenOrClose = 1.0f;
    [SerializeField] float timeOpen = 2.0f;

    public void Open()
    {
        StartCoroutine(OpenAndClose());
    }
    IEnumerator OpenAndClose()
    {
        float timer = timeToOpenOrClose;
        Vector3 origRot = transform.rotation.eulerAngles;
        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(openAngle, origRot.y, origRot.z)), 1 - timer / timeToOpenOrClose);
            yield return null;
        }
        yield return new WaitForSeconds(timeOpen);
        origRot = transform.rotation.eulerAngles;
        timer = timeToOpenOrClose;
        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, origRot.y, origRot.z)), 1 - timer / timeToOpenOrClose);
            yield return null;
        }
    }

}
