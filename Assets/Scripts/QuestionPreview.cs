using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;

public class QuestionPreview : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI questionText;

    [HideInInspector] public string fileName;

    [HideInInspector] public int questionIndex;

    private QuestionEditorController questionEditorController;

    private QuestionData currentData;

    private UISceneController controller;

    private void Start()
    {
        questionEditorController = FindObjectOfType<QuestionEditorController>(true);

        controller = FindObjectOfType<UISceneController>();
    }

    public void EditQuestion(QuestionData data)
    {
        UIManager.Instance.EnterQuestionEditor();

        currentData = data;

        // Limpiar listeners previos
        questionEditorController.questionText.onValueChanged.RemoveAllListeners();
        questionEditorController.timerText.onValueChanged.RemoveAllListeners();
        foreach (var input in questionEditorController.answerTexts)
            input.onValueChanged.RemoveAllListeners();
        foreach (var toggle in questionEditorController.answerToggles)
            toggle.onValueChanged.RemoveAllListeners();
        foreach (var button in questionEditorController.answerDeleteButtons)
            button.onClick.RemoveAllListeners();

        questionEditorController.questionText.text = data.text;
        // Suscribimos el evento para que el título se actualice en tiempo real
        questionEditorController.questionText.onValueChanged.AddListener(UpdateQuestionText);

        questionEditorController.timerText.text = data.timer.ToString();
        questionEditorController.timerText.onValueChanged.AddListener(UpdateTimerText);

        for (int i = 0; i < questionEditorController.answers.Count; i++) {
            if (i < data.answers.Count)
            {
                questionEditorController.answers[i].SetActive(true);
                questionEditorController.answerTexts[i].text = data.answers[i];
                int index = i;
                questionEditorController.answerTexts[i].onValueChanged.AddListener((string value) => UpdateAnswerText(index, value));
                questionEditorController.answerDeleteButtons[i].onClick.AddListener(() => ShowDeleteAnswerPanel(index));
            }
            else
            {
                questionEditorController.answers[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < questionEditorController.answerToggles.Count; i++)
        {
            int index = i;

            // Estado inicial
            questionEditorController.answerToggles[i].SetIsOnWithoutNotify(data.CorrectAnswer == i);

            // Listener
            questionEditorController.answerToggles[i].onValueChanged.AddListener((bool value) => UpdateCorrectAnswerToggle(index, value));
        }

        // Suscribir el botón de añadir respuesta a este preview
        questionEditorController.addAnswerButton.onClick.RemoveAllListeners();
        questionEditorController.addAnswerButton.onClick.AddListener(() => AddAnswer());
    }
    public void ShowDeleteQuestionPanel(QuestionPreview preview, GameObject previewObj)
    {
        UIManager.Instance.ShowDeleteQuestionConfirmationPanel();

        controller.buttonDictionary.TryGetValue("Confirm Question - Button", out var confirmQuestionDeleteButton);
        confirmQuestionDeleteButton.onClick.RemoveAllListeners();
        confirmQuestionDeleteButton.onClick.AddListener(() => DeleteQuestion(preview, previewObj));
    }

    public void DeleteQuestion(QuestionPreview preview, GameObject previewObj)
    {
        // No permitir borrar si solo queda una pregunta
        if (QuizManager.Instance.currentQuiz.Questions.Count <= 1)
        {
            Debug.Log("No se puede borrar la última pregunta, debe quedar al menos una.");
            UIManager.Instance.HideDeleteQuestionConfirmationPanel();
            return;
        }

        int siblingIndex = previewObj.transform.GetSiblingIndex();
        int firstPreviewIndex = 1; // después del título
        int index = siblingIndex - firstPreviewIndex;

        if (index >= 0 && index < QuizManager.Instance.currentQuiz.Questions.Count)
        {
            QuizManager.Instance.currentQuiz.Questions.RemoveAt(index);
            QuizManager.Instance.currentQuiz.N_Questions--;
        }

        Destroy(previewObj);

        // Regenerar la lista para reindexar todos los previews
        FindObjectOfType<QuestionSelectionController>().GenerateQuestions();

        UIManager.Instance.HideDeleteQuestionConfirmationPanel();
    }

    public void AddAnswer()
    {
        int index = currentData.answers.Count;

        // ¿Quedan slots disponibles?
        if (index < 4)
        {
            // Activamos el siguiente slot
            questionEditorController.answers[index].SetActive(true);

            // Inicializamos texto vacío
            questionEditorController.answerTexts[index].text = "Respuesta de ejemplo";
            currentData.answers.Add("Respuesta de ejemplo");

            // Suscribimos eventos
            questionEditorController.answerTexts[index].onValueChanged.RemoveAllListeners();
            questionEditorController.answerTexts[index].onValueChanged.AddListener(
                (string value) => UpdateAnswerText(index, value)
            );

            questionEditorController.answerToggles[index].onValueChanged.RemoveAllListeners();
            questionEditorController.answerToggles[index].onValueChanged.AddListener(
                (bool value) => UpdateCorrectAnswerToggle(index, value)
            );

            questionEditorController.answerDeleteButtons[index].onClick.RemoveAllListeners();
            questionEditorController.answerDeleteButtons[index].onClick.AddListener(() => ShowDeleteAnswerPanel(index));
        }
    }

    public void ShowDeleteAnswerPanel(int index)
    {
        UIManager.Instance.ShowDeleteAnswerConfirmationPanel();

        controller.buttonDictionary.TryGetValue("Confirm Answer - Button", out var confirmAnswerDeleteButton);
        confirmAnswerDeleteButton.onClick.RemoveAllListeners();
        confirmAnswerDeleteButton.onClick.AddListener(() => DeleteAnswer(index));
    }

    public void DeleteAnswer(int index)
    {
        if (index < 0 || index >= currentData.answers.Count) return;

        // No permitir borrar si solo queda una respuesta
        if (currentData.answers.Count <= 2  )
        {
            Debug.Log("No se puede borrar la última respuesta, deben quedar al menos dos.");
            UIManager.Instance.HideDeleteAnswerConfirmationPanel();
            return;
        }

        // Eliminar dato
        currentData.answers.RemoveAt(index);

        // Ajustar índice de la respuesta correcta
        if (currentData.CorrectAnswer == index)
        {
            // Si borras la correcta, mueve la correcta al mismo índice (o al anterior si era el último)
            if (currentData.answers.Count > 0)
                currentData.CorrectAnswer = Math.Min(index, currentData.answers.Count - 1);
            else
                currentData.CorrectAnswer = -1; // sin respuestas
        }
        else if (currentData.CorrectAnswer > index)
        {
            // Si la correcta estaba después, desplázala una posición hacia arriba
            currentData.CorrectAnswer--;
        }

        UIManager.Instance.HideDeleteAnswerConfirmationPanel();

        // Repintar todo para reindexar UI + listeners
        RefreshAnswers();
    }

    private void RefreshAnswers()
    {
        // Re-dibuja y re-suscribe todo desde los datos actuales
        for (int i = 0; i < questionEditorController.answers.Count; i++)
        {
            // Limpieza previa de listeners
            questionEditorController.answerTexts[i].onValueChanged.RemoveAllListeners();
            questionEditorController.answerToggles[i].onValueChanged.RemoveAllListeners();

            if (i < currentData.answers.Count)
            {
                // Activar slot y pintar datos
                questionEditorController.answers[i].SetActive(true);
                questionEditorController.answerTexts[i].text = currentData.answers[i];

                // Toggle correcto
                bool isCorrect = currentData.CorrectAnswer == i;
                questionEditorController.answerToggles[i].SetIsOnWithoutNotify(isCorrect);

                int idx = i;
                // Re-suscribir listeners con índice actualizado
                questionEditorController.answerTexts[i].onValueChanged.AddListener(
                    (string value) => UpdateAnswerText(idx, value)
                );
                questionEditorController.answerToggles[i].onValueChanged.AddListener(
                    (bool value) => UpdateCorrectAnswerToggle(idx, value)
                );
            }
            else
            {
                // Desactivar slot y limpiar
                questionEditorController.answers[i].SetActive(false);
                questionEditorController.answerTexts[i].text = "";
                questionEditorController.answerToggles[i].SetIsOnWithoutNotify(false);
            }
        }
    }

    private void UpdateQuestionText(string newText)
    {
        questionEditorController.questionText.textComponent.enableWordWrapping = true;

        // Actualizamos el InputField y el quiz
        questionEditorController.questionText.text = newText;
        currentData.text = newText;
    }

    private void UpdateTimerText(string newTime)
    {
        // Validar: no permitir "0"
        if (newTime.Trim() == "0" || newTime.Trim() == "00")
        {
            Debug.LogWarning("El tiempo no puede ser 0.");
            questionEditorController.timerText.text = currentData.timer.ToString();
            return;
        }

        // Actualizamos el InputField y el quiz
        questionEditorController.timerText.text = newTime;
        currentData.timer = Convert.ToInt32(newTime);
    }

    private void UpdateAnswerText(int index, string newText)
    {
        questionEditorController.answerTexts[index].textComponent.enableWordWrapping = true;

        // Actualizamos el InputField y el quiz
        questionEditorController.answerTexts[index].text = newText;
        currentData.answers[index] = newText;
    }

    private void UpdateCorrectAnswerToggle(int index, bool active)
    {
        if (!active)
        {
            // Si intentan apagar el toggle que está marcado como correcto, lo volvemos a encender
            if (currentData.CorrectAnswer == index)
            {
                questionEditorController.answerToggles[index].SetIsOnWithoutNotify(true);
            }
            return;
        }

        // Apaga el resto
        for (int i = 0; i < questionEditorController.answerToggles.Count; i++)
        {
            if (i == index) continue;
            questionEditorController.answerToggles[i].SetIsOnWithoutNotify(false);
        }

        currentData.CorrectAnswer = index;
    }
}
