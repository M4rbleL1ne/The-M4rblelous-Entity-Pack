using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using System.Collections.Generic;
using UnityEngine;
using static PathCost.Legality;
using Fisobs.Sandbox;
using Watcher;

namespace LBMergedMods.Creatures;

sealed class MiniLeechCritob : Critob
{
    internal MiniLeechCritob() : base(CreatureTemplateType.MiniBlackLeech)
    {
        Icon = new SimpleIcon("Kill_BXLeech", new(.36862746f, .36862746f, 37f / 85f));
        LoadedPerformanceCost = 10f;
        SandboxPerformanceCost = new(.3f, .1f);
        RegisterUnlock(KillScore.Constant(0), SandboxUnlockID.MiniBlackLeech);
    }

    public override int ExpeditionScore() => 0;

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Leech;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(.36862746f, .36862746f, 37f / 85f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "mblc";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.Swimming
    ];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.Leech, Type, "Mini Black Leech")
        {
            TileResistances = new()
            {
                Air = new(1f, Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                ShortCut = new(1f, Allowed),
                NPCTransportation = new(10f, Allowed)
            },
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Leech),
            HasAI = false,
            DamageResistances = new() { Base = .05f, Water = 2f },
            StunResistances = new() { Base = .05f, Water = 2f },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1f)
        }.IntoTemplate();
        t.instantDeathDamageLimit = .5f;
        t.abstractedLaziness = 50;
        t.offScreenSpeed = .2f;
        t.bodySize = .05f;
        t.visualRadius = 1000f;
        t.throughSurfaceVision = 0f;
        t.waterVision = 1f;
        t.wormGrassImmune = true;
        t.wormgrassTilesIgnored = true;
        t.shortcutColor = new(.36862746f, .36862746f, 37f / 85f);
        t.shortcutSegments = 1;
        t.scaryness = .1f;
        t.deliciousness = 0f;
        return t;
    }

    public override void EstablishRelationships()
    {
        var l = new Relationships(Type);
        l.IgnoredBy(CreatureTemplateType.SurfaceSwimmer);
        l.Ignores(CreatureTemplateType.SurfaceSwimmer);
        l.IgnoredBy(CreatureTemplateType.Polliwog);
        l.Ignores(CreatureTemplateType.Polliwog);
        l.IgnoredBy(CreatureTemplateType.MoleSalamander);
        l.Ignores(CreatureTemplateType.MoleSalamander);
        l.IgnoredBy(CreatureTemplateType.MiniLeviathan);
        l.Fears(CreatureTemplateType.MiniLeviathan, 1f);
        l.IgnoredBy(CreatureTemplateType.WaterBlob);
        l.Ignores(CreatureTemplateType.WaterBlob);
        l.IgnoredBy(CreatureTemplateType.WaterSpitter);
        l.Ignores(CreatureTemplateType.WaterSpitter);
        l.IgnoredBy(CreatureTemplateType.HazerMom);
        l.Ignores(CreatureTemplateType.HazerMom);
        l.IgnoredBy(CreatureTemplate.Type.Hazer);
        l.Ignores(CreatureTemplate.Type.Hazer);
        l.Fears(CreatureTemplate.Type.Snail, 1f);
        l.Fears(CreatureTemplateType.BouncingBall, 1f);
        l.Fears(CreatureTemplate.Type.CicadaA, 1f);
        l.Fears(CreatureTemplate.Type.CicadaB, 1f);
        l.Fears(CreatureTemplate.Type.JetFish, 1f);
        l.Fears(CreatureTemplate.Type.BigEel, 1f);
        l.IgnoredBy(CreatureTemplate.Type.BigEel);
        l.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        l.Ignores(Type);
        if (ModManager.Watcher)
        {
            l.Ignores(WatcherEnums.CreatureTemplateType.Angler);
            l.Ignores(WatcherEnums.CreatureTemplateType.PeachLizard);
        }
    }

    public override IEnumerable<string> WorldFileAliases() => ["miniblackleech", "mini black leech", "mini blackleech", "miniblack leech"];

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new MiniLeech(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new NoHealthState(acrit);

    public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => null;

    public override void KillsMatter(ref bool killsMatter) => killsMatter = false;

    public override void LoadResources(RainWorld rainWorld) { }
}