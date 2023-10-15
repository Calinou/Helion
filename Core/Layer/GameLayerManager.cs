using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Helion.Audio.Sounds;
using Helion.Layer.Consoles;
using Helion.Layer.EndGame;
using Helion.Layer.Images;
using Helion.Layer.Menus.New;
using Helion.Layer.Worlds;
using Helion.Menus.Impl;
using Helion.Menus.New.LoadSave;
using Helion.Menus.New.Misc;
using Helion.Render;
using Helion.Render.Common.Context;
using Helion.Render.Common.Renderers;
using Helion.Resources.Archives.Collection;
using Helion.Resources.Definitions.MapInfo;
using Helion.Util;
using Helion.Util.Configs;
using Helion.Util.Consoles;
using Helion.Util.Consoles.Commands;
using Helion.Util.Extensions;
using Helion.Util.Profiling;
using Helion.Util.Timing;
using Helion.Window;
using Helion.World.Save;
using static Helion.Util.Assertion.Assert;

namespace Helion.Layer;

/// <summary>
/// Responsible for coordinating input, logic, and rendering calls in order
/// for different kinds of layers.
/// </summary>
public class GameLayerManager : IGameLayerManager
{
    public event EventHandler<IGameLayer>? GameLayerAdded;

    public ConsoleLayer? ConsoleLayer { get; private set; }
    public MenuLayer? MenuLayer { get; private set; }
    public ReadThisLayer? ReadThisLayer { get; private set; }
    public TitlepicLayer? TitlepicLayer { get; private set; }
    public EndGameLayer? EndGameLayer { get; private set; }
    public IntermissionLayer? IntermissionLayer { get; private set; }

    private static readonly string[] MenuIgnoreCommands = new[] { Constants.Input.Screenshot, Constants.Input.Console };

    public WorldLayer? WorldLayer { get; private set; }
    public SaveGameEvent? LastSave;
    private readonly IConfig m_config;
    private readonly IWindow m_window;
    private readonly HelionConsole m_console;
    private readonly ConsoleCommands m_consoleCommands;
    private readonly ArchiveCollection m_archiveCollection;
    private readonly SoundManager m_soundManager;
    private readonly SaveGameManager m_saveGameManager;
    private readonly Profiler m_profiler;
    private readonly Stopwatch m_stopwatch = new();
    private readonly Action<IRenderableSurfaceContext> m_renderDefaultAction;
    private readonly Action<IHudRenderContext> m_renderHudAction;

    private readonly HudRenderContext m_hudContext = new(default);

    private Renderer m_renderer;
    private IRenderableSurfaceContext m_ctx;
    private bool m_disposed;

    internal IEnumerable<IGameLayer> Layers => new List<IGameLayer?>
    {
        ConsoleLayer, MenuLayer, ReadThisLayer, TitlepicLayer, EndGameLayer, IntermissionLayer, WorldLayer
    }.WhereNotNull();

    public GameLayerManager(IConfig config, IWindow window, HelionConsole console, ConsoleCommands consoleCommands,
        ArchiveCollection archiveCollection, SoundManager soundManager, SaveGameManager saveGameManager,
        Profiler profiler)
    {
        m_config = config;
        m_window = window;
        m_console = console;
        m_consoleCommands = consoleCommands;
        m_archiveCollection = archiveCollection;
        m_soundManager = soundManager;
        m_saveGameManager = saveGameManager;
        m_profiler = profiler;
        m_stopwatch.Start();
        m_renderDefaultAction = new(RenderDefault);
        m_renderHudAction = new(RenderHud);
        m_renderer = null!;
        m_ctx = null!;

        m_saveGameManager.GameSaved += SaveGameManager_GameSaved;
    }

    ~GameLayerManager()
    {
        FailedToDispose(this);
        PerformDispose();
    }

    private void SaveGameManager_GameSaved(object? sender, SaveGameEvent e)
    {
        if (e.Success)
            LastSave = e;
    }

    public bool HasMenuOrConsole() => MenuLayer != null || ConsoleLayer != null;

    public bool ShouldFocus()
    {
        if (ConsoleLayer != null)
            return false;

        if (TitlepicLayer != null)
            return MenuLayer == null;

        if (WorldLayer != null)
            return WorldLayer.ShouldFocus;

        return true;
    }

