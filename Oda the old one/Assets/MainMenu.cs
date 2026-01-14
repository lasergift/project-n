using UnityEngine;
using UnityEngine.SceneManagement; // Indispensable pour changer de scène

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Charge la scène nommée exactement "SampleScene"
        SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()
    {
        // Ferme l'application (ne fonctionne que dans le jeu buildé, pas dans l'éditeur)
        Debug.Log("Le jeu se ferme...");
        Application.Quit();
    }
}