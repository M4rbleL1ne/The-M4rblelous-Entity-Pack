global using LBMergedMods.Enums;
using DevInterface;
using MoreSlugcats;
using System.Collections.Generic;
using Menu;

namespace LBMergedMods.Enums;

public static class CommunityID
{
    public static CreatureCommunities.CommunityID TintedBeetles = new(nameof(TintedBeetles), true);

    public static void UnregisterValues()
    {
        if (TintedBeetles is not null)
        {
            TintedBeetles.Unregister();
            TintedBeetles = null!;
        }
    }
}

public static class DevObjectCategories
{
    public static ObjectsPage.DevObjectCategories M4rblelousEntities = new(nameof(M4rblelousEntities), true);

    public static void UnregisterValues()
    {
        if (M4rblelousEntities is not null)
        {
            M4rblelousEntities.Unregister();
            M4rblelousEntities = null!;
        }
    }
}

public static class DevEffectsCategories
{
    public static RoomSettingsPage.DevEffectsCategories M4rblelousEntities = new(nameof(M4rblelousEntities), true);

    public static void UnregisterValues()
    {
        if (M4rblelousEntities is not null)
        {
            M4rblelousEntities.Unregister();
            M4rblelousEntities = null!;
        }
    }
}

public static class RoomEffectType
{
    public static RoomSettings.RoomEffect.Type SeedBats = new(nameof(SeedBats), true),
        Bigrubs = new(nameof(Bigrubs), true);

    public static void UnregisterValues()
    {
        if (SeedBats is not null)
        {
            SeedBats.Unregister();
            SeedBats = null!;
        }
        if (Bigrubs is not null)
        {
            Bigrubs.Unregister();
            Bigrubs = null!;
        }
    }
}

public static class CreatureTemplateType
{
    public static HashSet<CreatureTemplate.Type> M4RCreatureList;
    public static CreatureTemplate.Type SilverLizard = new(nameof(SilverLizard), true),
        ThornBug = new(nameof(ThornBug), true),
        NoodleEater = new(nameof(NoodleEater), true),
        SurfaceSwimmer = new(nameof(SurfaceSwimmer), true),
        Scutigera = new(nameof(Scutigera), true),
        RedHorrorCenti = new(nameof(RedHorrorCenti), true),
        Polliwog = new(nameof(Polliwog), true),
        MiniLeviathan = new(nameof(MiniLeviathan), true),
        Hoverfly = new(nameof(Hoverfly), true),
        FatFireFly = new(nameof(FatFireFly), true),
        BouncingBall = new(nameof(BouncingBall), true),
        Sporantula = new(nameof(Sporantula), true),
        WaterSpitter = new(nameof(WaterSpitter), true),
        WaterBlob = new(nameof(WaterBlob), true),
        HunterSeeker = new(nameof(HunterSeeker), true),
        FlyingBigEel = new(nameof(FlyingBigEel), true),
        MiniFlyingBigEel = new(nameof(MiniFlyingBigEel), true),
        HazerMom = new(nameof(HazerMom), true),
        TintedBeetle = new(nameof(TintedBeetle), true),
        Blizzor = new(nameof(Blizzor), true),
        MoleSalamander = new(nameof(MoleSalamander), true),
        MiniBlackLeech = new(nameof(MiniBlackLeech), true),
        Denture = new(nameof(Denture), true),
        CommonEel = new(nameof(CommonEel), true),
        DivingBeetle = new(nameof(DivingBeetle), true),
        Killerpillar = new(nameof(Killerpillar), true),
        Glowpillar = new(nameof(Glowpillar), true),
        ChipChop = new(nameof(ChipChop), true),
        MiniScutigera = new(nameof(MiniScutigera), true),
        Xylo = new(nameof(Xylo), true),
        XyloWorm = new(nameof(XyloWorm), true),
        SparkEye = new(nameof(SparkEye), true),
        ScavengerSentinel = new(nameof(ScavengerSentinel), true),
        AlphaOrange = new(nameof(AlphaOrange), true),
        MamaBug = new(nameof(MamaBug), true);

    static CreatureTemplateType()
    {
        M4RCreatureList = [SilverLizard,
            ThornBug,
            NoodleEater,
            SurfaceSwimmer,
            Scutigera,
            RedHorrorCenti,
            Polliwog,
            MiniLeviathan,
            Hoverfly,
            FatFireFly,
            BouncingBall,
            Sporantula,
            WaterSpitter,
            WaterBlob,
            HunterSeeker,
            FlyingBigEel,
            MiniFlyingBigEel,
            HazerMom,
            TintedBeetle,
            MiniBlackLeech,
            Denture,
            Blizzor,
            MoleSalamander,
            CommonEel,
            DivingBeetle,
            Killerpillar,
            Glowpillar,
            ChipChop,
            MiniScutigera,
            Xylo,
            XyloWorm,
            SparkEye,
            ScavengerSentinel,
            AlphaOrange,
            MamaBug];
    }

