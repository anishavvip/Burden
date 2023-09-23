using System;
using UnityEngine;

public class ShootingGalleryNewTargetLineUpMessage
{
    public ShootingGalleryTargetModel[] targets;
}
[Serializable]
public class InputSyncData
{
    public string name = "";
    public float xPos, yPos, zPos, xRot, yRot, zRot, wRot;
    public bool left = false, right = false, up = false, down = false, jump = false,
        sprint = false, pause = false, rightClicked = false, leftHold = false;
    public float mouseX, mouseY, timestamp;
}
public class AudioDetails
{
    public string name;
    public string audioType;
}
public class GrabDetails
{
    public string name;
    public float xPos, yPos, zPos, xRot, yRot, zRot, wRot;
    public bool isDrag = false;
}
public class ItemDetails
{
    public string itemName;
    public string name;
}