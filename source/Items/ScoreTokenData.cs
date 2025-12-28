using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace LBMergedMods.Items;

public class ScoreTokenData(PlacedObject owner) : PlacedObject.ResizableObjectData(owner)
{
    public static bool DataChanged;
    public List<SlugcatStats.Name> UnavailableToPlayers = [];
    public Vector2 PanelPos;
    public int Score;
    public string ID = string.Empty;

    public override void FromString(string s)
    {
        var array = s.Split('~');
        if (array.Length >= 7)
        {
            float.TryParse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture, out handlePos.x);
            float.TryParse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture, out handlePos.y);
            float.TryParse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture, out PanelPos.x);
            float.TryParse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture, out PanelPos.y);
            int.TryParse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture, out Score);
            ID = array[5];
            var ar = array[6].Split(["<m4r>"], StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < ar.Length; i++)
                UnavailableToPlayers.Add(new(ar[i]));
            unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        var unv = UnavailableToPlayers;
        for (var i = 0; i < unv.Count; i++)
        {
            if (i > 0)
                sb.Append("<m4r>");
            sb.Append(unv[i]);
        }
        var s2 = sb.ToString();
        var ar = ID.Split('_');
        if (ar.Length > 0)
        {
            var regPath = AssetManager.ResolveFilePath(Path.Combine("world", ar[0].ToLowerInvariant()));
            if (Directory.Exists(regPath))
            {
                var path = Path.Combine(regPath, "mpkscoretokens.txt");
                var id2 = ID + "~";
                var flag = false;
                if (File.Exists(path))
                {
                    var lines = File.ReadAllLines(path);
                    for (var i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].StartsWith(id2))
                        {
                            lines[i] = id2 + s2;
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        Array.Resize(ref lines, lines.Length + 1);
                        lines[lines.Length - 1] = id2 + s2;
                    }
                    File.WriteAllLines(path, lines);
                }
                else
                    File.WriteAllText(path, id2 + s2);
            }
        }
        DataChanged = true;
        return SaveUtils.AppendUnrecognizedStringAttrs(new StringBuilder(handlePos.x.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(handlePos.y.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(PanelPos.x.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(PanelPos.y.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(Score.ToString(CultureInfo.InvariantCulture))
            .Append('~')
            .Append(ID)
            .Append('~').ToString() + s2, "~", unrecognizedAttributes);
    }
}