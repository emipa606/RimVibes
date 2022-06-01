using System;
using System.Collections.Generic;
using System.Text;

namespace RimVibes.EventHandling;

public class Responses
{
    private static readonly StringBuilder str = new StringBuilder();

    public List<EventResponse> All = new List<EventResponse>();

    public void Deserialize(string data)
    {
        All.Clear();
        var array = data.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var data2 in array)
        {
            var eventResponse = new EventResponse();
            eventResponse.Deserialize(data2);
            All.Add(eventResponse);
        }
    }

    public string Serialize()
    {
        str.Clear();
        for (var i = 0; i < All.Count; i++)
        {
            var eventResponse = All[i];
            if (eventResponse == null)
            {
                continue;
            }

            str.Append(eventResponse.Serialize());
            if (i != All.Count - 1)
            {
                str.Append('|');
            }
        }

        return str.ToString();
    }

    public void Handle(EventType type, bool firstOnly)
    {
        if (type == EventType.None)
        {
            return;
        }

        foreach (var item in All)
        {
            if (item == null || item.ActivatedUpon != type)
            {
                continue;
            }

            item.Run();
            if (firstOnly)
            {
                break;
            }
        }
    }
}