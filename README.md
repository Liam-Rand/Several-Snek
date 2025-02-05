![](SeveralSnekTitle.png)

<br />

![Project Type](https://custom-icon-badges.demolab.com/badge/Project%20Type-Personal-F25278?style=for-the-badge&logo=file&logoColor=white) ![C#](https://custom-icon-badges.demolab.com/badge/Language-C%23-brightgreen?style=for-the-badge&logo=command-palette&logoColor=white) ![App Type](https://custom-icon-badges.demolab.com/badge/Application%20Type-Game-blue?style=for-the-badge&logo=terminal&logoColor=white) ![Game Engine](https://custom-icon-badges.demolab.com/badge/Game%20Engine-Unity-purple?style=for-the-badge&logo=controller&logoColor=white)

<details open> 
<summary><h2>🗒️ Project Description</h2></summary>

<br />
Several Snek! is a remake of the classic Snake games that I did as a fun project! Along with your snake, called a snek in this game, all of the classic rules apply! You can't hit yourself or you die! Eating apples will increase the length of your snek by one!  Along with these and depending on how you want to play, you can also enable/disable classic features such as map wrapping (where you go off one side of the map and appear on the opposite side) for a more or less challenging experience!

<br /><br />Along with the classic snake experience, more fun features have been added!

<br /><br />This project was created to familiarize myself with a multiplayer-focused game development architecture within the Unity game engine.
Some aspects of this project include:
  - Utilization of Unity's Lobby system and multiplayer services to allow games to be hosted and joined without the need to port forward.
  - Client Authoritative networking
  - Custom data handling and snycronization for the player characters over the network
  - Networked player customization (color)
  - Proper network error handling (i.e. host disconnect)
  - Utilizing multiple scenes within a networked game

There are some additional elements that were a part of this project, such as basic map generation, powerups, and different game-modes, but the multiplayer networking was the primary focus.

<br />

<h2>⬇️ Completed Project Download</h2>
This project is fully playable and available on my itch.io page! <br />
It can be viewed and downloaded here: https://coba-platinum.itch.io/severalsnek

<br /><br />

*Coba Platinum is an alias that I have used since highscool when releasing anything to the public!

<br />

<h2>📦 Project Repository</h2>
This project is fully available in this repository!
</details>

<br/>

<p float="left">
  <img src="Screenshots/Image Sequence_009_3840x2160.png" width="49%" />
  <img src="Screenshots/Image Sequence_010_3840x2160.png" width="49%" />
</p>
<p float="left">
  <img src="Screenshots/Image Sequence_012_3840x2160.png" width="49%" />
  <img src="Screenshots/Image Sequence_015_3840x2160.png" width="49%" />
</p>

<details open> 
<summary><h2>🛠️ Tools Used</h2></summary>

<br />

<img align="center" alt="Unity" width="40px" style="padding-right:10px;padding-bottom:10px;" src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/unity/unity-original.svg"/> [Unity] - Primary Game Engine

<img align="center" alt="Git" width="40px" style="padding-right:10px;padding-bottom:10px;" src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/git/git-original.svg"/> [Git] - Source Control

</details> 

<br />

<p float="left">
  <img src="Screenshots/GameOptions.png" width="49%" />
  <img src="Screenshots/Image Sequence_001_3840x2160.png" width="49%" />
</p>
<p float="left">
  <img src="Screenshots/Image Sequence_008_3840x2160.png" width="49%" />
  <img src="Screenshots/Image Sequence_011_3840x2160.png" width="49%" />
</p>
<p float="left">
  <img src="Screenshots/Image Sequence_013_3840x2160.png" width="49%" />
  <img src="Screenshots/Image Sequence_014_3840x2160.png" width="49%" />
</p>
<p align="center">
  <img src="Screenshots/Image Sequence_016_3840x2160.png" width="49%" />
</p>

<details open> 
<summary><h2>🧠 Project Reflection</h2></summary>

- **How would you describe the process of creating and polishing the artifact?**
  
I feel that creating and polishing this artifact was very enjoyable and challenging, as I had to learn a completely different game architecture to create a fun and engaging multiplayer experience. I started with a basic idea of what I wanted to do, which was a multiplayer version of the classic snake game, and began from there. I first created a singleplayer prototype of the game, allowing my to plan and develop the different game mechanics for the game, followed by implementing working multiplayer into the game. Though this is not the recommended method of creating a multiplayer game, as it takes a lot more work, it did allow me to get a full understanding of the differences between crafting a singleplayer and multiplayer experience from behind the scenes.
  
- **What did you learn as you were creating it and improving it?**

This project gave me a chance to learn the many systems and techniques that make a multiplayer game possible. I was able to gain knowledge with with different game-related multiplayer architectures, such as peer-to-peer, client-server, etc. and when their strengths and weaknesses for different play styles within a game. I was able to learn how clients communicate with the server, and vice versa, in order to sync the game state for all players, whether that be player locations, enemy locations, animations, player statistics, and more. Whilw working on this project, I had to completely rethink about how game logic clasically works within a singleplayer experience, as a lot of different implementations for one more than likely will not work for the other. I also gained experience with Unity's multiplayer tools, specifically netcode for game objects and their lobby system, in order to complete this project.

- **What challenges did you face?**

As mentioned in the previous section, majority of the challenges faced within the development of this game were related to having to completely rethink how the logic within a game should be implemented during development. Having to decide what information should be client-side vs server-side, what information should be only calculated on the clients, what information should be sent to/from the server, and more were all very challenging experiences in order to make a fully functional and optimized/effecient multiplayer game.

- **How could this artifact be improved?**

There are many different aspects of this project that I feel could be improved, but if I were to work on the project again, my top priority would probably be to swith the networking from a client-authoritative to a server-authoritative model. I initially decided to develop with a client-authoritative model due to the lower amount of complexity when developming and implementing the networking logic for the game, but this eventually led to some unintended consequences, especially wth collision detection for the different player characters (the snakes). Switching to a server-authoritative model where the player movement and collision is all calculated on the server and then synced to the connected clients would essentially solve a lot of the issues that I ended up encountering during the course of developing this project!
</details> 

<br />

<p align="center">
  <img src="LiamRandLogo.png" width="10%" />
</p>
