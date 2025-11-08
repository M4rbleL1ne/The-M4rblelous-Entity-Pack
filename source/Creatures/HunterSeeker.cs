using UnityEngine;
using Watcher;
using RWCustom;

namespace LBMergedMods.Creatures;

public class HunterSeeker : Lizard
{
    public override float VisibilityBonus
    {
        get
        {
            var vis = base.VisibilityBonus;
            if (graphicsModule is HunterSeekerGraphics g)
            {
                if (vis == 0f)
                    return -g.Camouflaged;
                return Mathf.Lerp(-g.Camouflaged, vis, Mathf.Pow(Mathf.Clamp01(Vector3.Distance(Custom.RGB2Vec3(g.whiteCamoColor), Custom.RGB2Vec3(mudColor))), .25f));
            }
            return vis;
        }
    }

    public HunterSeeker(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        effectColor = lizardParams.standardColor;
        if (rotModule is LizardRotModule mod && LizardState.rotType != LizardState.RotType.Slight)
            effectColor = Color.Lerp(effectColor, mod.RotEyeColor, LizardState.rotType == LizardState.RotType.Opossum ? .2f : .8f);
        jumpModule = new(this);
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new HunterSeekerGraphics(this);

    public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos onAppendagePos, Vector2 direction)
    {
        var res = base.SpearStick(source, dmg, chunk, onAppendagePos, direction);
        if ((onAppendagePos is not null && rotModule is not null) || chunk is null || (chunk.index == 0 && HitHeadShield(direction)))
            return res;
        var flag = chunk.index == 0 && HitInMouth(direction);
        if (source is Spear s && !dead && !flag && jumpModule.gasLeakPower > 0f && jumpModule.gasLeakSpear is null && chunk.index < 2 && (animation == Animation.Jumping || animation == Animation.PrepareToJump || Random.value < (chunk.index == 1 ? .5f : .25f)))
            jumpModule.gasLeakSpear = s;
        return res;
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}