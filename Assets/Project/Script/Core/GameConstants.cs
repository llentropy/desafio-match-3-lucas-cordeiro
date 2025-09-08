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
        public const float TimeForMultiplierDecay = 2;

        //Time in seconds for the match to end
        public const float MatchTime = 60;
    }
}
