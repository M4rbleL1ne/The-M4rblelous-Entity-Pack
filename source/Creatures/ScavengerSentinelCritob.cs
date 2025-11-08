using Fisobs.Creatures;
using Fisobs.Core;
using Fisobs.Sandbox;
using UnityEngine;
using System.Collections.Generic;
using DevInterface;
using Random = UnityEngine.Random;
using static PathCost.Legality;
using Watcher;

namespace LBMergedMods.Creatures;

sealed class ScavengerSentinelCritob : Critob
{
    internal ScavengerSentinelCritob() : base(CreatureTemplateType.ScavengerSentinel)
    {
        Icon = new SimpleIcon("Kill_ScavengerSentinel", Ext.MenuGrey);
        SandboxPerformanceCost = new(.5f, .925f);
        LoadedPerformanceCost = 300f;
        RegisterUnlock(KillScore.Configurable(12), SandboxUnlockID.ScavengerSentinel);
    }

    public override int ExpeditionScore() => 12;

    public override Color DevtoolsMapColor(AbstractCreature acrit)
    {
        var absAI = (acrit.abstractAI as ScavengerAbstractAI)!;
        if (absAI.freeze > 0)
            return Color.gray;
        if (absAI.squad is not ScavengerAbstractAI.ScavengerSquad squad)
            return new(0f, .2f, .14f);
        var mission = squad.missionType;
        if (Random.value < .3f && mission != ScavengerAbstractAI.ScavengerSquad.MissionID.None)
        {
            if (mission == ScavengerAbstractAI.ScavengerSquad.MissionID.HuntCreature)
                return Color.red;
            if (mission == ScavengerAbstractAI.ScavengerSquad.MissionID.GuardOutpost)
                return Color.blue;
            if (mission == ScavengerAbstractAI.ScavengerSquad.MissionID.ProtectCreature)
                return Color.green;
            if (mission == ScavengerAbstractAI.ScavengerSquad.MissionID.Trade)
                return new(1f, 1f, 0f);
        }
        if (squad.leader == acrit)
            return Color.Lerp(squad.color, Color.white, Random.value);
        return squad.color;
    }

    public override string DevtoolsMapName(AbstractCreature acrit)
    {
        var text = "St";
        if ((acrit.abstractAI as ScavengerAbstractAI)!.squad is not ScavengerAbstractAI.ScavengerSquad squad)
            return text;
        var mission = squad.missionType;
        if (mission != ScavengerAbstractAI.ScavengerSquad.MissionID.None)
        {
            if (mission == ScavengerAbstractAI.ScavengerSquad.MissionID.HuntCreature)
                return text + "(H)";
            if (mission == ScavengerAbstractAI.ScavengerSquad.MissionID.GuardOutpost)
                return text + "(G)";
            if (mission == ScavengerAbstractAI.ScavengerSquad.MissionID.ProtectCreature)
                return text + "(P)";
            if (mission == ScavengerAbstractAI.ScavengerSquad.MissionID.Trade)
                return text + "(T)";
        }
        return text;
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.Swimming,
        RoomAttractivenessPanel.Category.LikesWater
    ];

    public override void GraspParalyzesPlayer(Creature.Grasp grasp, ref bool paralyzing) => paralyzing = true;

    public override IEnumerable<string> WorldFileAliases() => ["scavsentinel", "scav sentinel", "scavenger sentinel", "scavengersentinel"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.Scavenger, this)
        {
            TileResistances = new()
            {
                OffScreen = new(1f, Allowed),
                Floor = new(1f, Allowed),
                Corridor = new(1f, Allowed),
                Climb = new(2.5f, Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                OpenDiagonal = new(3f, Allowed),
                ReachOverGap = new(3f, Allowed),
                ReachUp = new(2f, Allowed),
                DoubleReachUp = new(2f, Allowed),
                ReachDown = new(2f, Allowed),
                SemiDiagonalReach = new(2f, Allowed),
                DropToFloor = new(10f, Allowed),
                DropToClimb = new(10f, Allowed),
                DropToWater = new(10f, Allowed),
                ShortCut = new(2.5f, Allowed),
                NPCTransportation = new(225f, Allowed),
                Slope = new(1.5f, Allowed),
                RegionTransportation = new(400f, Allowed),
                OffScreenMovement = new(1.2f, Allowed),
                BetweenRooms = new(15f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, .1f),
            DamageResistances = new() { Base = 2.5f, Explosion = 2.5f },
            StunResistances = new() { Base = 3f, Explosion = 2.75f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Scavenger)
        }.IntoTemplate();
        t.SetDoubleReachUpConnectionParams(AItile.Accessibility.Climb, AItile.Accessibility.Air, AItile.Accessibility.Climb);
        t.SetNodeType(AbstractRoomNode.Type.Den, false);
        t.SetNodeType(AbstractRoomNode.Type.RegionTransportation, true);
        t.instantDeathDamageLimit = 4f;
        t.bodySize = 1.3f;
        t.visualRadius = 1400f;
        t.movementBasedVision = .35f;
        t.dangerousToPlayer = .85f;
        t.meatPoints = 5;
        return t;
    }

    public override void EstablishRelationships()
    {
        var me = new Relationships(Type);
        me.FearedBy(CreatureTemplate.Type.BlueLizard, .6f);
        me.Attacks(CreatureTemplate.Type.BlueLizard, .5f);
        me.FearedBy(CreatureTemplate.Type.CyanLizard, .2f);
        me.Attacks(CreatureTemplate.Type.CyanLizard, .5f);
        me.FearedBy(CreatureTemplate.Type.YellowLizard, .4f);
        me.Attacks(CreatureTemplate.Type.YellowLizard, .5f);
        me.FearedBy(CreatureTemplate.Type.PinkLizard, .4f);
        me.Attacks(CreatureTemplate.Type.PinkLizard, .5f);
        me.FearedBy(CreatureTemplate.Type.WhiteLizard, .3f);
        me.Attacks(CreatureTemplate.Type.WhiteLizard, .5f);
        me.FearedBy(CreatureTemplate.Type.Salamander, .3f);
        me.Attacks(CreatureTemplate.Type.Salamander, .5f);
        me.Attacks(CreatureTemplate.Type.BlackLizard, .25f);
        if (ModManager.DLCShared)
        {
            me.FearedBy(DLCSharedEnums.CreatureTemplateType.ZoopLizard, .5f);
            me.Attacks(DLCSharedEnums.CreatureTemplateType.ZoopLizard, .5f);
        }
        me.FearedBy(CreatureTemplateType.Polliwog, .6f);
        me.Attacks(CreatureTemplateType.Polliwog, .5f);
        me.FearedBy(CreatureTemplateType.HunterSeeker, .1f);
        me.Attacks(CreatureTemplateType.HunterSeeker, .5f);
        me.Attacks(CreatureTemplateType.MoleSalamander, .25f);
        if (ModManager.Watcher)
        {
            me.Attacks(WatcherEnums.CreatureTemplateType.PeachLizard, .5f);
            me.FearedBy(WatcherEnums.CreatureTemplateType.PeachLizard, .6f);
        }
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new ScavengerSentinelAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new ScavengerSentinel(acrit, acrit.world);

    public override AbstractCreatureAI? CreateAbstractAI(AbstractCreature acrit) => new ScavengerAbstractAI(acrit.world, acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Scavenger;
}