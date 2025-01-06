using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AssetReplacer;

public class MeshReplacement : MonoBehaviour
{
    public enum Mode
    {
        ByMeshName,
        ByObjectName,
    }
    public Mode mode = Mode.ByMeshName;
    public string targetName = "";
    //public List<string> targetPaths;

    /* ... */
//#if !MINIBUILD
//    internal SettingsDropDownList usageMode;
//    internal SettingsDropDownList customMode;
//    internal SettingsTextBox customTargetName;

//    //class SaveCrap
//    //{
//    //    internal int usageMode;
//    //    internal int customMode;
//    //    internal string customTargetName;
//    //    internal SaveCrap(MeshReplacement replacement)
//    //    {
//    //        usageMode = 0;
//    //        customMode = (int)replacement.mode;
//    //        customTargetName = replacement.targetName;
//    //    }
//    //}
//    //SaveCrap saveCrap;

//    internal void CreateOverrideSettings(string fileName, PopupSetting popup, string jsonCrap)
//    {
//        //saveCrap = new SaveCrap(this);

//        popup.AddText($"<b>{gameObject.name}:</b>");

//        usageMode = popup.AddDropDownList(SaveID(nameof(usageMode)), ""
//            , new string[] { "Use normally", "Use with custom overrides", "Disable" }, GetSave(nameof(usageMode), 0), ToggleSettingsVisibility);
//        customMode = popup.AddDropDownList(SaveID(nameof(customMode)), "Mode"
//            , Enum.GetNames(typeof(Mode)), GetSave(nameof(customMode), (int)mode));
//        customTargetName = popup.AddTextBox(SaveID(nameof(customTargetName)), "Target Name", GetSave(nameof(customTargetName), targetName), "");

//        ToggleSettingsVisibility();

//        string SaveID(string crap)
//        {
//            return $"{fileName}{gameObject.name}{crap}";
//        }

//        T GetSave<T>(string crap, T def)
//        {
//            try
//            {
//                //string key = SaveID(crap);
//                JObject obj = JObject.Parse(jsonCrap);
//                //if (!obj.ContainsKey(key))
//                //    return def;
//                //return obj[key].ToObject<T>();
//                return def;
//            }
//            catch (Exception e)
//            {
//                ModConsole.LogError(e.ToString());
//                return def;
//            }
//        }
//    }

//    void ToggleSettingsVisibility()
//    {
//        bool b = usageMode.GetSelectedItemIndex() == 1;
//        customMode.SetVisibility(b);
//        customTargetName.SetVisibility(b);
//    }
//#endif
}