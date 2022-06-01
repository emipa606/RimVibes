using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Common;

public class AppConnection : IDisposable
{
    private Dictionary<ushort, RemoteRequest> activeRequests = new Dictionary<ushort, RemoteRequest>();

    private ushort lastID = 1;

    public Peer peer;

    private Stopwatch timer = new Stopwatch();

    private Queue<ushort> toTimeOut = new Queue<ushort>();

    public AppConnection(int writePort, int readPort, ushort appVerification)
    {
        peer = new Peer(writePort, readPort, appVerification);
        peer.OnMessageIn += OnMessageIn;
    }

    public int ReadPort => peer?.ReadPort ?? 0;

    public int WritePort => peer?.WritePort ?? 0;

    public int ActiveRequestCount => activeRequests?.Count ?? 0;

    public int RequestTimeout { get; set; } = 8000;

    public void Dispose()
    {
        if (peer == null)
        {
            return;
        }

        Stop();
        activeRequests.Clear();
        activeRequests = null;
        toTimeOut = null;
        timer = null;
        peer?.Dispose();
        peer = null;
    }


    public event Action<NetData> HandleExecute;

    public event Func<NetData, Action<NetData>> HandleRequest;

    private void OnMessageIn(NetData msg)
    {
        var num = msg.ReadBoolean();
        if (num)
        {
            var id = msg.ReadUInt16();
            var response = HandleRequest?.Invoke(msg);
            var result = RequestResult.Success;
            if (response == null)
            {
                result = RequestResult.ErrorReceivedButNoResponse;
            }

            peer.SendMessage(delegate(NetData outMsg)
            {
                outMsg.Write(false);
                outMsg.Write(true);
                outMsg.Write(id);
                outMsg.Write((byte)result);
                response?.Invoke(outMsg);
            });
        }
        else if (!msg.ReadBoolean())
        {
            HandleExecute?.Invoke(msg);
        }
        else
        {
            var num2 = msg.ReadUInt16();
            var arg = (RequestResult)msg.ReadByte();
            if (activeRequests.ContainsKey(num2))
            {
                var remoteRequest = activeRequests[num2];
                activeRequests.Remove(num2);
                remoteRequest.Action?.Invoke(arg, msg);
            }
            else
            {
                Net.LogInternalError(
                    $"Response with ID {num2} arrived but the response action was not found, it must have timed out.");
            }
        }
    }

    public void Start()
    {
        peer?.Start();
    }

    public void Update()
    {
        if (RequestTimeout <= 0 || peer == null)
        {
            return;
        }

        timer.Stop();
        foreach (var activeRequest in activeRequests)
        {
            var value = activeRequest.Value;
            value.TimeActive += timer.Elapsed.TotalMilliseconds;
            if (value.TimeActive >= RequestTimeout)
            {
                toTimeOut.Enqueue(activeRequest.Key);
            }
        }

        timer.Restart();
        while (toTimeOut.Count > 0)
        {
            var key = toTimeOut.Dequeue();
            activeRequests[key].Action?.Invoke(RequestResult.ErrorTimedOut, null);
            activeRequests.Remove(key);
        }
    }

    public void Stop()
    {
        peer?.Stop();
    }

    public void SendRequest(Action<NetData> makeMessage, Action<RequestResult, NetData> onResponse)
    {
        var id = GetNewRequestID();
        if (id == 0)
        {
            Net.LogInternalError(
                "Ran out of request ID's to use! Send requests less often, give the other end time to respond!");
            return;
        }

        var remoteRequest = new RemoteRequest
        {
            Action = onResponse,
            TimeActive = 0.0
        };
        activeRequests.Add(id, remoteRequest);
        peer.SendMessage(delegate(NetData msg)
        {
            msg.Write(true);
            msg.Write(false);
            msg.Write(id);
            makeMessage(msg);
        });
    }

    public void SendExecute(Action<NetData> makeMessage)
    {
        peer.SendMessage(delegate(NetData msg)
        {
            msg.Write(false);
            msg.Write(false);
            makeMessage(msg);
        });
    }

    private ushort GetNewRequestID()
    {
        var num = lastID;
        for (var i = 0; i < 65535; i++)
        {
            if (activeRequests.ContainsKey(num))
            {
                num = (ushort)(num + 1);
                if (num == ushort.MaxValue)
                {
                    num = 1;
                }

                continue;
            }

            lastID = num;
            return num;
        }

        return 0;
    }

    private class RemoteRequest
    {
        public Action<RequestResult, NetData> Action;

        public double TimeActive;
    }
}