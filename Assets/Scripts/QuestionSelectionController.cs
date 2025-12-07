using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionSelectionController : MonoBehaviour
{
    [SerializeField] public GameObject previewPrefab;
    [SerializeField] public Transform previewContainer;
    [SerializeField] public TMP_InputField titleText;

    private readonly Dictionary<string, GameObject> loadedQuestions = new Dictionary<string, GameObject>();

    void Start()
    {
        // Si no estamos editando nada, inicializamos un quiz vacío
        if (!QuizManager.Instance.getEditing())
        {
            QuizManager.Instance.currentQuiz = new RootData();
            QuizManager.Instance.currentQuiz.Title = "Título de ejemplo"; // Título inicial
            QuizManager.Instance.setEditing(true);
        }

        GenerateQuestions();

        // Suscribimos el evento para que el título se actualice en tiempo real
        titleText.onValueChanged.AddListener(UpdateQuizTitle);
    }

    private void OnEnable()
    {
        GenerateQuestions();
    }

    void GenerateTitle()
    {
        titleText.text = QuizManager.Instance.currentQuiz.Title;
    }

    public void GenerateQuestions()
    {
        for (int i = 0; i < previewContainer.childCount; i++)
        {
            Transform child = previewContainer.GetChild(i);

            // Si es el objeto para añadir preguntas, lo dejamos
            if (child.gameObject.name == "AddQuestion" || child.gameObject.name == "HollowhootTitle")
                continue;

            Destroy(child.gameObject);
        }

        // Tomamos el cuestionario desde QuizManager si estamos editando
        if (QuizManager.Instance.getEditing())
        {
            RootData quiz = QuizManager.Instance.currentQuiz;

            for (int i = 0; i < quiz.Questions.Count; i++)
            {
                int firstPreviewIndex = 1; // después del título
                int lastPreviewIndex = previewContainer.childCount - 1; // antes del AddQuestion

                GameObject previewObj = Instantiate(previewPrefab, previewContainer);
                previewObj.transform.SetSiblingIndex(firstPreviewIndex + i);
                QuestionPreview preview = previewObj.GetComponent<QuestionPreview>();

                preview.questionText.text = quiz.Questions[i].text;

                preview.questionIndex = i;

                Button editButton = preview.GetComponent<Button>();

                int index = i;
                editButton.onClick.AddListener(() => preview.EditQuestion(quiz.Questions[index]));

                Transform deleteButtonTransform = preview.transform.Find("Question - Layout/Delete - Button");
                if (deleteButtonTransform != null)
                {
                    Button deleteButton = deleteButtonTransform.GetComponent<Button>();
                    if (deleteButton != null)
                    {
                        deleteButton.onClick.AddListener(() => preview.ShowDeleteQuestionPanel(preview, previewObj));
                    }
                }
            }

            GenerateTitle();
        }
    }

    public void AddQuestion()
    {
        RootData quiz = QuizManager.Instance.currentQuiz;

        // Añadir primero el dato al modelo
        var newData = new QuestionData();
        newData.text = "Pregunta de ejemplo";
        quiz.Questions.Add(newData);
        quiz.N_Questions++;

        // El índice de la nueva pregunta es el último
        int newIndex = quiz.Questions.Count - 1;

        // Instanciar el preview
        GameObject previewObj = Instantiate(previewPrefab, previewContainer);
        previewObj.transform.SetSiblingIndex(newIndex + 1); // después del título

        QuestionPreview preview = previewObj.GetComponent<QuestionPreview>();
        preview.questionText.text = newData.text;
        preview.questionIndex = newIndex;

        // Botón de editar
        Button editButton = preview.GetComponent<Button>();
        editButton.onClick.AddListener(() => preview.EditQuestion(quiz.Questions[newIndex]));

        // Botón de borrar
        Transform deleteButtonTransform = preview.transform.Find("Question - Layout/Delete - Button");
        if (deleteButtonTransform != null)
        {
            Button deleteButton = deleteButtonTransform.GetComponent<Button>();
            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener(() => preview.ShowDeleteQuestionPanel(preview, previewObj));
            }
        }
    }

    // Método que se llama cada vez que cambia el texto del InputField
    private void UpdateQuizTitle(string newTitle)
    {
        // Lista de caracteres inválidos en Windows
        char[] invalidChars = Path.GetInvalidFileNameChars();

        // Eliminamos caracteres inválidos
        foreach (char c in invalidChars)
        {
            newTitle = newTitle.Replace(c.ToString(), "");
        }

        // Actualizamos el InputField y el quiz
        titleText.text = newTitle;
        QuizManager.Instance.currentQuiz.Title = newTitle;
    }

    public void SaveJSON()
    {
        RootData quiz = QuizManager.Instance.currentQuiz;

        // No permitir guardar si no hay título
        if (string.IsNullOrWhiteSpace(quiz.Title))
        {
            Debug.LogError("No se puede guardar un quiz sin título.");
            return;
        }

        // Nombre nuevo basado en el título
        string newJsonFileName = quiz.Title + ".json";
        string newJsonPath = Path.Combine(Application.streamingAssetsPath, newJsonFileName);

        string newXmlFileName = quiz.Title + ".xml";
        string newXmlPath = Path.Combine(Application.streamingAssetsPath, newXmlFileName);

        // Comprobar si ya existe otro archivo con ese nombre
        // (y no es el mismo que estamos editando)
        if (File.Exists(newJsonPath) && quiz.OriginalFileName != newJsonFileName)
        {
            Debug.LogError($"Ya existe un quiz con el nombre '{quiz.Title}'. Cambia el título antes de guardar.");
            return;
        }

        // Si ya teníamos un archivo original y el nombre cambió
        if (!string.IsNullOrEmpty(quiz.OriginalFileName) && quiz.OriginalFileName != newJsonFileName)
        {
            string oldJsonPath = Path.Combine(Application.streamingAssetsPath, quiz.OriginalFileName);

            // Renombrar JSON
            if (File.Exists(oldJsonPath))
            {
                File.Move(oldJsonPath, newJsonPath);
            }

            // Renombrar XML si existe
            string oldXmlFileName = Path.ChangeExtension(quiz.OriginalFileName, ".xml");
            string oldXmlPath = Path.Combine(Application.streamingAssetsPath, oldXmlFileName);

            if (File.Exists(oldXmlPath))
            {
                File.Move(oldXmlPath, newXmlPath);
            }
        }

        // Guardamos el JSON en el nuevo path
        string json = JsonUtility.ToJson(quiz, true);
        File.WriteAllText(newJsonPath, json);

        // Actualizamos el nombre original para futuras ediciones
        quiz.OriginalFileName = newJsonFileName;

        UIManager.Instance.ExitHollowhootEditor();
    }
}