﻿namespace LBMergedMods.Creatures;

public class HazerMomState(AbstractCreature creature) : HealthState(creature)
{
    public int OrigRoom = -1, PlacedObjectIndex = -1;
}