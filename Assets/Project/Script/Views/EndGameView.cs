using Gazeus.DesafioMatch3.Views;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3
{
    public class EndGameView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button playAgainButton;
        public event Action PlayAgainButtonPressed;

        public void Awake()
        {
            playAgainButton.onClick.AddListener(InvokeButtonClickAction);
        }
        private void OnDestroy()
        {
            playAgainButton.onClick.RemoveListener(InvokeButtonClickAction);
        }

        private void InvokeButtonClickAction()
        {
            PlayAgainButtonPressed();
        }
        public void SetFinalScore(int score)
        {
            finalScoreText.text = $"Final score: {score}";
        }

    }
}
