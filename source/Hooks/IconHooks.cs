global using static LBMergedMods.Hooks.IconHooks;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using HUD;
using Fisobs.Core;

namespace LBMergedMods.Hooks;

public static class IconHooks
{
    public static Color BalloonColor = new(.5f, .1f, .35f),
        PhysalisColor = new(198f / 255f, 110f / 255f, 0f),
        StationPlantCol = new(181f / 255f, 110f / 255f, 220f / 255f),
        AntherCol = new(171f / 255f, 56f / 255f, 27f / 255f),
        MarineCol = new(17f / 255f, 118f / 255f, 223f / 255f);

    internal static Color On_CreatureSymbol_ColorOfCreature(On.CreatureSymbol.orig_ColorOfCreature orig, IconSymbol.IconSymbolData iconData)
    {
        var tp = iconData.critType;
        var dt = iconData.intData;
        if (dt == M4R_DATA_NUMBER)
        {
            if (tp == CreatureTemplate.Type.TubeWorm)
                return Color.green;
            if (tp == CreatureTemplate.Type.Hazer || tp == CreatureTemplateType.Denture || tp == CreatureTemplateType.Glowpillar)
                return Color.white;
            if (tp == CreatureTemplateType.ThornBug)
                return new(0f, 133f / 255f, 250f / 255f);
            if (tp == CreatureTemplateType.FatFireFly)
                return new(33f / 255f, 155f / 255f, 217f / 255f);
            if (tp == CreatureTemplateType.NoodleEater)
                return new(138f / 255f, 245f / 255f, 0f);
            if (tp == CreatureTemplateType.HazerMom)
                return new(18f / 85f, .7921569f, 33f / 85f);
            if (tp == CreatureTemplateType.TintedBeetle)
                return new(80f / 255f, 167f / 255f, 233f / 255f);
            if (tp == CreatureTemplateType.CommonEel)
                return new(0f, 72f / 255f, 1f);
            if (tp == CreatureTemplateType.Xylo)
                return new(.2f, .05f, .05f);
            if (tp == CreatureTemplate.Type.TentaclePlant)
                return Menu.Menu.MenuRGB(Menu.Menu.MenuColors.DarkGrey);
            if (tp == CreatureTemplateType.Hoverfly)
                return new(46f / 51f, .05490196f, .05490196f);
        }
        else if (dt == M4R_DATA_NUMBER2)
        {
            if (tp == CreatureTemplate.Type.TubeWorm)
                return new(1f, .65f, .05f);
            if (tp == CreatureTemplateType.FatFireFly)
                return Color.Lerp(Color.white, Color.red, .1f);
            if (tp == CreatureTemplateType.NoodleEater)
                return new(138f / 255f, 245f / 255f, 0f);
            if (tp == CreatureTemplateType.Xylo)
                return new(236f / 255f, 1f, .0f);
            if (tp == CreatureTemplateType.CommonEel)
                return Color.Lerp(Color.white, CommonEel.EelCol, .1f);
        }
        else if (dt == M4R_DATA_NUMBER3)
        {
            if (tp == CreatureTemplate.Type.TubeWorm)
                return new(.05f, .3f, .7f);
            if (tp == CreatureTemplateType.NoodleEater)
                return new(138f / 255f, 245f / 255f, 0f);
            if (tp == CreatureTemplateType.FatFireFly)
                return Color.Lerp(Color.white, Color.blue, .1f);
            if (tp == CreatureTemplateType.CommonEel)
                return Color.Lerp(Color.white, new(0f, 72f / 255f, 1f), .1f);
        }
        return orig(iconData);
    }

    internal static bool On_CreatureSymbol_DoesCreatureEarnATrophy(On.CreatureSymbol.orig_DoesCreatureEarnATrophy orig, CreatureTemplate.Type creature) => creature != CreatureTemplateType.MiniScutigera && creature != CreatureTemplateType.XyloWorm && orig(creature);

    internal static string On_CreatureSymbol_LizardSpriteName(On.CreatureSymbol.orig_LizardSpriteName orig, string defaultSpriteName, int intData)
    {
        if (defaultSpriteName == "Kill_NoodleEater")
        {
            if (intData == M4R_DATA_NUMBER3)
                return "Kill_ProtoLizard";
            if (intData == M4R_DATA_NUMBER2)
                return "Kill_RotLizard";
        }
        return orig(defaultSpriteName, intData);
    }

