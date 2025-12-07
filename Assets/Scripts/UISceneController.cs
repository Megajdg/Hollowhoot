using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISceneController : MonoBehaviour
{
    [SerializeField] private List<GameObject> panels;

    [SerializeField] private List<Button> buttons;

    [SerializeField] public TMP_InputField nameText;

    [SerializeField] public TextMeshProUGUI errorMessageText;

    //Almacena las botones a utilizar
    public readonly Dictionary<string, Button> buttonDictionary = new Dictionary<string, Button>();

    private void Start()
    {
        foreach (var p in panels) UIManager.Instance.RegisterPanel(p);
        foreach (var b in buttons) RegisterButton(b);

        StartMenuSceneButtons();
        StartEditorSceneButtons();
    }

    private void StartMenuSceneButtons()
    {
        if (SceneManager.GetActiveScene().name == "MenuScene")
        {
            #region MainMenuPanel
            buttonDictionary.TryGetValue("Hollowhoots - Button", out var enterHollowhootSelectionButton);
            enterHollowhootSelectionButton.onClick.AddListener(() => UIManager.Instance.EnterHollowhootSelection());
            
            buttonDictionary.TryGetValue("Reports - Button", out var enterReportsButton);
            enterReportsButton.onClick.AddListener(() => UIManager.Instance.EnterReports());

            buttonDictionary.TryGetValue("Exit - Button", out var closeGameButton);
            closeGameButton.onClick.AddListener(() => UIManager.Instance.CloseGame());
            #endregion

            #region SelectionPanel
            buttonDictionary.TryGetValue("Back - Button", out var goBackButton);
            goBackButton.onClick.AddListener(() => UIManager.Instance.GoBackToLastPanel());

            buttonDictionary.TryGetValue("Create - Button", out var enterHollowhootEditorButton);
            enterHollowhootEditorButton.onClick.AddListener(() => UIManager.Instance.EnterHollowhootEditor());

            buttonDictionary.TryGetValue("Cancel - Button", out var cancelHollowhootDeleteButton);
            cancelHollowhootDeleteButton.onClick.AddListener(() => UIManager.Instance.HideDeleteHollowhootConfirmationPanel());
            
            buttonDictionary.TryGetValue("Back Name - Button", out var backNameButton);
            backNameButton.onClick.AddListener(() => UIManager.Instance.HideNamePanel());
            #endregion

            #region ReportPanel
            buttonDictionary.TryGetValue("Back Report - Button", out var backReportButton);
            backReportButton.onClick.AddListener(() => UIManager.Instance.GoBackToLastPanel());
            #endregion

            #region AboutPanel
            buttonDictionary.TryGetValue("About - Button", out var aboutButton);
            aboutButton.onClick.AddListener(() => UIManager.Instance.EnterAbout());

            buttonDictionary.TryGetValue("Back About - Button", out var backAboutButton);
            backAboutButton.onClick.AddListener(() => UIManager.Instance.GoBackToLastPanel());
            #endregion
        }
    }

    private void StartEditorSceneButtons()
    {
        if (SceneManager.GetActiveScene().name == "EditorScene")
        {
            #region QuestionSelectionPanel
            buttonDictionary.TryGetValue("Back - Button", out var exitHollowhootEditorButton);
            exitHollowhootEditorButton.onClick.AddListener(() => UIManager.Instance.ExitHollowhootEditor());

            buttonDictionary.TryGetValue("Cancel Question - Button", out var cancelQuestionDeleteButton);
            cancelQuestionDeleteButton.onClick.AddListener(() => UIManager.Instance.HideDeleteQuestionConfirmationPanel());
            #endregion

            #region QuestionEditorPanel
            buttonDictionary.TryGetValue("BackEditor - Button", out var goBackEditorButton);
            goBackEditorButton.onClick.AddListener(() => UIManager.Instance.GoBackToLastPanel());

            buttonDictionary.TryGetValue("Cancel Answer - Button", out var cancelAnswerDeleteButton);
            cancelAnswerDeleteButton.onClick.AddListener(() => UIManager.Instance.HideDeleteAnswerConfirmationPanel());
            #endregion
        }
    }

    public void RegisterButton(Button button)
    {
        if (button == null)
        {
            Debug.LogError("[UIManager] Botón nulo al registrar.");
            return;
        }

        if (buttonDictionary.ContainsKey(button.name))
        {
            Debug.LogWarning($"[UIManager] Botón duplicado: {button.name}");
            return;
        }

        buttonDictionary.Add(button.name, button);
    }
}
