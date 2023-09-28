using System;

[Serializable]
public class ShootData
{
    public bool isTrigger = false;
    public string name;
}

public class PauseStatus
{
    public string name;
    public bool isPaused = false;
}