    internal static string On_CreatureSymbol_SpriteNameOfCreature(On.CreatureSymbol.orig_SpriteNameOfCreature orig, IconSymbol.IconSymbolData iconData)
    {
        var tp = iconData.critType;
        var dt = iconData.intData;
        if (dt == M4R_DATA_NUMBER)
        {
            if (tp == CreatureTemplate.Type.Fly)
                return "Kill_SeedBat";
            if (tp == CreatureTemplateType.XyloWorm)
                return "Kill_BigXyloWorm";
            if (tp == CreatureTemplate.Type.TubeWorm)
                return "Kill_Bigrub";
        }
        if (dt is M4R_DATA_NUMBER2 or M4R_DATA_NUMBER3 && tp == CreatureTemplate.Type.TubeWorm)
            return "Kill_Bigrub";
        var res = orig(iconData);
        if (tp == CreatureTemplateType.NoodleEater || tp == CreatureTemplateType.SilverLizard || tp == CreatureTemplateType.WaterSpitter || tp == CreatureTemplateType.MoleSalamander || tp == CreatureTemplateType.Polliwog || tp == CreatureTemplateType.HunterSeeker)
            res = CreatureSymbol.LizardSpriteName(res, iconData.intData);
        return res;
    }

    internal static IconSymbol.IconSymbolData On_CreatureSymbol_SymbolDataFromCreature(On.CreatureSymbol.orig_SymbolDataFromCreature orig, AbstractCreature creature)
    {
        var res = orig(creature);
        var tp = creature.creatureTemplate.type;
        if (tp == CreatureTemplateType.FatFireFly || tp == CreatureTemplateType.CommonEel)
        {
            if (creature.Albino())
                res.intData = creature.superSizeMe ? M4R_DATA_NUMBER3 : M4R_DATA_NUMBER2;
            else if (creature.superSizeMe)
                res.intData = M4R_DATA_NUMBER;
        }
        else if ((tp == CreatureTemplate.Type.Fly && creature.IsSeed()) ||
            (tp == CreatureTemplate.Type.TentaclePlant && creature.RottenMode()) ||
            ((tp == CreatureTemplate.Type.Hazer || tp == CreatureTemplateType.Denture || tp == CreatureTemplateType.Glowpillar) && creature.Albino()) ||
            ((tp == CreatureTemplateType.ThornBug || tp == CreatureTemplateType.HazerMom || tp == CreatureTemplateType.TintedBeetle || tp == CreatureTemplateType.Xylo || tp == CreatureTemplateType.XyloWorm || tp == CreatureTemplateType.Hoverfly) && creature.superSizeMe))
            res.intData = M4R_DATA_NUMBER;
        else if (tp == CreatureTemplateType.Xylo && creature.Albino())
            res.intData = M4R_DATA_NUMBER2;
        else if (tp == CreatureTemplateType.NoodleEater)
        {
            if (!ModManager.Watcher || creature.state is not LizardState ls || ls.rotType == LizardState.RotType.None)
            {
                if (creature.superSizeMe)
                    res.intData = M4R_DATA_NUMBER;
            }
            else
            {
                if (creature.superSizeMe)
                    res.intData = ls.rotType != LizardState.RotType.Full ? M4R_DATA_NUMBER2 : M4R_DATA_NUMBER3;
            }
        }
        else if (tp == CreatureTemplate.Type.TubeWorm && creature.IsBig(out var prop))
            res.intData = prop.NormalLook ? M4R_DATA_NUMBER3 : (creature.superSizeMe ? M4R_DATA_NUMBER2 : M4R_DATA_NUMBER);
        return res;
    }

