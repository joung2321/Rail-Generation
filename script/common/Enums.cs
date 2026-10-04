using System;

[Flags]
public enum Terminal
{
    None = 0,
    Initial = 0b1,
    Final = 0b1 << 1
}

public enum Reverser
{
    Forward = 1,
    Neutral = 0,
    Reverse = -1
}