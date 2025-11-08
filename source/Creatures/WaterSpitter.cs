using RWCustom;
using UnityEngine;
using Watcher;

namespace LBMergedMods.Creatures;

public class WaterSpitter : Lizard
{
    public WaterSpitter(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        buoyancy = .915f;
        effectColor = Color.white;
        if (rotModule is LizardRotModule mod && LizardState.rotType != LizardState.RotType.Slight)
            effectColor = Color.Lerp(effectColor, mod.RotEyeColor, LizardState.rotType == LizardState.RotType.Opossum ? .2f : .8f);
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new WaterSpitterGraphics(this);

    public override void Update(bool eu)
    {
        base.Update(eu);
        buoyancy = .915f;
    }

    public virtual void SpitWater()
    {
        if (Submersion >= 1f || grasps[0] is not null || room is not Room rm || AI?.redSpitAI is not LizardAI.LizardSpitTracker a)
            return;
        bodyWiggleCounter = 0;
        JawOpen = Mathf.Clamp(JawOpen + .2f, 0f, 1f);
        var ctr = safariControlled;
        if (!a.spitting && !ctr)
            EnterAnimation(Animation.Standard, true);
        else
        {
            var vector = a.AimPos();
            if (vector is Vector2 value)
            {
                BodyChunk mc = mainBodyChunk, b1 = bodyChunks[1], b0 = firstChunk, b2 = bodyChunks[2];
                if (a.AtSpitPos)
                {
                    var vector2 = rm.MiddleOfTile(a.spitFromPos);
                    mc.vel += Vector2.ClampMagnitude(vector2 - Custom.DirVec(vector2, value) * bodyChunkConnections[0].distance - mc.pos, 10f) / 500f;
                    b1.vel += Vector2.ClampMagnitude(vector2 - b1.pos, 10f) / 500f;
                }
                if (!AI.UnpleasantFallRisk(Room.StaticGetTilePosition(mc.pos)))
                {
                    var ltr = Custom.DirVec(mc.pos, value) * LegsGripping * .02f;
                    mc.vel += ltr * 2f;
                    b1.vel -= ltr;
                    b2.vel -= ltr;
                }
                if (a.delay < 1)
                {
                    var fl = Custom.DirVec(b1.pos, b0.pos);
                    Vector2 vector3 = b0.pos + fl * 10f, vector4 = Custom.DirVec(vector3, value);
                    if (Vector2.Dot(vector4, fl) > .3f || ctr)
                    {
                        if (ctr)
                        {
                            EnterAnimation(Animation.Standard, true);
                            ReleaseGrasp(0);
                        }
                        rm.PlaySound(SoundID.Splashing_Water_Into_Terrain, vector3, 1.6f, 1.2f, abstractPhysicalObject);
                        rm.AddObject(new LizardWaterSpit(vector3, vector4 * 28f, this));
                        a.delay = 0;
                        b2.pos -= vector4 * .01f;
                        b1.pos -= vector4 * .005f;
                        b2.vel -= vector4 * .0025f;
                        b1.vel -= vector4 * .00125f;
                        JawOpen = 1f;
                    }
                }
            }
        }
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}