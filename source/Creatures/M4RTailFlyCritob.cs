using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using System.Collections.Generic;
using UnityEngine;
using static PathCost.Legality;

namespace LBMergedMods.Creatures;

sealed class M4RTailFlyCritob : Critob
{
    internal M4RTailFlyCritob() : base(CreatureTemplateType.Tailfly)
    {
        Icon = new SimpleIcon("Kill_Tailfly", Color.yellow);
        RegisterUnlock(KillScore.Configurable(2), SandboxUnlockID.Tailfly);
        SandboxPerformanceCost = new(.4f, .5f);
        LoadedPerformanceCost = 25f;
    }

    public override int ExpeditionScore() => 2;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.yellow;

    public override string DevtoolsMapName(AbstractCreature acrit) => "tlf";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.LikesOutside,
        RoomAttractivenessPanel.Category.Flying
    ];

    public override IEnumerable<string> WorldFileAliases() => ["tailfly", "tail fly"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplateType.Hoverfly, this)
        {
            TileResistances = new()
            {
                Air = new(1f, Allowed),
                Corridor = new(10f, Unwanted),
                Floor = new(10f, Unwanted)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                ShortCut = new(1f, Allowed),
                NPCTransportation = new(10f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(10f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Afraid, 1f),
            DamageResistances = new() { Base = .75f },
            StunResistances = new() { Base = .6f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Fly)
        }.IntoTemplate();
        t.bodySize = .55f;
        t.movementBasedVision = .5f;
        t.communityInfluence = .5f;
        t.meatPoints = 2;
        return t;
    }

    public override void EstablishRelationships()
    {
        var self = new Relationships(Type);
        self.Ignores(Type);
    }

    public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => new M4RTailFlyAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new M4RTailFly(acrit, acrit.world);

    public override AbstractCreatureAI? CreateAbstractAI(AbstractCreature acrit) => new HoverflyAbstractAI(acrit.world, acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.CicadaB;
}