    public static void UnregisterValues()
    {
        if (HazerMom is not null)
        {
            HazerMom.Unregister();
            HazerMom = null!;
        }
        if (SilverLizard is not null)
        {
            SilverLizard.Unregister();
            SilverLizard = null!;
        }
        if (ThornBug is not null)
        {
            ThornBug.Unregister();
            ThornBug = null!;
        }
        if (NoodleEater is not null)
        {
            NoodleEater.Unregister();
            NoodleEater = null!;
        }
        if (SurfaceSwimmer is not null)
        {
            SurfaceSwimmer.Unregister();
            SurfaceSwimmer = null!;
        }
        if (Scutigera is not null)
        {
            Scutigera.Unregister();
            Scutigera = null!;
        }
        if (RedHorrorCenti is not null)
        {
            RedHorrorCenti.Unregister();
            RedHorrorCenti = null!;
        }
        if (Polliwog is not null)
        {
            Polliwog.Unregister();
            Polliwog = null!;
        }
        if (MiniLeviathan is not null)
        {
            MiniLeviathan.Unregister();
            MiniLeviathan = null!;
        }
        if (Hoverfly is not null)
        {
            Hoverfly.Unregister();
            Hoverfly = null!;
        }
        if (FatFireFly is not null)
        {
            FatFireFly.Unregister();
            FatFireFly = null!;
        }
        if (BouncingBall is not null)
        {
            BouncingBall.Unregister();
            BouncingBall = null!;
        }
        if (Sporantula is not null)
        {
            Sporantula.Unregister();
            Sporantula = null!;
        }
        if (WaterSpitter is not null)
        {
            WaterSpitter.Unregister();
            WaterSpitter = null!;
        }
        if (WaterBlob is not null)
        {
            WaterBlob.Unregister();
            WaterBlob = null!;
        }
        if (HunterSeeker is not null)
        {
            HunterSeeker.Unregister();
            HunterSeeker = null!;
        }
        if (FlyingBigEel is not null)
        {
            FlyingBigEel.Unregister();
            FlyingBigEel = null!;
        }
        if (MiniFlyingBigEel is not null)
        {
            MiniFlyingBigEel.Unregister();
            MiniFlyingBigEel = null!;
        }
        if (TintedBeetle is not null)
        {
            TintedBeetle.Unregister();
            TintedBeetle = null!;
        }
        if (Blizzor is not null)
        {
            Blizzor.Unregister();
            Blizzor = null!;
        }
        if (MoleSalamander is not null)
        {
            MoleSalamander.Unregister();
            MoleSalamander = null!;
        }
        if (MiniBlackLeech is not null)
        {
            MiniBlackLeech.Unregister();
            MiniBlackLeech = null!;
        }
        if (Denture is not null)
        {
            Denture.Unregister();
            Denture = null!;
        }
        if (CommonEel is not null)
        {
            CommonEel.Unregister();
            CommonEel = null!;
        }
        if (DivingBeetle is not null)
        {
            DivingBeetle.Unregister();
            DivingBeetle = null!;
        }
        if (Killerpillar is not null)
        {
            Killerpillar.Unregister();
            Killerpillar = null!;
        }
        if (Glowpillar is not null)
        {
            Glowpillar.Unregister();
            Glowpillar = null!;
        }
        if (ChipChop is not null)
        {
            ChipChop.Unregister();
            ChipChop = null!;
        }
        if (MiniScutigera is not null)
        {
            MiniScutigera.Unregister();
            MiniScutigera = null!;
        }
        if (Xylo is not null)
        {
            Xylo.Unregister();
            Xylo = null!;
        }
        if (XyloWorm is not null)
        {
            XyloWorm.Unregister();
            XyloWorm = null!;
        }
        if (SparkEye is not null)
        {
            SparkEye.Unregister();
            SparkEye = null!;
        }
        if (ScavengerSentinel is not null)
        {
            ScavengerSentinel.Unregister();
            ScavengerSentinel = null!;
        }
        if (AlphaOrange is not null)
        {
            AlphaOrange.Unregister();
            AlphaOrange = null!;
        }
        if (MamaBug is not null)
        {
            MamaBug.Unregister();
            MamaBug = null!;
        }
    }
}

