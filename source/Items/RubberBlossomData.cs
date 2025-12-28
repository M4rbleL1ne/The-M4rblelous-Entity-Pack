using UnityEngine;
using System.Globalization;
using System.Text;

namespace LBMergedMods.Items;

public class RubberBlossomData : PlacedObject.ConsumableObjectData
{
    public float Red, Green, Blue, MaxUpwardVel = 20f;
    public int CyclesOpen = 1, CyclesClosed = 1, FoodAmount;
    public bool RandomOpen, RandomClosed, FoodChance, StartsOpen = true, AlwaysOpen, AlwaysClosed;

    public virtual Color Color => new(Red, Green, Blue);

    public RubberBlossomData(PlacedObject owner) : base(owner) => minRegen = maxRegen = 2;

    public override void FromString(string s)
    {
        var array = s.Split('~');
        if (array.Length >= 14)
        {
            float.TryParse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture, out panelPos.x);
            float.TryParse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture, out panelPos.y);
            int.TryParse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture, out minRegen);
            int.TryParse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture, out maxRegen);
            int.TryParse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture, out CyclesOpen);
            int.TryParse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture, out CyclesClosed);
            int.TryParse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture, out FoodAmount);
            RandomOpen = array[7] == "1";
            RandomClosed = array[8] == "1";
            FoodChance = array[9] == "1";
            StartsOpen = array[10] == "1";
            float.TryParse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture, out Red);
            float.TryParse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture, out Green);
            float.TryParse(array[13], NumberStyles.Any, CultureInfo.InvariantCulture, out Blue);
            if (array.Length >= 15)
            {
                float.TryParse(array[14], NumberStyles.Any, CultureInfo.InvariantCulture, out MaxUpwardVel);
                if (array.Length >= 17)
                {
                    AlwaysOpen = array[15] == "1" && array[16] != "1";
                    AlwaysClosed = array[16] == "1" && array[15] != "1";
                    unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 17);
                }
                else
                    unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 15);
            }
            else
                unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 14);
        }
    }

    public override string ToString() => SaveUtils.AppendUnrecognizedStringAttrs(new StringBuilder()
            .Append(panelPos.x.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(panelPos.y.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(minRegen.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(maxRegen.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(CyclesOpen.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(CyclesClosed.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(FoodAmount.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(RandomOpen ? '1' : '0')
            .Append('~')
            .Append(RandomClosed ? '1' : '0')
            .Append('~')
            .Append(FoodChance ? '1' : '0')
            .Append('~')
            .Append(StartsOpen ? '1' : '0')
            .Append('~')
            .Append(Red.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(Green.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(Blue.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(MaxUpwardVel.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(AlwaysOpen && !AlwaysClosed ? '1' : '0')
            .Append('~')
            .Append(AlwaysClosed && !AlwaysOpen ? '1' : '0').ToString(), "~", unrecognizedAttributes);
}