using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionEditorController : MonoBehaviour
{
    [SerializeField] public TMP_InputField questionText;
    [SerializeField] public List<GameObject> answers;
    [SerializeField] public List<Button> answerDeleteButtons;
    [SerializeField] public List<Toggle> answerToggles;
    [SerializeField] public List<TMP_InputField> answerTexts;
    [SerializeField] public TMP_InputField timerText;
    [SerializeField] public Button addAnswerButton;
}
