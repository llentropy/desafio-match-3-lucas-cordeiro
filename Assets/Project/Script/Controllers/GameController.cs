using DG.Tweening;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Views;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gazeus.DesafioMatch3.Controllers
{
    
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private ScoreView _scoreView;
        [SerializeField] private TimerView _timerView;
        [SerializeField] private EndGameView _endGameView;
        [SerializeField] private VersusModeStatusView _versusModeStatusView;
        private NetworkManager _networkManager;


        [SerializeField] private int _boardHeight = 10;
        [SerializeField] private int _boardWidth = 10;

        private GameService _gameEngine;
        private bool _isAnimating;
        private int _selectedX = -1;
        private int _selectedY = -1;
        private bool _isMatchRunning = true;

        private Coroutine multiplierDecayCoroutine;

        private float remainingMatchTime = GameConstants.MatchTime;

        #region Unity
        private void Awake()
        {
            _gameEngine = new GameService();
            _boardView.TileClicked += OnTileClick;
            _endGameView.MainMenuButtonPressed += ExitMatch;

            _networkManager = FindAnyObjectByType<NetworkManager>();
            if(_networkManager != null && _networkManager._connectionMode != ConnectionMode.Disconnected)
            {
                //Bind the events to the GameService and NetworkManager behaviours
                _gameEngine.MatchGameMode = GameMode.Versus;
                //_gameEngine.SendBlockedTilesToOpponnentEvent += (quantity) => SendBlockedTiles(quantity);
                _networkManager.ReceivedBlockedTilesEvent += (quantity) => ReceiveBlockedTiles(quantity);
                SetupVersusModeUI();
            } else
            {
                _gameEngine.MatchGameMode = GameMode.SinglePlayer;
            }
        }

        private void SendBlockedTiles(int quantity)
        {
            _networkManager.SetQueuedMessage($"SendBlockedTiles;{quantity}");
        }

        private void ReceiveBlockedTiles(int quantity)
        {
            _versusModeStatusView.UpdateStatusForReceivedBlockedTiles(quantity, _networkManager.OpponentName);
            _gameEngine.IncrementQuantityOfNextBlockedTiles(quantity);
        }

        private void SetupVersusModeUI()
        {
            _versusModeStatusView.gameObject.SetActive(true);
            _versusModeStatusView.SetPlayerNames(_networkManager.PlayerName, _networkManager.OpponentName);
        }

        private void OnDestroy()
        {
            _boardView.TileClicked -= OnTileClick;
            _endGameView.MainMenuButtonPressed -= ExitMatch;
        }

        private void Start()
        {
            StartMatch();
        }

        private void ExitMatch()
        {
            SceneManager.LoadScene("MainMenu");
        }


        private void Update()
        {
            _timerView.UpdateTimerText(remainingMatchTime);
            remainingMatchTime -= Time.deltaTime;
            if(remainingMatchTime <= 0 && _isMatchRunning )
            {
                EndMatch();
            }
        }
        #endregion

        private void StartMatch()
        {
            _isMatchRunning = true;
            _boardView.gameObject.SetActive(true);
            _scoreView.gameObject.SetActive(true);
            _timerView.gameObject.SetActive(true);
            if (_gameEngine.MatchGameMode == GameMode.Versus) {
                _versusModeStatusView.gameObject.SetActive(true);
            } else
            {
                _versusModeStatusView.gameObject.SetActive(false);

            }
            _endGameView.gameObject.SetActive(false);
            List<List<Tile>> board = _gameEngine.StartGame(_boardWidth, _boardHeight);
            _boardView.CreateBoard(board);
        }

        private void EndMatch()
        {
            _isMatchRunning = false;
            DOTween.KillAll();
            _boardView.gameObject.SetActive(false);
            _scoreView.gameObject.SetActive(false);
            _timerView.gameObject.SetActive(false);
            if (_gameEngine.MatchGameMode == GameMode.Versus)
            {
                _versusModeStatusView.gameObject.SetActive(false);
            }
            _endGameView.gameObject.SetActive(true);
            _endGameView.SetFinalScore(_gameEngine.GameScore);
        }

        private IEnumerator ResetScoreMultiplierAfterDecayTime()
        {
            //This coroutine will always reset the multiplier after the decay time, unless it is cancelled by a new score
            yield return new WaitForSeconds(GameConstants.TimeForMultiplierDecay);
            _gameEngine.ResetScoreMultiplier();
            _scoreView.UpdateScoreMultiplier(1);
        }

        private void AnimateBoard(List<BoardSequence> boardSequences, int index, Action onComplete)
        {
            BoardSequence boardSequence = boardSequences[index];

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_boardView.DestroyTiles(boardSequence.MatchedPosition));
            sequence.Append(_boardView.MoveTiles(boardSequence.MovedTiles));
            sequence.Append(_boardView.CreateTile(boardSequence.AddedTiles));
            sequence.Append(_scoreView.UpdateScore(boardSequence.UpdatedScore));
            sequence.Append(_scoreView.UpdateScoreMultiplier(boardSequence.UpdatedScoreMultiplier));

            index += 1;
            if (index < boardSequences.Count)
            {
                sequence.onComplete += () => AnimateBoard(boardSequences, index, onComplete);
            }
            else
            {
                int totalOfTilesToSend = 0;
                foreach(var boardSequenceIt in boardSequences)
                {
                    totalOfTilesToSend += boardSequenceIt.QuantityOfBlockedTilesToSend;
                }
                if(_gameEngine.MatchGameMode == GameMode.Versus)
                {
                    sequence.onComplete += () => {

                        SendBlockedTiles(totalOfTilesToSend);
                        onComplete();
                        
                        };
                }
                else
                {
                    sequence.onComplete += () => onComplete();
                }
            }
        }

        private void OnTileClick(int x, int y)
        {
            _gameEngine.RevalidateBlockedTiles(Time.time);
            if (_gameEngine.IsTileBlocked(x, y)) return;
            if (_isAnimating) return;

            if (_selectedX > -1 && _selectedY > -1)
            {
                if (Mathf.Abs(_selectedX - x) + Mathf.Abs(_selectedY - y) > 1)
                {
                    _selectedX = -1;
                    _selectedY = -1;
                }
                else
                {
                    _isAnimating = true;
                    _boardView.SwapTiles(_selectedX, _selectedY, x, y).onComplete += () =>
                    {
                        bool isValid = _gameEngine.IsValidMovement(_selectedX, _selectedY, x, y, Time.time);
                        if (isValid)
                        {
                            
                            List<BoardSequence> swapResult = _gameEngine.SwapTile(_selectedX, _selectedY, x, y);
                            if(swapResult.Count > 1 && multiplierDecayCoroutine != null)
                            {
                                StopCoroutine(multiplierDecayCoroutine);
                            }
                            AnimateBoard(swapResult, 0, () => { 
                                _isAnimating = false;
                                multiplierDecayCoroutine = StartCoroutine(ResetScoreMultiplierAfterDecayTime());
                                _scoreView.InitializeMultiplierDecayColorTweener();
                            });
                        }
                        else
                        {
                            _boardView.SwapTiles(x, y, _selectedX, _selectedY).onComplete += () => _isAnimating = false;
                        }
                        _selectedX = -1;
                        _selectedY = -1;
                    };
                }
            }
            else
            {
                _selectedX = x;
                _selectedY = y;
            }
        }
    }
}
