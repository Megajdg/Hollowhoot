using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;

public class GameSceneController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] List<Button> answerButtons;

    [SerializeField] private TextMeshProUGUI questionTime;
    [SerializeField] private Image timerImage;
    [SerializeField] private TextMeshProUGUI streakText;

    [SerializeField] private TextMeshProUGUI questionStateText;
    [SerializeField] private TextMeshProUGUI questionCorrectAnswerText;
    [SerializeField] private TextMeshProUGUI newPointsText;
    [SerializeField] private TextMeshProUGUI totalPointsText;
    [SerializeField] private TextMeshProUGUI nameText;

    private float timeRemaining;
    private bool timerRunning;
    private float totalTime;
    private int streak = 0;

    private int newPoints = 0;
    private int totalPoints = 0;

    private List<int> questionsDone = new List<int>();

    private QuestionData currentQuestion;
    private List<string> shuffledAnswers;
    private int correctAnswerIndex;

    void Start()
    {
        // Tomamos el cuestionario desde QuizManager
        RootData quiz = QuizManager.Instance.currentQuiz;

        // Elegimos una pregunta aleatoria
        int index = UnityEngine.Random.Range(0, quiz.Questions.Count);
        currentQuestion = quiz.Questions[index];

        // Guardar que ya se ha hecho
        questionsDone.Add(index);

        // Mostramos la pregunta
        questionText.text = currentQuestion.text;

        timeRemaining = currentQuestion.timer;
        totalTime = timeRemaining;
        timerRunning = true;

        // Copiar y barajar respuestas
        shuffledAnswers = new List<string>(currentQuestion.answers);
        ShuffleList(shuffledAnswers);

        // Averiguar dónde quedó la correcta tras barajar
        string correctAnswerText = currentQuestion.answers[currentQuestion.CorrectAnswer];
        correctAnswerIndex = shuffledAnswers.IndexOf(correctAnswerText);

        // Activar solo los botones necesarios
        for (int i = 0; i < answerButtons.Count; i++)
        {
            if (i < shuffledAnswers.Count)
            {
                answerButtons[i].gameObject.SetActive(true);

                string answer = shuffledAnswers[i];
                answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = answer;

                answerButtons[i].onClick.RemoveAllListeners();
                int capturedIndex = i;
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(capturedIndex));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
        nameText.text = PlayerPrefs.GetString("PlayerName");
    }
    void Update()
    {
        if (timerRunning)
        {
            timeRemaining -= Time.deltaTime; // resta segundos reales

            timerImage.fillAmount = timeRemaining / totalTime;

            if (timeRemaining <= 0)
            {
                OnAnswerSelected();
            }

            // Mostrar el tiempo como número entero
            questionTime.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }

    void OnAnswerSelected(int index = -1)
    {
        UIManager.Instance.SetPanel("Game - Panel", false);
        UIManager.Instance.SetPanel("QuestionResult - Panel", true);

        timerRunning = false;
        

        if (index == correctAnswerIndex)
        {
            float questionPoints = Mathf.Lerp(0, 1000, timeRemaining / totalTime);
            newPoints = Convert.ToInt32(questionPoints);
            totalPoints += newPoints;

            questionStateText.text = "¡CORRECTO!";
            questionCorrectAnswerText.text = "";
            streak++;
        }
        else if (index == -1)
        {
            newPoints = 0;
            questionStateText.text = "¡TIEMPO AGOTADO!";
            questionCorrectAnswerText.text = $"La respuesta correcta era: {shuffledAnswers[correctAnswerIndex]}";
            streak = 0;
        }
        else
        {
            newPoints = 0;
            questionStateText.text = "¡INCORRECTO!";
            questionCorrectAnswerText.text = $"La respuesta correcta era: {shuffledAnswers[correctAnswerIndex]}";
            streak = 0;
        }

        newPointsText.text = "+ " + newPoints.ToString();
        totalPointsText.text = totalPoints.ToString();

        // Actualizar el texto de la racha
        UpdateStreakText();
    }

    void NextQuestion()
    {
        // Tomamos el cuestionario desde QuizManager
        RootData quiz = QuizManager.Instance.currentQuiz;

        // Si ya hemos hecho todas las preguntas, puedes terminar el quiz
        if (questionsDone.Count >= quiz.Questions.Count)
        {
            PlayerDataManager.Instance.playerData.playerPoints = totalPoints;
            SceneManager.LoadScene("LeaderboardScene");
            return;
        }

        UIManager.Instance.SetPanel("QuestionResult - Panel", false);
        UIManager.Instance.SetPanel("Game - Panel", true);

        // Elegir un índice aleatorio que no esté en questionsDone
        int index;
        do
        {
            index = UnityEngine.Random.Range(0, quiz.Questions.Count);
        } while (questionsDone.Contains(index));
        currentQuestion = quiz.Questions[index];

        // Guardar que ya se ha hecho
        questionsDone.Add(index);

        // Mostramos la pregunta
        questionText.text = currentQuestion.text;

        timeRemaining = currentQuestion.timer;
        totalTime = timeRemaining;
        timerRunning = true;

        // Copiar y barajar respuestas
        shuffledAnswers = new List<string>(currentQuestion.answers);
        ShuffleList(shuffledAnswers);

        // Averiguar dónde quedó la correcta tras barajar
        string correctAnswerText = currentQuestion.answers[currentQuestion.CorrectAnswer];
        correctAnswerIndex = shuffledAnswers.IndexOf(correctAnswerText);

        // Activar solo los botones necesarios
        for (int i = 0; i < answerButtons.Count; i++)
        {
            if (i < shuffledAnswers.Count)
            {
                answerButtons[i].gameObject.SetActive(true);

                string answer = shuffledAnswers[i];
                answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = answer;

                answerButtons[i].onClick.RemoveAllListeners();
                int capturedIndex = i;
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(capturedIndex));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    void UpdateStreakText()
    {
        streakText.text = streak.ToString();
    }
}
