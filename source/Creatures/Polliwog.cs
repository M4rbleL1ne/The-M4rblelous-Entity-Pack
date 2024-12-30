﻿using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;

public class Polliwog : Lizard
{
    public Polliwog(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        tongue ??= new(this);
        effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.708f, .1f, .6f), .482f, Custom.ClampedRandomVariation(.5f, .15f, .1f));
        buoyancy = .92f;
        Random.state = state;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        lungs = 1f;
        if (LizardState?.limbHealth is float[] ar)
        {
            ar[2] = 0f;
            ar[3] = 0f;
        }
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new PolliwogGraphics(this);

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}