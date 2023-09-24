using Colyseus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Avatars
{
    Mom, Child
}
public class LobbyController : MonoBehaviour
{
    string roomID = "";
    [SerializeField]
    private GameObject container = null;
    [SerializeField]
    private GameObject connectingCover = null;
    [SerializeField]
    private GameObject errorConnectingCover = null;
    //Variables to initialize the room controller
    string roomName = "BurdenRoom";
    ColyseusRoomAvailable[] rooms;

    private void Awake()
    {
        connectingCover.SetActive(true);
        errorConnectingCover.SetActive(false);
    }

    private IEnumerator Start()
    {
        container.SetActive(true);
        roomID = "";
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        while (!ExampleManager.IsReady)
        {
            yield return new WaitForEndOfFrame();
        }

        Dictionary<string, object> roomOptions = new Dictionary<string, object>
        {
            ["logic"] = "Burden", //The name of our custom logic file
        };

        ExampleManager.Instance.Initialize(roomName, roomOptions);

        ExampleManager.onRoomsReceived += OnRoomsReceived;
        CreateUser();
    }

    private void OnDestroy()
    {
        ExampleManager.onRoomsReceived -= OnRoomsReceived;
    }

    public void CreateUser()
    {
        string desiredUserName = Guid.NewGuid().ToString();

        Debug.Log(desiredUserName);

        PlayerPrefs.SetString("UserName", desiredUserName);
        ExampleManager.Instance.UserName = desiredUserName;
        //Do user creation stuff

        ExampleManager.Instance.GetAvailableRooms();

        connectingCover.SetActive(false);
    }
    public void AssignRoom(string avatar)
    {
        try
        {
            TextToSpeech.Instance.StopAudio();
            connectingCover.SetActive(true);
            string oppositeRoomId = GetOppositeID(avatar);
            if (rooms != null)
                if (rooms.Length > 0)
                {
                    foreach (var room in rooms)
                    {
                        if (room.roomId.Contains(avatar))
                        {
                            JoinRoom(room.roomId);
                            roomID = room.roomId;
                            return;
                        }
                    }
                }

            CreateRoom(oppositeRoomId + "_" + ExampleManager.Instance.UserName);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            errorConnectingCover.SetActive(true);
            SceneManager.LoadScene("Lobby");
        }
    }

    private static string GetOppositeID(string avatar)
    {
        string oppositeRoomId;
        if (avatar == Avatars.Mom.ToString())
        {
            //Mom
            oppositeRoomId = Avatars.Child.ToString();
            ExampleManager.Instance.Avatar = Avatars.Mom;
        }
        else
        {
            //Child
            oppositeRoomId = Avatars.Mom.ToString();
            ExampleManager.Instance.Avatar = Avatars.Child;
        }

        return oppositeRoomId;
    }
    public void CreateRoom(string id)
    {
        LoadGallery(() => { ExampleManager.Instance.CreateNewRoom(id); });
    }

    public void JoinRoom(string id)
    {
        LoadGallery(() => { ExampleManager.Instance.JoinExistingRoom(id); });
    }

    public void OnConnectedToServer()
    {
        Debug.Log("OnConnectedToServer");
    }

    private void OnRoomsReceived(ColyseusRoomAvailable[] rooms)
    {
        this.rooms = rooms;
    }

    private void LoadGallery(Action onComplete)
    {
        StartCoroutine(LoadSceneAsync("Main", onComplete));
    }

    private IEnumerator LoadSceneAsync(string scene, Action onComplete)
    {
        Scene currScene = SceneManager.GetActiveScene();
        AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        while (op.progress <= 0.9f)
        {
            //Wait until the scene is loaded
            yield return new WaitForEndOfFrame();
        }
        container.SetActive(false);
        onComplete.Invoke();
        op.allowSceneActivation = true;
        SceneManager.UnloadSceneAsync(currScene);
    }
}