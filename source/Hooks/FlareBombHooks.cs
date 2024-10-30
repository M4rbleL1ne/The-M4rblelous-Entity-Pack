﻿global using static LBMergedMods.Hooks.FlareBombHooks;
using Random = UnityEngine.Random;
using RWCustom;

namespace LBMergedMods.Hooks;

public static class FlareBombHooks
{
    internal static void On_FlareBomb_Update(On.FlareBomb.orig_Update orig, FlareBomb self, bool eu)
    {
        orig(self, eu);
        if (self.burning > 0f)
        {
            var crits = self.room.abstractRoom.creatures;
            for (var i = 0; i < crits.Count; i++)
            {
                if (crits[i]?.realizedCreature is MiniLeech l && l!.dead && Custom.DistLess(self.firstChunk.pos, l.firstChunk.pos, self.LightIntensity * 600f))
                {
                    l.airDrown = 1f;
                    l.Die();
                    l.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 7f;
                }
            }
        }
    }
}