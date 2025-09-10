using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3
{
    public static class GameConstants
    {
        //Base value for incrementing score
        public const int BaseScoreIncrementPerPiece = 100 ;

        //Decay time in seconds
        public const float TimeForMultiplierDecay = 3;

        //Time in seconds for the match to end
        public const float MatchTime = 120;

        //Chance of generating a new blocked tile
        public const float ProbabilityToGenerateBlockedTile = 1.0f;

        //Each time the player scores, the next generated tiles will contain
        //at least one blocked tile and at most the value bellow
        public const int MaxBlockedTilesGeneratedPerScore = 6;

        //Default time for a blocked tile to become unblocked
        public const float BlockedTileDuration = 15;

        //Default versus mode connection port
        //The actual port will be higher in case this default is alteready in use
        public const int DefaultConnectionPort = 9000;
    }
}
