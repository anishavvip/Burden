using Colyseus;
using Colyseus.Schema;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GalleryGameManager : MonoBehaviour
{
    [SerializeField] GameObject endScreen;
    private bool setWaitingText = false;
    private int maxEntities = 6;
    private string _countDownString = "";

    private bool _showCountdown;

    // State variables
    //============================
    public PlayerController mom, child;
    public GameUIController uiController;
    private string userReadyState = "";
    public static GalleryGameManager Instance { get; private set; }
    public AudioClip ouchClip;
    public enum eGameState
    {
        NONE,
        WAITING,
    }

    private eGameState currentGameState;
    private eGameState lastGameState;
    //============================

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        uiController.gameObject.SetActive(true);
    }

    private IEnumerator Start()
    {
        while (ExampleManager.Instance.IsInRoom == false)
        {
            yield return 0;
        }
        InvokeRepeating(nameof(HelpJoinPlayer), 1, 1f);
    }

    //Subscribe to messages that will be sent from the server
    private void OnEnable()
    {
        ExampleRoomController.onAddNetworkEntity += OnNetworkAdd;
        ExampleRoomController.onRemoveNetworkEntity += OnNetworkRemove;
        ExampleRoomController.onRemoveRoom += OnQuitGame;
        ExampleRoomController.OnCurrentUserStateChanged += OnUserStateChanged;
        ExampleRoomController.onEnd += End;
        uiController.AllowExit(true);
    }

    private void End()
    {
        endScreen.gameObject.SetActive(true);
        mom.gameObject.SetActive(false);
        child.gameObject.SetActive(false);
        TextToSpeech.Instance.Refresh();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    //Unsubscribe
    private void OnDisable()
    {
        ExampleRoomController.onAddNetworkEntity -= OnNetworkAdd;
        ExampleRoomController.onRemoveNetworkEntity -= OnNetworkRemove;
        ExampleRoomController.onRemoveRoom -= OnQuitGame;
        ExampleRoomController.OnCurrentUserStateChanged -= OnUserStateChanged;
        ExampleRoomController.onEnd -= End;
    }

    private async void HelpJoinPlayer()
    {
        if (maxEntities != ExampleManager.Instance._roomController.Entities.Count)
        {
            if (!setWaitingText)
            {
                setWaitingText = true;
                uiController.SetWaitingText();
            }

            await Task.Delay((int)(1000 * Random.value) + 1);
            ExampleManager.Instance.GetAvailableRoomsToRejoin();
        }
        else
        {
            CancelInvoke(nameof(HelpJoinPlayer));
            uiController.AllPlayersHaveJoined();
        }
    }

    private string GetWinningMessage(Winner winner)
    {
        string winnerMessage = "";

        if (winner.tie)
        {
            winnerMessage = $"TIE!\nThese players tied with a top score of {winner.score}:\n";
            for (int i = 0; i < winner.tied.Length; i++)
            {
                PlayerController p = GetPlayerView(winner.tied[i]);
                if (p != null)
                {
                    winnerMessage += $"{(p ? p.userName : winner.tied[i])}\n";
                }
            }
        }
        else
        {
            PlayerController p = GetPlayerView(winner.id);
            if (p != null)
            {
                winnerMessage = $"Round Over!\n{(p ? p.userName : winner.id)} wins!";
            }
        }

        return winnerMessage;
    }

    private void OnUserStateChanged(MapSchema<string> attributeChanges)
    {
        if (attributeChanges.TryGetValue("readyState", out string readyState))
        {
            userReadyState = readyState;
        }
    }

    private string GetPlayerReadyState()
    {
        string readyState = "Waiting for you to ready up!";

        PlayerController player = GetPlayerView(ExampleManager.Instance.CurrentNetworkedEntity.id);
        if (player != null)
        {
            readyState = player.isReady ? "Waiting on other players..." : "Waiting for you to ready up!";
        }

        return readyState;
    }

    public bool AwaitingPlayerReady()
    {
        //Returns true if we're waiting for THIS player to be ready
        if (currentGameState == eGameState.WAITING)
        {
            return true;
        }

        return false;
    }

    private bool AwaitingAnyPlayerReady()
    {
        //Returns true if the server is waiting for anyone to be ready
        return currentGameState == eGameState.WAITING;
    }

    private void OnNetworkAdd(ExampleNetworkedEntity entity)
    {

    }

    private void OnNetworkRemove(ExampleNetworkedEntity entity, ColyseusNetworkedEntityView view)
    {
        if (view != null)
        {
            RemoveView(view);
        }
    }

    private void CreateView(ExampleNetworkedEntity entity)
    {
        StartCoroutine(WaitingEntityAdd(entity));
    }

    IEnumerator WaitingEntityAdd(ExampleNetworkedEntity entity)
    {
        PlayerController playerController;
        if (ExampleManager.Instance.Avatar == Avatars.Mom)
        {
            playerController = mom;
        }
        else
        {
            playerController = child;
        }
        PlayerController newView = Instantiate(playerController);
        mom.gameObject.SetActive(false);
        child.gameObject.SetActive(false);
        ExampleManager.Instance.RegisterNetworkedEntityView(entity, newView);
        newView.gameObject.SetActive(true);
        float seconds = 0;
        float delayAmt = 1.0f;
        //Wait until we have the view's username to add it's scoreboard entry
        while (string.IsNullOrEmpty(newView.userName))
        {
            yield return new WaitForSeconds(delayAmt);
            seconds += delayAmt;
            if (seconds >= 30) //If 30 seconds go by and we don't have a userName, should still continue
            {
                newView.userName = "GUEST";
            }
        }
    }

    private void RemoveView(ColyseusNetworkedEntityView view)
    {
        OnQuitGame();
        if (view == null) return;
        view.SendMessage("OnEntityRemoved", SendMessageOptions.DontRequireReceiver);
    }

    public void PlayerReadyToPlay()
    {
        ExampleManager.NetSend("setAttribute",
            new ExampleAttributeUpdateMessage
            {
                userId = ExampleManager.Instance.CurrentUser.id,
                attributesToSet = new Dictionary<string, string> { { "readyState", "ready" } }
            });

        PlayerController player = GetPlayerView(ExampleManager.Instance.CurrentNetworkedEntity.id);
        if (player != null)
        {
            player.UpdateReadyState(true);
        }
    }

    public PlayerController GetPlayerView(string entityID)
    {
        if (ExampleManager.Instance.HasEntityView(entityID))
        {
            return ExampleManager.Instance.GetEntityView(entityID) as PlayerController;
        }

        return null;
    }

    public void OnQuitRoomToJoinAnotherRoom()
    {
        if (ExampleManager.Instance.IsInRoom)
        {
            //Find playerController for this player
            PlayerController pc = GetPlayerView(ExampleManager.Instance.CurrentNetworkedEntity.id);
            if (pc != null)
            {
                pc.enabled = false; //Stop all the messages and updates
            }

            ExampleManager.Instance.LeaveAllRooms(null);
        }
    }
    public void OnQuitGame()
    {
        if (ExampleManager.Instance.IsInRoom)
        {
            //Find playerController for this player
            PlayerController pc = GetPlayerView(ExampleManager.Instance.CurrentNetworkedEntity.id);
            if (pc != null)
            {
                pc.enabled = false; //Stop all the messages and updates
            }
            
            TextToSpeech.Instance.Refresh();
            ExampleManager.Instance.LeaveAllRooms(() => { SceneManager.LoadScene("Lobby"); });
        }
    }

#if UNITY_EDITOR
    private void OnDestroy()
    {
        ExampleManager.Instance.OnEditorQuit();
    }
#endif
}