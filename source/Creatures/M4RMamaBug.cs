using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;

public class M4RMamaBug : EggBug
{
    public static Color BugCol = Custom.HSL2RGB(5f / 36f, 1f, .5f);

    public M4RMamaBug(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var bs = bodyChunks;
        var b0 = bs[0];
        var b1 = bs[1];
        b0.mass *= 1.45f;
        b1.mass *= 1.45f;
        b0.rad = 5.5f;
        b1.rad = 9.5f;
        bodyChunkConnections[0].distance *= 1.6f;
        gravity = .95f;
        hue = Mathf.Lerp(.55f, .65f, Custom.ClampedRandomVariation(.5f, .5f, 2f));
    }

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new M4RMamaBugGraphics(this);
        graphicsModule.Reset();
    }

    public override void Update(bool eu)
    {
        if (!dead && State.health > 0f && State.health < 1f && Random.value < .02f && poison < .1f)
            State.health = Mathf.Min(1f, State.health - .25f / Mathf.Lerp(140f, 50f, State.health));
        base.Update(eu);
        var bs = bodyChunks;
        if (Consious && Footing)
        {
            for (var i = 0; i < bs.Length; i++)
                bs[i].vel *= .9f;
        }
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        if (type == DamageType.Explosion)
            damage *= 2.5f;
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }
}