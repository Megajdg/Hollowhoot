using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class ReportController : MonoBehaviour
{
    [SerializeField] private GameObject reportPreviewPrefab;
    [SerializeField] private Transform previewContainer;
    [SerializeField] private TextMeshProUGUI reportContentText; // Panel donde mostrar el contenido

    private readonly Dictionary<string, GameObject> loadedReports = new Dictionary<string, GameObject>();

    void Start()
    {
        GeneratePreviews();
    }

    void GeneratePreviews()
    {
        string folder = Path.Combine(Application.streamingAssetsPath, "Reports");
        if (!Directory.Exists(folder))
        {
            Debug.Log("No existe carpeta de reportes");
            return;
        }

        string[] files = Directory.GetFiles(folder, "*.txt");

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileName(filePath);
            if (loadedReports.ContainsKey(fileName)) continue;

            // Crear preview
            GameObject previewObj = Instantiate(reportPreviewPrefab, previewContainer);
            ReportPreview preview = previewObj.GetComponent<ReportPreview>();

            preview.fileName = fileName;
            preview.titleText.text = fileName; // puedes formatear el nombre si quieres

            Button openButton = preview.GetComponent<Button>();
            openButton.onClick.AddListener(() => ShowReport(filePath));

            loadedReports.Add(fileName, previewObj);
        }
    }

    public void ShowReport(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("El archivo no existe: " + path);
            return;
        }

        string content = File.ReadAllText(path);
        reportContentText.text = content;
    }
}