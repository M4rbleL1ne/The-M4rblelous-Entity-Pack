using UnityEngine;
using RWCustom;
using MoreSlugcats;

namespace LBMergedMods.Creatures;

public class MiniScutigera(AbstractCreature abstractCreature, World world) : Centipede(abstractCreature, world)
{
    public override void InitiateGraphicsModule() => graphicsModule ??= new MiniScutigeraGraphics(this);

    public override void Update(bool eu)
    {
        shockCharge = 0f;
        base.Update(eu);
        if (room is Room rm && !dead)
        {
            var cAr = bodyChunks;
            for (var i = 0; i < cAr.Length; i++)
            {
                var b = cAr[i];
                if (Random.value < .005f)
                    rm.AddObject(new ScutigeraFlash(b.pos, b.rad / (b.rad * 2.5f)));
            }
            if (grabbedBy.Count > 0)
            {
                var grabber = grabbedBy[0].grabber;
                Shock(grabber);
                if (ModManager.MSC && grabber is Player p && p.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
                    p.SaintStagger(680);
                Stun(11);
                for (var j = 0; j < cAr.Length; j++)
                    cAr[j].vel += Custom.RNV() * (Random.value * 6f);
            }
            var mbc = mainBodyChunk;
            if (mbc.submersion > .1f)
            {
                rm.PlaySound(SoundID.Centipede_Shock, mbc);
                rm.AddObject(new UnderwaterShock(rm, this, mbc.pos, 14, 600f, .35f, this, new(.7f, 1f, .7f)));
                Die();
            }
        }
    }

    public override Color ShortCutColor() => Custom.HSL2RGB(Mathf.Lerp(.1527777777777778f, .1861111111111111f, .5f), Mathf.Lerp(.294f, .339f, .5f), .5f);

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        stunBonus *= .6f;
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }
}