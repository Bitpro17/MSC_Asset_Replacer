#if !MINIBUILD
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MSCLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AssetReplacer;

public class AssetReplacer : Mod
{
    public override string ID => "AssetReplacer"; // Your (unique) mod ID 
    public override string Name => "Asset Replacer"; // Your mod name
    public override string Author => "Bitpro17"; // Name of the Author (your name)
    public override string Version => "0.1"; // Version
    public override string Description => ""; // Short description of your mod

    public override void ModSetup()
    {
        SetupFunction(Setup.PostLoad, Mod_PostLoad);
        SetupFunction(Setup.ModSettingsLoaded, Mod_SettingsLoaded);
        SetupFunction(Setup.ModSettings, Mod_Settings);
    }

    SettingsCheckBox printDebug;
    const string configButtonName = "Configure Mesh Replacements";
    private void Mod_Settings()
    {
        //Settings.AddButton(configButtonName, () => { popup.ShowPopup((string s) => { OpenConfig(); }); }, SettingsButton.ButtonIcon.Settings);
        printDebug = Settings.AddCheckBox(nameof(printDebug), "Print debug text");
    }

    private void Mod_PostLoad()
    {
        LoadConfigs();
        HandleConfigs();
        HandleSoundFiles();
    }

    internal static AssetReplacer instance;
    void Mod_SettingsLoaded()
    {
        instance = this;
        //CreateConfigPopup();
    }

    //void SaveEverything(string t)
    //{
    //    jsonCrap = t;
    //    SaveLoad.WriteValue(this, nameof(jsonCrap), jsonCrap);
    //}

    PopupSetting popup;
    SettingsDropDownList fileSelection;
    SettingsDropDownList[] meshSelections;
    void CreateConfigPopup()
    {
        popup = ModUI.CreatePopupSetting(configButtonName, "Configure");
        fileSelection = popup.AddDropDownList("a", "File to configure:", meshConfigs.Select(x => x.sourceFile).ToArray()
            , 0, ToggleVisibility);
        meshSelections = new SettingsDropDownList[meshConfigs.Count];
        for (int i = 0; i < meshConfigs.Count; ++i)
            meshSelections[i] = popup.AddDropDownList("a", "Asset to configure:"
                , meshConfigs[i].replacements.Select(x => x.descriptiveName).ToArray(), visibleByDefault: false);
        meshSelections[0].SetVisibility(true);
    }

    void ToggleVisibility()
    {
        foreach (SettingsDropDownList list in meshSelections)
            list.SetVisibility(false);
        meshSelections[fileSelection.GetSelectedItemIndex()].SetVisibility(true);
    }
    void OpenConfig()
    {
        int selectedFile = fileSelection.GetSelectedItemIndex();
        int selectedAsset = meshSelections[selectedFile].GetSelectedItemIndex();
        meshConfigs[selectedAsset].replacements[selectedAsset].ShowConfigPopup();
    }

    void LoadConfigs()
    {
        Log("Loading Mesh Replacements.");
        meshConfigs = new List<MeshConfig>();

        foreach (string file in Directory.GetFiles(ModLoader.GetModAssetsFolder(this), "*.arp", SearchOption.AllDirectories))
        {
            try
            {
                LoadFile(file);
            }
            catch (System.Exception e)
            {
                LogError($"Issue loading file \"{file}\":\n{e.ToString()}");
            }
        }
    }

    List<GameObject> crapCache;
    void LoadFile(string file)
    {
        crapCache = new List<GameObject>();
        MeshConfig config = new MeshConfig(Path.GetFileName(file));
        meshConfigs.Add(config);
        AssetBundle ab = LoadAssets.LoadBundle(this, file);
        foreach (string prefab in ab.GetAllAssetNames().Where(x => x.EndsWith(".prefab")))
        {
            LogDebug($"Loading prefab {prefab}...");
            GameObject obj = ab.LoadAsset<GameObject>(prefab);
            MeshReplacement meshReplacement = obj.GetComponent<MeshReplacement>();
            if (meshReplacement == null)
            {
                LogDebug($"Prefab doesn't have {nameof(MeshReplacement)} component, skipping.");
                continue;
            }
            if (string.IsNullOrEmpty(meshReplacement.targetName))
            {
                LogDebug($"No target name specified, skipping.");
                continue;
            }

            config.replacements.Add(meshReplacement);
            meshReplacement.Init(config.sourceFile);
        }
        ab.Unload(false);
    }

    //void ChangeConfigsPopup()
    //{
    //    PopupSetting setting = ModUI.CreatePopupSetting(configButtonName, "Cancel");
    //    setting.AddText("Select a file to configure:");
    //    foreach (MeshConfig config in meshConfigs)
    //    {
    //        setting.AddButton($"<b>{config.sourceFile}</b> ( <b>{config.replacements.Count}</b> assets )"
    //            , () => { ModConsole.Log("hello"); setting.ClosePopup(); ConfigPopup(config); });
    //    }
    //    setting.ShowPopup((string s) => { });
    //}

    //void ConfigPopup(MeshConfig config)
    //{