public static class SandboxUnlockID
{
    public static HashSet<MultiplayerUnlocks.SandboxUnlockID> M4RUnlockList;
    public static MultiplayerUnlocks.SandboxUnlockID ThornyStrawberry = new(nameof(ThornyStrawberry), true),
        SilverLizard = new(nameof(SilverLizard), true),
        ThornBug = new(nameof(ThornBug), true),
        NoodleEater = new(nameof(NoodleEater), true),
        SurfaceSwimmer = new(nameof(SurfaceSwimmer), true),
        SeedBat = new(nameof(SeedBat), true),
        Scutigera = new(nameof(Scutigera), true),
        RedHorrorCenti = new(nameof(RedHorrorCenti), true),
        Bigrub = new(nameof(Bigrub), true),
        Polliwog = new(nameof(Polliwog), true),
        MiniLeviathan = new(nameof(MiniLeviathan), true),
        Hoverfly = new(nameof(Hoverfly), true),
        FatFireFly = new(nameof(FatFireFly), true),
        BouncingBall = new(nameof(BouncingBall), true),
        Sporantula = new(nameof(Sporantula), true),
        WaterSpitter = new(nameof(WaterSpitter), true),
        WaterBlob = new(nameof(WaterBlob), true),
        HunterSeeker = new(nameof(HunterSeeker), true),
        FlyingBigEel = new(nameof(FlyingBigEel), true),
        MiniFlyingBigEel = new(nameof(MiniFlyingBigEel), true),
        LittleBalloon = new(nameof(LittleBalloon), true),
        BouncingMelon = new(nameof(BouncingMelon), true),
        HazerMom = new(nameof(HazerMom), true),
        TintedBeetle = new(nameof(TintedBeetle), true),
        Physalis = new(nameof(Physalis), true),
        LimeMushroom = new(nameof(LimeMushroom), true),
        Blizzor = new(nameof(Blizzor), true),
        MoleSalamander = new(nameof(MoleSalamander), true),
        MarineEye = new(nameof(MarineEye), true),
        MiniBlackLeech = new(nameof(MiniBlackLeech), true),
        StarLemon = new(nameof(StarLemon), true),
        Denture = new(nameof(Denture), true),
        DendriticNeuron = new(nameof(DendriticNeuron), true),
        CommonEel = new(nameof(CommonEel), true),
        DivingBeetle = new(nameof(DivingBeetle), true),
        Killerpillar = new(nameof(Killerpillar), true),
        Glowpillar = new(nameof(Glowpillar), true),
        MiniBlueFruit = new(nameof(MiniBlueFruit), true),
        SporeProjectile = new(nameof(SporeProjectile), true),
        ChipChop = new(nameof(ChipChop), true),
        MiniScutigera = new(nameof(MiniScutigera), true),
        Xylo = new(nameof(Xylo), true),
        XyloWorm = new(nameof(XyloWorm), true),
        BigXyloWorm = new(nameof(BigXyloWorm), true),
        FumeFruit = new(nameof(FumeFruit), true),
        Durian = new(nameof(Durian), true),
        DarkGrub = new(nameof(DarkGrub), true),
        SparkEye = new(nameof(SparkEye), true),
        ScavengerSentinel = new(nameof(ScavengerSentinel), true),
        AlphaOrange = new(nameof(AlphaOrange), true),
        MamaBug = new(nameof(MamaBug), true);

    static SandboxUnlockID()
    {
        M4RUnlockList = [ThornyStrawberry,
            SilverLizard,
            ThornBug,
            NoodleEater,
            SurfaceSwimmer,
            SeedBat,
            Scutigera,
            RedHorrorCenti,
            Bigrub,
            Polliwog,
            MiniLeviathan,
            Hoverfly,
            FatFireFly,
            BouncingBall,
            Sporantula,
            WaterSpitter,
            WaterBlob,
            HunterSeeker,
            FlyingBigEel,
            MiniFlyingBigEel,
            LittleBalloon,
            BouncingMelon,
            HazerMom,
            TintedBeetle,
            Physalis,
            LimeMushroom,
            Blizzor,
            MoleSalamander,
            MarineEye,
            MiniBlackLeech,
            StarLemon,
            Denture,
            DendriticNeuron,
            CommonEel,
            DivingBeetle,
            Killerpillar,
            Glowpillar,
            MiniBlueFruit,
            SporeProjectile,
            ChipChop,
            MiniScutigera,
            Xylo,
            XyloWorm,
            BigXyloWorm,
            FumeFruit,
            Durian,
            DarkGrub,
            SparkEye,
            ScavengerSentinel,
            AlphaOrange,
            MamaBug];
    }

