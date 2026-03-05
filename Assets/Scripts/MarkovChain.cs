using System;
using UnityEngine;

public class MarkovChain : MonoBehaviour
{
    private MarkovState _rainyState = new MarkovState("Rainy");
    private MarkovState _sunnyState = new MarkovState("Sunny");

    private MarkovState _currentState;

    private void Start()
    {
        _rainyState.AddLink(new MarkovLink { NextState = _sunnyState, Probability = 0.5f });
        _rainyState.AddLink(new MarkovLink { NextState = _rainyState, Probability = 0.5f });

        _sunnyState.AddLink(new MarkovLink { NextState = _sunnyState, Probability = 0.9f });
        _sunnyState.AddLink(new MarkovLink { NextState = _rainyState, Probability = 0.1f });


        _currentState = _sunnyState;
        
        for (var i = 0; i < 1000; i++)
        {
            Debug.Log(_currentState.Name);
            _currentState = _currentState.NextState();
        }
    }
}