    //}

    List<MeshConfig> meshConfigs;
    class MeshConfig
    {
        internal string sourceFile;
        internal List<MeshReplacement> replacements;
        internal MeshConfig(string sourceFile)
        {
            this.sourceFile = sourceFile;
            replacements = new List<MeshReplacement>();
        }
    }

    void HandleSoundFiles()
    {
        Dictionary<string, AudioClip> replaceClips = new Dictionary<string, AudioClip>();
        string[] supported = new string[] { ".mp3", ".ogg", ".wav", ".aiff", ".flac" };
        foreach (string file in Directory.GetFiles(ModLoader.GetModAssetsFolder(this), "*.*", SearchOption.AllDirectories)
            .Where(f => supported.Any(s => f.EndsWith(s))))
        {
            try
            {
                LogDebug($"Loading sound file {file}");
                replaceClips.Add(Path.GetFileNameWithoutExtension(file), ModAudio.LoadAudioClipFromFile(file, false));
            }
            catch (System.Exception e)
            {
                ModConsole.LogError(Message($"Failed loading file {file}: {e.ToString()}"));
            }
        }
        foreach (AudioSource source in Resources.FindObjectsOfTypeAll<AudioSource>())
        {
            AudioClip clip;
            if (source.clip != null && replaceClips.TryGetValue(source.clip.name, out clip))
            {
                LogDebug($"Replacing clip {source.clip.name} on {source.name}");

                bool isPlaying = source.isPlaying;
                source.clip = clip;
                if (isPlaying || source.playOnAwake)
                    source.Play(); //this might fuck something up ¯\_(ツ)_/¯
            }
        }
    }
    Dictionary<string, GameObject> replaceMeshes;
    Dictionary<string, GameObject> replaceGameObjects;
    void HandleConfigs()
    {
        replaceMeshes = new Dictionary<string, GameObject>();
        replaceGameObjects = new Dictionary<string, GameObject>();

        foreach (MeshReplacement replacement in meshConfigs.SelectMany(x => x.replacements))
        {
            //switch (replacement.usageMode.GetSelectedItemIndex())
            //{
            //    case 1:
            //        replacement.mode = (MeshReplacement.Mode)replacement.customMode.GetSelectedItemIndex();
            //        replacement.targetName = replacement.customTargetName.GetValue();
            //        break;
            //    case 2:
            //        LogDebug($"Skipping {replacement.gameObject.name} since it is disabled.");
            //        continue;
            //}
            Dictionary<string, GameObject> dict = replacement.mode == MeshReplacement.Mode.ByMeshName
                                           ? replaceMeshes : replaceGameObjects;
            if (dict.ContainsKey(replacement.targetName))
                Log($"{replacement.targetName} is being replaced twice...");

            dict[replacement.targetName] = replacement.gameObject;

            //clean stuff up
            foreach (Collider col in replacement.GetComponentsInChildren<Collider>(true))
                col.enabled = false;
            Behaviour.Destroy(replacement);
        }

        MeshFilter[] filters = Resources.FindObjectsOfTypeAll<MeshFilter>();
        foreach (MeshFilter filter in filters)
        {
            GameObject obj;
            string name = UnFuckName(filter.mesh.name);
            if (replaceMeshes.TryGetValue(name, out obj))
                ReplaceMesh(filter, obj);
        }

        foreach (KeyValuePair<string, GameObject> pair in replaceGameObjects)
        {
            MeshFilter filter = filters.FirstOrDefault(x => x.name == pair.Key);
            if (filter == null)
                LogError($"Couldn't find object {pair.Key}");
            else
                ReplaceMesh(filter, pair.Value);
        }
    }

    void ReplaceMesh(MeshFilter filter, GameObject replaceWith)
    {
        LogDebug($"Replacing mesh on {filter.name}");

        Transform parent = filter.transform;
        MeshRenderer rend = filter.GetComponent<MeshRenderer>();
        if (rend != null)
            Behaviour.Destroy(rend);
        Behaviour.Destroy(filter);
        Transform replacement = GameObject.Instantiate(replaceWith).transform;
        replacement.parent = parent;
        replacement.position = parent.position;
        replacement.rotation = parent.rotation;
    }

    string UnFuckName(string name)
    {
        string crap = " Instance";
        if (name.EndsWith(crap))
            return name.Remove(name.Length - crap.Length);
        return name;
    }

    internal static void LogDebug(string text)
    {
        if (instance.printDebug.GetValue())
            ModConsole.Log(Message(text));
    }

    internal static void Log(string text)
    {
        ModConsole.Log(Message(text));
    }

    internal static void LogError(string text)
    {
        ModConsole.LogError(Message(text));
    }

    internal static string Message(string text)
    {
        return $"<color=orange>[Asset Replacer]</color> {text}";
    }
}

internal class CoroutineRunner : MonoBehaviour
{
    static CoroutineRunner instance;
    internal static CoroutineRunner Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("Asset Replacer Coroutine Helper");
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<CoroutineRunner>();
            }
            return instance;
        }
    }
}
#endif