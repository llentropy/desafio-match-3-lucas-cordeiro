using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
namespace Gazeus.DesafioMatch3.Views
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI scoreMultiplierText;

        private void Start()
        {
            scoreText.text = "Score: 0";
        }

        public Tween UpdateScore(int updatedScore)
        {
            scoreText.text = $"Score: {updatedScore}";

            return DOVirtual.DelayedCall(0.2f, () => { });
        }

        internal Tween UpdateScoreMultiplier(int updatedScoreMultiplier)
        {
            return DOVirtual.DelayedCall(0.2f, () => { });
        }
    }
}
