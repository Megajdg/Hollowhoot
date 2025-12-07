using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la interfaz de usuario del juego, controlando que pantallas se muestran u ocultan.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    //Almacena las pantallas a mostrar y ocultar
    private readonly Dictionary<string, GameObject> panelDictionary = new Dictionary<string, GameObject>();

    private readonly Stack<string> panelHistory = new Stack<string>();

    private bool loadingFromEditor = false;

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
        // Limpia los paneles previos
        panelDictionary.Clear();

        StartCoroutine(InitializePanelsNextFrame(scene.name));
    }

    private IEnumerator InitializePanelsNextFrame(string sceneName)
    {
        yield return null;

        if (sceneName == "MenuScene" && loadingFromEditor == false || sceneName == "MenuScene" && PlayerDataManager.Instance.observing == false)
        {
            SetAllPanelsToFalse();
            SetPanel("Main Menu - Panel", true);
        }
        else if (sceneName == "MenuScene")
        {
            SetAllPanelsToFalse();
            SetPanel("Main Menu - Panel", true);
            SetPanel("Hollowhoot Selection - Panel", true);
        }
        else if (sceneName == "GameScene")
        {
            SetAllPanelsToFalse();
            SetPanel("Game - Panel", true);
        }
        else if (sceneName == "EditorScene")
        {
            SetAllPanelsToFalse();
            SetPanel("Question Selection - Panel", true);
        }
    }

    public void RegisterPanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogError("[UIManager] Panel nulo al registrar.");
            return;
        }

        if (panelDictionary.ContainsKey(panel.name))
        {
            Debug.LogWarning($"[UIManager] Panel duplicado: {panel.name}");
            return;
        }

        panelDictionary.Add(panel.name, panel);
    }

    /// <summary>
    /// Se utiliza para mostrar u ocultar una pantalla en la UI según su nombre.
    /// </summary>
    /// <param name="panel">Variable de la pantalla que queremos mostrar u ocultar (tenemos que poner el nombre EXACTO 
    /// que le hemos puesto al GameObject).</param>
    /// <param name="active">Variable para indicar si queremos que active la pantalla (true) o la desactive (false).</param>
    public void SetPanel(string panel, bool active)
    {
        //Si no consigue encontrar la pantalla en el diccionario o es nula, devuelve un error
        if (!panelDictionary.TryGetValue(panel, out var actualPanel) || actualPanel == null)
        {
            Debug.LogError($"[UIManager] El panel '{panel}' no fue encontrado en el diccionario.");
            return;
        }

        // Buscar cuál está activo ahora y guardarlo en el historial
        foreach (var kvp in panelDictionary)
        {
            if (kvp.Value.activeSelf)
            {
                    panelHistory.Push(kvp.Key);
                    kvp.Value.SetActive(false);
            }
        }

        //Activa la pantalla indicada
        actualPanel.SetActive(active);
        if (active)
        {
            Debug.Log($"Pantalla activada: {actualPanel}");
        }
        else
        {
            Debug.Log($"Pantalla desactivada: {actualPanel}");
        }
    }

    public void SetAllPanelsToFalse()
    {
        foreach (var p in panelDictionary.Values) p.SetActive(false);
    }

    /// <summary>
    /// Se utiliza para obtener el estado de una pantalla de la UI en concreto.
    /// </summary>
    /// <param name="panel">Variable de la pantalla la cual queremos saber su estado (tenemos que poner el nombre EXACTO 
    /// que le hemos puesto al GameObject).</param>
    /// <returns>El estado de la pantalla que hemos pedido.</returns>
    public bool IsPanelActive(string panel)
    {
        //Devuelve el estado de la pantalla que se ha pedido siempre que no sea nula
        return panelDictionary.TryGetValue(panel, out var actualPanel) && actualPanel != null && actualPanel.activeSelf;
    }

    #region buttons

    public void EnablePanel(GameObject panel)
    {
        SetPanel(panel.name, true);
    }

    public void EnterHollowhootSelection()
    {
        SetPanel("Hollowhoot Selection - Panel", true);
    }

    public void EnterHollowhootEditor(bool editing = false)
    {
        if (!editing)
        {
            QuizManager.Instance.setEditing(false);
            QuizManager.Instance.currentQuiz = null;
        }

        SceneManager.LoadScene("EditorScene");
    }

    public void EnterReports()
    {
        SetPanel("Report - Panel", true);
    }

    public void ExitLeaderboard()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void ExitHollowhootEditor()
    {
        SceneManager.LoadScene("MenuScene");
        loadingFromEditor = true;
    }
    
    public void ExitLeaderboardObserving()
    {
        PlayerDataManager.Instance.observing = false;
        SceneManager.LoadScene("MenuScene");
    }

    public void ShowDeleteHollowhootConfirmationPanel()
    {
        panelDictionary.TryGetValue("Delete Confirmation - Panel", out var actualPanel);
        actualPanel.SetActive(true);
    }
    
    public void HideDeleteHollowhootConfirmationPanel()
    {
        panelDictionary.TryGetValue("Delete Confirmation - Panel", out var actualPanel);
        actualPanel.SetActive(false);
    }

    public void ShowNamePanel()
    {
        panelDictionary.TryGetValue("Name - Panel", out var actualPanel);
        actualPanel.SetActive(true);
    }
    
    public void HideNamePanel()
    {
        panelDictionary.TryGetValue("Name - Panel", out var actualPanel);
        actualPanel.SetActive(false);
    }

    public void ShowDeleteQuestionConfirmationPanel()
    {
        panelDictionary.TryGetValue("Delete Confirmation Question - Panel", out var actualPanel);
        actualPanel.SetActive(true);
    }

    public void HideDeleteQuestionConfirmationPanel()
    {
        panelDictionary.TryGetValue("Delete Confirmation Question - Panel", out var actualPanel);
        actualPanel.SetActive(false);
    }

    public void ShowDeleteAnswerConfirmationPanel()
    {
        panelDictionary.TryGetValue("Delete Confirmation Answer - Panel", out var actualPanel);
        actualPanel.SetActive(true);
    }

    public void HideDeleteAnswerConfirmationPanel()
    {
        panelDictionary.TryGetValue("Delete Confirmation Answer - Panel", out var actualPanel);
        actualPanel.SetActive(false);
    }

    public void EnterQuestionEditor()
    {
        SetPanel("Question Editor - Panel", true);
    }

    public void GoBackToLastPanel()
    {
        if (panelHistory.Count > 0)
        {
            string lastPanel = panelHistory.Pop();
            SetAllPanelsToFalse();
            SetPanel(lastPanel, true);
        }
        else
        {
            Debug.LogWarning("[UIManager] No hay panel anterior en el historial.");
        }
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    #endregion
}