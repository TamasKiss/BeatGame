namespace BeatGame.Core;

public abstract class Screen
{
    public GameStateManager Manager { get; internal set; } = null!;

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public abstract void Update(float deltaTime);
    public abstract void Draw();
}
