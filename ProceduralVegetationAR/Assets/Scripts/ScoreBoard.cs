using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreBoard : MonoBehaviour
{
    public TextMeshPro titel;
    public TextMeshPro text;
    public TextMeshPro highscoreText;
    public TextMeshPro currentScoreText;

    public void UpdateText(string titel, string text, string highscoreText, string currentScoreText)
    {
        this.titel.text = titel;
        this.text.text = text;
        this.highscoreText.text = highscoreText;
        this.currentScoreText.text = currentScoreText;
    }
}
