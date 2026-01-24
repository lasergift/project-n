using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panneaux Menu")]
    public GameObject mainPanel; // Glisse l'objet "MainButtons" ici
    public GameObject slotPanel; // Glisse l'objet "SlotButtons" ici

    [Header("Configuration des Slots")]
    public Text[] slotTexts; 

    void Start()
    {
        // On s'assure que le menu principal est visible et les slots cachés au début
        mainPanel.SetActive(true);
        slotPanel.SetActive(false);
        RefreshSlotsVisual();
    }

    // --- LOGIQUE DE NAVIGATION ---

    public void OpenSlots()
    {
        mainPanel.SetActive(false); // Cache Play/Quit
        slotPanel.SetActive(true);  // Affiche les 3 Slots
        RefreshSlotsVisual();       // Met à jour les textes New Game / Continue
    }

    public void CloseSlots()
    {
        mainPanel.SetActive(true);  // Affiche Play/Quit
        slotPanel.SetActive(false); // Cache les Slots
    }

    // --- LOGIQUE DE SAUVEGARDE (Inchangée) ---

    public void RefreshSlotsVisual()
    {
        for (int i = 0; i < 3; i++)
        {
            PlayerData data = SaveSystem.Load(i);
            if (data == null) slotTexts[i].text = "New Game";
            else slotTexts[i].text = "Slot " + (i + 1) + " - " + data.lastSceneName;
        }
    }
    // Ajoute cette fonction dans MainMenu.cs
public void DeleteSlot(int index)
{
    // 1. Supprimer le fichier via le SaveSystem
    SaveSystem.DeleteSave(index);
    
    // 2. Si on supprime le slot sur lequel on jouait, on nettoie le PlayerPrefs
    if (PlayerPrefs.GetInt("CurrentSlot") == index)
    {
        PlayerPrefs.DeleteKey("CurrentSlot");
    }

    // 3. Mettre à jour l'affichage immédiatement
    RefreshSlotsVisual();
    
    Debug.Log("Sauvegarde du Slot " + index + " supprimée.");
}

    public void SelectSlot(int index)
    {
        PlayerPrefs.SetInt("CurrentSlot", index);
        PlayerData data = SaveSystem.Load(index);

if (data == null)
{
    PlayerData newData = new PlayerData();
    newData.slotIndex = index;
    newData.lastSceneName = "SampleScene"; 
    
    // REMPLACE PAR TES COORDONNÉES RÉELLES (regarde dans l'Inspector de ton Player)
    newData.x = -25.745f; 
    newData.y = -2.318f; 

    SaveSystem.Save(newData, index);
    SceneManager.LoadScene(newData.lastSceneName);
}
    }

    public void QuitGame() => Application.Quit();
}