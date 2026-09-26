using System;

[Flags]
public enum Terminal
{
    None = 0,
    Initial = 0b1,
    Final = 0b1 << 1
}