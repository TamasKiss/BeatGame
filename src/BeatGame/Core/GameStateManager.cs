namespace BeatGame.Core;

public sealed class GameStateManager
{
    private readonly Dictionary<GameState, Screen> _screens = new();
    private Screen? _currentScreen;
    private GameState _currentState;
    private GameState? _requestedState;

    public GameState CurrentState => _currentState;
    public bool ExitRequested { get; private set; }

    public void RegisterScreen(GameState state, Screen screen)
    {
        ArgumentNullException.ThrowIfNull(screen);
        screen.Manager = this;
        _screens[state] = screen;
    }

    public void SetInitialState(GameState state)
    {
        _currentState = state;
        if (_screens.TryGetValue(state, out Screen? screen))
        {
            _currentScreen = screen;
            screen.OnEnter();
        }
    }

    public void Transition(GameState newState)
    {
        if (newState == GameState.Exit)
        {
            ExitRequested = true;
            return;
        }
        _requestedState = newState;
    }

    public void Update(float deltaTime)
    {
        ApplyPendingTransition();
        _currentScreen?.Update(deltaTime);
        ApplyPendingTransition();
    }

    public void Draw()
    {
        _currentScreen?.Draw();
    }

    private void ApplyPendingTransition()
    {
        if (_requestedState is null) return;
        GameState target = _requestedState.Value;
        _requestedState = null;

        if (target == _currentState) return;
        if (!_screens.TryGetValue(target, out Screen? next)) return;

        _currentScreen?.OnExit();
        _currentState = target;
        _currentScreen = next;
        _currentScreen.OnEnter();
    }
}
