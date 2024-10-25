﻿global using static LBMergedMods.Hooks.MainHooks;

namespace LBMergedMods.Hooks;

public static class MainHooks
{
    internal static bool s_init;

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
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.Physalis))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.Physalis);
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.LimeMushroom))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.LimeMushroom);
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.MarineEye))
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.MarineEye);
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
                RoomEffectType.UnregisterValues();
                SandboxUnlockID.UnregisterValues();
                CreatureTemplateType.UnregisterValues();
                AbstractObjectType.UnregisterValues();
                PlacedObjectType.UnregisterValues();
                MiscItemType.UnregisterValues();
                MultiplayerItemType.UnregisterValues();
                SlugFood.UnregisterValues();
                NewSoundID.UnregisterValues();
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
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.LittleBalloon))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.LittleBalloon);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.ThornyStrawberry))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.ThornyStrawberry);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.BouncingMelon))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.BouncingMelon);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.Physalis))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.Physalis);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.LimeMushroom))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.LimeMushroom);
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.MarineEye))
            MultiplayerUnlocks.ItemUnlockList.Add(SandboxUnlockID.MarineEye);
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
            //heheh, later than other mods for modcat compatibility
            On.PlayerGraphics.DrawSprites += On_PlayerGraphics_DrawSprites;
            if (ModManager.MSC)
            {
                SlugFood.ThornyStrawberry = new(nameof(SlugFood.ThornyStrawberry), true);
                SlugFood.BlobPiece = new(nameof(SlugFood.BlobPiece), true);
                SlugFood.LittleBalloon = new(nameof(SlugFood.LittleBalloon), true);
                SlugFood.Physalis = new(nameof(SlugFood.Physalis), true);
                SlugFood.GummyAnther = new(nameof(SlugFood.GummyAnther), true);
                SlugFood.MarineEye = new(nameof(SlugFood.MarineEye), true);
                ResizeGourmandCombos();
                InitGourmandCombos();
                On.MoreSlugcats.GourmandCombos.InitCraftingLibrary += On_GourmandCombos_InitCraftingLibrary;
                On.MoreSlugcats.SlugNPCAI.GetFoodType += On_SlugNPCAI_GetFoodType;
                On.MoreSlugcats.SlugNPCAI.AteFood += On_SlugNPCAI_AteFood;
                On.MoreSlugcats.SlugNPCAI.WantsToEatThis += On_SlugNPCAI_WantsToEatThis;
                On.MoreSlugcats.BigJellyFish.ValidGrabCreature += On_BigJellyFish_ValidGrabCreature;
                On.MoreSlugcats.StowawayBugAI.WantToEat += On_StowawayBugAI_WantToEat;
            }
            s_init = true;
        }
        _ = PlacedObjectType.ThornyStrawberry;
        _ = MultiplayerItemType.ThornyStrawberry;
        _ = MiscItemType.ThornyStrawberry;
        _ = AbstractObjectType.ThornyStrawberry;
        _ = SandboxUnlockID.ThornyStrawberry;
        _ = RoomEffectType.SeedBats;
    }

    internal static void On_RainWorld_UnloadResources(On.RainWorld.orig_UnloadResources orig, RainWorld self)
    {
        orig(self);
        if (Futile.atlasManager.DoesContainAtlas("lbmergedmodsspr"))
            Futile.atlasManager.UnloadAtlas("lbmergedmodsspr");
        if (Futile.atlasManager.DoesContainAtlas("BlizzorNeck"))
            Futile.atlasManager.UnloadAtlas("BlizzorNeck");
        LBMergedModsPlugin.Bundle?.Unload(true);
        LBMergedModsPlugin.Bundle = null;
    }

    internal static void On_SoundLoader_LoadSounds(On.SoundLoader.orig_LoadSounds orig, SoundLoader self)
    {
        _ = NewSoundID.Hoverfly_Fly_LOOP;
        orig(self);
    }
}