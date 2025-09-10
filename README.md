# Match-3

## Original game content

The original code had this content, which consisted only of selectable tiles, that combine when aliged in groups of three or more:

![Match-3](/Match3.png?raw=true "Match-3")

## Implemented game content

My version implements the following features:

### Main Single player game loop

In this mode, the game has a set timer for the match to end. After this time, the final score is shown and the player has the option to play again.

The score is calculated with a base value, set in the script ```Script/Core/GameConstants.cs```. The score also has a multiplier, that increments this base value if the player can make combinations in quick succession.

Finnally, each successful combination generates blocked tiles, that will not generate new combinations while blocked. After a while, they are unblocked and may be used in new combinations. It is important to note that the unblocked
tiles do not make combinations as soon as they are unblocked: the player has to interact with the board in order for them to combine again. This was a deliberate choice, as to not make the board update on its own, but this might be a bit confusing. An example of this happening can be seen below (notice the four yellow blocks on the fourth row):

![Unblocking tiles](/Doc/UnblockingTiles.gif?raw=true "Unblocking tiles")

Selecting any two available tiles on the board should make these unblocked tiles combine again.
This blocking mechanic was chosen for three main reasons: 
1. To increase the game's difficulty a little 
2. To reduce the amount of automatic chains each time a combination is made
3. To allow a player to interact with their opponent in the Versus Mode (described in the next section)

### Versus Mode

In this mode, each combination made by a player sends an equivalent number of blocked tiles to the opponent. After the set ammount of time, the scores of both players are compared to elect a winner. This mode was tested with two instances on the same computer, and with two instances in different computers that are on the same local network. A demonstration of this mode can be seen below:

![Versus Mode](/Doc/VersusMode.gif?raw=true "Versus Mode")

## Development Organization

I tried to follow the main project structure, with each component having its own responsibilities. As the game needs to have some default behaviours, as mentioned, the file ```Script/Core/GameConstants.cs``` was created in order to provide adjustable parameters to each script

Also of note is the NetworkManager implementation. I used Unity Transport and its [documentation](https://docs.unity3d.com/Packages/com.unity.transport@2.5/manual/client-server-simple.html) as a reference, with a peer-to-peer messaging approach. Each event that needs to be communicated is turned into a fixed-size string that is then sent to the other peer. Both endpoints share most of the responsibilities, with the exception of the match clock, which is updated always by the "Server", which then synchronizes it with the "Client".

## Additional Comments

Most of the code should hopefully be easy to understand with comments used to clarify the reasoning for some of my choices. The versus mode is not very robust, with not many options for customizing the match, but it is functional and implements the same mechanics of the single player mode with a twist on the way blocked tiles are generated