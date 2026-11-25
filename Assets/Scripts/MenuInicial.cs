using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicial : MonoBehaviour
{
    public void Jugar(){
        SceneManager.LoadScene("MapaNuevo");
    }

    public void Salir(){
        Debug.Log("Salir...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // en el editor, Quit no hace nada
#endif
    }

}
