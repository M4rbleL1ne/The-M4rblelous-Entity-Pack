using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;

public class ScavengerSentinel(AbstractCreature abstractCreature, World world) : Scavenger(abstractCreature, world)
{
    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        if (room is not Room rm || !RippleViolenceCheck(source))
            return;
        stunBonus *= .9f;
        if (hitChunk?.index == 2)
            damage *= .09f;
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        if (dead && !readyToReleaseMask && rm?.world is World w && rm.game is RainWorldGame game && graphicsModule is ScavengerGraphics grs)
        {
            readyToReleaseMask = true;
            var mask = new VultureMask.AbstractVultureMask(w, null, rm.GetWorldCoordinate(firstChunk.pos), game.GetNewID(), abstractPhysicalObject.ID.RandomSeed, false, false, grs.maskGfx?.overrideSprite);
            rm.abstractRoom?.AddEntity(mask);
            mask.RealizeInRoom();
            if (mask.realizedObject is VultureMask rlMask)
            {
                rlMask.rotVel = new(20f, 0f);
                rlMask.firstChunk.vel = (directionAndMomentum is Vector2 vec ? vec.normalized : Custom.RNV()) * 20f;
            }
        }
    }

    public override void GrabbedObjectSnatched(PhysicalObject grabbedObject, Creature thief)
    {
        if (grabbedObject is not null && thief is not null)
            AI?.agitation = 1f;
        base.GrabbedObjectSnatched(grabbedObject, thief);
    }

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new ScavengerSentinelGraphics(this);
        graphicsModule.Reset();
    }
}