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
        public const float ProbabilityToGenerateBlockedTile = 0.6f;

        //Default time for a blocked tile to become unblocked
        public const float BlockedTileDuration = 10;

        public const int DefaultConnectionPort = 9000;
    }
}
