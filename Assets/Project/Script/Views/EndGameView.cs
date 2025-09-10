using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3
{
    public class EndGameView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button mainMenuButton;
        public event Action MainMenuButtonPressed;

        public void Awake()
        {
            mainMenuButton.onClick.AddListener(InvokeButtonClickAction);
        }
        private void OnDestroy()
        {
            mainMenuButton.onClick.RemoveListener(InvokeButtonClickAction);
        }

        private void InvokeButtonClickAction()
        {
            MainMenuButtonPressed();
        }
        public void SetFinalScore(int myScore, int opponentScore = -1, string playerName = "Your", string opponentName = "")
        {
            finalScoreText.text = $"Your final score: {myScore}";
            if(opponentScore != -1)
            {
                finalScoreText.text += $"\n{opponentName} final score: {opponentScore}";
                if (myScore > opponentScore)
                {
                    finalScoreText.text += $"\n You won!!!";
                } else if (myScore == opponentScore)
                {
                    finalScoreText.text += $"\n It's a draw :O";
                } else
                {
                    finalScoreText.text += $"\n You lost :(";
                }
            } 
        }

    }
}
