﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject waitingUI = null;
    [SerializeField]
    private TextMeshProUGUI waitingText = null;

    [SerializeField]
    private Button exitButton = null;

    [SerializeField]
    private Button readyButton = null;

    private void Start()
    {
        waitingUI.SetActive(true);
    }
    public void UpdatePlayerReadiness(bool showButton)
    {
        readyButton.gameObject.SetActive(showButton);
    }
    public void AllPlayersHaveJoined()
    {
        waitingUI.SetActive(false);
    }
    public void SetWaitingText()
    {
        string text;
        if (ExampleManager.Instance.Avatar == Avatars.Mom)
            text = $"{Avatars.Child}, you there?";
        else 
            text = $"{Avatars.Mom}, you there?";
        waitingText.text = text;
    }
    public void AllowExit(bool allowed)
    {
        exitButton.gameObject.SetActive(allowed);
    }

    public void ButtonOnReady()
    {
        GalleryGameManager.Instance.PlayerReadyToPlay();
    }

    public void ButtonOnExit()
    {
        GalleryGameManager.Instance.OnQuitGame();
    }
}