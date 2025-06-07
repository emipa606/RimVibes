using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Common;

public class Peer : IDisposable
{
    private readonly UdpClient client;

    private IPEndPoint localIP;

    private Thread t;

    public Peer(int writePort, int readPort, ushort appVerification)
    {
        ReadPort = readPort;
        WritePort = writePort;
        AppVerification = appVerification;
        localIP = new IPEndPoint(IPAddress.Any, ReadPort);
        client = new UdpClient();
        client.Client.Bind(localIP);
    }

    public int WritePort { get; }

    public int ReadPort { get; }

    public bool IsRunning { get; private set; }

    public ushort AppVerification { get; set; }

    public void Dispose()
    {
        client?.Client?.Disconnect(true);
        client?.Dispose();
    }

    public event Action<NetData> OnMessageIn;

    public void SendMessage(Action<NetData> constructMsg)
    {
        if (constructMsg == null)
        {
            return;
        }

        var netData = NetData.Create();
        netData.Write(AppVerification);
        constructMsg(netData);
        SendMessage(netData);
    }

    private void SendMessage(NetData data)
    {
        var array = data.ToArray();
        client.Send(array, array.Length, "localhost", WritePort);
        NetData.Recycle(data);
    }

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        IsRunning = true;
        t = new Thread(RunThread)
        {
            Name = "Net Peer Thread"
        };
        t.Start();
    }

    private void RunThread()
    {
        Net.Trace("Started read thread.");
        while (IsRunning)
        {
            byte[] array = null;
            try
            {
                array = client.Receive(ref localIP);
            }
            catch
            {
                // ignored
            }

            if (array == null)
            {
                continue;
            }

            var netData = NetData.Create(array);
            if (netData.ReadUInt16() != AppVerification)
            {
                Net.LogInternalError("Message came but verification is incorrect; conflicting port?");
            }
            else
            {
                try
                {
                    OnMessageIn?.Invoke(netData);
                }
                catch (Exception ex)
                {
                    Net.LogInternalError($"Exception handling message: {ex}");
                }
            }

            NetData.Recycle(netData);
        }

        Net.Trace("Stopped read thread.");
    }

    public void Stop()
    {
        client.Dispose();
        IsRunning = false;
    }
}