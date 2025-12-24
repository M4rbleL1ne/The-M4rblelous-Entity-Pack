using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;

public class BouncingBall : Snail
{
    public float Alpha = 1f, LastAlpha = 1f, Consciousness, Lerper;
    public bool LerpUp = true;

    public BouncingBall(AbstractCreature abstractCreature, World world) : base(abstractCreature, world) => bounce = 1.75f;

    public override void Die()
    {
        if (room is Room rm && killTag?.creatureTemplate.type != CreatureTemplateType.Xylo)
        {
            var vector = mainBodyChunk.pos;
            var clr = shellColor[1];
            rm.AddObject(new SootMark(rm, vector, 50f, true));
            rm.AddObject(new Explosion(rm, this, vector, 5, 110f, 5f, 1.1f, 60f, .3f, this, .8f, 0f, .7f));
            for (var i = 0; i < 14; i++)
                rm.AddObject(new Explosion.ExplosionSmoke(vector, Custom.RNV() * (5f * Random.value), 1f));
            rm.AddObject(new Explosion.ExplosionLight(vector, 160f, 1f, 3, clr));
            rm.AddObject(new ExplosionSpikes(rm, vector, 9, 4f, 5f, 5f, 90f, clr));
            rm.AddObject(new ShockWave(vector, 60f, .045f, 4));
            for (var j = 0; j < 20; j++)
            {
                var vector2 = Custom.RNV();
                rm.AddObject(new Spark(vector + vector2 * (Random.value * 40f), vector2 * Mathf.Lerp(4f, 30f, Random.value), clr, null, 4, 18));
            }
            rm.ScreenMovement(vector, default, .7f);
            rm.PlaySound(SoundID.Bomb_Explode, mainBodyChunk);
        }
        base.Die();
        Destroy();
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new BouncingBallGraphics(this);

    public virtual bool NarrowSpace()
    {
        if (room is Room rm)
        {
            var pos = abstractCreature.pos;
            if (rm.GetTile(pos with { y = pos.y + 1 }).Solid && rm.GetTile(pos with { y = pos.y - 1 }).Solid)
                return true;
            if (rm.GetTile(pos with { x = pos.x + 1 }).Solid && rm.GetTile(pos with { x = pos.x - 1 }).Solid)
                return true;
            if (rm.GetTile(pos).shortCut != 0)
                return true;
        }
        return false;
    }

    public override Color ShortCutColor() => shellColor[0];

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        if (!justClicked && speed > 3f && (!safariControlled || Random.value < .2f))
            triggered = true;
        base.TerrainImpact(chunk, direction, speed, firstContact);
        var ar = bodyChunks;
        for (var i = 0; i < ar.Length; i++)
        {
            ref var vel = ref ar[i].vel;
            vel = new(vel.x + Random.Range(.1f, -.1f) - vel.x / 4f, vel.y - vel.y / 4f);
        }
    }

    public override void Update(bool eu)
    {
        Consciousness = Mathf.Clamp01(Consciousness + (!Consious ? .02f : -.0075f));
        Lerper = Mathf.Clamp(Lerper + (LerpUp ? .0075f : -.0075f), -.75f, 1f);
        if (Lerper == 1f)
            LerpUp = false;
        else if (Lerper == -.75f)
            LerpUp = true;
        LastAlpha = Alpha;
        Alpha = Mathf.Lerp(1f, 0f, Mathf.Max(Consciousness, Lerper));
        base.Update(eu);
        var ar = bodyChunks;
        for (var i = 0; i < ar.Length; i++)
        {
            var ari = ar[i];
            ref float velX = ref ari.vel.x, velY = ref ari.vel.y;
            if (velX < -30f || velX > 30f)
                velX = 0f;
            if (velY < -30f || velY > 30f)
                velY = 0f;
        }
    }

    public override void LoseAllGrasps() { }
}