using UnityEngine;
using RWCustom;
using Watcher;

namespace LBMergedMods.Creatures;

public class Polliwog : Lizard
{
    public Polliwog(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        tongue ??= new(this);
        effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.708f, .1f, .6f), .482f, Custom.ClampedRandomVariation(.5f, .15f, .1f));
        if (rotModule is LizardRotModule mod && LizardState.rotType != LizardState.RotType.Slight)
            effectColor = Color.Lerp(effectColor, mod.RotEyeColor, LizardState.rotType == LizardState.RotType.Opossum ? .2f : .8f);
        Random.state = state;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (!ModManager.MMF)
            buoyancy = .92f;
        if (LizardState?.limbHealth is float[] ar)
        {
            ar[2] = 0f;
            ar[3] = 0f;
        }
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new PolliwogGraphics(this);

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}