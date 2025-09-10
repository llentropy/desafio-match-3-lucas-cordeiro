using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gazeus.DesafioMatch3.Core
{
    public enum GameMode
    {
        SinglePlayer,
        Versus
    }
    public class GameService
    {
        private List<List<Tile>> _boardTiles;
        private List<int> _tilesTypes;
        private int _tileCount;

        private int scoreMultiplier = 1;
        public int GameScore { get; private set; } = 0;
        private int quantityOfNextBlockedTiles = 0;
        public GameMode MatchGameMode = GameMode.SinglePlayer;

        #region Main game behaviour
        public bool IsValidMovement(int fromX, int fromY, int toX, int toY, float timestamp)
        {
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            (newBoard[toY][toX], newBoard[fromY][fromX]) = (newBoard[fromY][fromX], newBoard[toY][toX]);

            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
                {
                    if (x > 1 &&
                        !(newBoard[y][x].IsBlocked || newBoard[y][x - 1].IsBlocked || newBoard[y][x - 2].IsBlocked) &&
                        newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                        newBoard[y][x - 1].Type == newBoard[y][x - 2].Type)
                    {
                        return true;
                    }

                    if (y > 1 &&
                        !(newBoard[y][x].IsBlocked || newBoard[y - 1][x].IsBlocked || newBoard[y - 2][x].IsBlocked) &&
                        newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                        newBoard[y - 1][x].Type == newBoard[y - 2][x].Type)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<List<Tile>> StartGame(int boardWidth, int boardHeight)
        {
            GameScore = 0;
            _tilesTypes = new List<int> { 0, 1, 2, 3 };
            _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);

            return _boardTiles;
        }

        public void EndGame()
        {
            GameScore = 0;
        }
        public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
        {
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            (newBoard[toY][toX], newBoard[fromY][fromX]) = (newBoard[fromY][fromX], newBoard[toY][toX]);

            List<BoardSequence> boardSequences = new();
            List<List<bool>> matchedTiles = FindMatches(newBoard);


            while (HasMatch(matchedTiles))
            {
                int quantityOfBlockedTilesToSend = 0;
                //List to store the tiles that will contribute to the next match score
                List<Tile> tilesToScore = new();

                //Cleaning the matched tiles
                List<Vector2Int> matchedPosition = new();
                for (int y = 0; y < newBoard.Count; y++)
                {
                    for (int x = 0; x < newBoard[y].Count; x++)
                    {
                        if (matchedTiles[y][x])
                        {
                            matchedPosition.Add(new Vector2Int(x, y));
                            tilesToScore.Add(newBoard[y][x]);
                            newBoard[y][x] = new Tile { Id = -1, Type = -1 };
                        }
                    }
                }

                //Calculate game score for each removed tile
                CalculateUpdatedScore(tilesToScore);

                if(MatchGameMode == GameMode.Versus)
                {
                    quantityOfBlockedTilesToSend += tilesToScore.Count;
                }

                // Dropping the tiles
                Dictionary<int, MovedTileInfo> movedTiles = new();
                List<MovedTileInfo> movedTilesList = new();
                for (int i = 0; i < matchedPosition.Count; i++)
                {
                    int x = matchedPosition[i].x;
                    int y = matchedPosition[i].y;
                    if (y > 0)
                    {
                        for (int j = y; j > 0; j--)
                        {
                            Tile movedTile = newBoard[j - 1][x];
                            newBoard[j][x] = movedTile;
                            if (movedTile.Type > -1)
                            {
                                if (movedTiles.ContainsKey(movedTile.Id))
                                {
                                    movedTiles[movedTile.Id].To = new Vector2Int(x, j);
                                }
                                else
                                {
                                    MovedTileInfo movedTileInfo = new()
                                    {
                                        From = new Vector2Int(x, j - 1),
                                        To = new Vector2Int(x, j)
                                    };
                                    movedTiles.Add(movedTile.Id, movedTileInfo);
                                    movedTilesList.Add(movedTileInfo);
                                }
                            }
                        }

                        newBoard[0][x] = new Tile
                        {
                            Id = -1,
                            Type = -1
                        };
                    }
                }

                // Filling the board
                List<AddedTileInfo> addedTiles = new();
                for (int y = newBoard.Count - 1; y > -1; y--)
                {
                    for (int x = newBoard[y].Count - 1; x > -1; x--)
                    {
                        if (newBoard[y][x].Type == -1)
                        {
                            int tileType = Random.Range(0, _tilesTypes.Count);
                            Tile tile = newBoard[y][x];
                            tile.Id = _tileCount++;
                            tile.Type = _tilesTypes[tileType];
                            tile.SpawnTimestamp = Time.time;
                            tile.IsBlocked = false;
                            tile.BlockedStatusDuration = 0;
                            if(quantityOfNextBlockedTiles > 0)
                            {
                                if(Random.Range(0, 1.0f) <= GameConstants.ProbabilityToGenerateBlockedTile)
                                {
                                    quantityOfNextBlockedTiles--;
                                    tile.IsBlocked = true;
                                    tile.BlockedStatusDuration = GameConstants.BlockedTileDuration;
                                }
                            }


                            addedTiles.Add(new AddedTileInfo
                            {
                                Position = new Vector2Int(x, y),
                                Type = tile.Type,
                                SpawnTimestamp = tile.SpawnTimestamp,
                                IsBlocked = tile.IsBlocked,
                                BlockedStatusDuration = tile.BlockedStatusDuration

                            });
                        }
                    }
                }

                BoardSequence sequence = new()
                {
                    MatchedPosition = matchedPosition,
                    MovedTiles = movedTilesList,
                    AddedTiles = addedTiles,
                    UpdatedScore = GameScore,
                    UpdatedScoreMultiplier = scoreMultiplier,
                    QuantityOfBlockedTilesToSend = quantityOfBlockedTilesToSend
                };
                boardSequences.Add(sequence);
                matchedTiles = FindMatches(newBoard);
            }


            _boardTiles = newBoard;

            return boardSequences;
        }

        private static List<List<Tile>> CopyBoard(List<List<Tile>> boardToCopy)
        {
            List<List<Tile>> newBoard = new(boardToCopy.Count);
            for (int y = 0; y < boardToCopy.Count; y++)
            {
                newBoard.Add(new List<Tile>(boardToCopy[y].Count));
                for (int x = 0; x < boardToCopy[y].Count; x++)
                {
                    Tile tile = boardToCopy[y][x];
                    newBoard[y].Add(new Tile { Id = tile.Id, Type = tile.Type, IsBlocked = tile.IsBlocked, BlockedStatusDuration = tile.BlockedStatusDuration, SpawnTimestamp = tile.SpawnTimestamp });
                }
            }

            return newBoard;
        }

        private List<List<Tile>> CreateBoard(int width, int height, List<int> tileTypes)
        {
            List<List<Tile>> board = new(height);
            _tileCount = 0;
            for (int y = 0; y < height; y++)
            {
                board.Add(new List<Tile>(width));
                for (int x = 0; x < width; x++)
                {
                    board[y].Add(new Tile { Id = -1, Type = -1 });
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    List<int> noMatchTypes = new(tileTypes.Count);
                    for (int i = 0; i < tileTypes.Count; i++)
                    {
                        noMatchTypes.Add(_tilesTypes[i]);
                    }

                    if (x > 1 &&
                        board[y][x - 1].Type == board[y][x - 2].Type)
                    {
                        noMatchTypes.Remove(board[y][x - 1].Type);
                    }

                    if (y > 1 &&
                        board[y - 1][x].Type == board[y - 2][x].Type)
                    {
                        noMatchTypes.Remove(board[y - 1][x].Type);
                    }

                    board[y][x].Id = _tileCount++;
                    board[y][x].Type = noMatchTypes[Random.Range(0, noMatchTypes.Count)];
                }
            }

            return board;
        }

        private static List<List<bool>> FindMatches(List<List<Tile>> newBoard)
        {
            List<List<bool>> matchedTiles = new();
            for (int y = 0; y < newBoard.Count; y++)
            {
                matchedTiles.Add(new List<bool>(newBoard[y].Count));
                for (int x = 0; x < newBoard.Count; x++)
                {
                    matchedTiles[y].Add(false);
                }
            }

            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
                {
                    if (x > 1 &&
                        !(newBoard[y][x].IsBlocked || newBoard[y][x - 1].IsBlocked || newBoard[y][x - 2].IsBlocked) &&
                        newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                        newBoard[y][x - 1].Type == newBoard[y][x - 2].Type)
                    {
                        matchedTiles[y][x] = true;
                        matchedTiles[y][x - 1] = true;
                        matchedTiles[y][x - 2] = true;
                    }

                    if (y > 1 &&
                        !(newBoard[y][x].IsBlocked || newBoard[y - 1][x].IsBlocked || newBoard[y - 2][x].IsBlocked) &&
                        newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                        newBoard[y - 1][x].Type == newBoard[y - 2][x].Type)
                    {
                        matchedTiles[y][x] = true;
                        matchedTiles[y - 1][x] = true;
                        matchedTiles[y - 2][x] = true;
                    }
                }
            }

            return matchedTiles;
        }

        private static bool HasMatch(List<List<bool>> list)
        {
            for (int y = 0; y < list.Count; y++)
            {
                for (int x = 0; x < list[y].Count; x++)
                {
                    if (list[y][x])
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        #region Score management
        public void ResetScoreMultiplier()
        {
            scoreMultiplier = 1;
        }

        private void CalculateUpdatedScore(List<Tile> tilesToScore)
        {
            if(MatchGameMode == GameMode.SinglePlayer)
            {
                quantityOfNextBlockedTiles += Random.Range(1, GameConstants.MaxBlockedTilesGeneratedPerScore);
            }

            int scoreIncrement = 0;
            Dictionary<int, int> quantityPerType = new();
            foreach(var tile in tilesToScore)
            {
                if (!quantityPerType.ContainsKey(tile.Type))
                {
                    quantityPerType[tile.Type] = 1;
                } else
                {
                    quantityPerType[tile.Type] = quantityPerType[tile.Type] + 1;
                }
            }

            foreach(var type in quantityPerType.Keys)
            {
                scoreMultiplier++;
                scoreIncrement += GameConstants.BaseScoreIncrementPerPiece * quantityPerType[type] * scoreMultiplier;
            }

            GameScore += scoreIncrement;
        }

        #endregion

        #region Blocked tiles management
        public void RevalidateBlockedTiles(float timestamp)
        {
            foreach (var tileList in _boardTiles)
            {
                foreach (var tile in tileList)
                {
                    if (tile.IsBlocked && timestamp > (tile.BlockedStatusDuration + tile.SpawnTimestamp))
                    {
                        tile.IsBlocked = false;
                    }
                }
            }
        }
        public void IncrementQuantityOfNextBlockedTiles(int increment)
        {
            quantityOfNextBlockedTiles += increment;
        }
        public bool IsTileBlocked(int x, int y)
        {
            Tile tile = _boardTiles[y][x];
            return tile.IsBlocked;
        }

        #endregion

    }
}
