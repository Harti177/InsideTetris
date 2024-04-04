## Experience Tetris like never before in VR. Shape, move, conquer the wall for endless immersive fun!

## Inspiration
Combining the popularity of fitness games and puzzle games in virtual reality, I sought to create a unique fusion of both elements within InsideTetris.

## What it does
InsideTetris transforms defined walls into a Tetris board, allowing players to manipulate Tetris pieces using hand tracking. Players must strategically place the pieces while also engaging in fitness activities like boxing, utilizing hand tracking for puzzle placement. The dynamic gameplay adapts to any wall shape defined by the player, ensuring optimal gameplay by avoiding real-world obstructions and maximizing gaming space.

## How I built it
Leveraging Unity and C#. I implemented gameplay mechanics using Oculus Fast-Hands sample such as left and right movement by fist gestures, rotation upon stop gesture, and downward movement by slice gestures. The scoring system awards points for each piece placed and additional points for completing lines scored similar to normal Tetris game, ensuring a fair scoring mechanism that considers the size of the walls. Points are also saved in azure and the application shows the top ten high scores. 

## Challenges I ran into
Developing algorithms to generate cubes based on defined walls posed a significant challenge. Crafting a scoring system that remains engaging and fair while accounting for the dynamically changing gameplay and varying wall sizes required careful consideration and iteration.

## Accomplishments that I am proud of
Through the development process, I gained valuable experience with Oculus SDKs, including Passthrough, Scene Unserstanding, and Interaction SDK. 

## What I learned
I learned the importance of planning the entire gameplay and scoring system thoroughly before commencing development. This foresight ensures smoother development and a more cohesive gaming experience.

## What's next for InsideTetris
Future plans for InsideTetris involve improving performance and refining the scoring system. Additionally, I aim to release the game publicly, allowing a broader audience to experience the unique blend of fitness and puzzle gaming in virtual reality.
