using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Gazeus.DesafioMatch3
{
    public class VersusModeStatusView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _playerNames;
        [SerializeField] TextMeshProUGUI _versusStatusMessagePrefab;

        [SerializeField] private Color PlayerColor = Color.green;
        [SerializeField] private Color OpponentColor = Color.red;
        
        public void SetPlayerNames(string player1, string player2)
        {
            _playerNames.text = $"<color=#{ColorUtility.ToHtmlStringRGB(PlayerColor)}>{player1}</color> VS <color=#{ColorUtility.ToHtmlStringRGB(OpponentColor)}>{player2}</color>";
        }

        internal void UpdateStatusForReceivedBlockedTiles(int quantity, string opponentName)
        {
            var newStatusMessage = GameObject.Instantiate(_versusStatusMessagePrefab);
            newStatusMessage.transform.parent = this.transform;
            newStatusMessage.color = Color.gray;
            newStatusMessage.text = $"{opponentName} sent {quantity} blocked tiles!";
            Sequence fadeMessageSequence = DOTween.Sequence();
            //Display the message for 4/5 of the defined duration, then fade it out
            fadeMessageSequence.PrependInterval(GameConstants.StatusMessageDuration * 0.9f);
            fadeMessageSequence.Append(DOVirtual.Float(newStatusMessage.alpha, 0, GameConstants.StatusMessageDuration * 0.1f, (alpha) =>  newStatusMessage.alpha = alpha ));
            fadeMessageSequence.OnComplete(() => Destroy(newStatusMessage.gameObject));
            fadeMessageSequence.SetLink(newStatusMessage.gameObject);
        }
    }
}