    public static void UnregisterValues()
    {
        if (HazerMom is not null)
        {
            HazerMom.Unregister();
            HazerMom = null!;
        }
        if (ThornyStrawberry is not null)
        {
            ThornyStrawberry.Unregister();
            ThornyStrawberry = null!;
        }
        if (SilverLizard is not null)
        {
            SilverLizard.Unregister();
            SilverLizard = null!;
        }
        if (ThornBug is not null)
        {
            ThornBug.Unregister();
            ThornBug = null!;
        }
        if (NoodleEater is not null)
        {
            NoodleEater.Unregister();
            NoodleEater = null!;
        }
        if (SurfaceSwimmer is not null)
        {
            SurfaceSwimmer.Unregister();
            SurfaceSwimmer = null!;
        }
        if (SeedBat is not null)
        {
            SeedBat.Unregister();
            SeedBat = null!;
        }
        if (Scutigera is not null)
        {
            Scutigera.Unregister();
            Scutigera = null!;
        }
        if (RedHorrorCenti is not null)
        {
            RedHorrorCenti.Unregister();
            RedHorrorCenti = null!;
        }
        if (Bigrub is not null)
        {
            Bigrub.Unregister();
            Bigrub = null!;
        }
        if (Polliwog is not null)
        {
            Polliwog.Unregister();
            Polliwog = null!;
        }
        if (MiniLeviathan is not null)
        {
            MiniLeviathan.Unregister();
            MiniLeviathan = null!;
        }
        if (Hoverfly is not null)
        {
            Hoverfly.Unregister();
            Hoverfly = null!;
        }
        if (FatFireFly is not null)
        {
            FatFireFly.Unregister();
            FatFireFly = null!;
        }
        if (BouncingBall is not null)
        {
            BouncingBall.Unregister();
            BouncingBall = null!;
        }
        if (Sporantula is not null)
        {
            Sporantula.Unregister();
            Sporantula = null!;
        }
        if (WaterSpitter is not null)
        {
            WaterSpitter.Unregister();
            WaterSpitter = null!;
        }
        if (WaterBlob is not null)
        {
            WaterBlob.Unregister();
            WaterBlob = null!;
        }
        if (HunterSeeker is not null)
        {
            HunterSeeker.Unregister();
            HunterSeeker = null!;
        }
        if (FlyingBigEel is not null)
        {
            FlyingBigEel.Unregister();
            FlyingBigEel = null!;
        }
        if (MiniFlyingBigEel is not null)
        {
            MiniFlyingBigEel.Unregister();
            MiniFlyingBigEel = null!;
        }
        if (LittleBalloon is not null)
        {
            LittleBalloon.Unregister();
            LittleBalloon = null!;
        }
        if (BouncingMelon is not null)
        {
            BouncingMelon.Unregister();
            BouncingMelon = null!;
        }
        if (TintedBeetle is not null)
        {
            TintedBeetle.Unregister();
            TintedBeetle = null!;
        }
        if (Physalis is not null)
        {
            Physalis.Unregister();
            Physalis = null!;
        }
        if (LimeMushroom is not null)
        {
            LimeMushroom.Unregister();
            LimeMushroom = null!;
        }
        if (Blizzor is not null)
        {
            Blizzor.Unregister();
            Blizzor = null!;
        }
        if (MoleSalamander is not null)
        {
            MoleSalamander.Unregister();
            MoleSalamander = null!;
        }
        if (MarineEye is not null)
        {
            MarineEye.Unregister();
            MarineEye = null!;
        }
        if (MiniBlackLeech is not null)
        {
            MiniBlackLeech.Unregister();
            MiniBlackLeech = null!;
        }
        if (StarLemon is not null)
        {
            StarLemon.Unregister();
            StarLemon = null!;
        }
        if (Denture is not null)
        {
            Denture.Unregister();
            Denture = null!;
        }
        if (DendriticNeuron is not null)
        {
            DendriticNeuron.Unregister();
            DendriticNeuron = null!;
        }
        if (CommonEel is not null)
        {
            CommonEel.Unregister();
            CommonEel = null!;
        }
        if (DivingBeetle is not null)
        {
            DivingBeetle.Unregister();
            DivingBeetle = null!;
        }
        if (Killerpillar is not null)
        {
            Killerpillar.Unregister();
            Killerpillar = null!;
        }
        if (Glowpillar is not null)
        {
            Glowpillar.Unregister();
            Glowpillar = null!;
        }
        if (MiniBlueFruit is not null)
        {
            MiniBlueFruit.Unregister();
            MiniBlueFruit = null!;
        }
        if (SporeProjectile is not null)
        {
            SporeProjectile.Unregister();
            SporeProjectile = null!;
        }
        if (ChipChop is not null)
        {
            ChipChop.Unregister();
            ChipChop = null!;
        }
        if (MiniScutigera is not null)
        {
            MiniScutigera.Unregister();
            MiniScutigera = null!;
        }
        if (Xylo is not null)
        {
            Xylo.Unregister();
            Xylo = null!;
        }
        if (XyloWorm is not null)
        {
            XyloWorm.Unregister();
            XyloWorm = null!;
        }
        if (BigXyloWorm is not null)
        {
            BigXyloWorm.Unregister();
            BigXyloWorm = null!;
        }
        if (FumeFruit is not null)
        {
            FumeFruit.Unregister();
            FumeFruit = null!;
        }
        if (Durian is not null)
        {
            Durian.Unregister();
            Durian = null!;
        }
        if (DarkGrub is not null)
        {
            DarkGrub.Unregister();
            DarkGrub = null!;
        }
        if (SparkEye is not null)
        {
            SparkEye.Unregister();
            SparkEye = null!;
        }
        if (ScavengerSentinel is not null)
        {
            ScavengerSentinel.Unregister();
            ScavengerSentinel = null!;
        }
        if (AlphaOrange is not null)
        {
            AlphaOrange.Unregister();
            AlphaOrange = null!;
        }
        if (MamaBug is not null)
        {
            MamaBug.Unregister();
            MamaBug = null!;
        }
    }
}

public static class PlacedObjectType
{
    public static PlacedObject.Type ThornyStrawberry = new(nameof(ThornyStrawberry), true),
        LittleBalloon = new(nameof(LittleBalloon), true),
        BouncingMelon = new(nameof(BouncingMelon), true),
        Physalis = new(nameof(Physalis), true),
        HazerMom = new(nameof(HazerMom), true),
        DeadHazerMom = new(nameof(DeadHazerMom), true),
        AlbinoHazerMom = new(nameof(AlbinoHazerMom), true),
        DeadAlbinoHazerMom = new(nameof(DeadAlbinoHazerMom), true),
        AlbinoFormHazer = new(nameof(AlbinoFormHazer), true),
        DeadAlbinoFormHazer = new(nameof(DeadAlbinoFormHazer), true),
        LimeMushroom = new(nameof(LimeMushroom), true),
        RubberBlossom = new(nameof(RubberBlossom), true),
        MarineEye = new(nameof(MarineEye), true),
        StarLemon = new(nameof(StarLemon), true),
        DendriticNeuron = new(nameof(DendriticNeuron), true),
        MiniFruitBranch = new(nameof(MiniFruitBranch), true),
        BonusScoreToken = new(nameof(BonusScoreToken), true),
        SporeProjectile = new(nameof(SporeProjectile), true),
        XyloNest = new(nameof(XyloNest), true),
        AltXyloNest = new(nameof(AltXyloNest), true),
        YellowXyloNest = new(nameof(YellowXyloNest), true),
        XyloWorm = new(nameof(XyloWorm), true),
        BigXyloWorm = new(nameof(BigXyloWorm), true),
        DeadXyloWorm = new(nameof(DeadXyloWorm), true),
        DeadBigXyloWorm = new(nameof(DeadBigXyloWorm), true),
        FumeFruit = new(nameof(FumeFruit), true),
        Durian = new(nameof(Durian), true),
        DarkGrub = new(nameof(DarkGrub), true);

