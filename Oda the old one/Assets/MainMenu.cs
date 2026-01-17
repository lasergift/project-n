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

    public void SelectSlot(int index)
    {
        PlayerPrefs.SetInt("CurrentSlot", index);
        PlayerData data = SaveSystem.Load(index);

        if (data == null)
        {
            PlayerData newData = new PlayerData();
            newData.slotIndex = index;
            newData.lastSceneName = "SampleScene"; 
            SaveSystem.Save(newData, index);
            SceneManager.LoadScene(newData.lastSceneName);
        }
        else
        {
            SceneManager.LoadScene(data.lastSceneName);
        }
    }

    public void QuitGame() => Application.Quit();
}