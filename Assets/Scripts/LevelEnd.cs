using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEnd : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        //Cursor.visible = true;
        SceneManager.LoadScene("Main Menu");
    }
}