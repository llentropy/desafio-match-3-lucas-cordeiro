using System;
using TMPro;
using UnityEngine;

namespace Gazeus.DesafioMatch3
{
    public class TimerView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;

        public void UpdateTimerText(float seconds)
        {
            timerText.text = $"Time: {Math.Truncate(seconds)}";
        }
    }
}