    public void Add(IGameLayer? gameLayer)
    {
        switch (gameLayer)
        {
        case ConsoleLayer layer:
            Remove(ConsoleLayer);
            ConsoleLayer = layer;
            break;
        case MenuLayer layer:
            Remove(MenuLayer);
            MenuLayer = layer;
            break;
        case ReadThisLayer layer:
            Remove(ReadThisLayer);
            ReadThisLayer = layer;
            break;
        case EndGameLayer layer:
            Remove(EndGameLayer);
            EndGameLayer = layer;
            break;
        case TitlepicLayer layer:
            Remove(TitlepicLayer);
            TitlepicLayer = layer;
            break;
        case IntermissionLayer layer:
            Remove(IntermissionLayer);
            IntermissionLayer = layer;
            break;
        case WorldLayer layer:
            Remove(WorldLayer);
            WorldLayer = layer;
            break;
        case null:
            break;
        default:
            throw new ArgumentException($"Unknown object passed for layer: {gameLayer.GetType()}");
        }

        if (gameLayer != null)
            GameLayerAdded?.Invoke(this, gameLayer);
    }

    public void SubmitConsoleText(string text)
    {
        m_console.ClearInputText();
        m_console.AddInput(text);
        m_console.SubmitInputText();
    }

    public LinkedList<string> GetConsoleSubmittedInput() => m_console.SubmittedInput;

    public void ClearAllExcept(params IGameLayer[] layers)
    {
        foreach (IGameLayer existingLayer in Layers)
            if (!layers.Contains(existingLayer))
                Remove(existingLayer);
    }

    public void Remove(object? layer)
    {
        if (layer == null)
            return;

        if (ReferenceEquals(layer, ConsoleLayer))
        {
            ConsoleLayer?.Dispose();
            ConsoleLayer = null;
        }
        else if (ReferenceEquals(layer, MenuLayer))
        {
            MenuLayer?.Dispose();
            MenuLayer = null;
        }
        else if (ReferenceEquals(layer, ReadThisLayer))
        {
            ReadThisLayer?.Dispose();
            ReadThisLayer = null;
        }
        else if (ReferenceEquals(layer, EndGameLayer))
        {
            EndGameLayer?.Dispose();
            EndGameLayer = null;
        }
        else if (ReferenceEquals(layer, TitlepicLayer))
        {
            TitlepicLayer?.Dispose();
            TitlepicLayer = null;
        }
        else if (ReferenceEquals(layer, IntermissionLayer))
        {
            IntermissionLayer?.Dispose();
            IntermissionLayer = null;
        }
        else if (ReferenceEquals(layer, WorldLayer))
        {
            WorldLayer?.Dispose();
            WorldLayer = null;
        }
    }

    private void HandleInput(IInputManager inputManager, TickerInfo tickerInfo)
    {
        IConsumableInput input = inputManager.Poll(tickerInfo.Ticks > 0);
        HandleInput(input);

        // Only clear keys if new tick since they are only processed each tick.
        // Mouse movement is always processed to render most up to date view.
        if (input.HandleKeyInput)
            inputManager.ProcessedKeys();

        inputManager.ProcessedMouseMovement();
    }

    public void HandleInput(IConsumableInput input)
    {        
        if (input.HandleKeyInput)
        {
            if (ConsumeCommandPressed(Constants.Input.Console, input))
                ToggleConsoleLayer(input);
            ConsoleLayer?.HandleInput(input);

            if (ShouldCreateMenu(input))
            {
                if (ReadThisLayer != null)
                    Remove(ReadThisLayer);
                CreateMenuLayer();
            }

            if (!HasMenuOrConsole())
            {
                EndGameLayer?.HandleInput(input);
                TitlepicLayer?.HandleInput(input);
                IntermissionLayer?.HandleInput(input);
            }

            if (ReadThisLayer == null)
                MenuLayer?.HandleInput(input);

            if (ConsoleLayer == null)
                ReadThisLayer?.HandleInput(input);
        }

        WorldLayer?.HandleInput(input);
    }

    private bool ShouldCreateMenu(IConsumableInput input)
    {
        if (MenuLayer != null || ConsoleLayer != null)
            return false;

        bool hasMenuInput = (ReadThisLayer != null && ConsumeCommandDown(Constants.Input.Menu, input))
            || input.Manager.HasAnyKeyPressed();

        if (TitlepicLayer != null && hasMenuInput && !CheckIgnoreMenuCommands(input))
        {
            // Eat everything including escape key if it exists, otherwise the menu will immediately close.
            input.ConsumeAll();
            return true;
        }
        
        return ConsumeCommandPressed(Constants.Input.Menu, input);
    }

    private bool ConsumeCommandPressed(string command, IConsumableInput input)
    {
        return m_config.Keys.ConsumeCommandKeyPress(command, input, out _);
    }

    private bool ConsumeCommandDown(string command, IConsumableInput input)
    {
        return m_config.Keys.ConsumeCommandKeyPress(command, input, out _);
    }

    private bool CheckIgnoreMenuCommands(IConsumableInput input)
    {
        for (int i = 0; i < MenuIgnoreCommands.Length; i++)
        {
            if (m_config.Keys.IsCommandKeyDown(MenuIgnoreCommands[i], input))
                return true;
        }

        return false;
    }

