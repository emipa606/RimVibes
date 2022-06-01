using System;
using System.Collections.Generic;

namespace Common;

public class MessageHandler
{
    private readonly Dictionary<byte, Action<NetData>> processors = new Dictionary<byte, Action<NetData>>();

    private readonly Dictionary<byte, Func<NetData, Action<NetData>>> requestProcessors =
        new Dictionary<byte, Func<NetData, Action<NetData>>>();

    public MessageHandler(AppConnection connection)
    {
        connection.HandleExecute += OnMessageIn;
        connection.HandleRequest += OnRequestIn;
    }

    public bool WarnIfUnhandled { get; set; } = true;


    public event Action<byte, Exception> OnProcessorException;

    public void ClearHandlers()
    {
        processors.Clear();
        requestProcessors.Clear();
    }

    public void AddHandler(byte id, Action<NetData> processor)
    {
        if (!processors.ContainsKey(id))
        {
            processors.Add(id, processor);
        }
        else
        {
            processors[id] = processor;
        }
    }

    public void AddRequestHandler(byte id, Func<NetData, Action<NetData>> processor)
    {
        if (!requestProcessors.ContainsKey(id))
        {
            requestProcessors.Add(id, processor);
        }
        else
        {
            requestProcessors[id] = processor;
        }
    }

    private void OnMessageIn(NetData data)
    {
        var b = data.ReadByte();
        if (processors.ContainsKey(b))
        {
            try
            {
                processors[b]?.Invoke(data);
                return;
            }
            catch (Exception arg)
            {
                OnProcessorException?.Invoke(b, arg);
                return;
            }
        }

        if (WarnIfUnhandled)
        {
            Net.LogInternalError($"Message processor does not have a handler for execute ID {b}.");
        }
    }

    private Action<NetData> OnRequestIn(NetData data)
    {
        var b = data.ReadByte();
        if (requestProcessors.ContainsKey(b))
        {
            try
            {
                return requestProcessors[b]?.Invoke(data);
            }
            catch (Exception arg)
            {
                OnProcessorException?.Invoke(b, arg);
                return null;
            }
        }

        if (!WarnIfUnhandled)
        {
            return null;
        }

        Net.LogInternalError($"Message processor does not have a handler for request ID {b}.");
        return null;
    }
}