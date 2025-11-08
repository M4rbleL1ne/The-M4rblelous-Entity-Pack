using RWCustom;
using UnityEngine;

namespace LBMergedMods.Creatures;

public class CommonEel : Lizard
{
    public static Color EelCol = Color.Lerp(new(46f / 51f, .05490196f, .05490196f), Color.gray, .2f);
    public float AirInLungs = 1f;

    public CommonEel(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        effectColor = Custom.HSL2RGB(abstractCreature.superSizeMe ? Custom.WrappedRandomVariation(225f / 360f, .02f, .6f) : Custom.WrappedRandomVariation(.0025f, .02f, .6f), 1f, Custom.ClampedRandomVariation(.5f, .15f, .1f));
        abstractCreature.HypothermiaImmune = true;
        var chs = bodyChunks;
        for (var i = 0; i < chs.Length; i++)
        {
            var c = chs[i];
            c.rad *= .75f;
            c.mass *= .75f;
        }
        LizardState.rotType = LizardState.RotType.None;
        rotModule = null;
    }

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new CommonEelGraphics(this);
        graphicsModule.Reset();
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (!ModManager.MMF)
            buoyancy = .92f;
        if (LizardState?.limbHealth is float[] ar)
        {
            for (var i = 0; i < ar.Length; i++)
                ar[i] = 0f;
        }
        if (Submersion < .2f)
        {
            if (AirInLungs > 0f)
                AirInLungs -= .002f;
            GoThroughFloors = false;
            if (Consious && room is Room rm)
            {
                var chs = bodyChunks;
                for (var i = 0; i < chs.Length; i++)
                {
                    var ch = chs[i];
                    ch.vel += Custom.RNV();
                    var tl = rm.GetTile(ch.pos);
                    if (tl.Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                        ch.vel += Custom.IntVector2ToVector2(rm.ShorcutEntranceHoleDirection(new(tl.X, tl.Y))) * 15f;
                }
            }
        }
        else
        {
            GoThroughFloors = !dead;
            AirInLungs = 1f;
        }
        if (AirInLungs <= 0f)
            Die();
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}