using RWCustom;
using UnityEngine;
using Watcher;

namespace LBMergedMods.Creatures;

public class MoleSalamander : Lizard
{
    public bool Black;

    public MoleSalamander(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        if (world.region is Region reg)
            Black = Random.value < reg.regionParams.blackSalamanderChance;
        else
            Black = Random.value < 1f / 3f;
        effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.9f, .15f, .6f), 1f, Custom.ClampedRandomVariation(.4f, .15f, .2f));
        if (rotModule is LizardRotModule mod && LizardState.rotType != LizardState.RotType.Slight)
            effectColor = Color.Lerp(effectColor, mod.RotEyeColor, LizardState.rotType == LizardState.RotType.Opossum ? .2f : .8f);
        Random.state = state;
        abstractCreature.HypothermiaImmune = true;
        firstChunk.rad *= 1.15f;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new MoleSalamanderGraphics(this);

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (!ModManager.MMF)
            buoyancy = .92f;
    }

    public override Color ShortCutColor() => Black ? Color.Lerp(Color.black, Color.gray, .5f) : base.ShortCutColor();

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}