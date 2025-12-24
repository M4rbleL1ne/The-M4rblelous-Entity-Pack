using Fisobs.Creatures;
using Fisobs.Core;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using System.Collections.Generic;
using DevInterface;
using RWCustom;

namespace LBMergedMods.Creatures;

sealed class M4RJawsCritob : Critob, ISandboxHandler
{
    internal M4RJawsCritob() : base(CreatureTemplateType.SparkEye)
    {
        Icon = new SimpleIcon("Kill_M4RDoubleJaw", new(.75f, .15f, 0f));
        LoadedPerformanceCost = 100f;
        SandboxPerformanceCost = new(1.5f, .5f);
        ShelterDanger = ShelterDanger.TooLarge;
        RegisterUnlock(KillScore.Configurable(25), SandboxUnlockID.SparkEye);
    }

    public override int ExpeditionScore() => 25;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(.75f, .15f, 0f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "spEy";

    public override IEnumerable<string> WorldFileAliases() => ["sparkeye", "spark eye"];

    public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow)
    {
        var prox = map.getTerrainProximity(tilePos);
        if (prox <= 1)
            allow = false;
        else if (map.room?.game?.GetArenaGameSession?.arenaSitting?.sandboxPlayMode is true or null)
        {
            var aItile = map.getAItile(tilePos);
            if (aItile.smoothedFloorAltitude > 2 && aItile.smoothedFloorAltitude + aItile.floorAltitude > Custom.LerpMap(prox, 2f, 6f, 6f, 4f) * 2f)
                allow = false;
        }
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.LikesOutside,
        RoomAttractivenessPanel.Category.Dark
    ];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.MirosBird, this)
        {
            TileResistances = new()
            {
                Air = new(1f, Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                OutsideRoom = new(1f, Allowed),
                SideHighway = new(100f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(10f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1f),
            DamageResistances = new() { Base = 15f },
            StunResistances = new() { Base = 1.75f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.MirosBird)
        }.IntoTemplate();
        t.canSwim = false;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
        t.visualRadius = 2000f;
        t.movementBasedVision = .9f;
        t.dangerousToPlayer = 1f;
        t.meatPoints = 15;
        t.bodySize = 7.5f;
        t.jumpAction = "Hiss";
        t.canAutoAbstractPath = false;
        t.isTooCloseToTerrain = (aimap, pos) =>
        {
            var prox = aimap.getTerrainProximity(pos);
            if (prox <= 1)
                return 1;
            var aItile = aimap.getAItile(pos);
            return (aItile.smoothedFloorAltitude > 2 && (aItile.smoothedFloorAltitude + aItile.floorAltitude) > Custom.LerpMap(prox, 2f, 6f, 6f, 4f) * 2f) ? 1 : 0;
        };
        return t;
    }

    public override void EstablishRelationships()
    {
        var rels = new Relationships(Type);
        rels.Antagonizes(Type, .5f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new M4RJawsAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new M4RJaws(acrit, acrit.world);

    public override AbstractCreatureAI? CreateAbstractAI(AbstractCreature acrit) => new M4RJawsAbstractAI(acrit.world, acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.MirosBird;
}