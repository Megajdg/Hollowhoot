using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class QuestionData
{
    public string text;
    public int timer;
    public List<string> answers;
    public int CorrectAnswer;

    public QuestionData() {
        this.text = "Pregunta de ejemplo";
        this.timer = 1;
        this.answers = new List<string>();
        this.answers.Add("Respuesta de ejemplo");
        this.answers.Add("Respuesta de ejemplo");
        this.CorrectAnswer = 0;
    }
}

[System.Serializable]
public class RootData
{
    public string Title;
    public int N_Questions;
    public List<QuestionData> Questions;

    [System.NonSerialized]
    public string OriginalFileName;
    public RootData()
    {
        this.Title = "Título de ejemplo";
        this.N_Questions = 1;
        this.Questions = new List<QuestionData>();
        this.Questions.Add(new QuestionData());
    }
}

public class HollowhootSelectionController : MonoBehaviour
{
    [SerializeField] public GameObject previewPrefab;
    [SerializeField] public Transform previewContainer;
    public float checkInterval = 5f;

    private Sprite[] backgrounds;
    private readonly Dictionary<string, GameObject> loadedPreviews = new Dictionary<string, GameObject>();

    private static HashSet<string> errorFiles = new HashSet<string>();

    void Start()
    {
        backgrounds = Resources.LoadAll<Sprite>("Hollowhoot Previews");

        InvokeRepeating(nameof(GeneratePreviews), 0f, checkInterval);
    }

    void GeneratePreviews()
    {
        string folder = UnityEngine.Application.streamingAssetsPath;
        string[] files = Directory.GetFiles(folder, "*.json");

        HashSet<string> currentFiles = new HashSet<string>();
        foreach (string file in files) currentFiles.Add(Path.GetFileName(file));
        
        foreach (string fileName in currentFiles)
        {
            if (loadedPreviews.ContainsKey(fileName)) continue;

            string path = Path.Combine(folder, fileName);

            try
            {
                string json = File.ReadAllText(path);
                RootData data = JsonUtility.FromJson<RootData>(json);

                data.OriginalFileName = fileName;

                GameObject previewObj = Instantiate(previewPrefab, previewContainer);
                HollowhootPreview preview = previewObj.GetComponent<HollowhootPreview>();

                preview.titleText.text = data.Title;
                preview.nQuestionsText.text = data.N_Questions.ToString();
                preview.fileName = fileName;

                Button startButton = preview.GetComponent<Button>();

                RootData quizCopy = data;
                startButton.onClick.AddListener(() => preview.ShowNamePanel(quizCopy));

                Transform leaderboardTransform = preview.transform.Find("Hollowhoot - Layout/Leaderboard - Button");
                if (leaderboardTransform != null)
                {
                    Button leaderboardButton = leaderboardTransform.GetComponent<Button>();
                    if (leaderboardButton != null)
                    {
                        leaderboardButton.onClick.AddListener(() => preview.ShowQuizLeaderboard(quizCopy));
                    }
                }

                Transform editButtonTransform = preview.transform.Find("Hollowhoot - Layout/Hollowhoot - Buttons/Edit - Button");
                if (editButtonTransform != null)
                {
                    Button editButton = editButtonTransform.GetComponent<Button>();
                    if (editButton != null)
                    {
                        editButton.onClick.AddListener(() => preview.EditQuiz(quizCopy));
                    }
                }

                int index = Mathf.Abs(fileName.GetHashCode()) % backgrounds.Length;
                preview.backgroundImage.sprite = backgrounds[index];

                loadedPreviews.Add(fileName, previewObj);
            }
            catch (ArgumentException ex) 
            {
                GenerateReport(ex, fileName);
            }
            catch (SystemException ex)
            {
                GenerateReport(ex, fileName);
            }
        }

        List<string> toRemove = new List<string>();
        foreach (var kvp in loadedPreviews)
        {
            if (!currentFiles.Contains(kvp.Key))
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);

                string xmlFileName = Path.ChangeExtension(kvp.Key, ".xml");
                string xmlPath = Path.Combine(folder, xmlFileName);
                Debug.Log(xmlPath);
                if (File.Exists(xmlPath))
                {
                    File.Delete(xmlPath);
                }
            }
        }

        foreach (string key in toRemove) loadedPreviews.Remove(key);
    }

    private void GenerateReport(Exception ex, string fileName)
    {
        if (errorFiles.Contains(fileName))
            return; // ya se generó informe para este archivo

        errorFiles.Add(fileName);

        // Crear informe de error
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string reportFileName = $"ErrorReport_{timestamp}.txt";
        string errorPath = UnityEngine.Application.streamingAssetsPath + "/Reports";

        if (!Directory.Exists(errorPath))
        {
            Directory.CreateDirectory(errorPath);
        }
        else Debug.Log("La carpeta ya existe!");

        string reportPath = Path.Combine(errorPath, reportFileName);

        string reportContent =
            "Informe de error de Hollowhoot\n" +
            "---------------------------------\n" +
            $"Fecha y hora: {System.DateTime.Now}\n" +
            $"Archivo: {fileName}\n" +
            "Descripción: Error al leer el archivo JSON. Formato incorrecto o archivo corrupto.\n" +
            $"Detalles técnicos: {ex.Message}\n";

        File.WriteAllText(reportPath, reportContent);

        Debug.LogError($"Error al leer {fileName}. Informe generado en {reportPath}");
    }
}
