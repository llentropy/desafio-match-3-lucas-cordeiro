using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace Gazeus.DesafioMatch3
{
    public class VersusModeStatusView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _playerNames;
        [SerializeField] TextMeshProUGUI _versusStatusMessage;

        [SerializeField] private Color PlayerColor = Color.green;
        [SerializeField] private Color OpponentColor = Color.red;
        
        public void SetPlayerNames(string player1, string player2)
        {
            _playerNames.text = $"<color=#{ColorUtility.ToHtmlStringRGB(PlayerColor)}>{player1}</color> VS <color=#{ColorUtility.ToHtmlStringRGB(OpponentColor)}>{player2}</color>";
        }

        internal void UpdateStatusForReceivedBlockedTiles(int quantity, string opponentName)
        {
            _versusStatusMessage.text = $"{opponentName} sent {quantity} blocked tiles!";
        }
    }
}
