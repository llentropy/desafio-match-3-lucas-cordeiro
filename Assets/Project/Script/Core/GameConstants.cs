namespace Gazeus.DesafioMatch3
{
    public static class GameConstants
    {
        #region Constants For All Modes
        //Base value for incrementing score
        public const int BaseScoreIncrementPerPiece = 100;

        //Score multiplier decay time in seconds. It becomes 1X after this timespan
        public const float TimeForMultiplierDecay = 3;

        //Time in seconds for the match to end
        public const float MatchTime = 120;

        //Chance of generating a new blocked tile. 1.0f means a blocked tile is always generated
        public const float ProbabilityToGenerateBlockedTile = 0.7f;

        //Default time for a blocked tile to become unblocked
        public const float BlockedTileDuration = 15;

        #endregion

        #region Single Player Constants
        //In a single player game, this controls the maximum quantity of blocked tiles
        //each score generates
        public const int MaxBlockedTilesGeneratedPerScore = 6;
        #endregion

        #region Multiplayer Constants
        //Default versus mode connection port
        //The actual port will be higher in case this default is already in use
        public const int DefaultConnectionPort = 9000;

        //Interval for the server to send another clock update to the client
        //0.1f means the clocks are synchronized each 1/10 of a second
        public const float ClientUpdateClockInterval = 0.1f;

        //Duration in seconds for the sent blocked tiles status updates
        public const float StatusMessageDuration = 8.0f;

        #endregion
    }
}