    public static void UnregisterValues()
    {
        if (ThornyStrawberry is not null)
        {
            ThornyStrawberry.Unregister();
            ThornyStrawberry = null!;
        }
        if (LittleBalloon is not null)
        {
            LittleBalloon.Unregister();
            LittleBalloon = null!;
        }
        if (BouncingMelon is not null)
        {
            BouncingMelon.Unregister();
            BouncingMelon = null!;
        }
        if (HazerMom is not null)
        {
            HazerMom.Unregister();
            HazerMom = null!;
        }
        if (DeadHazerMom is not null)
        {
            DeadHazerMom.Unregister();
            DeadHazerMom = null!;
        }
        if (AlbinoHazerMom is not null)
        {
            AlbinoHazerMom.Unregister();
            AlbinoHazerMom = null!;
        }
        if (DeadAlbinoHazerMom is not null)
        {
            DeadAlbinoHazerMom.Unregister();
            DeadAlbinoHazerMom = null!;
        }
        if (AlbinoFormHazer is not null)
        {
            AlbinoFormHazer.Unregister();
            AlbinoFormHazer = null!;
        }
        if (DeadAlbinoFormHazer is not null)
        {
            DeadAlbinoFormHazer.Unregister();
            DeadAlbinoFormHazer = null!;
        }
        if (Physalis is not null)
        {
            Physalis.Unregister();
            Physalis = null!;
        }
        if (LimeMushroom is not null)
        {
            LimeMushroom.Unregister();
            LimeMushroom = null!;
        }
        if (RubberBlossom is not null)
        {
            RubberBlossom.Unregister();
            RubberBlossom = null!;
        }
        if (MarineEye is not null)
        {
            MarineEye.Unregister();
            MarineEye = null!;
        }
        if (StarLemon is not null)
        {
            StarLemon.Unregister();
            StarLemon = null!;
        }
        if (DendriticNeuron is not null)
        {
            DendriticNeuron.Unregister();
            DendriticNeuron = null!;
        }
        if (MiniFruitBranch is not null)
        {
            MiniFruitBranch.Unregister();
            MiniFruitBranch = null!;
        }
        if (BonusScoreToken is not null)
        {
            BonusScoreToken.Unregister();
            BonusScoreToken = null!;
        }
        if (SporeProjectile is not null)
        {
            SporeProjectile.Unregister();
            SporeProjectile = null!;
        }
        if (XyloNest is not null)
        {
            XyloNest.Unregister();
            XyloNest = null!;
        }
        if (AltXyloNest is not null)
        {
            AltXyloNest.Unregister();
            AltXyloNest = null!;
        }
        if (YellowXyloNest is not null)
        {
            YellowXyloNest.Unregister();
            YellowXyloNest = null!;
        }
        if (XyloWorm is not null)
        {
            XyloWorm.Unregister();
            XyloWorm = null!;
        }
        if (BigXyloWorm is not null)
        {
            BigXyloWorm.Unregister();
            BigXyloWorm = null!;
        }
        if (DeadXyloWorm is not null)
        {
            DeadXyloWorm.Unregister();
            DeadXyloWorm = null!;
        }
        if (DeadBigXyloWorm is not null)
        {
            DeadBigXyloWorm.Unregister();
            DeadBigXyloWorm = null!;
        }
        if (FumeFruit is not null)
        {
            FumeFruit.Unregister();
            FumeFruit = null!;
        }
        if (Durian is not null)
        {
            Durian.Unregister();
            Durian = null!;
        }
        if (DarkGrub is not null)
        {
            DarkGrub.Unregister();
            DarkGrub = null!;
        }
    }
}

public static class AbstractObjectType
{
    public static HashSet<AbstractPhysicalObject.AbstractObjectType> M4RItemList;
    public static AbstractPhysicalObject.AbstractObjectType ThornyStrawberry = new(nameof(ThornyStrawberry), true),
        SporeProjectile = new(nameof(SporeProjectile), true),
        BlobPiece = new(nameof(BlobPiece), true),
        LittleBalloon = new(nameof(LittleBalloon), true),
        BouncingMelon = new(nameof(BouncingMelon), true),
        Physalis = new(nameof(Physalis), true),
        LimeMushroom = new(nameof(LimeMushroom), true),
        RubberBlossom = new(nameof(RubberBlossom), true),
        GummyAnther = new(nameof(GummyAnther), true),
        MarineEye = new(nameof(MarineEye), true),
        StarLemon = new(nameof(StarLemon), true),
        DendriticNeuron = new(nameof(DendriticNeuron), true),
        MiniBlueFruit = new(nameof(MiniBlueFruit), true),
        MiniFruitSpawner = new(nameof(MiniFruitSpawner), true),
        FumeFruit = new(nameof(FumeFruit), true),
        Durian = new(nameof(Durian), true),
        DarkGrub = new(nameof(DarkGrub), true);

