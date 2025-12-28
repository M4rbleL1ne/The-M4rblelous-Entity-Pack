using MoreSlugcats;

namespace LBMergedMods.Items;

public static class StalkUtils
{
    public static bool StalkActive(this BubbleGrass self) => self.growPos.HasValue;

    public static bool StalkActive(this DandelionPeach self) => self.stalk is DandelionPeach.Stalk st && st.nut is not null && !st.slatedForDeletetion;

    public static bool StalkActive(this DangleFruit self) => self.stalk is DangleFruit.Stalk st && st.fruit is not null && !st.slatedForDeletetion;

    public static bool StalkActive(this FirecrackerPlant self) => self.growPos.HasValue;

    public static bool StalkActive(this FlareBomb self) => self.stalk is FlareBomb.Stalk st && st.fruit is not null && !st.slatedForDeletetion;

    public static bool StalkActive(this FlyLure self) => self.growPos.HasValue;

    public static bool StalkActive(this GlowWeed self) => self.stalk is GlowWeed.Stalk st && st.fruit is not null && !st.slatedForDeletetion;

    public static bool StalkActive(this GooieDuck self) => !self.StringsBroke && self.StringGoals is not null;

    public static bool StalkActive(this KarmaFlower self) => self.growPos.HasValue;

    public static bool StalkActive(this Lantern self) => self.stick is LanternStick st && st.lantern is not null && !st.slatedForDeletetion;

    public static bool StalkActive(this LillyPuck self) => self.myStalk is LillyPuck.Stalk st && st.fruit is not null && !st.slatedForDeletetion;

    public static bool StalkActive(this Mushroom self) => self.growPos.HasValue;

    public static bool StalkActive(this NeedleEgg self) => self.stalk is NeedleEgg.Stalk st && st.egg is not null && !st.slatedForDeletetion;

    public static bool StalkActive(this SlimeMold self) => self.stuckPos.HasValue;

    public static bool StalkActive(this SporePlant self) => self.stalk is SporePlant.Stalk st && st.sporePlant is not null && !st.slatedForDeletetion;

    public static bool StalkActive(this WaterNut self) => self.stalk is WaterNut.Stalk st && st.nut is not null && !st.slatedForDeletetion;

    public static bool StalkActive(this SkyDandelions.SkyDandelion self) => self.stuck;

    public static bool StalkActive(this Pomegranate self) => !self.disconnected;
}