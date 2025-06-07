using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Common;
using HarmonyLib;
using Mlie;
using RimVibes.Components;
using RimVibes.EventHandling;
using RimVibes.IO;
using RimVibes.Patches;
using RimVibes.RemoteApp;
using RimVibes.UI;
using RimVibes.Utils;
using UnityEngine;
using Verse;
using EventType = RimVibes.EventHandling.EventType;
using Object = UnityEngine.Object;
using ThreadPriority = System.Threading.ThreadPriority;

namespace RimVibes;

public class RimVibesMod : Mod
{
    private static string currentVersion;
    private bool runUpdateThread;

    private float sendPingTimer;
    private Vibe status;

    private float suppressLerp;

    private string xOffBuffer;

    private string xOffBuffer2;

    private string yOffBuffer;

    private string yOffBuffer2;

    public RimVibesMod(ModContentPack content)
        : base(content)
    {
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
        Instance = this;
        Settings = GetSettings<MyModSettings>();
        //Log.Message("_____________________");
        //Log.Message("");
        Log.Message("[RimVibes]: Starting up");
        //Log.Message("Creating hook game object.");
        var gameObject = new GameObject("RimVibes Hook Game Object");
        Object.DontDestroyOnLoad(gameObject);
        Hook = gameObject.AddComponent<HookComponent>();
        gameObject.AddComponent<AdditionalHook>();
        Hook.OnExit += OnRimworldExit;
        Net.OnTrace += delegate
        {
            //Log.Message($"[TRACE] {txt}");
        };
        Net.OnInternalError += delegate(string txt) { Log.Error($"[ERROR] {txt}"); };
        //Log.Message("Patching...");
        var harmony = new Harmony("com.github.Epicguru.RimVibes");
        harmony.PatchAll();
        //Log.Message($"Patched {harmony.GetPatchedMethods().Count()} methods.");
        //Log.Message("Extracting binaries...");
        if (!Decompresser.EnsureExtracted(Application.platform))
        {
            Log.Error($"CRITICAL: Failed to decompress platform binaries. Platform is {Application.platform}.");
        }

        //Log.Message("Launching update thread.");
        var thread = new Thread(RunUpdate)
        {
            Name = "RimVibes Update Thread",
            Priority = ThreadPriority.BelowNormal
        };
        runUpdateThread = true;
        thread.Start();
        //Log.Message("Creating internal network...");
        Connection = new AppConnection(7868, 7869, 46032);
        MessageHandler = new MessageHandler(Connection);
        MessageHandler.OnProcessorException += delegate(byte id, Exception ex)
        {
            Log.Warning($"Exception in message handler, ID {id}");
            Log.Warning(ex.ToString());
        };
        MessageHandler.AddHandler(0, Handlers.HandlePing);
        MessageHandler.AddHandler(2, Handlers.HandlePlaybackState);
        MessageHandler.AddHandler(4, Handlers.HandleAuthError);
        //Log.Message("Launching connection thread...");
        Connection.Start();
        EventManager.OnEvent += delegate(EventType e)
        {
            if (e == EventType.None)
            {
                return;
            }

            //Log.Message($"RimVibes Event: {e}");
            foreach (var item in Settings.Responses.All)
            {
                if (item is not { IsEnabled: true } || item.ActivatedUpon != e)
                {
                    continue;
                }

                item.Run();
                //Log.Message($"Run event action: {item.ResponseType}");
                break;
            }
        };
        //Log.Message("");
        //Log.Message("_____________________");
    }

    public static RimVibesMod Instance { get; private set; }

    public static bool HasShownMainMenu { get; private set; }

    public Vibe Status
    {
        get => status;
        internal set
        {
            if (value == status)
            {
                return;
            }

            var arg = status;
            status = value;
            OnStatusChanged?.Invoke(arg, status);
        }
    }

    public MyModSettings Settings { get; }

    public PlaybackState PlaybackState { get; } = new PlaybackState();


    public Stopwatch PingTimer { get; } = new Stopwatch();


    public AppConnection Connection { get; }

    public MessageHandler MessageHandler { get; }

    public HookComponent Hook { get; }

    public AppManager AppManager { get; } = new AppManager();


    public event Action<Vibe, Vibe> OnStatusChanged;

    public static bool TrySendExecute(Action<NetData> createMsg)
    {
        if (createMsg == null)
        {
            return false;
        }

        if (Instance.Status == Vibe.Disconnected)
        {
            return false;
        }

        if (Instance.Connection == null)
        {
            return false;
        }

        Instance.Connection.SendExecute(createMsg);
        return true;
    }