    static AbstractObjectType()
    {
        M4RItemList = [ThornyStrawberry,
            SporeProjectile,
            BlobPiece,
            LittleBalloon,
            BouncingMelon,
            Physalis,
            LimeMushroom,
            RubberBlossom,
            GummyAnther,
            MarineEye,
            StarLemon,
            DendriticNeuron,
            MiniBlueFruit,
            MiniFruitSpawner,
            FumeFruit,
            Durian,
            DarkGrub];
    }

    public static void UnregisterValues()
    {
        if (ThornyStrawberry is not null)
        {
            ThornyStrawberry.Unregister();
            ThornyStrawberry = null!;
        }
        if (SporeProjectile is not null)
        {
            SporeProjectile.Unregister();
            SporeProjectile = null!;
        }
        if (BlobPiece is not null)
        {
            BlobPiece.Unregister();
            BlobPiece = null!;
        }
        if (LittleBalloon is not null)
        {
            LittleBalloon.Unregister();
            LittleBalloon = null!;
        }
        if (BouncingMelon is not null)
        {
            BouncingMelon.Unregister();
            BouncingMelon = null!;
        }
        if (Physalis is not null)
        {
            Physalis.Unregister();
            Physalis = null!;
        }
        if (LimeMushroom is not null)
        {
            LimeMushroom.Unregister();
            LimeMushroom = null!;
        }
        if (RubberBlossom is not null)
        {
            RubberBlossom.Unregister();
            RubberBlossom = null!;
        }
        if (GummyAnther is not null)
        {
            GummyAnther.Unregister();
            GummyAnther = null!;
        }
        if (MarineEye is not null)
        {
            MarineEye.Unregister();
            MarineEye = null!;
        }
        if (StarLemon is not null)
        {
            StarLemon.Unregister();
            StarLemon = null!;
        }
        if (DendriticNeuron is not null)
        {
            DendriticNeuron.Unregister();
            DendriticNeuron = null!;
        }
        if (MiniBlueFruit is not null)
        {
            MiniBlueFruit.Unregister();
            MiniBlueFruit = null!;
        }
        if (MiniFruitSpawner is not null)
        {
            MiniFruitSpawner.Unregister();
            MiniFruitSpawner = null!;
        }
        if (FumeFruit is not null)
        {
            FumeFruit.Unregister();
            FumeFruit = null!;
        }
        if (Durian is not null)
        {
            Durian.Unregister();
            Durian = null!;
        }
        if (DarkGrub is not null)
        {
            DarkGrub.Unregister();
            DarkGrub = null!;
        }
    }
}

public static class MultiplayerItemType
{
    public static PlacedObject.MultiplayerItemData.Type ThornyStrawberry = new(nameof(ThornyStrawberry), true),
        LittleBalloon = new(nameof(LittleBalloon), true),
        BouncingMelon = new(nameof(BouncingMelon), true),
        Physalis = new(nameof(Physalis), true),
        LimeMushroom = new(nameof(LimeMushroom), true),
        MarineEye = new(nameof(MarineEye), true),
        StarLemon = new(nameof(StarLemon), true),
        SporeProjectile = new(nameof(SporeProjectile), true),
        FumeFruit = new(nameof(FumeFruit), true),
        Durian = new(nameof(Durian), true),
        DarkGrub = new(nameof(DarkGrub), true);

    public static void UnregisterValues()
    {
        if (ThornyStrawberry is not null)
        {
            ThornyStrawberry.Unregister();
            ThornyStrawberry = null!;
        }
        if (LittleBalloon is not null)
        {
            LittleBalloon.Unregister();
            LittleBalloon = null!;
        }
        if (BouncingMelon is not null)
        {
            BouncingMelon.Unregister();
            BouncingMelon = null!;
        }
        if (Physalis is not null)
        {
            Physalis.Unregister();
            Physalis = null!;
        }
        if (LimeMushroom is not null)
        {
            LimeMushroom.Unregister();
            LimeMushroom = null!;
        }
        if (MarineEye is not null)
        {
            MarineEye.Unregister();
            MarineEye = null!;
        }
        if (StarLemon is not null)
        {
            StarLemon.Unregister();
            StarLemon = null!;
        }
        if (SporeProjectile is not null)
        {
            SporeProjectile.Unregister();
            SporeProjectile = null!;
        }
        if (FumeFruit is not null)
        {
            FumeFruit.Unregister();
            FumeFruit = null!;
        }
        if (Durian is not null)
        {
            Durian.Unregister();
            Durian = null!;
        }
        if (DarkGrub is not null)
        {
            DarkGrub.Unregister();
            DarkGrub = null!;
        }
    }
}

