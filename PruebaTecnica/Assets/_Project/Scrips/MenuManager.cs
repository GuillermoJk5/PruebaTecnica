using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // ==========================================
    // PUBLIC METHODS (BUTTON TRIGGERS)
    // ==========================================

   
    public void Jugar()
    {
        // Carga la escena que está en la posición 1 del Build Settings (tu juego)
        SceneManager.LoadScene(1);
    }

   
    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
