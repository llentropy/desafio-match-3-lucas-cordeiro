# Match-3

## Original game content

The original code had this content:

![Match-3](/Match3.png?raw=true "Match-3")

## Implemented game content

My version implements the following features:

### Main Single player game loop

In this mode, the game has a set timer for the match to end. After this time, the final score is shown and the player has the option to play again.

The score is calculated with a base value, set in the script ```Script/Core/GameConstants.cs```. The score also has a multiplier, that increments this base value if the player can make combinations in quick succession