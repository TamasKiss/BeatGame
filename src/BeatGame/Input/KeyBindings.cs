namespace BeatGame.Input;

public sealed class KeyBindings
{
    public const int LaneCount = 5;
    private static readonly char[] Defaults = { 'A', 'S', 'D', 'F', 'G' };

    private readonly char[] _bindings;

    public KeyBindings()
    {
        _bindings = new char[LaneCount];
        Array.Copy(Defaults, _bindings, LaneCount);
    }

    public KeyBindings(char[] initial)
    {
        ArgumentNullException.ThrowIfNull(initial);
        if (initial.Length != LaneCount)
        {
            throw new ArgumentException($"Initial bindings must have exactly {LaneCount} entries.", nameof(initial));
        }
        _bindings = new char[LaneCount];
        for (int i = 0; i < LaneCount; i++)
        {
            char normalized = char.ToUpperInvariant(initial[i]);
            if (!IsValidKey(normalized))
            {
                throw new ArgumentException($"Initial binding at index {i} '{initial[i]}' is not a letter A-Z.", nameof(initial));
            }
            _bindings[i] = normalized;
        }
    }

    public char GetKey(int lane)
    {
        EnsureLaneInRange(lane);
        return _bindings[lane];
    }

    public bool SetKey(int lane, char key)
    {
        EnsureLaneInRange(lane);
        char normalized = char.ToUpperInvariant(key);
        if (!IsValidKey(normalized))
        {
            return false;
        }
        _bindings[lane] = normalized;
        return true;
    }

    public char[] ToArray()
    {
        char[] copy = new char[LaneCount];
        Array.Copy(_bindings, copy, LaneCount);
        return copy;
    }

    public int? FindLaneByKey(char key)
    {
        char normalized = char.ToUpperInvariant(key);
        for (int i = 0; i < LaneCount; i++)
        {
            if (_bindings[i] == normalized) return i;
        }
        return null;
    }

    public static bool IsValidKey(char key)
    {
        char upper = char.ToUpperInvariant(key);
        return upper >= 'A' && upper <= 'Z';
    }

    private static void EnsureLaneInRange(int lane)
    {
        if (lane < 0 || lane >= LaneCount)
        {
            throw new ArgumentOutOfRangeException(nameof(lane), lane, $"Lane must be 0-{LaneCount - 1}.");
        }
    }
}
