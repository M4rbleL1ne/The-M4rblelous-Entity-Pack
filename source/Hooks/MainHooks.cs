global using static LBMergedMods.Hooks.MainHooks;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace LBMergedMods.Hooks;

public static class MainHooks
{
    [StructLayout(LayoutKind.Sequential)]
    public sealed class ScoreTokens
    {
        public Dictionary<string, List<string>> RegionScoreTokens = [];
        public Dictionary<string, List<List<SlugcatStats.Name>>> RegionScoreTokensInaccessibility = [];
    }

    public static ConditionalWeakTable<RainWorld, ScoreTokens> RegionScoreData = new();
    internal static bool s_init;

    internal static void On_RainWorld_ClearTokenCacheInMemory(On.RainWorld.orig_ClearTokenCacheInMemory orig, RainWorld self)
    {
        orig(self);
        if (RegionScoreData.TryGetValue(self, out _))
            RegionScoreData.Remove(self);
    }

    internal static void On_RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);
        for (var i = 0; i < newlyDisabledMods.Length; i++)
        {
            if (newlyDisabledMods[i].id == "lb-fgf-m4r-ik.modpack")
            {
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.ThornyStrawberry))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.ThornyStrawberry);
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.LittleBalloon))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.LittleBalloon);
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.BouncingMelon))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.BouncingMelon);
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.MiniBlueFruit))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.MiniBlueFruit);
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.Physalis))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.Physalis);
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.LimeMushroom))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.LimeMushroom);
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.MarineEye))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.MarineEye);
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.StarLemon))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.StarLemon);
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.DendriticNeuron))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.DendriticNeuron);
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.SporeProjectile))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.SporeProjectile);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.NoodleEater))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.NoodleEater);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.SilverLizard))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.SilverLizard);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.SurfaceSwimmer))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.SurfaceSwimmer);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.ThornBug))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.ThornBug);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.SeedBat))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.SeedBat);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.Scutigera))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.Scutigera);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.RedHorrorCenti))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.RedHorrorCenti);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.Bigrub))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.Bigrub);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.Polliwog))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.Polliwog);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.MiniLeviathan))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.MiniLeviathan);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.Hoverfly))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.Hoverfly);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.FatFireFly))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.FatFireFly);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.BouncingBall))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.BouncingBall);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.Sporantula))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.Sporantula);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.WaterSpitter))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.WaterSpitter);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.WaterBlob))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.WaterBlob);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.HunterSeeker))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.HunterSeeker);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.FlyingBigEel))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.FlyingBigEel);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.MiniFlyingBigEel))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.MiniFlyingBigEel);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.HazerMom))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.HazerMom);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.TintedBeetle))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.TintedBeetle);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.Blizzor))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.Blizzor);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.MoleSalamander))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.MoleSalamander);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.Denture))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.Denture);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.CommonEel))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.CommonEel);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.DivingBeetle))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.DivingBeetle);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.Killerpillar))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.Killerpillar);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.Glowpillar))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.Glowpillar);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.ChipChop))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.ChipChop);
                if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.MiniScutigera))
                    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.MiniScutigera);
                RoomEffectType.UnregisterValues();
                SandboxUnlockID.UnregisterValues();
                CreatureTemplateType.UnregisterValues();
                AbstractObjectType.UnregisterValues();
                PlacedObjectType.UnregisterValues();
                MiscItemType.UnregisterValues();
                MultiplayerItemType.UnregisterValues();
                SlugFood.UnregisterValues();
                NewSoundID.UnregisterValues();
                DevEffectsCategories.UnregisterValues();
                DevObjectCategories.UnregisterValues();
                CommunityID.UnregisterValues();
                NewTickerID.UnregisterValues();
                break;
            }
        }
    }

    internal static void On_RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        if (!Futile.atlasManager.DoesContainAtlas("lbmergedmodsspr"))
            Futile.atlasManager.LoadAtlas("atlases/lbmergedmodsspr");
        if (!Futile.atlasManager.DoesContainAtlas("BlizzorNeck"))
            Futile.atlasManager.ActuallyLoadAtlasOrImage("BlizzorNeck", "atlases/BlizzorNeck" + Futile.resourceSuffix, string.Empty);
        if (!Futile.atlasManager.DoesContainAtlas("wwdvb_fur"))
            Futile.atlasManager.ActuallyLoadAtlasOrImage("wwdvb_fur", "atlases/wwdvb_fur" + Futile.resourceSuffix, string.Empty);
        if (!Futile.atlasManager.DoesContainAtlas("wwdvb_lh"))
            Futile.atlasManager.ActuallyLoadAtlasOrImage("wwdvb_lh", "atlases/wwdvb_lh" + Futile.resourceSuffix, string.Empty);
        if (!Futile.atlasManager.DoesContainAtlas("wwdvb_wingw"))
            Futile.atlasManager.ActuallyLoadAtlasOrImage("wwdvb_wingw", "atlases/wwdvb_wingw" + Futile.resourceSuffix, string.Empty).texture.wrapMode = TextureWrapMode.Clamp;
        if (!Futile.atlasManager.DoesContainAtlas("wwdvb_wingw2"))
            Futile.atlasManager.ActuallyLoadAtlasOrImage("wwdvb_wingw2", "atlases/wwdvb_wingw2" + Futile.resourceSuffix, string.Empty).texture.wrapMode = TextureWrapMode.Clamp;
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.LittleBalloon))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.LittleBalloon);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.ThornyStrawberry))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.ThornyStrawberry);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.BouncingMelon))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.BouncingMelon);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.MiniBlueFruit))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.MiniBlueFruit);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.Physalis))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.Physalis);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.SporeProjectile))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.SporeProjectile);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.LimeMushroom))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.LimeMushroom);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.MarineEye))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.MarineEye);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.StarLemon))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.StarLemon);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.DendriticNeuron))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.DendriticNeuron);
        if (!MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.SeedBat))
            MultiplayerUnlocks.CreatureUnlockList.Add(SandboxUnlockID.SeedBat);
        if (!MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.Bigrub))
            MultiplayerUnlocks.CreatureUnlockList.Add(SandboxUnlockID.Bigrub);
    }

    internal static void On_RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);
        if (!s_init)
        {
            //heheh, later than other mods for mod compatibility
            On.PlayerGraphics.DrawSprites += On_PlayerGraphics_DrawSprites;
            On.Expedition.ChallengeTools.GenerateCreatureScores += On_ChallengeTools_GenerateCreatureScores;
            On.Expedition.ChallengeTools.CreatureName += On_ChallengeTools_CreatureName;
            On.Water.DrawSprites += On_Water_DrawSprites;
            On.DaddyAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_DaddyAI_IUseARelationshipTracker_UpdateDynamicRelationship;
            On.CreatureSymbol.ColorOfCreature += On_CreatureSymbol_ColorOfCreature;
            On.CreatureSymbol.SpriteNameOfCreature += On_CreatureSymbol_SpriteNameOfCreature;
            On.CreatureSymbol.SymbolDataFromCreature += On_CreatureSymbol_SymbolDataFromCreature;
            if (ModManager.MSC)
            {
                SlugFood.ThornyStrawberry = new(nameof(SlugFood.ThornyStrawberry), true);
                SlugFood.BlobPiece = new(nameof(SlugFood.BlobPiece), true);
                SlugFood.LittleBalloon = new(nameof(SlugFood.LittleBalloon), true);
                SlugFood.Physalis = new(nameof(SlugFood.Physalis), true);
                SlugFood.GummyAnther = new(nameof(SlugFood.GummyAnther), true);
                SlugFood.MarineEye = new(nameof(SlugFood.MarineEye), true);
                SlugFood.StarLemon = new(nameof(SlugFood.StarLemon), true);
                SlugFood.DendriticNeuron = new(nameof(SlugFood.DendriticNeuron), true);
                SlugFood.MiniBlueFruit = new(nameof(SlugFood.MiniBlueFruit), true);
                SlugFood.MiniScutigera = new(nameof(SlugFood.MiniScutigera), true);
                ResizeGourmandCombos();
                InitGourmandCombos();
                On.MoreSlugcats.GourmandCombos.InitCraftingLibrary += On_GourmandCombos_InitCraftingLibrary;
                On.MoreSlugcats.SlugNPCAI.GetFoodType += On_SlugNPCAI_GetFoodType;
                On.MoreSlugcats.SlugNPCAI.AteFood += On_SlugNPCAI_AteFood;
                On.MoreSlugcats.SlugNPCAI.WantsToEatThis += On_SlugNPCAI_WantsToEatThis;
                On.MoreSlugcats.BigJellyFish.ValidGrabCreature += On_BigJellyFish_ValidGrabCreature;
                On.MoreSlugcats.StowawayBugAI.WantToEat += On_StowawayBugAI_WantToEat;
                IL.MoreSlugcats.InspectorAI.IUseARelationshipTracker_UpdateDynamicRelationship += IL_InspectorAI_IUseARelationshipTracker_UpdateDynamicRelationship;
                IL.MoreSlugcats.SlugNPCAI.PassingGrab += IL_SlugNPCAI_PassingGrab;
                On.MoreSlugcats.SlugNPCAI.LethalWeaponScore += On_SlugNPCAI_LethalWeaponScore;
            }
            s_init = true;

            if (ModManager.ActiveMods.Any(x => x.id == "slime-cubed.devconsole")) DevConsoleCommands.RegisterDevConsole();
        }
        _ = PlacedObjectType.ThornyStrawberry;
        _ = MultiplayerItemType.ThornyStrawberry;
        _ = MiscItemType.ThornyStrawberry;
        _ = AbstractObjectType.ThornyStrawberry;
        _ = SandboxUnlockID.ThornyStrawberry;
        _ = RoomEffectType.SeedBats;
        _ = DevObjectCategories.M4rblelousEntities;
        _ = DevEffectsCategories.M4rblelousEntities;
        _ = CommunityID.TintedBeetles;
        _ = NewTickerID.ScoreTokens;
    }

    internal static void On_RainWorld_UnloadResources(On.RainWorld.orig_UnloadResources orig, RainWorld self)
    {
        orig(self);
        if (Futile.atlasManager.DoesContainAtlas("lbmergedmodsspr"))
            Futile.atlasManager.UnloadAtlas("lbmergedmodsspr");
        if (Futile.atlasManager.DoesContainAtlas("BlizzorNeck"))
            Futile.atlasManager.UnloadAtlas("BlizzorNeck");
        if (Futile.atlasManager.DoesContainAtlas("wwdvb_wingw2"))
            Futile.atlasManager.UnloadAtlas("wwdvb_wingw2");
        if (Futile.atlasManager.DoesContainAtlas("wwdvb_wingw"))
            Futile.atlasManager.UnloadAtlas("wwdvb_wingw");
        if (Futile.atlasManager.DoesContainAtlas("wwdvb_lh"))
            Futile.atlasManager.UnloadAtlas("wwdvb_lh");
        if (Futile.atlasManager.DoesContainAtlas("wwdvb_fur"))
            Futile.atlasManager.UnloadAtlas("wwdvb_fur");
        if (Futile.atlasManager.DoesContainAtlas("wwdvb_spr"))
            Futile.atlasManager.UnloadAtlas("wwdvb_spr");
        LBMergedModsPlugin.Bundle?.Unload(true);
        LBMergedModsPlugin.Bundle = null;
    }

    internal static void On_SoundLoader_LoadSounds(On.SoundLoader.orig_LoadSounds orig, SoundLoader self)
    {
        _ = NewSoundID.Hoverfly_Fly_LOOP;
        orig(self);
    }
}