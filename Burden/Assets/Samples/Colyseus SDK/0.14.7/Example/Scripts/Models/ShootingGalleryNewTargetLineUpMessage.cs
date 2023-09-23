using System;

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
        sprint = false, pause = false, rightClicked = false;
    public float mouseX, mouseY, timestamp;
}
public class GrabDetails
{
    public string itemName;
    public string name;
}
public class ItemDetails
{
    public string itemName;
    public string name;
}