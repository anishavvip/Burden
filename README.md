# Technical Documentation
The multiplayer tool used is Colyseus. It runs on a Node.js server, with the backend written in typescript. 

The game is coded to have an auto join, and allocation of rooms with respect to the avatar types. For example; if a player has joined as a mother, they would get allocated to a player who has joined as a child. Just like life, which is unpredictable; I wanted the users to play the roles without being given the choice to create specific rooms to play with friends.

The game uses TTS to avoid heavy load of audio clips, and enhance flexibility of change of dialogues. 

The game only starts when both players are present in the room and game ends when either, quit the game. Mom and child are codependent, hence the idea behind closing the room if  a player left. 

Once both the players have joined, they are placed in the environment. All the movement, inputs, interactions, animations, sounds, physics based actions, non-physics based action are synchronized via the communication between the server.