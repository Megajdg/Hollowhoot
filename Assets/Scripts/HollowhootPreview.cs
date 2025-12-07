using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HollowhootPreview : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI titleText;
    [SerializeField] public TextMeshProUGUI nQuestionsText;
    [SerializeField] public Image backgroundImage;
    [SerializeField] public GameObject deleteConfirmationPanelPrefab;

    [HideInInspector] public string fileName;

    private UISceneController controller;

    private void Start()
    {
        controller = FindObjectOfType<UISceneController>();
    }

    public void ShowNamePanel(RootData quizData)
    {
        UIManager.Instance.ShowNamePanel();

        PlayerDataManager.Instance.nameText.onValueChanged.RemoveAllListeners();
        PlayerDataManager.Instance.nameText.onValueChanged.AddListener(RefreshName);
        PlayerDataManager.Instance.nameText.onEndEdit.AddListener(SaveName);

        PlayerDataManager.Instance.nameText.text = PlayerPrefs.GetString("PlayerName");

        controller.buttonDictionary.TryGetValue("Start - Button", out var startNameButton);
        startNameButton.onClick.RemoveAllListeners();
        startNameButton.onClick.AddListener(() => StartQuiz(quizData));
    }

    public void RefreshName(string newText)
    {
        PlayerDataManager.Instance.playerData.playerName = newText;
    }

    public void SaveName(string newText)
    {
        PlayerPrefs.SetString("PlayerName", newText);
        PlayerPrefs.Save();
    }

    public void StartQuiz(RootData quizData)
    {
        QuizManager.Instance.currentQuiz = quizData;
        SceneManager.LoadScene("GameScene");
    }

    public void ShowQuizLeaderboard(RootData quizData)
    {
        QuizManager.Instance.currentQuiz = quizData;
        PlayerDataManager.Instance.observing = true;
        SceneManager.LoadScene("LeaderboardScene");
    }

    public void EditQuiz(RootData quizData)
    {
        QuizManager.Instance.currentQuiz = quizData;
        QuizManager.Instance.setEditing(true);
        UIManager.Instance.EnterHollowhootEditor(true);
    }

    public void ShowDeleteQuizPanel()
    {
        UIManager.Instance.ShowDeleteHollowhootConfirmationPanel();

        controller.buttonDictionary.TryGetValue("Confirm - Button", out var confirmHollowhootDeleteButton);
        confirmHollowhootDeleteButton.onClick.RemoveAllListeners();
        confirmHollowhootDeleteButton.onClick.AddListener(() => DeleteQuiz());
    }

    public void DeleteQuiz()
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Archivo borrado: " + fileName);
        }
        else
        {
            Debug.LogWarning("El archivo no existe: " + fileName);
        }

        UIManager.Instance.HideDeleteHollowhootConfirmationPanel();
    }
}
