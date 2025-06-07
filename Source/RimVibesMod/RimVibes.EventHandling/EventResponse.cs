using System;
using System.Text;
using Common;
using Verse;
using Random = UnityEngine.Random;

namespace RimVibes.EventHandling;

public class EventResponse
{
    private static readonly StringBuilder str = new StringBuilder();

    public EventType ActivatedUpon;

    public float DrawHeight;

    public bool IsEnabled = true;

    public bool IsPlaylist;

    public bool RandomFromPlaylist = true;

    public ResponseType ResponseType;

    public string SpotifyID = "";

    public void Run()
    {
        if (!IsEnabled)
        {
            return;
        }

        switch (ResponseType)
        {
            case ResponseType.Pause_Music:
                RimVibesMod.TrySendExecute(delegate(NetData msg)
                {
                    msg.Write((byte)1);
                    msg.Write(true);
                    msg.Write(false);
                });
                break;
            case ResponseType.Play_Music:
            {
                var id = SpotifyID.Trim();
                if (string.IsNullOrWhiteSpace(id))
                {
                    break;
                }

                if (!IsPlaylist)
                {
                    RimVibesMod.TrySendExecute(delegate(NetData msg)
                    {
                        msg.Write((byte)17);
                        msg.Write(id);
                    });
                }
                else if (RandomFromPlaylist)
                {
                    RimVibesMod.TrySendRequest(delegate(NetData outMsg)
                    {
                        outMsg.Write((byte)18);
                        outMsg.Write(id);
                    }, delegate(RequestResult inResult, NetData inMsg)
                    {
                        if (inResult != 0)
                        {
                            Log.Warning(
                                $"Failed to get info about playlist for ID {id}: request failed with code {inResult}.");
                        }
                        else if (!inMsg.ReadBoolean())
                        {
                            Log.Warning(
                                $"Failed to get info about playlist for ID {id}: playlist does not exist, or is not a playlist (cannot be album).");
                        }
                        else
                        {
                            var arg = inMsg.ReadString();
                            var num = inMsg.ReadInt32();
                            //Log.Message($"Got info about playlist: '{arg}' with {num} items.");
                            var randomItem = Random.Range(0, num);
                            RimVibesMod.TrySendExecute(delegate(NetData msg)
                            {
                                msg.Write((byte)14);
                                msg.Write(id);
                                msg.Write(randomItem);
                                msg.Write(0);
                            });
                        }
                    });
                }
                else
                {
                    RimVibesMod.TrySendExecute(delegate(NetData msg)
                    {
                        msg.Write((byte)14);
                        msg.Write(id);
                        msg.Write(0);
                        msg.Write(0);
                    });
                }

                break;
            }
            case ResponseType.None:
                break;
        }
    }

    public string Serialize()
    {
        str.Clear();
        str.Append(ActivatedUpon.ToString());
        str.Append(',');
        str.Append(ResponseType.ToString());
        str.Append(',');
        str.Append(IsPlaylist.ToString());
        str.Append(',');
        str.Append(RandomFromPlaylist.ToString());
        str.Append(',');
        str.Append(SpotifyID);
        str.Append(',');
        str.Append(IsEnabled.ToString());
        return str.ToString();
    }

    public void Deserialize(string data)
    {
        var array = data.Split(',');
        ActivatedUpon = (EventType)Enum.Parse(typeof(EventType), array[0]);
        ResponseType = (ResponseType)Enum.Parse(typeof(ResponseType), array[1]);
        IsPlaylist = bool.Parse(array[2]);
        RandomFromPlaylist = bool.Parse(array[3]);
        SpotifyID = array[4];
        IsEnabled = bool.Parse(array[5]);
    }
}