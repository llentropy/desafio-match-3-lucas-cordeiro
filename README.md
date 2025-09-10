# Match-3

## Original game content

The original code had this content:

![Match-3](/Match3.png?raw=true "Match-3")

## Implemented game content

My version implements the following features:

### Main Single player game loop

In this mode, the game has a set timer for the match to end. After this time, the final score is shown and the player has the option to play again.

The score is calculated with a base value, set in the script ```Script/Core/GameConstants.cs```. The score also has a multiplier, that increments this base value if the player can make combinations in quick succession.

Finnally, each successful combination generates blocked tiles, that will not generate new combinations while blocked. After a while, they are unblocked and may be used in new combinations. It is important to note that the unblocked
tiles do not make combinations as soon as they are unblocked: the player has to interact with the board in order for them to combine again. This was a deliberate choice, as to not make the board update on its own, but this might be a bit confusing. An example of this happening can be seen below (notice the four yellow blocks on the fourth row):

![Unblocking tiles](/Doc/UnblockingTiles.gif?raw=true "Unblocking tiles")

Selecting any two available tiles on the board should make these unblocked tiles combine again

### Versus mode

My version also has a versus mode. In this mode, each combination made by a player sends an equivalent number of blocked tiles to the opponent. After the set ammount of time, the scores of both players are compared to elect a winner. This mode was tested with two instances on the same computer, and with two instances in different computers that are on the same local network. A demonstration of this mode can be seen below:

![Versus Mode](/Doc/VersusMode.gif?raw=true "Versus Mode")
