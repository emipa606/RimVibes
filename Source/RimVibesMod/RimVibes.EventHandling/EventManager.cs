using System;

namespace RimVibes.EventHandling;

public static class EventManager
{
    public static event Action<EventType> OnEvent;

    internal static void PostEvent(EventType type)
    {
        OnEvent?.Invoke(type);
    }
}