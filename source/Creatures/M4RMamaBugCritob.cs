using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;

namespace LBMergedMods.Creatures;

sealed class M4RMamaBugCritob : Critob
{
    internal M4RMamaBugCritob() : base(CreatureTemplateType.MamaBug)
    {
        Icon = new SimpleIcon("Kill_EggBug", M4RMamaBug.BugCol);
        RegisterUnlock(KillScore.Configurable(4), SandboxUnlockID.MamaBug);
        SandboxPerformanceCost = new(.4f, .4f);
        LoadedPerformanceCost = 30f;
    }

    public override int ExpeditionScore() => 4;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => M4RMamaBug.BugCol;

    public override string DevtoolsMapName(AbstractCreature acrit) => "mm";

    public override IEnumerable<string> WorldFileAliases() => ["mamabug", "mama bug"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.EggBug, Type, "Mama Bug")
        {
            TileResistances = new()
            {
                OffScreen = new(1f, Allowed),
                Floor = new(1f, Allowed),
                Corridor = new(1f, Allowed),
                Climb = new(2.5f, Allowed),
                Wall = new(50f, Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                DropToFloor = new(10f, Allowed),
                DropToWater = new(10f, Allowed),
                DropToClimb = new(10f, Allowed),
                ShortCut = new(1.5f, Allowed),
                NPCTransportation = new(3f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(5f, Allowed),
                Slope = new(1.5f, Allowed),
                OpenDiagonal = new(3f, Allowed),
                ReachOverGap = new(3f, Allowed),
                ReachUp = new(2f, Allowed),
                SemiDiagonalReach = new(2f, Allowed),
                ReachDown = new(2f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Afraid, .1f),
            DamageResistances = new() { Base = 2.25f },
            StunResistances = new() { Base = 1.75f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.WhiteLizard)
        }.IntoTemplate();
        t.bodySize = 1.5f;
        t.instantDeathDamageLimit = 2f;
        t.visualRadius = 1600f;
        t.meatPoints = 2;
        return t;
    }

    public override void EstablishRelationships()
    {
        var me = new Relationships(Type);
        me.Ignores(Type);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new EggBugAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new M4RMamaBug(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.EggBug;
}