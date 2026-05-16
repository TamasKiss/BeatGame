using BeatGame.Core;
using Xunit;

namespace BeatGame.Tests.Models;

public class GameStateManagerTests
{
    private sealed class FakeScreen : Screen
    {
        public int EnterCount { get; private set; }
        public int ExitCount { get; private set; }
        public override void OnEnter() => EnterCount++;
        public override void OnExit() => ExitCount++;
        public override void Update(float deltaTime) { }
        public override void Draw() { }
    }

    [Fact]
    public void SetInitialState_InvokesOnEnter()
    {
        GameStateManager manager = new();
        FakeScreen menu = new();
        manager.RegisterScreen(GameState.Menu, menu);

        manager.SetInitialState(GameState.Menu);

        Assert.Equal(GameState.Menu, manager.CurrentState);
        Assert.Equal(1, menu.EnterCount);
    }

    [Fact]
    public void Transition_FromMenuToSettings_SwitchesScreens()
    {
        GameStateManager manager = new();
        FakeScreen menu = new();
        FakeScreen settings = new();
        manager.RegisterScreen(GameState.Menu, menu);
        manager.RegisterScreen(GameState.Settings, settings);
        manager.SetInitialState(GameState.Menu);

        manager.Transition(GameState.Settings);
        manager.Update(0.016f);

        Assert.Equal(GameState.Settings, manager.CurrentState);
        Assert.Equal(1, menu.ExitCount);
        Assert.Equal(1, settings.EnterCount);
    }

    [Fact]
    public void Transition_FromMenuToSelection_SwitchesScreens()
    {
        GameStateManager manager = new();
        FakeScreen menu = new();
        FakeScreen selection = new();
        manager.RegisterScreen(GameState.Menu, menu);
        manager.RegisterScreen(GameState.Selection, selection);
        manager.SetInitialState(GameState.Menu);

        manager.Transition(GameState.Selection);
        manager.Update(0.016f);

        Assert.Equal(GameState.Selection, manager.CurrentState);
    }

    [Fact]
    public void Transition_ToExit_SetsExitRequested()
    {
        GameStateManager manager = new();
        FakeScreen menu = new();
        manager.RegisterScreen(GameState.Menu, menu);
        manager.SetInitialState(GameState.Menu);

        manager.Transition(GameState.Exit);

        Assert.True(manager.ExitRequested);
    }

    [Fact]
    public void Transition_ToSameState_DoesNotReinvokeEnter()
    {
        GameStateManager manager = new();
        FakeScreen menu = new();
        manager.RegisterScreen(GameState.Menu, menu);
        manager.SetInitialState(GameState.Menu);

        manager.Transition(GameState.Menu);
        manager.Update(0.016f);

        Assert.Equal(1, menu.EnterCount);
        Assert.Equal(0, menu.ExitCount);
    }
}
