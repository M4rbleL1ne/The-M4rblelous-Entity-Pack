using System.Globalization;
using System.Text;
using RWCustom;

namespace LBMergedMods.Items;

public class DarkGrubData(PlacedObject owner) : PlacedObject.ConsumableObjectData(owner)
{
    public IntVector2 RootDir = new(-1, 0);

    public virtual string RootDirText
    {
        get
        {
            if (RootDir == new IntVector2(0, 1))
                return "Up";
            if (RootDir == new IntVector2(0, -1))
                return "Down";
            if (RootDir == new IntVector2(-1, 0))
                return "Left";
            if (RootDir == new IntVector2(1, 0))
                return "Right";
            return "None";
        }
    }

    public override void FromString(string s)
    {
        var array = s.Split('~');
        if (array.Length >= 6)
        {
            float.TryParse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture, out panelPos.x);
            float.TryParse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture, out panelPos.y);
            int.TryParse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture, out minRegen);
            int.TryParse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture, out maxRegen);
            int.TryParse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture, out RootDir.x);
            int.TryParse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture, out RootDir.y);
            unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
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
            .Append(RootDir.x.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(RootDir.y.ToString(CultureInfo.InvariantCulture)).ToString(), "~", unrecognizedAttributes);
}