    internal static Color On_ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        if (itemType == AbstractObjectType.ThornyStrawberry)
            return new(189f / 255f, 0f, 0f);
        if (itemType == AbstractObjectType.SporeProjectile)
            return new(.9f, 1f, .8f);
        if (itemType == AbstractObjectType.LittleBalloon)
            return Color.Lerp(BalloonColor, Color.white, .2f);
        if (itemType == AbstractObjectType.BouncingMelon)
            return new(34f / 255f, 130f / 255f, 44f / 255f);
        if (itemType == AbstractObjectType.Physalis)
            return PhysalisColor;
        if (itemType == AbstractObjectType.LimeMushroom)
            return new(179f / 255f, 1f, 0f);
        if (itemType == AbstractObjectType.RubberBlossom)
            return StationPlantCol;
        if (itemType == AbstractObjectType.GummyAnther)
            return AntherCol;
        if (itemType == AbstractObjectType.MarineEye)
            return MarineCol;
        if (itemType == AbstractObjectType.StarLemon)
            return new(1f, 210f / 255f, 0f);
        if (itemType == AbstractObjectType.DendriticNeuron)
            return Ext.MenuGrey;
        if (itemType == AbstractObjectType.MiniBlueFruit)
            return Color.blue;
        if (itemType == AbstractObjectType.FumeFruit)
            return new(251f / 255f, 231f / 255f, 14f / 255f);
        if (itemType == AbstractObjectType.Durian)
            return new(121f / 255f, 130f / 255f, 15f / 255f);
        if (itemType == AbstractObjectType.MiniFruitSpawner)
            return default;
        if (itemType == AbstractObjectType.DarkGrub)
            return Color.green;
        return orig(itemType, intData);
    }

    internal static string On_ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        if (itemType == AbstractObjectType.ThornyStrawberry)
            return "Symbol_ThornyStrawberry";
        if (itemType == AbstractObjectType.SporeProjectile)
            return "Symbol_SporeProjectile";
        if (itemType == AbstractObjectType.BlobPiece)
            return "Kill_WaterBlob";
        if (itemType == AbstractObjectType.LittleBalloon)
            return "Symbol_LBHBulb";
        if (itemType == AbstractObjectType.BouncingMelon)
            return "Symbol_BouncingMelon";
        if (itemType == AbstractObjectType.Physalis)
            return "Symbol_Physalis";
        if (itemType == AbstractObjectType.LimeMushroom)
            return "Symbol_LimeMushroom";
        if (itemType == AbstractObjectType.RubberBlossom)
            return "Symbol_BigStationPlant";
        if (itemType == AbstractObjectType.GummyAnther)
            return "Symbol_BigStationPlantFruit";
        if (itemType == AbstractObjectType.MarineEye)
            return "Symbol_MarineEye";
        if (itemType == AbstractObjectType.StarLemon)
            return "Symbol_StarLemon";
        if (itemType == AbstractObjectType.DendriticNeuron)
            return "Symbol_DendriticNeuron";
        if (itemType == AbstractObjectType.MiniBlueFruit)
            return "Symbol_MiniBlueFruit";
        if (itemType == AbstractObjectType.FumeFruit)
            return "Symbol_FumeFruit";
        if (itemType == AbstractObjectType.Durian)
            return "Symbol_Durian";
        if (itemType == AbstractObjectType.DarkGrub)
            return "Symbol_DarkGrub";
        if (itemType == AbstractObjectType.MiniFruitSpawner)
            return "1x1TPX4";
        return orig(itemType, intData);
    }

    internal static void IL_Map_Draw(ILContext il)
    {
        var vars = il.Body.Variables;
        var c = new ILCursor(il);
        var label = il.DefineLabel();
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_WhiteLizard,
            s_MatchCall_Any,
            s_MatchBrfalse_Any))
        {
            label.Target = c.Next;
            c.Index -= 5;
            var l = vars[s_loc1];
            c.EmitDelegate((AbstractCreature crit) => crit.creatureTemplate.type == CreatureTemplateType.HunterSeeker);
            c.Emit(OpCodes.Brtrue, label)
             .Emit(OpCodes.Ldloc, l);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook HUD.Map.Draw! (part 1)");
        if (c.TryGotoNext(
            s_MatchLdloc_OutLoc1,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Overseer))
        {
            ++c.Index;
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((AbstractCreature crit, Map self) =>
             {
                 if (Jelly.TryGetValue(crit, out var jelly) && jelly.IsJelly)
                 {
                     var symbol = self.creatureSymbols[self.creatureSymbols.Count - 1];
                     symbol.myColor = jelly.Color;
                     symbol.spriteName = "Kill_JellyLL";
                     symbol.shadowSprite2.element = symbol.shadowSprite1.element = symbol.symbolSprite.element = Futile.atlasManager.GetElementWithName("Kill_JellyLL");
                     symbol.graphWidth = symbol.symbolSprite.element.sourcePixelSize.x;
                     symbol.symbolSprite.scale = symbol.shadowSprite2.scale = symbol.shadowSprite1.scale = 1f + jelly.IconRadBonus;
                 }
             });
            c.Emit(OpCodes.Ldloc, vars[s_loc1]);
            if (c.TryGotoNext(MoveType.After,
                s_MatchCallOrCallvirt_IconSymbol_Draw))
            {
                c.Emit(OpCodes.Ldarg_0)
                 .Emit(OpCodes.Ldloc, vars[s_loc1])
                 .EmitDelegate((Map self, AbstractCreature crit) =>
                 {
                     CreatureSymbol symbol;
                     if (Jelly.TryGetValue(crit, out var jelly) && jelly.IsJelly)
                     {
                         symbol = self.creatureSymbols[self.creatureSymbols.Count - 1];
                         var rs = 1f + jelly.IconRadBonus;
                         symbol.symbolSprite.scale *= rs;
                         symbol.shadowSprite2.scale *= rs;
                         symbol.shadowSprite1.scale *= rs;
                     }
                     else if (crit.realizedCreature is Denture dt && (!ModManager.MSC || !self.hud.rainWorld.safariMode))
                     {
                         symbol = self.creatureSymbols[self.creatureSymbols.Count - 1];
                         symbol.shadowSprite1.alpha = symbol.shadowSprite2.alpha = symbol.symbolSprite.alpha = Mathf.Lerp(symbol.symbolSprite.alpha, 0f, dt.JawOpen);
                     }
                 });
            }
            else
                LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook HUD.Map.Draw! (part 3)");
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook HUD.Map.Draw! (part 2)");
    }
}