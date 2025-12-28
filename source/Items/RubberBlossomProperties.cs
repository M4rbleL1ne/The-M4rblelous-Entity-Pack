using System.Runtime.InteropServices;
using UnityEngine;

namespace LBMergedMods.Items;

[StructLayout(LayoutKind.Sequential)]
public sealed class RubberBlossomProperties(bool startsOpen, int numberOfFruits, int remainingOpenCycles, bool alwaysOpen, bool alwaysClosed)
{
    public Color ForceColor;
    public int NumberOfFruits = numberOfFruits, RemainingOpenCycles = remainingOpenCycles;
    public float ForceMaxVel;
    public bool FirstTimeRealized = true, StartsOpen = startsOpen, AlwaysOpen = alwaysOpen, AlwaysClosed = alwaysClosed, Open, DevSpawn; // default is already false
}
// bools at the end in this Sequential class