using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Gazeus.DesafioMatch3.Core;
namespace Gazeus.DesafioMatch3.Views
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI scoreMultiplierText;
        private Tweener multiplierDecayColorTweener;

        private void Start()
        {
            scoreText.text = "Score: 0";
            //Tweener for fading the color to represent the multiplier reseting
            InitializeMultiplierDecayColorTweener();
            multiplierDecayColorTweener.Pause();
        }

        public void InitializeMultiplierDecayColorTweener()
        {
            multiplierDecayColorTweener = DOVirtual.Color(Color.yellow, Color.white, GameConstants.TimeForMultiplierDecay, (color) => scoreMultiplierText.color = color);
        }

        public Tween UpdateScore(int updatedScore)
        {
            scoreText.text = $"Score: {updatedScore}";

            return DOVirtual.DelayedCall(0.2f, () => { });
        }

        public Tween UpdateScoreMultiplier(int updatedScoreMultiplier)
        {
            scoreMultiplierText.text = $"X{updatedScoreMultiplier}";
            if(updatedScoreMultiplier != 1)
            {
                //Change color to emphasize the multiplier
                scoreMultiplierText.color = Color.yellow;
            }
            return DOVirtual.DelayedCall(0.2f, () => { });
        }
    }
}