    public static bool TrySendRequest(Action<NetData> createMsg, Action<RequestResult, NetData> onResponse)
    {
        if (createMsg == null)
        {
            return false;
        }

        if (Instance.Status == Vibe.Disconnected)
        {
            return false;
        }

        if (Instance.Connection == null)
        {
            return false;
        }

        Instance.Connection.SendRequest(createMsg, onResponse);
        return true;
    }

    private void RunUpdate()
    {
        while (runUpdateThread)
        {
            Update(1f / 30f);
            Thread.Sleep(33);
        }
    }

    private void Update(float deltaTime)
    {
        sendPingTimer += deltaTime;
        if (sendPingTimer >= 1f)
        {
            sendPingTimer = 0f;
            Connection.SendExecute(delegate(NetData msg) { msg.Write((byte)0); });
        }

        if (PingTimer.ElapsedMilliseconds > 9000)
        {
            Status = AppManager.IsRemoteProcessRunning ? Vibe.NotResponding : Vibe.Disconnected;
        }

        if (status == Vibe.ConnectedReady && HUD.IsPlaying)
        {
            PlaybackState.ProgressMS += (int)(1000f * deltaTime);
            PlaybackState.ProgressMS = Mathf.Clamp(PlaybackState.ProgressMS, 0, PlaybackState.Item.LengthMS);
        }

        UpdateSuppressVanillaSongs(deltaTime);
    }

    internal void OnRimworldShowMainMenu()
    {
        if (HasShownMainMenu)
        {
            return;
        }

        HasShownMainMenu = true;
        var original = Instance.Content.assetBundles.GetByName("rimvibes").LoadAsset<GameObject>("Canvas");
        var gameObject = Object.Instantiate(original);
        gameObject.GetComponent<Canvas>().worldCamera = ContentLoader.cam;
        var componentInChildren = gameObject.GetComponentInChildren<Animator>();
        var mainMenuLogoComponent = gameObject.AddComponent<MainMenuLogoComponent>();
        mainMenuLogoComponent.Anim = componentInChildren;
        mainMenuLogoComponent.Offset = componentInChildren.transform.parent.transform as RectTransform;
        Object.DontDestroyOnLoad(gameObject.gameObject);
        if (!Terms.HasUserAgreed)
        {
            Log.Message("User has not accepted terms, opening window.");
            TermsUI.Open();
        }
        else
        {
            //Log.Message("Launching external process...");
            if (!AppManager.TryLaunch(true))
            {
                Log.Error(
                    "CRITICAL: Failed to launch process, RimVibes will not work. Process re-launch can be attempted from the menu.");
            }
        }

        PingTimer.Start();
    }

    private void OnRimworldExit()
    {
        runUpdateThread = false;
        Connection.SendExecute(delegate(NetData msg) { msg.Write((byte)3); });
        Connection?.Dispose();
        ImageDownloader.Dispose();
    }

