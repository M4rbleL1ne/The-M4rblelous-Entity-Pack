using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using System.Collections.Generic;
using UnityEngine;
using Fisobs.Sandbox;
using Watcher;

namespace LBMergedMods.Creatures;

sealed class PolliwogCritob : Critob
{
    internal PolliwogCritob() : base(CreatureTemplateType.Polliwog)
    {
        Icon = new SimpleIcon("Kill_Polliwog", new(.38f, .259f, .741f));
        LoadedPerformanceCost = 50f;
        SandboxPerformanceCost = new(.5f, .6f);
        RegisterUnlock(KillScore.Configurable(5), SandboxUnlockID.Polliwog);
    }

    public override int ExpeditionScore() => 5;

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Salamander;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(.38f, .259f, .741f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "Polliwog";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.LikesWater,
        RoomAttractivenessPanel.Category.Lizards,
        RoomAttractivenessPanel.Category.Swimming
    ];

    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard));

    public override void EstablishRelationships()
    {
        var p = new Relationships(Type);
        p.Fears(CreatureTemplate.Type.GreenLizard, .3f);
        p.Fears(CreatureTemplate.Type.RedLizard, .5f);
        p.Fears(CreatureTemplate.Type.CyanLizard, .4f);
        p.Fears(CreatureTemplateType.HunterSeeker, .4f);
        p.Attacks(CreatureTemplate.Type.YellowLizard, .3f);
        p.IsInPack(Type, .6f);
        p.Eats(CreatureTemplate.Type.Snail, .4f);
        p.EatenBy(CreatureTemplate.Type.BigEel, .6f);
        p.Fears(CreatureTemplate.Type.BigEel, .7f);
        p.Fears(CreatureTemplate.Type.Vulture, .7f);
        p.Fears(CreatureTemplate.Type.KingVulture, .8f);
        p.EatenBy(CreatureTemplate.Type.GreenLizard, .5f);
        p.EatenBy(CreatureTemplate.Type.RedLizard, .5f);
        p.EatenBy(CreatureTemplate.Type.CyanLizard, .4f);
        p.EatenBy(CreatureTemplateType.HunterSeeker, .4f);
        p.AttackedBy(CreatureTemplate.Type.YellowLizard, .5f);
        p.EatenBy(CreatureTemplate.Type.Vulture, .6f);
        p.EatenBy(CreatureTemplate.Type.KingVulture, .5f);
        p.IgnoredBy(CreatureTemplate.Type.Leech);
        if (ModManager.Watcher)
        {
            p.Attacks(WatcherEnums.CreatureTemplateType.PeachLizard, .3f);
            p.AttackedBy(WatcherEnums.CreatureTemplateType.PeachLizard, .3f);
        }
    }

    public override IEnumerable<string> WorldFileAliases() => ["polliwog"];

    public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => new PolliwogAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Polliwog(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }
}