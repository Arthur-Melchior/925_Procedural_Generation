using System.Collections.Generic;
using UnityEngine;

public struct MarkovLink
{
    public float Probability;
    public MarkovState NextState;
}

public class MarkovState
{
    private List<MarkovLink> _links;

    public string Name { get; }

    public MarkovState(string name)
    {
        Name = name;
        _links = new List<MarkovLink>();
    }

    public void AddLink(MarkovLink link)
    {
        if (_links.Exists(l => l.NextState == link.NextState)) return;
        _links.Add(link);
    }

    public MarkovState NextState()
    {
        var rng = Random.value;
        var rngSum = 0f;
        foreach (var link in _links)
        {
            if (rng < link.Probability + rngSum)
            {
                return link.NextState;
            }

            rngSum += link.Probability;
        }

        return null;
    }
}