    public override void DoSettingsWindowContents(Rect rect)
    {
        var width = 400f;
        MoveDown(7f);
        if (Widgets.ButtonText(new Rect(rect.x, rect.y, 220f, 32f), "RiVi.CustomEvents".Translate()))
        {
            EventMusicUI.Open();
            return;
        }

        MoveDown(40f);
        Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f),
            new GUIContent("RiVi.PauseMode".Translate(),
                "RiVi.PauseModeTooltip".Translate()));
        Widgets.Dropdown(new Rect(rect.x + 130f, rect.y, 150f, 30f), SongPauseMode.Pause_For_Tense_Song,
            value => value.ToString(), PauseModeDropdownGen, Settings.SongPauseMode.ToString().Replace('_', ' '));
        MoveDown(38f);
        Widgets.CheckboxLabeled(new Rect(rect.x, rect.y, width, 30f), "RiVi.SilenceVanilla".Translate(),
            ref Settings.ShouldSilenceVanillaMusic, false, null, null, true);
        MoveDown(36f);
        Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f),
            "RiVi.UiScale".Translate(Settings.HUDScale.ToStringPercent()));
        MoveDown(28f);
        Settings.HUDScale = Widgets.HorizontalSlider(new Rect(rect.x, rect.y, Mathf.Min(300f, rect.width), 30f),
            Settings.HUDScale, 0.5f, 2f, true, null, null, null, 0.05f);
        MoveDown(36f);
        Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), "RiVi.UiAnchor".Translate());
        Widgets.Dropdown(new Rect(rect.x + 130f, rect.y, 150f, 30f), HUDAnchor.Right, value => value.ToString(),
            AnchorDropdownGen, Settings.HUDAnchor.ToString());
        MoveDown(36f);
        var val = Settings.HUDOffset.x;
        Widgets.TextFieldNumericLabeled(new Rect(rect.x, rect.y, width, 30f), "RiVi.UiXOffset".Translate(), ref val,
            ref xOffBuffer,
            float.MinValue);
        Settings.HUDOffset.x = val;
        MoveDown(36f);
        var val2 = Settings.HUDOffset.y;
        Widgets.TextFieldNumericLabeled(new Rect(rect.x, rect.y, width, 30f), "RiVi.UiYOffset".Translate(), ref val2,
            ref yOffBuffer,
            float.MinValue);
        Settings.HUDOffset.y = val2;
        MoveDown(36f);
        Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), "RiVi.UiVisibility".Translate());
        Widgets.Dropdown(new Rect(rect.x + 130f, rect.y, 150f, 30f), HUDVisibility.AutoHide, value => value.ToString(),
            VisDropdownGen, Settings.HUDVisibility.ToString());
        MoveDown(36f);
        val = Settings.MainMenuButtonOffset.x;
        Widgets.TextFieldNumericLabeled(new Rect(rect.x, rect.y, width, 30f), "RiVi.ButtonXOffset".Translate(), ref val,
            ref xOffBuffer2);
        Settings.MainMenuButtonOffset.x = val;
        MoveDown(36f);
        val2 = Settings.MainMenuButtonOffset.y;
        Widgets.TextFieldNumericLabeled(new Rect(rect.x, rect.y, width, 30f), "RiVi.ButtonYOffset".Translate(),
            ref val2,
            ref yOffBuffer2);
        Settings.MainMenuButtonOffset.y = val2;
        MoveDown(36f);
        Widgets.CheckboxLabeled(new Rect(rect.x, rect.y, width, 30f), "RiVi.Debug".Translate(),
            ref Settings.LaunchDebugWindow, false, null, null, true);

        if (currentVersion != null)
        {
            MoveDown(36f);
            GUI.contentColor = Color.gray;
            Widgets.Label(new Rect(rect.x, rect.y, 250f, 30f), "RiVi.CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        base.DoSettingsWindowContents(rect);
        return;

        void MoveDown(float amount)
        {
            rect.y += amount;
            rect.height -= amount;
        }
    }

    private IEnumerable<Widgets.DropdownMenuElement<string>> AnchorDropdownGen(HUDAnchor _)
    {
        foreach (var item in Enum.GetValues(typeof(HUDAnchor)))
        {
            yield return new Widgets.DropdownMenuElement<string>
            {
                payload = "",
                option = new FloatMenuOption(item.ToString(), delegate { Settings.HUDAnchor = (HUDAnchor)item; })
            };
        }
    }

    private IEnumerable<Widgets.DropdownMenuElement<string>> VisDropdownGen(HUDVisibility _)
    {
        foreach (var item in Enum.GetValues(typeof(HUDVisibility)))
        {
            yield return new Widgets.DropdownMenuElement<string>
            {
                payload = "",
                option = new FloatMenuOption(item.ToString(),
                    delegate { Settings.HUDVisibility = (HUDVisibility)item; })
            };
        }
    }

    private IEnumerable<Widgets.DropdownMenuElement<string>> PauseModeDropdownGen(SongPauseMode _)
    {
        foreach (var item in Enum.GetValues(typeof(SongPauseMode)))
        {
            yield return new Widgets.DropdownMenuElement<string>
            {
                payload = "",
                option = new FloatMenuOption(getItemName(item),
                    delegate { Settings.SongPauseMode = (SongPauseMode)item; })
            };
        }

        yield break;

        static string getItemName(object obj)
        {
            var songPauseMode = (SongPauseMode)obj;
            return songPauseMode switch
            {
                SongPauseMode.Never_Auto_Pause => "Never pause",
                SongPauseMode.Pause_For_Any_Song => "Always pause for vanilla music",
                SongPauseMode.Pause_For_Tense_Song =>
                    "Only pause for tense vanilla music (raids, fires, disasters etc.)",
                _ => songPauseMode.ToString()
            };
        }
    }

    public override string SettingsCategory()
    {
        return "RimVibesMod";
    }

    private void UpdateSuppressVanillaSongs(float dt)
    {
        if (!Settings.ShouldSilenceVanillaMusic)
        {
            OverrideMusicVolumePatch.VolumeScale = 1f;
            return;
        }

        suppressLerp += (Status == Vibe.ConnectedReady && PlaybackState.IsPlaying ? -1f : 1f) * dt * 0.5f;
        suppressLerp = Mathf.Clamp01(suppressLerp);
        OverrideMusicVolumePatch.VolumeScale = suppressLerp;
    }
}