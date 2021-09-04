using UnityEngine;

public class CloseDoorTrigger : MonoBehaviour
{
    [SerializeField] Door door = null;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            door.Close();
            transform.parent.gameObject.SetActive(false);
        }
    }
}