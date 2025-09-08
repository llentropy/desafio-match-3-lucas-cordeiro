using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Views;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private ScoreView _scoreView;

        [SerializeField] private int _boardHeight = 10;
        [SerializeField] private int _boardWidth = 10;

        private GameService _gameEngine;
        private bool _isAnimating;
        private int _selectedX = -1;
        private int _selectedY = -1;

        private Coroutine multiplierDecayCoroutine;

        #region Unity
        private void Awake()
        {
            _gameEngine = new GameService();
            _boardView.TileClicked += OnTileClick;
        }

        private void OnDestroy()
        {
            _boardView.TileClicked -= OnTileClick;
        }

        private void Start()
        {
            List<List<Tile>> board = _gameEngine.StartGame(_boardWidth, _boardHeight);
            _boardView.CreateBoard(board);
        }
        #endregion

        private IEnumerator ResetScoreMultiplierAfterDecayTime()
        {
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
                sequence.onComplete += () => onComplete();
            }
        }

       

        private void OnTileClick(int x, int y)
        {
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
                        bool isValid = _gameEngine.IsValidMovement(_selectedX, _selectedY, x, y);
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