public static class MiscItemType
{
    public static SLOracleBehaviorHasMark.MiscItemType ThornyStrawberry = new(nameof(ThornyStrawberry), true),
        BlobPiece = new(nameof(BlobPiece), true),
        LittleBalloon = new(nameof(LittleBalloon), true),
        BouncingMelon = new(nameof(BouncingMelon), true),
        Physalis = new(nameof(Physalis), true),
        LimeMushroom = new(nameof(LimeMushroom), true),
        GummyAnther = new(nameof(GummyAnther), true),
        MarineEye = new(nameof(MarineEye), true),
        StarLemon = new(nameof(StarLemon), true),
        SporeProjectile = new(nameof(SporeProjectile), true),
        DendriticNeuron = new(nameof(DendriticNeuron), true),
        MiniBlueFruit = new(nameof(MiniBlueFruit), true),
        FumeFruit = new(nameof(FumeFruit), true),
        Durian = new(nameof(Durian), true),
        DarkGrub = new(nameof(DarkGrub), true),
        ScavengerRagMask = new(nameof(ScavengerRagMask), true);

    public static void UnregisterValues()
    {
        if (ThornyStrawberry is not null)
        {
            ThornyStrawberry.Unregister();
            ThornyStrawberry = null!;
        }
        if (BlobPiece is not null)
        {
            BlobPiece.Unregister();
            BlobPiece = null!;
        }
        if (LittleBalloon is not null)
        {
            LittleBalloon.Unregister();
            LittleBalloon = null!;
        }
        if (BouncingMelon is not null)
        {
            BouncingMelon.Unregister();
            BouncingMelon = null!;
        }
        if (Physalis is not null)
        {
            Physalis.Unregister();
            Physalis = null!;
        }
        if (LimeMushroom is not null)
        {
            LimeMushroom.Unregister();
            LimeMushroom = null!;
        }
        if (GummyAnther is not null)
        {
            GummyAnther.Unregister();
            GummyAnther = null!;
        }
        if (MarineEye is not null)
        {
            MarineEye.Unregister();
            MarineEye = null!;
        }
        if (StarLemon is not null)
        {
            StarLemon.Unregister();
            StarLemon = null!;
        }
        if (SporeProjectile is not null)
        {
            SporeProjectile.Unregister();
            SporeProjectile = null!;
        }
        if (DendriticNeuron is not null)
        {
            DendriticNeuron.Unregister();
            DendriticNeuron = null!;
        }
        if (MiniBlueFruit is not null)
        {
            MiniBlueFruit.Unregister();
            MiniBlueFruit = null!;
        }
        if (FumeFruit is not null)
        {
            FumeFruit.Unregister();
            FumeFruit = null!;
        }
        if (Durian is not null)
        {
            Durian.Unregister();
            Durian = null!;
        }
        if (DarkGrub is not null)
        {
            DarkGrub.Unregister();
            DarkGrub = null!;
        }
        if (ScavengerRagMask is not null)
        {
            ScavengerRagMask.Unregister();
            ScavengerRagMask = null!;
        }
    }
}

public static class SlugFood
{
    public static SlugNPCAI.Food? ThornyStrawberry, BlobPiece, LittleBalloon, Physalis, GummyAnther, MarineEye, StarLemon, DendriticNeuron, MiniBlueFruit, MiniScutigera, XyloWorm, DarkGrub;

    public static void UnregisterValues()
    {
        if (ThornyStrawberry is not null)
        {
            ThornyStrawberry.Unregister();
            ThornyStrawberry = null;
        }
        if (BlobPiece is not null)
        {
            BlobPiece.Unregister();
            BlobPiece = null;
        }
        if (LittleBalloon is not null)
        {
            LittleBalloon.Unregister();
            LittleBalloon = null;
        }
        if (Physalis is not null)
        {
            Physalis.Unregister();
            Physalis = null;
        }
        if (GummyAnther is not null)
        {
            GummyAnther.Unregister();
            GummyAnther = null;
        }
        if (MarineEye is not null)
        {
            MarineEye.Unregister();
            MarineEye = null;
        }
        if (StarLemon is not null)
        {
            StarLemon.Unregister();
            StarLemon = null;
        }
        if (DendriticNeuron is not null)
        {
            DendriticNeuron.Unregister();
            DendriticNeuron = null;
        }
        if (MiniBlueFruit is not null)
        {
            MiniBlueFruit.Unregister();
            MiniBlueFruit = null;
        }
        if (MiniScutigera is not null)
        {
            MiniScutigera.Unregister();
            MiniScutigera = null;
        }
        if (XyloWorm is not null)
        {
            XyloWorm.Unregister();
            XyloWorm = null;
        }
        if (DarkGrub is not null)
        {
            DarkGrub.Unregister();
            DarkGrub = null;
        }
    }
}

