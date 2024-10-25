﻿using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using DevInterface;
using RWCustom;

namespace LBMergedMods.Creatures;

sealed class MiniLeviathanCritob : Critob
{
    internal static Color s_col = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

    internal MiniLeviathanCritob() : base(CreatureTemplateType.MiniLeviathan)
    {
        Icon = new SimpleIcon("Kill_MiniLeviathan", s_col);
        ShelterDanger = ShelterDanger.TooLarge;
        RegisterUnlock(KillScore.Configurable(6), SandboxUnlockID.MiniLeviathan);
        SandboxPerformanceCost = new(.5f, .5f);
        LoadedPerformanceCost = 50f;
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.LikesWater,
        RoomAttractivenessPanel.Category.Swimming
    ];

    public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow) => allow = map.getTerrainProximity(tilePos) > 1;

    public override int ExpeditionScore() => 6;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => s_col;

    public override string DevtoolsMapName(AbstractCreature acrit) => "mlev";

    public override IEnumerable<string> WorldFileAliases() => ["minileviathan"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.BigEel, this)
        {
            TileResistances = new()
            {
                Air = new(1f, Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                OutsideRoom = new(1f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(10f, Allowed),
                SeaHighway = new(100f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1f),
            DamageResistances = new() { Base = 8f },
            StunResistances = new() { Base = 8f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.BigEel)
        }.IntoTemplate();
        t.bodySize = 5f;
        t.visualRadius = 450f;
        t.throughSurfaceVision = .45f;
        t.meatPoints = 6;
        t.shortcutColor = s_col;
        t.abstractedLaziness = 10;
        return t;
    }

    public override void EstablishRelationships()
    {
        var l = new Relationships(Type);
        l.Ignores(Type);
        l.Ignores(CreatureTemplate.Type.TentaclePlant);
        l.Ignores(CreatureTemplate.Type.PoleMimic);
        l.Ignores(CreatureTemplate.Type.Centipede);
        l.Eats(CreatureTemplate.Type.SmallCentipede, 1f);
        l.Ignores(CreatureTemplate.Type.MirosBird);
        l.Ignores(CreatureTemplate.Type.Vulture);
        l.Ignores(CreatureTemplate.Type.KingVulture);
        l.Ignores(CreatureTemplate.Type.TempleGuard);
        l.Ignores(CreatureTemplate.Type.DaddyLongLegs);
        l.Ignores(CreatureTemplate.Type.BrotherLongLegs);
        l.Ignores(CreatureTemplate.Type.Deer);
        l.Ignores(CreatureTemplate.Type.BigEel);
        l.Ignores(CreatureTemplate.Type.GreenLizard);
        l.Ignores(CreatureTemplate.Type.RedLizard);
        l.Ignores(CreatureTemplate.Type.Leech);
        l.FearedBy(CreatureTemplate.Type.TentaclePlant, 1f);
        l.FearedBy(CreatureTemplate.Type.PoleMimic, 1f);
        l.IgnoredBy(CreatureTemplate.Type.Centipede);
        l.FearedBy(CreatureTemplate.Type.SmallCentipede, 1f);
        l.IgnoredBy(CreatureTemplate.Type.MirosBird);
        l.IgnoredBy(CreatureTemplate.Type.Vulture);
        l.IgnoredBy(CreatureTemplate.Type.KingVulture);
        l.IgnoredBy(CreatureTemplate.Type.TempleGuard);
        l.IgnoredBy(CreatureTemplate.Type.DaddyLongLegs);
        l.IgnoredBy(CreatureTemplate.Type.BrotherLongLegs);
        l.IgnoredBy(CreatureTemplate.Type.Deer);
        l.Ignores(CreatureTemplate.Type.Deer);
        l.IgnoredBy(CreatureTemplate.Type.BigEel);
        l.IgnoredBy(CreatureTemplate.Type.GreenLizard);
        l.IgnoredBy(CreatureTemplate.Type.RedLizard);
        l.IgnoredBy(CreatureTemplate.Type.Leech);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new BigEelAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new BigEel(acrit, acrit.world);

    public override AbstractCreatureAI? CreateAbstractAI(AbstractCreature acrit) => new BigEelAbstractAI(acrit.world, acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.JetFish;
}