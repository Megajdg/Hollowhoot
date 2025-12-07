using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardSceneController : MonoBehaviour
{
    private string newPath, newFileName;
    private List<PlayerData> playerData = new List<PlayerData>();

    [SerializeField] GameObject scorePrefab;
    [SerializeField] Transform scoreContainer;
    [SerializeField] TextMeshProUGUI quizTitleText;
    [SerializeField] Button exitButton;

    private static HashSet<string> errorFiles = new HashSet<string>();

    void Start()
    {
        RootData quiz = QuizManager.Instance.currentQuiz;

        quizTitleText.text = quiz.Title + " - Clasificación";

        newFileName = quiz.Title + ".xml";
        newPath = Path.Combine(Application.streamingAssetsPath, newFileName);

        if (!PlayerDataManager.Instance.observing)
        {
            if (!File.Exists(newPath))
            {
                SaveXML();
            }
            else
            {
                AddScore();
            }

            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(UIManager.Instance.ExitLeaderboard);
        }
        else
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(UIManager.Instance.ExitLeaderboardObserving);
        }
        
        LoadXML();

        playerData = playerData.OrderByDescending(points => points.GetPoints()).ToList();

        CreatePreviews();
    }

    private void SaveXML()
    {
        XmlDocument xmlDoc = new XmlDocument();

        XmlElement root = xmlDoc.CreateElement("Jugadores");
        xmlDoc.AppendChild(root);

        XmlElement jugador = xmlDoc.CreateElement("Jugador");
        root.AppendChild(jugador);

        XmlElement nombre = xmlDoc.CreateElement("Nombre");
        nombre.InnerText = PlayerDataManager.Instance.playerData.playerName;
        jugador.AppendChild(nombre);

        XmlElement points = xmlDoc.CreateElement("Puntos");
        points.InnerText = PlayerDataManager.Instance.playerData.playerPoints.ToString();
        jugador.AppendChild(points);

        xmlDoc.Save(newPath);
    }

    private void AddScore()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(newPath);

        XmlElement root = xmlDoc.DocumentElement;

        XmlElement jugador = xmlDoc.CreateElement("Jugador");
        root.AppendChild(jugador);

        XmlElement nombre = xmlDoc.CreateElement("Nombre");
        nombre.InnerText = PlayerDataManager.Instance.playerData.playerName;
        jugador.AppendChild(nombre);

        XmlElement points = xmlDoc.CreateElement("Puntos");
        points.InnerText = PlayerDataManager.Instance.playerData.playerPoints.ToString();
        jugador.AppendChild(points);

        xmlDoc.Save(newPath);
    }

    private void LoadXML()
    {
        // Si no existe el archivo, lo creamos vacío y salimos sin generar reporte
        if (!File.Exists(newPath))
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement("Jugadores");
            xmlDoc.AppendChild(root);
            xmlDoc.Save(newPath);

            Debug.Log($"Archivo XML creado vacío: {newPath}");
            return;
        }

        try
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(newPath);

            XmlNodeList jugadores = xmlDoc.GetElementsByTagName("Jugador");

            foreach (XmlNode data in jugadores)
            {
                string nombre = data["Nombre"].InnerText;
                int puntos = int.Parse(data["Puntos"].InnerText);
                playerData.Add(new PlayerData(nombre, puntos));
            }
        }
        catch (ArgumentException ex)
        {
            GenerateXmlReport(ex, newFileName);
        }
        catch (SystemException ex)
        {
            GenerateXmlReport(ex, newFileName);
        }
    }

    private void GenerateXmlReport(Exception ex, string fileName)
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

        string reportPath = Path.Combine(errorPath, reportFileName);

        string reportContent =
            "Informe de error de Hollowhoot\n" +
            "---------------------------------\n" +
            $"Fecha y hora: {System.DateTime.Now}\n" +
            $"Archivo: {fileName}\n" +
            "Descripción: Error al leer el archivo XML. Formato incorrecto o archivo corrupto.\n" +
            $"Detalles técnicos: {ex.Message}\n";

        File.WriteAllText(reportPath, reportContent);

        Debug.LogError($"Error al leer {fileName}. Informe XML generado en {reportPath}");
    }

    private void CreatePreviews()
    {
        for (int i = 0; i < playerData.Count; i++)
        {
            GameObject preview = Instantiate(scorePrefab, scoreContainer);
            ScorePreview scorePreview = preview.GetComponent<ScorePreview>();

            int index = i + 1;

            scorePreview.positionText.text = index.ToString() + ".";
            scorePreview.nameText.text = playerData[i].playerName;
            scorePreview.scoreText.text = playerData[i].playerPoints.ToString();
        }
    }
}
