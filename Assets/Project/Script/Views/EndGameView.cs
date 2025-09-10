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
        public void SetFinalScore(int score)
        {
            finalScoreText.text = $"Final score: {score}";
        }

    }
}