public static class NewSoundID
{
    public static SoundID M4R_Hoverfly_Startle = new(nameof(M4R_Hoverfly_Startle), true),
        M4R_Hoverfly_Idle = new(nameof(M4R_Hoverfly_Idle), true),
        M4R_Hoverfly_Fly_LOOP = new(nameof(M4R_Hoverfly_Fly_LOOP), true),
        M4R_Flying_Leviathan_Bite = new(nameof(M4R_Flying_Leviathan_Bite), true),
        M4R_WaterSpitter_Voice_A = new(nameof(M4R_WaterSpitter_Voice_A), true),
        M4R_WaterSpitter_Voice_B = new(nameof(M4R_WaterSpitter_Voice_B), true),
        M4R_WaterSpitter_Voice_C = new(nameof(M4R_WaterSpitter_Voice_C), true),
        M4R_CommonEel_Voice = new(nameof(M4R_CommonEel_Voice), true),
        M4R_CommonEel_Hiss = new(nameof(M4R_CommonEel_Hiss), true),
        M4R_CommonEel_BigHiss = new(nameof(M4R_CommonEel_BigHiss), true),
        M4R_Xylo_Swallow = new(nameof(M4R_Xylo_Swallow), true),
        M4R_Xylo_Swell = new(nameof(M4R_Xylo_Swell), true),
        M4R_ChipChop_Chip = new(nameof(M4R_ChipChop_Chip), true),
        M4R_TintedBeetle_Chip = new(nameof(M4R_TintedBeetle_Chip), true),
        M4R_TintedBeetle_BigChip = new(nameof(M4R_TintedBeetle_BigChip), true),
        M4R_Caterpillar_Crawl_LOOP = new(nameof(M4R_Caterpillar_Crawl_LOOP), true),
        M4R_SurfaceSwimmer_Chip = new(nameof(M4R_SurfaceSwimmer_Chip), true),
        M4R_GenericBug_BigChip = new(nameof(M4R_GenericBug_BigChip), true),
        M4R_GenericBug_Chip = new(nameof(M4R_GenericBug_Chip), true),
        M4R_DoubleJaw_Hiss = new(nameof(M4R_DoubleJaw_Hiss), true);

    public static void UnregisterValues()
    {
        if (M4R_Hoverfly_Startle is not null)
        {
            M4R_Hoverfly_Startle.Unregister();
            M4R_Hoverfly_Startle = null!;
        }
        if (M4R_Hoverfly_Idle is not null)
        {
            M4R_Hoverfly_Idle.Unregister();
            M4R_Hoverfly_Idle = null!;
        }
        if (M4R_Hoverfly_Fly_LOOP is not null)
        {
            M4R_Hoverfly_Fly_LOOP.Unregister();
            M4R_Hoverfly_Fly_LOOP = null!;
        }
        if (M4R_Flying_Leviathan_Bite is not null)
        {
            M4R_Flying_Leviathan_Bite.Unregister();
            M4R_Flying_Leviathan_Bite = null!;
        }
        if (M4R_WaterSpitter_Voice_A is not null)
        {
            M4R_WaterSpitter_Voice_A.Unregister();
            M4R_WaterSpitter_Voice_A = null!;
        }
        if (M4R_WaterSpitter_Voice_B is not null)
        {
            M4R_WaterSpitter_Voice_B.Unregister();
            M4R_WaterSpitter_Voice_B = null!;
        }
        if (M4R_WaterSpitter_Voice_C is not null)
        {
            M4R_WaterSpitter_Voice_C.Unregister();
            M4R_WaterSpitter_Voice_C = null!;
        }
        if (M4R_CommonEel_Voice is not null)
        {
            M4R_CommonEel_Voice.Unregister();
            M4R_CommonEel_Voice = null!;
        }
        if (M4R_CommonEel_Hiss is not null)
        {
            M4R_CommonEel_Hiss.Unregister();
            M4R_CommonEel_Hiss = null!;
        }
        if (M4R_CommonEel_BigHiss is not null)
        {
            M4R_CommonEel_BigHiss.Unregister();
            M4R_CommonEel_BigHiss = null!;
        }
        if (M4R_Xylo_Swallow is not null)
        {
            M4R_Xylo_Swallow.Unregister();
            M4R_Xylo_Swallow = null!;
        }
        if (M4R_Xylo_Swell is not null)
        {
            M4R_Xylo_Swell.Unregister();
            M4R_Xylo_Swell = null!;
        }
        if (M4R_ChipChop_Chip is not null)
        {
            M4R_ChipChop_Chip.Unregister();
            M4R_ChipChop_Chip = null!;
        }
        if (M4R_TintedBeetle_Chip is not null)
        {
            M4R_TintedBeetle_Chip.Unregister();
            M4R_TintedBeetle_Chip = null!;
        }
        if (M4R_TintedBeetle_BigChip is not null)
        {
            M4R_TintedBeetle_BigChip.Unregister();
            M4R_TintedBeetle_BigChip = null!;
        }
        if (M4R_Caterpillar_Crawl_LOOP is not null)
        {
            M4R_Caterpillar_Crawl_LOOP.Unregister();
            M4R_Caterpillar_Crawl_LOOP = null!;
        }
        if (M4R_SurfaceSwimmer_Chip is not null)
        {
            M4R_SurfaceSwimmer_Chip.Unregister();
            M4R_SurfaceSwimmer_Chip = null!;
        }
        if (M4R_GenericBug_Chip is not null)
        {
            M4R_GenericBug_Chip.Unregister();
            M4R_GenericBug_Chip = null!;
        }
        if (M4R_GenericBug_BigChip is not null)
        {
            M4R_GenericBug_BigChip.Unregister();
            M4R_GenericBug_BigChip = null!;
        }
        if (M4R_DoubleJaw_Hiss is not null)
        {
            M4R_DoubleJaw_Hiss.Unregister();
            M4R_DoubleJaw_Hiss = null!;
        }
    }
}

public static class NewTickerID
{
    public static StoryGameStatisticsScreen.TickerID ScoreTokens = new(nameof(ScoreTokens), true);

    internal static void UnregisterValues()
    {
        if (ScoreTokens is not null)
        {
            ScoreTokens.Unregister();
            ScoreTokens = null!;
        }
    }
}