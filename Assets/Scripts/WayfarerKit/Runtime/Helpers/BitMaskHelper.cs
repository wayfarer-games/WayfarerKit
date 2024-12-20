using System;
public class BitMaskHelper
{
    public static void SetBit(ref int eventMask, int bitIndex, bool value)
    {
        if (bitIndex is < 0 or >= sizeof(int) * 8)
            throw new ArgumentOutOfRangeException(nameof(bitIndex), "Index must be in range 0-31");

        if (value)
            eventMask |= 1 << bitIndex;
        else
            eventMask &= ~(1 << bitIndex);
    }

    public static bool IsBitSet(int eventMask, int bitIndex)
    {
        if (bitIndex is < 0 or >= sizeof(int) * 8)
            throw new ArgumentOutOfRangeException(nameof(bitIndex), "Index must be in range 0-31");

        return (eventMask & 1 << bitIndex) != 0;
    }
}