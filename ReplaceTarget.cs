using MSCLoader;
using System;
using System.Collections;
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
    public string descriptiveName = "";
    public string saveKey = "";
    public Mode mode = Mode.ByMeshName;
    public string targetName = "";
    //public Material[] colorableMaterials = new Material[0];

#if !MINIBUILD
    Mode savedMode;
    string savedTargetName;
    string saveID;
    internal void Init(string fileName)
    {
        return;

        savedMode = mode;
        savedTargetName = targetName;
        if (string.IsNullOrEmpty(descriptiveName))
        {
            AssetReplacer.LogError($"Descriptive name is empty on prefab {gameObject.name}! Please set it to something nice:)");
            descriptiveName = gameObject.name;
        }
        if (string.IsNullOrEmpty(saveKey))
            saveKey = descriptiveName;
        saveID = $"{fileName}{saveKey}";

        string save = SaveLoad.ValueExists(AssetReplacer.instance, saveID)
            ? SaveLoad.ReadValue<string>(AssetReplacer.instance, saveID) : "{}";
        ParseSettings(save);
    }

    //class SaveCrap
    //{
    //    internal int usageMode;
    //    internal int customMode;
    //    internal string customTargetName;
    //    internal SaveCrap(MeshReplacement replacement)
    //    {
    //        usageMode = 0;
    //        customMode = (int)replacement.mode;
    //        customTargetName = replacement.targetName;
    //    }
    //}
    //SaveCrap saveCrap;

    //internal void CreateOverrideSettings(string fileName, PopupSetting popup, string jsonCrap)
    //{
    //    //saveCrap = new SaveCrap(this);

    //    popup.AddText($"<b>{gameObject.name}:</b>");

    //    usageMode = popup.AddDropDownList(SaveID(nameof(usageMode)), ""
    //        , new string[] { "Use normally", "Use with custom overrides", "Disable" }, GetSave(nameof(usageMode), 0), ToggleSettingsVisibility);
    //    customMode = popup.AddDropDownList(SaveID(nameof(customMode)), "Mode"
    //        , Enum.GetNames(typeof(Mode)), GetSave(nameof(customMode), (int)mode));
    //    customTargetName = popup.AddTextBox(SaveID(nameof(customTargetName)), "Target Name", GetSave(nameof(customTargetName), targetName), "");

    //    ToggleSettingsVisibility();

    //    string SaveID(string crap)
    //    {
    //        return $"{fileName}{gameObject.name}{crap}";
    //    }

    //    T GetSave<T>(string crap, T def)
    //    {
    //        try
    //        {
    //            //string key = SaveID(crap);
    //            JObject obj = JObject.Parse(jsonCrap);
    //            //if (!obj.ContainsKey(key))
    //            //    return def;
    //            //return obj[key].ToObject<T>();
    //            return def;
    //        }
    //        catch (Exception e)
    //        {
    //            ModConsole.LogError(e.ToString());
    //            return def;
    //        }
    //    }
    //}

    Save save;
    class Save
    {
        internal int usageMode;
        internal int customMode;
        internal string customTargetName;
    }

    internal SettingsDropDownList usageMode;
    internal SettingsDropDownList customMode;
    internal SettingsTextBox customTargetName;

    internal void ShowConfigPopup()
    {
        PopupSetting popup = ModUI.CreatePopupSetting(descriptiveName.ToUpper(), "Apply");
        usageMode = popup.AddDropDownList(nameof(usageMode), "Usage Mode"
            , new string[] { "Default", "Use with custom overrides", "Disable" }, save.usageMode, ToggleSettingsVisibility );
        customMode = popup.AddDropDownList(nameof(customMode), "Mode", Enum.GetNames(typeof(Mode)), save.customMode);
        customTargetName = popup.AddTextBox(nameof(customTargetName), "Target Name", save.customTargetName, "");

        ToggleSettingsVisibility();
        CoroutineRunner.Instance.StartCoroutine(WaitPopup(popup));
    }

    IEnumerator WaitPopup(PopupSetting popup)
    {
        yield return null;
        popup.ShowPopup((s) => { ParseSettings(s); SaveSettings(s); });
    }

    void ToggleSettingsVisibility()
    {
        bool b = usageMode.GetSelectedItemIndex() == 1;
        customMode.SetVisibility(b);
        customTargetName.SetVisibility(b);
    }
    void ParseSettings(string settings)
    {
        save = ModUI.ParsePopupResponse<Save>(settings);
        switch (save.usageMode)
        {
            case 0:
                mode = savedMode;
                targetName = savedTargetName;
                break;
            case 1:
                mode = (Mode)save.customMode;
                targetName = save.customTargetName;
                break;
        }
    }

    void SaveSettings(string settings)
    {
        SaveLoad.WriteValue(AssetReplacer.instance, saveID, settings);
    }
#endif
}