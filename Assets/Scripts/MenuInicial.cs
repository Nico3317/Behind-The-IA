using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicial : MonoBehaviour
{
    public void Jugar(){
        SceneManager.LoadScene("test_player");
    }

    public void Salir(){
        Debug.Log("Salir...");
        Application.Quit();
    }

}
