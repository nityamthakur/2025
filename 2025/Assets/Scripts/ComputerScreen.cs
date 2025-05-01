using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ComputerScreen : MonoBehaviour
{
    private List<Action> programs;
    private Sprite computerImage;

    private void Start()
    {
        programs = new List<Action>
        {
            () => ClockIn(),
            () => ClockOut(),
            () => EMail(),
            () => HackMenu(),
            () => Background()
        };
    }

    private void ComputerStartUp()
    {

    }

    private void ComputerShutDown()
    {

    }

    private void ClockIn()
    {

    }

    private void ClockOut()
    {

    }

    private void EMail()
    {

    }

    private void HackMenu()
    {

    }

    private void Background()
    {

    }
}
