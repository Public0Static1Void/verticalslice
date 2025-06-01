using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
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

        UnityAction action = () =>
        {
            textBox.SetText("Place a <color=blue>core</color> to store your minerals. If your core is destroyed, <color=red>you lose.</color>");
            textBox.ShowText();

            StartCoroutine(WaitToAddEvent(textBox, () => { textBox.HideText(); }));
        };
        StartCoroutine(WaitToAddEvent(textBox, action));

    }

    private IEnumerator WaitToAddEvent(TextBox textBox, UnityAction action)
    {
        while (textBox.showing_text)
        {
            yield return null;
        }

        player_builder.events_place.AddListener(action);
    }
}