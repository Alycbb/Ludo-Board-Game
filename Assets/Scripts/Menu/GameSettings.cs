using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    //-----------------YELLOW-------------------
    public void SetYellowHumanType(bool on)
    {
        if (on) SaveSettings.players[0] = "HUMAN";
    }

    public void SetYellowCPUType(bool on)
    {
        if (on) SaveSettings.players[0] = "CPU";
    }

    //-----------------RED-------------------
    public void SetRedHumanType(bool on)
    {
        if (on) SaveSettings.players[1] = "HUMAN";
    }

    public void SetRedCPUType(bool on)
    {
        if (on) SaveSettings.players[1] = "CPU";
    }

    //-----------------BLUE-------------------
    public void SetBlueHumanType(bool on)
    {
        if (on) SaveSettings.players[2] = "HUMAN";
    }

    public void SetBlueCPUType(bool on)
    {
        if (on) SaveSettings.players[2] = "CPU";
    }

    //-----------------GREEN-------------------
    public void SetGREENHumanType(bool on)
    {
        if (on) SaveSettings.players[3] = "HUMAN";
    }

    public void SetGREENCPUType(bool on)
    {
        if (on) SaveSettings.players[3] = "CPU";
    }
}


public static class SaveSettings
{
    //  0      1    2    3
    // YELLOW RED BLUE GREEN
    public static string[] players = new string[4];

    public static string[] winners = new string[3]{ string.Empty, string.Empty, string.Empty};
}
