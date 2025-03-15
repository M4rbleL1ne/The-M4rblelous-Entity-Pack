using System.Runtime.InteropServices;
using UnityEngine;

namespace LBMergedMods.Items;

[StructLayout(LayoutKind.Sequential)]
public sealed class RubberBlossomProperties(bool startsOpen, int numberOfFruits, int remainingOpenCycles, bool alwaysOpen, bool alwaysClosed)
{
    public int NumberOfFruits = numberOfFruits, RemainingOpenCycles = remainingOpenCycles;
    public bool FirstTimeRealized = true, StartsOpen = startsOpen, AlwaysOpen = alwaysOpen, AlwaysClosed = alwaysClosed, Open, DevSpawn = false;
    public float forceMaxVel;
    public Color forceColor;
}