using MoreSlugcats;
using RWCustom;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace LBMergedMods.Creatures;

public class ScavengerSentinelGraphics(ScavengerSentinel ow) : ScavengerGraphics(ow), IMuddableGraphics
{
    public override void Update()
    {
        base.Update();
        if (!ModManager.DLCShared)
        {
            if (maskGfx is M4RScavMaskGraphics mask)
            {
                var lkUp = lastLookUp;
                var ntrFace = lastNeutralFace;
                lkUp *= 1f - ntrFace;
                mask.rotationA = Slerp2(maskGfx.rotationA, -Normalized(math.lerp(Normalized(HeadDir(0f)), -Custom.DegToFloat2(-BodyAxis(0f)), Mathf.Lerp(.5f, 1f, Math.Max(Mathf.Pow(lkUp, 1.1f), ntrFace)))), .5f);
                mask.rotationB = new(0f, 1f);
                mask.Update();
            }
        }
    }

    public override void Reset()
    {
        base.Reset();
        if (maskGfx is M4RScavMaskGraphics mask)
            mask.Reset();
    }

    public static float2 Slerp2(float2 a, float2 b, float t)
    {
        Vector3 a2 = new(a.x, a.y, 0f),
            b2 = new(b.x, b.y, 0f),
            vector = Vector3.Slerp(a2, b2, t);
        return new(vector.x, vector.y);
    }

    public static float2 Normalized(float2 f)
    {
        var mag = math.sqrt(f.x * f.x + f.y * f.y);
        if (mag > 9.999999747378752E-06)
            return f / mag;
        return default;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
        if (!ModManager.DLCShared)
            maskGfx?.AddToContainer(sLeaser, rCam, null);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (!ModManager.DLCShared)
            maskGfx?.ApplyPalette(sLeaser, rCam, palette);
        base.ApplyPalette(sLeaser, rCam, palette);
    }

    public virtual new bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite)
    {
        if (!ModManager.DLCShared)
        {
            if (maskGfx is VultureMaskGraphics gfx && gfx.firstSprite <= sprite && sprite < gfx.firstSprite + gfx.TotalSprites)
                return false;
        }
        return base.MuddableSprite(sLeaser, sprite);
    }
}