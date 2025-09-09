using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Gazeus.DesafioMatch3
{
    public class VersusModeStatusView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _playerNames;
        [SerializeField] TextMeshProUGUI _versusStatusMessage;
        
        public void SetPlayerNames(string player1, string player2)
        {
            _playerNames.text = $"{player1} VS {player2}";
        }
    }
}
