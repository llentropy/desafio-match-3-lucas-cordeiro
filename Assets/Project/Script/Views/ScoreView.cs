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
        private Tweener multiplierTextScaleTweener;
        private Color multiplierTextTweenColor = new Color(1, 0.23529f, 1);

        //private void Start()
        //{
        //    InitializeScoreView();
        //}

        //public void InitializeScoreView()
        //{
        //    scoreText.text = "Score: 0";
        //    //Tweeners for fading the color  and changing the scale to represent the multiplier reseting
        //    scoreMultiplierText.transform.localScale = Vector3.one;
        //    scoreMultiplierText.color = Color.white;
        //    UpdateScore(0);
        //    UpdateScoreMultiplier(1);
        //}

        private void KillTweensIfNotNull()
        {
            if (multiplierDecayColorTweener != null)
            {
                multiplierDecayColorTweener.Kill();
            }
            if (multiplierTextScaleTweener != null)
            {
                multiplierTextScaleTweener.Kill();
            }
        }

        public void InitializeMultiplierDecayColorTweener()
        {
            KillTweensIfNotNull();
            multiplierDecayColorTweener = DOVirtual.Color(multiplierTextTweenColor, Color.white, GameConstants.TimeForMultiplierDecay, (color) => scoreMultiplierText.color = color);
            multiplierTextScaleTweener = DOVirtual.Vector3(Vector3.one * 2, Vector3.one, GameConstants.TimeForMultiplierDecay, (scale) => scoreMultiplierText.transform.localScale = scale);
        }

        public Tween UpdateScore(int updatedScore)
        {
            scoreText.text = $"Score: {updatedScore}";

            return DOVirtual.DelayedCall(0.2f, () => { });
        }

        public Tween UpdateScoreMultiplier(int updatedScoreMultiplier)
        {
            KillTweensIfNotNull();
            scoreMultiplierText.text = $"X{updatedScoreMultiplier}";
            if (updatedScoreMultiplier != 1)
            {
                //Change color to emphasize the multiplier
                scoreMultiplierText.color = multiplierTextTweenColor;
                scoreMultiplierText.transform.localScale = Vector3.one * 2;

            }
            return DOVirtual.DelayedCall(0.2f, () => { });
        }
    }
}