    private void ToggleConsoleLayer(IConsumableInput input)
    {
        input.ConsumeAll();

        if (ConsoleLayer == null)
            Add(new ConsoleLayer(m_archiveCollection.GameInfo.TitlePage, m_config, m_console, m_consoleCommands));
        else
            Remove(ConsoleLayer);
    }

    private void CreateMenuLayer()
    {
        m_soundManager.PlayStaticSound(Constants.MenuSounds.Activate);

        MenuLayer menuLayer = new(this, m_config, m_archiveCollection, m_soundManager);
        Add(menuLayer);
    }

    public void GoToSaveOrLoadMenu(bool isSave)
    {
        if (MenuLayer == null)
            CreateMenuLayer();

        if (isSave)
            MenuLayer?.ClearAndUse(new SaveGameMenu(m_soundManager));
        else
            MenuLayer?.ClearAndUse(new LoadGameMenu(m_soundManager));
    }

    public void QuickSave()
    {
        if (WorldLayer == null || !LastSave.HasValue)
        {
            GoToSaveOrLoadMenu(true);
            return;
        }

        if (m_config.Game.QuickSaveConfirm)
        {
            if (MenuLayer == null)
                CreateMenuLayer();

            ConfirmMenu confirmMenu = new(m_soundManager, Confirm_Cleared,
                $"Are you sure you want to overwrite: {LastSave.Value.SaveGame.Model?.Text ?? "Save"}", 
                "Press Y to confirm.");
            MenuLayer?.ClearAndUse(confirmMenu);
            return;
        }

        WriteQuickSave();
    }

    private void Confirm_Cleared(bool result)
    {
        if (!result || !LastSave.HasValue)
            return;

        WriteQuickSave();
    }

    private void WriteQuickSave()
    {
        if (WorldLayer == null || LastSave == null)
            return;

        var world = WorldLayer.World;
        var save = LastSave.Value;
        m_saveGameManager.WriteSaveGame(world, world.MapInfo.GetMapNameWithPrefix(world.ArchiveCollection), save.SaveGame);
        world.DisplayMessage(world.Player, null, SaveMenu.SaveMessage);
    }

    public void RunLogic(TickerInfo tickerInfo)
    {
        HandleInput(m_window.InputManager, tickerInfo);

        ConsoleLayer?.RunLogic(tickerInfo);
        MenuLayer?.RunLogic(tickerInfo);
        ReadThisLayer?.RunLogic(tickerInfo);
        EndGameLayer?.RunLogic(tickerInfo);
        TitlepicLayer?.RunLogic(tickerInfo);
        IntermissionLayer?.RunLogic(tickerInfo);
        WorldLayer?.RunLogic(tickerInfo);

        if (!HasMenuOrConsole() && m_stopwatch.ElapsedMilliseconds >= 1000.0 / Constants.TicksPerSecond)
        {
            m_stopwatch.Restart();
            EndGameLayer?.OnTick();
        }
    }

    public void OnTick()
    {
        // Nothing to tick.
    }

    public void Render(Renderer renderer)
    {
        m_renderer = renderer;
        m_renderer.Default.Render(m_renderDefaultAction);
    }

    private void RenderDefault(IRenderableSurfaceContext ctx)
    {
        m_ctx = ctx;
        m_hudContext.Dimension = m_renderer.RenderDimension;

        ctx.Viewport(m_renderer.RenderDimension.Box);
        ctx.Clear(Renderer.DefaultBackground, true, true);

        WorldLayer?.Render(ctx);

        m_profiler.Render.MiscLayers.Start();
        ctx.Hud(m_hudContext, m_renderHudAction);
        m_profiler.Render.MiscLayers.Stop();
    }

    private void RenderHud(IHudRenderContext hudCtx)
    {
        IntermissionLayer?.Render(m_ctx, hudCtx);
        TitlepicLayer?.Render(hudCtx);
        EndGameLayer?.Render(m_ctx, hudCtx);
        MenuLayer?.Render(hudCtx);
        ReadThisLayer?.Render(hudCtx);
        ConsoleLayer?.Render(m_ctx, hudCtx);
    }

    public void Dispose()
    {
        PerformDispose();
        GC.SuppressFinalize(this);
    }

    private void PerformDispose()
    {
        if (m_disposed)
            return;

        Remove(WorldLayer);
        Remove(IntermissionLayer);
        Remove(EndGameLayer);
        Remove(TitlepicLayer);
        Remove(ReadThisLayer);
        Remove(MenuLayer);
        Remove(ConsoleLayer);

        m_disposed = true;
    }
}
