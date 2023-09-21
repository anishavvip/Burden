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
    public bool left = false, right = false, up = false, down = false, jump = false,sprint = false, pause = false;
    public float mouseX, mouseY;
}