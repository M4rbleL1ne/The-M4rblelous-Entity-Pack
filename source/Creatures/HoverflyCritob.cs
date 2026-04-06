using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using System.Collections.Generic;
using UnityEngine;
using System;
using Watcher;
using Random = UnityEngine.Random;
using static PathCost.Legality;

namespace LBMergedMods.Creatures;

sealed class HoverflyCritob : Critob, ISandboxHandler
{
    internal HoverflyCritob() : base(CreatureTemplateType.Hoverfly)
    {
        Icon = new SimpleIcon("Kill_Hoverfly", Color.green);
        RegisterUnlock(KillScore.Configurable(2), SandboxUnlockID.Hoverfly);
        SandboxPerformanceCost = new(.4f, .5f);
        LoadedPerformanceCost = 25f;
    }

    public override int ExpeditionScore() => 2;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.green;

    public override string DevtoolsMapName(AbstractCreature acrit) => "hvf";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.LikesOutside,
        RoomAttractivenessPanel.Category.Flying
    ];

    public override IEnumerable<string> WorldFileAliases() => ["hoverfly"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(this)
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
            DamageResistances = new() { Base = .8f },
            StunResistances = new() { Base = 1f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Fly)
        }.IntoTemplate();
        t.abstractedLaziness = 100;
        t.canFly = true;
        t.offScreenSpeed = .5f;
        t.bodySize = .6f;
        t.grasps = 1;
        t.visualRadius = 400f;
        t.movementBasedVision = .6f;
        t.communityInfluence = 1f;
        t.lungCapacity = 180f;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
        t.meatPoints = 3;
        t.usesNPCTransportation = true;
        t.pickupAction = "Pick up Food";
        t.throwAction = "Discard";
        return t;
    }

    public override void EstablishRelationships()
    {
        var hvf = new Relationships(Type);
        hvf.Ignores(Type);
        hvf.EatenBy(CreatureTemplate.Type.LizardTemplate, .6f);
        hvf.EatenBy(CreatureTemplate.Type.WhiteLizard, 1f);
        hvf.EatenBy(CreatureTemplate.Type.DropBug, 1f);
        hvf.EatenBy(CreatureTemplate.Type.Vulture, .05f);
        hvf.Ignores(CreatureTemplate.Type.Centipede);
        hvf.IgnoredBy(CreatureTemplate.Type.Centipede);
        hvf.IgnoredBy(CreatureTemplate.Type.Fly);
        hvf.Ignores(CreatureTemplate.Type.Fly);
        hvf.Ignores(CreatureTemplate.Type.EggBug);
        hvf.IgnoredBy(CreatureTemplate.Type.EggBug);
        hvf.IgnoredBy(CreatureTemplate.Type.Hazer);
        hvf.Ignores(CreatureTemplate.Type.Hazer);
        hvf.IgnoredBy(CreatureTemplate.Type.VultureGrub);
        hvf.Ignores(CreatureTemplate.Type.VultureGrub);
        hvf.Ignores(CreatureTemplate.Type.GarbageWorm);
        hvf.IgnoredBy(CreatureTemplate.Type.GarbageWorm);
        hvf.IgnoredBy(CreatureTemplate.Type.SmallNeedleWorm);
        hvf.Ignores(CreatureTemplate.Type.SmallNeedleWorm);
        hvf.IgnoredBy(CreatureTemplate.Type.TubeWorm);
        hvf.Ignores(CreatureTemplate.Type.TubeWorm);
        hvf.IgnoredBy(CreatureTemplate.Type.LanternMouse);
        hvf.Ignores(CreatureTemplate.Type.LanternMouse);
        hvf.IgnoredBy(CreatureTemplate.Type.Snail);
        hvf.Ignores(CreatureTemplate.Type.Snail);
        if (ModManager.Watcher)
        {
            hvf.Ignores(WatcherEnums.CreatureTemplateType.Barnacle);
            hvf.Ignores(WatcherEnums.CreatureTemplateType.Frog);
            hvf.IgnoredBy(WatcherEnums.CreatureTemplateType.Barnacle);
            hvf.IgnoredBy(WatcherEnums.CreatureTemplateType.Frog);
        }
    }

    public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => new HoverflyAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Hoverfly(acrit, acrit.world);

    public override AbstractCreatureAI? CreateAbstractAI(AbstractCreature acrit) => new HoverflyAbstractAI(acrit.world, acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.CicadaB;

    AbstractWorldEntity ISandboxHandler.ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock)
    {
        var text = data.CustomData + "SandboxData<cC>" + unlock.Data + "<cB>";
        var abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(data.Type.CritType), null, data.Pos, data.ID) { pos = data.Pos };
        abstractCreature.state.LoadFromString(text.Split(["<cB>"], StringSplitOptions.RemoveEmptyEntries));
        abstractCreature.setCustomFlags();
        var state = Random.state;
        Random.InitState(data.ID.RandomSeed);
        if (Random.value < .1f)
            abstractCreature.superSizeMe = true;
        Random.state = state;
        return abstractCreature;
    }
}