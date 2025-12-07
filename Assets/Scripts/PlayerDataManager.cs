using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public int playerPoints;

    public PlayerData()
    {
        playerName = "Nombre";
        playerPoints = 0;
    }

    public PlayerData(string nombre, int puntos)
    {
        playerName = nombre;
        playerPoints = puntos;
    }

    public int GetPoints()
    {
        return playerPoints;
    }
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    public TMP_InputField nameText;

    public PlayerData playerData = new PlayerData();

    public bool observing = false;

    private UISceneController controller;

    private void Awake()
    {
        // Patrón Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Se suscribe al evento para detectar nuevos paneles cuando cambia la escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MenuScene")
        {
            controller = FindObjectOfType<UISceneController>();
            nameText = controller.nameText;
        }
    }
}
