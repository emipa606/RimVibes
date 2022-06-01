using System;

namespace Common;

public static class Net
{
    public const byte PING = 0;

    public const byte PAUSE_OR_RESUME = 1;

    public const byte PLAYBACK_DATA = 2;

    public const byte SHUTDOWN = 3;

    public const byte AUTH_ERROR = 4;

    public const byte RETRY_LOGIN = 5;

    public const byte ENABLE_SEND_STATE = 6;

    public const byte DISABLE_SEND_STATE = 7;

    public const byte TOGGLE_SHUFFLE = 8;

    public const byte CYCLE_REPEAT = 9;

    public const byte NEXT_TRACK = 10;

    public const byte PREVIOUS_TRACK = 11;

    public const byte SEEK = 12;

    public const byte SET_VOLUME = 13;

    public const byte PLAY_PLAYLIST_OR_ALBUM = 14;

    public const byte FADE_OUT = 15;

    public const byte FADE_IN = 16;

    public const byte PLAY_SPECIFIC = 17;

    public const byte REQUEST_PLAYLIST_INFO = 18;

    public static event Action<string> OnInternalError;

    public static event Action<string> OnTrace;

    internal static void Trace(string text)
    {
        OnTrace?.Invoke(text);
    }

    internal static void LogInternalError(string text)
    {
        OnInternalError?.Invoke(text);
    }
}