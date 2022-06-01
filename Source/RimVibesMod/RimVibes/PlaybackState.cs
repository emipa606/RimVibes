using Common;

namespace RimVibes;

public class PlaybackState
{
    public int CurrentPositionMS;

    public Device Device = new Device();
    public bool IsPlaying;

    public Item Item = new Item();

    public int ProgressMS;

    public string RepeatState;

    public bool ShuffleState;

    public void Deserialize(NetData data)
    {
        IsPlaying = data.ReadBoolean();
        ProgressMS = data.ReadInt32();
        RepeatState = data.ReadString();
        ShuffleState = data.ReadBoolean();
        CurrentPositionMS = data.ReadInt32();
        if (data.ReadBoolean())
        {
            Device.IsActual = true;
            Device.Name = data.ReadString();
            Device.VolumePercentage = data.ReadInt32();
        }
        else
        {
            Device.IsActual = false;
        }

        if (data.ReadBoolean())
        {
            Item.IsActual = true;
            Item.Name = data.ReadString();
            Item.ArtistName = data.ReadString();
            Item.TracKID = data.ReadString();
            Item.LengthMS = data.ReadInt32();
            if (data.ReadBoolean())
            {
                Item.Album.IsActual = true;
                Item.Album.HasImage = data.ReadBoolean();
                if (!Item.Album.HasImage)
                {
                    return;
                }

                Item.Album.ImageURL = data.ReadString();
                Item.Album.ImageWidth = data.ReadInt32();
                Item.Album.ImageHeight = data.ReadInt32();
            }
            else
            {
                Item.Album.IsActual = false;
            }
        }
        else
        {
            Item.IsActual = false;
        }
    }
}