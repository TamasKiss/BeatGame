using BeatGame.Input;
using Xunit;

namespace BeatGame.Tests.Input;

public class KeyBindingsTests
{
    [Fact]
    public void Default_BindingsAreASDFG()
    {
        KeyBindings bindings = new();
        Assert.Equal('A', bindings.GetKey(0));
        Assert.Equal('S', bindings.GetKey(1));
        Assert.Equal('D', bindings.GetKey(2));
        Assert.Equal('F', bindings.GetKey(3));
        Assert.Equal('G', bindings.GetKey(4));
    }

    [Theory]
    [InlineData('a', 'A')]
    [InlineData('Z', 'Z')]
    [InlineData('m', 'M')]
    public void SetKey_NormalizesToUppercase(char input, char expected)
    {
        KeyBindings bindings = new();
        bool result = bindings.SetKey(0, input);
        Assert.True(result);
        Assert.Equal(expected, bindings.GetKey(0));
    }

    [Theory]
    [InlineData('1')]
    [InlineData('!')]
    [InlineData(' ')]
    [InlineData('\n')]
    [InlineData('5')]
    public void SetKey_RejectsNonLetters_ReturnsFalseAndKeepsPrevious(char invalid)
    {
        KeyBindings bindings = new();
        char before = bindings.GetKey(0);
        bool result = bindings.SetKey(0, invalid);
        Assert.False(result);
        Assert.Equal(before, bindings.GetKey(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(100)]
    public void GetKey_OutOfRangeLane_Throws(int lane)
    {
        KeyBindings bindings = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => bindings.GetKey(lane));
    }

    [Fact]
    public void IsValidKey_LettersAreValid_OthersAreNot()
    {
        Assert.True(KeyBindings.IsValidKey('A'));
        Assert.True(KeyBindings.IsValidKey('z'));
        Assert.True(KeyBindings.IsValidKey('m'));
        Assert.False(KeyBindings.IsValidKey('0'));
        Assert.False(KeyBindings.IsValidKey('!'));
        Assert.False(KeyBindings.IsValidKey(' '));
    }

    [Fact]
    public void FindLaneByKey_ReturnsLaneOrNull()
    {
        KeyBindings bindings = new();
        Assert.Equal(2, bindings.FindLaneByKey('D'));
        Assert.Equal(2, bindings.FindLaneByKey('d'));
        Assert.Null(bindings.FindLaneByKey('Z'));
    }

    [Fact]
    public void Constructor_FromArray_NormalizesAndValidates()
    {
        char[] custom = { 'q', 'w', 'e', 'r', 't' };
        KeyBindings bindings = new(custom);
        Assert.Equal('Q', bindings.GetKey(0));
        Assert.Equal('T', bindings.GetKey(4));
    }

    [Fact]
    public void Constructor_FromArray_RejectsInvalidLetters()
    {
        char[] invalid = { 'A', 'S', '1', 'F', 'G' };
        Assert.Throws<ArgumentException>(() => new KeyBindings(invalid));
    }

    [Fact]
    public void Constructor_FromArray_RejectsWrongLength()
    {
        char[] wrongLength = { 'A', 'S', 'D' };
        Assert.Throws<ArgumentException>(() => new KeyBindings(wrongLength));
    }
}
