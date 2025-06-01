using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialManager : MonoBehaviour
{
    public Builder player_builder;

    public UnityEvent events_place;
    

    public void AddPlaceEvents()
    {
        player_builder.events_place.AddListener(events_place.Invoke);
    }

    public void AddConveyorMessage(TextBox textBox)
    {
        player_builder.events_place.AddListener(() =>
        {
            SetConveyorMessage(textBox);
        });
    }
    public void SetConveyorMessage(TextBox textBox)
    {
        textBox.SetText("Place <color=blue>conveyors</color> to transport materials");
        textBox.ShowText();

        player_builder.events_place.AddListener(() =>
        {
            textBox.SetText("Place a <color=blue>core</color> to store your minerals. Your core is your most important structure.");
            textBox.ShowText();
        });
    }
}