using System.Globalization;
using System.Text;
using UnityEngine;

namespace LBMergedMods.Items;

public class MiniFruitSpawnerData(PlacedObject owner) : PlacedObject.ConsumableObjectData(owner)
{
    public Vector2 HandlePos = new(0f, 100f), RootHandlePos = new(-50f, 50f);
    public int FoodAmount;
    //public bool FoodChance;

    public float Rad => HandlePos.magnitude;

    public override void FromString(string s)
    {
        var array = s.Split('~');
        if (array.Length >= 9)
        {
            float.TryParse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture, out panelPos.x);
            float.TryParse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture, out panelPos.y);
            int.TryParse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture, out minRegen);
            int.TryParse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture, out maxRegen);
            float.TryParse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture, out HandlePos.x);
            float.TryParse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture, out HandlePos.y);
            float.TryParse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture, out RootHandlePos.x);
            float.TryParse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture, out RootHandlePos.y);
            int.TryParse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture, out FoodAmount);
            //FoodChance = array[9] == "1"; // removal intended
            unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 9);
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
            .Append(HandlePos.x.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(HandlePos.y.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(RootHandlePos.x.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(RootHandlePos.y.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(FoodAmount.ToString(CultureInfo.InvariantCulture))
            /*.Append('~') // removal intended
            .Append(FoodChance ? '1' : '0')*/.ToString(), "~", unrecognizedAttributes);
}