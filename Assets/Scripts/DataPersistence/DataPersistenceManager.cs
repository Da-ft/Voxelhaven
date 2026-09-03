using System;
using System.IO;
using UnityEngine;

public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager instance { get; private set; }

    [Header("File Storage Config")]
    [SerializeField] private string fileName = "savegame.json";

    private GameData gameData;
    private string filePath;

    public event Action<GameData> OnLoadData;
    public event Action<GameData> OnSaveData;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // "Saves" Folder in Game Folder
        string saveDirectory = Path.Combine(Application.dataPath, "Saves");

        // If no Folder = Create Folder
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        // Final Directory Path
        filePath = Path.Combine(saveDirectory, fileName);
    }

    private void Start()
    {
        LoadGame();
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        if (File.Exists(filePath))
        {
            try
            {
                string dataToLoad = File.ReadAllText(filePath);
                gameData = JsonUtility.FromJson<GameData>(dataToLoad);
                Debug.Log("[DataPersistence] Savegame geladen.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataPersistence] Fehler beim Laden: {e.Message}");
            }
        }

        // Fallback if no savegame exists
        if (gameData == null)
        {
            Debug.Log("[DataPersistence] Kein Savegame gefunden. Inititalisiere New Game.");
            NewGame();
        }

        // Call all registered Scripts
        OnLoadData?.Invoke(gameData);
    }

    public void SaveGame()
    {
        if (gameData == null) return;
        // Call all registered Scripts to save data to 'gamedata'
        OnSaveData?.Invoke(gameData);

        try
        {
            string dataToStore = JsonUtility.ToJson(gameData, true); // Pretty print = true == lesbar!
            File.WriteAllText(filePath, dataToStore);
            Debug.Log("[DataPersistence] Savegame erfolgreich gespeichert.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataPersistence] Fehler beim Speichern: {e.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
