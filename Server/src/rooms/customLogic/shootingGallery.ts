import { Client } from "colyseus";
import { ColyseusRoomState } from "../schema/ColyseusRoomState";
import { ShootingGalleryRoom } from "../ShootingGalleryRoom";

const logger = require("../../helpers/logger");

// string identifiers for keys in attributes
const CurrentState = "currentGameState";
const LastState = "lastGameState";
const GeneralMessage = "generalMessage";
const ClientReadyState = "readyState";

/** Enum for game state */
const ServerGameState = {
  None: "None",
  Waiting: "Waiting", // When a room first starts up this is the state it will be in
};

/** Reference to the room options that get passed in at time of Initialization. */
let roomOptions: any;

/**
 * The primary game loop on the server
 * @param {*} roomRef Reference to the Room object
 * @param {*} deltaTime Expects deltaTime to be in seconds, not milliseconds
 */
let gameLoop = function (roomRef: ShootingGalleryRoom, deltaTime: number) {
  // Update the game state
  switch (getGameState(roomRef, CurrentState)) {
    case ServerGameState.None:
      break;
    case ServerGameState.Waiting:
      waitingLogic(roomRef, deltaTime);
      break;
    default:
      logger.error(
        `Unknown Game State - ${getGameState(roomRef, CurrentState)}`
      );
      break;
  }
};

// Client Request Logic
// These functions get called by the client in the form of the "customMethod" message set up in the room.
//======================================
/**
 * Track a client scoring a target
 * @param {*} roomRef Reference to the room
 * @param {*} client The Client reporting the target score
 * @param {*} request Data including which client and which target
 */
const customMethods: any = {};
customMethods.setSyncInputs = function (
  roomRef: ShootingGalleryRoom,
  client: Client,
  request: any
) {
  const param = request.param;
  const inputs = param[0];
  roomRef.broadcast("syncData", inputs);
};

customMethods.syncAudio = function (
  roomRef: ShootingGalleryRoom,
  client: Client,
  request: any
) {
  const param = request.param;
  const inputs = param[0];
  roomRef.broadcast("syncAudio", inputs);
};

customMethods.itemInteract = function (
  roomRef: ShootingGalleryRoom,
  client: Client,
  request: any
) {
  const param = request.param;
  const inputs = param[0];
  roomRef.broadcast("itemInteract", inputs);
};

customMethods.itemGrab = function (
  roomRef: ShootingGalleryRoom,
  client: Client,
  request: any
) {
  const param = request.param;
  const inputs = param[0];
  roomRef.broadcast("itemGrab", inputs);
};
//====================================== END Client Request Logic

// GAME LOGIC
//======================================

/**
 * Checks if all the connected clients have a 'readyState' of "ready"
 * @param {*} users The collection of users from the room's state
 */
let checkIfUsersReady = function (users: ColyseusRoomState["networkedUsers"]) {
  let playersReady = true;
  for (let entry of Array.from(users.entries())) {
    let readyState = entry[1].attributes.get(ClientReadyState);

    if (readyState == null || readyState != "ready") {
      playersReady = false;
      break;
    }
  }

  return playersReady;
};

/**
 * Sets attriubte of the room
 * @param {*} roomRef Reference to the room
 * @param {*} key The key for the attribute you want to set
 * @param {*} value The value of the attribute you want to set
 */
let setRoomAttribute = function (
  roomRef: ShootingGalleryRoom,
  key: string,
  value: string
) {
  roomRef.state.attributes.set(key, value);
};
/**
 * Move the server game state to the new state
 * @param {*} roomRef Reference to the room
 * @param {*} newState The new state to move to
 */
let moveToState = function (roomRef: ShootingGalleryRoom, newState: string) {
  // LastState = CurrentState
  setRoomAttribute(roomRef, LastState, getGameState(roomRef, CurrentState));

  // CurrentState = newState
  setRoomAttribute(roomRef, CurrentState, newState);
};
/**
 * Returns the game state of the server
 * @param {*} roomRef Reference to the room
 * @param {*} gameState Key for which game state you want, either the Current game state for the Last game state
 */
let getGameState = function (roomRef: ShootingGalleryRoom, gameState: string) {
  return roomRef.state.attributes.get(gameState);
};

//======================================

// GAME STATE LOGIC
//======================================

/**
 * The logic run when the server is in the Waiting state
 * @param {*} roomRef Reference to the room
 * @param {*} deltaTime Server delta time in seconds
 */
let waitingLogic = function (roomRef: ShootingGalleryRoom, deltaTime: number) {
  let playersReady = false;
  // Switch on LastState since the waiting logic gets used in multiple places
  // Check if minimum # of clients to start a round exist
  const currentUsers = roomRef.state.networkedUsers.size;
  let maxPlayer = roomRef.maxClients;
  if (currentUsers < maxPlayer) {
    // Set room general message saying we're waiting for enough players to join the room
    roomRef.state.attributes.set(
      GeneralMessage,
      `Waiting for more players to join - (${currentUsers}/${maxPlayer})`
    );
    return;
  }

  // Now that we have enough players to start a round
  // check if all the users are ready to receive targets
  playersReady = checkIfUsersReady(roomRef.state.networkedUsers);

  // Return out if not all of the players are ready yet.
  if (playersReady == false) return;
  // Lock the room to prevent any more players from joining until after this round has ended
  roomRef.lock();
  // Time to send targets to the clients
  roomRef.broadcast("beginRound", {});
  moveToState(roomRef, ServerGameState.None);
};

//====================================== END GAME STATE LOGIC

// Room accessed functions
//======================================
/**
 * Initialize the Shooting Gallery logic
 * @param {*} roomRef Reference to the room
 * @param {*} options Options of the room from the client when it was created
 */
exports.InitializeLogic = function (
  roomRef: ShootingGalleryRoom,
  options: any
) {
  roomOptions = options;
  // Set initial game state to waiting for all clients to be ready
  setRoomAttribute(roomRef, CurrentState, ServerGameState.Waiting);
  setRoomAttribute(roomRef, LastState, ServerGameState.None);
};

/**
 * Run Game Loop Logic
 * @param {*} roomRef Reference to the room
 * @param {*} deltaTime Server delta time in milliseconds
 */
exports.ProcessLogic = function (
  roomRef: ShootingGalleryRoom,
  deltaTime: number
) {
  gameLoop(roomRef, deltaTime / 1000); // convert deltaTime from ms to seconds
};

/**
 * Processes requests from a client to run custom methods
 * @param {*} roomRef Reference to the room
 * @param {*} client Reference to the client the request came from
 * @param {*} request Request object holding any data from the client
 */
exports.ProcessMethod = function (
  roomRef: ShootingGalleryRoom,
  client: Client,
  request: any
) {
  // Check for and run the method if it exists
  if (
    request.method in customMethods &&
    typeof customMethods[request.method] === "function"
  ) {
    customMethods[request.method](roomRef, client, request);
  } else {
    throw "No Method: " + request.method + " found";
    return;
  }
};

/**
 * Process report of a user leaving.
 */
exports.ProcessUserLeft = function (roomRef: ShootingGalleryRoom) {};
//====================================== END Room accessed functions
