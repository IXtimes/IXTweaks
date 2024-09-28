using System;
//using System.Numerics;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using MonoMod.Cil;
using LethalLib.Modules;
using MoreShipUpgrades.API;
using Mono.Cecil.Cil;
using System.IO;
using System.Reflection;
using IXTweaks.MonoBehaviors;
using GameNetcodeStuff;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.Enemies;
using MoreShipUpgrades.Managers;
using Unity.Collections;
using IL;
using UnityEngine.Scripting;
using MoreShipUpgrades.UpgradeComponents.Items;

namespace IXTweaks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("com.sigurd.csync")]
[BepInDependency(LethalLib.Plugin.ModGUID)] 
[BepInDependency(MoreShipUpgrades.Misc.Metadata.GUID, BepInDependency.DependencyFlags.HardDependency)] 
[LobbyCompatibility(CompatibilityLevel.ClientOnly, VersionStrictness.None)]
public class IXTweaks : BaseUnityPlugin
{
    public static IXTweaks Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static new IXTweaksConfig? Config;
    public static AssetBundle IXTweaksAssets;

    private void Awake() {
        Logger = base.Logger;
        Instance = this;

        // Load config
        Config = new IXTweaksConfig(base.Config);

        // Load assets
        string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        IXTweaksAssets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "ixtweaks"));
        if (IXTweaksAssets == null) {
            Logger.LogError("Failed to load custom assets."); // ManualLogSource for your plugin
            return;
        }
        RegisterRPCs();

        // Basic item imports
        RegisterItem("JeremiahTheSlug.asset", Config.commonItemDropWeight, Config.commonItemMinVal, Config.commonItemMaxVal, "Assets/IXTweaks/Scraps/");
        //RegisterItem("Celsius.asset", Config.commonItemDropWeight, Config.commonItemMinVal, Config.commonItemMaxVal, "Assets/IXTweaks/Scraps/");
        //RegisterItem("RoastedNutCoffee.asset", Config.commonItemDropWeight, Config.commonItemMinVal, Config.commonItemMaxVal, "Assets/IXTweaks/Scraps/");
        //RegisterItem("MagicMushrooms.asset", Config.commonItemDropWeight, Config.commonItemMinVal, Config.commonItemMaxVal, "Assets/IXTweaks/Scraps/");
        //RegisterItem("Loaf.asset", Config.commonItemDropWeight, Config.commonItemMinVal, Config.commonItemMaxVal, "Assets/IXTweaks/Scraps/");
        RegisterItem("NyQuil.asset", Config.rareItemDropWeight, Config.rareItemMinVal, Config.rareItemMaxVal, "Assets/IXTweaks/Scraps/");
        //RegisterItem("VampireTeeth.asset", Config.rareItemDropWeight, Config.rareItemMinVal, Config.rareItemMaxVal, "Assets/IXTweaks/Scraps/");
        //RegisterItem("AirForceOnes.asset", Config.rareItemDropWeight, Config.rareItemMinVal, Config.rareItemMaxVal, "Assets/IXTweaks/Scraps/");
        //RegisterItem("PaperAirplane.asset", Config.rareItemDropWeight, Config.rareItemMinVal, Config.rareItemMaxVal, "Assets/IXTweaks/Scraps/");
        //RegisterItem("7LeafClover.asset", Config.legendaryItemDropWeight, Config.legendaryItemMinVal, Config.legendaryItemMaxVal, "Assets/IXTweaks/Scraps/");
        RegisterHunterDrop("HygrodereChunk.asset", "Blob", Config.slimeSampleMinVal, Config.slimeSampleMaxVal, Config.slimeHunterLevel, "Assets/IXTweaks/Samples/");
        RegisterHunterDrop("NutcrackerHead.asset", "Nutcracker", Config.nutcrackerSampleMinVal, Config.nutcrackerSampleMaxVal, Config.nutcrackerHunterLevel, "Assets/IXTweaks/Samples/");
        RegisterHunterDrop("CoilHead.asset", "Spring", Config.springSampleMinVal, Config.springSampleMaxVal, Config.springHunterLevel, "Assets/IXTweaks/Samples/");
        RegisterHunterDrop("JesterBox.asset", "Jester", Config.jesterSampleMinVal, Config.jesterSampleMaxVal, Config.jesterHunterLevel, "Assets/IXTweaks/Samples/");
        RegisterHunterDrop("EuropeanBlood.asset", "Butler", Config.butlerSampleMinVal, Config.butlerSampleMaxVal, Config.butlerHunterLevel, "Assets/IXTweaks/Samples/");
        RegisterHunterDrop("BrokenMask.asset", "Masked", Config.maskedSampleMinVal, Config.maskedSampleMaxVal, Config.maskedHunterLevel, "Assets/IXTweaks/Samples/");
        // Register items that take a little more work for some special payoff ;)
        RegisterSpecialItems();

        // Add patches
        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch() {

        Logger.LogDebug("Patching...");

        // Slime hunter patches
        On.BlobAI.HitEnemy += DropSampleOnHappy;
        IL.BlobAI.HitEnemy += PissedPatch;
        On.BlobAI.OnCollideWithPlayer += PissedOffInstaKill;
        On.BlobAI.Update += SlimeGas;

        // Audio patches
        On.FlowermanAI.Start += BrackenAudioPatch;
        On.HoarderBugAI.Start += HorderBugAudioPatch;

        // Invinsible enemy wrapper patches
        //IL.EnemyAI.KillEnemy += InvinsiblePatch;

        Logger.LogDebug("Attempting Damage Wrapper");
        On.EnemyAI.HitEnemy += DamageWrapperIfApplicable;
        Logger.LogDebug("Attempting Awake Wrapper");
        On.EnemyAI.Start += ApplyWrapperOnAwake;

        Logger.LogDebug("Finished patching!");
    }

    private static void HorderBugAudioPatch(On.HoarderBugAI.orig_Start orig, HoarderBugAI self) {
        // Perform general start method
        orig(self);

        // Load in audio files from asset bundle and place them in array
        string basePath = "Assets/IXtweaks/Assets/SFXs/";
        AudioClip[] clips = new AudioClip[18];
        for(int i = 0; i < 18; i++) {
            clips[i] = IXTweaksAssets.LoadAsset<AudioClip>(basePath + (i + 1).ToString() + ".wav");
            Debug.Log(basePath + (i + 1).ToString() + ".wav");
        }

        // Patch loot bug passive audio
        self.chitterSFX = clips;
        self.angryScreechSFX = clips;
    }


    private static void BrackenAudioPatch(On.FlowermanAI.orig_Start orig, FlowermanAI self) {
        // Perform general start method
        orig(self);

        // Load in audio files from asset bundle and place them in array
        string basePath = "Assets/IXtweaks/Assets/SFXs/";
        AudioClip clip = IXTweaksAssets.LoadAsset<AudioClip>(basePath + "12Brack.wav");

        // Patch bracken anger noise with audio patches
        self.creatureAngerVoice.clip = clip;
    }


    private static void ApplyWrapperOnAwake(On.EnemyAI.orig_Start orig, EnemyAI self) {
        // Initalize enemy AI
        orig(self);

        // If the enemy is of a certain AI bracket, add the wrapper to them
        if (self.GetComponent<SpringManAI>()) 
            self.gameObject.AddComponent<InvinsibleEnemyWrapper>().internalHealth = Config.springHitPoints;
        if (self.GetComponent<JesterAI>())
            self.gameObject.AddComponent<InvinsibleEnemyWrapper>().internalHealth = Config.jesterHitPoints;
        if (self.GetComponent<PufferAI>())
            self.gameObject.AddComponent<InvinsibleEnemyWrapper>().internalHealth = Config.pufferHitPoints;
        //if (self.GetComponent<ClaySurgeonAI>())
        //    self.gameObject.AddComponent<InvinsibleEnemyWrapper>().internalHealth = 4;
    }


    private static void DamageWrapperIfApplicable(On.EnemyAI.orig_HitEnemy orig, EnemyAI self, int force, PlayerControllerB playerWhoHit, bool playHitSFX, int hitID) {
        // Reg call
        orig(self, force, playerWhoHit, playHitSFX, hitID);

        // If this AI has a health wrapper call the damage event within it
        if (self.GetComponent<InvinsibleEnemyWrapper>() != null) {
            if (self.GetComponent<SpringManAI>() != null || self.GetComponent<JesterAI>() != null)
                self.GetComponent<InvinsibleEnemyWrapper>().DamageWrapper(force, true, playerWhoHit);
            else
                self.GetComponent<InvinsibleEnemyWrapper>().DamageWrapper(force, false, playerWhoHit);
        }
    }

    private static void PissedPatch(ILContext il) {
        // Create a cursor off of the context
        ILCursor c = new(il);

        // First, navigate the cursor to the instruction after the angered timer is set to insert our new logic
        c.GotoNext(
            MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdcR4(18),
            x => x.MatchStfld<BlobAI>("angeredTimer")
        );

        // Remove this statement
        c.Index -= 2;
        for(int i = 0; i < 3; i++)
            c.Remove();

        // Insert logic for only setting anger if not pissed off
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Action<BlobAI>>(self => {
            if (self.angeredTimer < 1000f)
                self.angeredTimer = 18f;
        });
    }


    private static void SlimeGas(On.BlobAI.orig_Update orig, BlobAI self) {
        // Perform normal logic
        orig(self);

        // Check if the slime is pissed off
        if(self.angeredTimer > 1000f) {
            // Ensure fast persuit speed
            self.agent.speed = 3f;
        }
    }


    private static void PissedOffInstaKill(On.BlobAI.orig_OnCollideWithPlayer orig, BlobAI self, Collider other) {
        // Check if the slime is pissed off and collided with the player
        PlayerControllerB playerControllerB = self.MeetsStandardPlayerCollisionConditions(other);
        if(self.angeredTimer > 1000f && playerControllerB != null) {
            playerControllerB.DamagePlayer(1000);
            if (playerControllerB.isPlayerDead) {
                self.SlimeKillPlayerEffectServerRpc((int)playerControllerB.playerClientId);
            }
        }

        // Perform normal logic
        orig(self, other);
    }

    private static void DropSampleOnHappy(On.BlobAI.orig_HitEnemy orig, BlobAI self, int force, PlayerControllerB playerWhoHit, bool playHitSFX, int hitID) {
        // Perform normal logic
        orig(self, force, playerWhoHit, playHitSFX, hitID);

        // Check if the slime WAS tammed before this hit
        Logger.LogDebug("Hit Slime");
        Logger.LogDebug((self.angeredTimer - 19f));
        if(self.tamedTimer > 0f && (self.angeredTimer - 19f) <= 0f) { // Not messin up the condition dis time :)
            // In such case, make the slime permanantly angry
            Logger.LogInfo("Slime was habby, make him angy >:(");
            self.angeredTimer = Mathf.Infinity;
            self.tamedTimer = 0f;
            self.thisSlimeMaterial.SetColor(Shader.PropertyToID("_Gradient_Color"), Color.red);
            self.thisSlimeMaterial.SetFloat("_Frequency", 8f);
            self.thisSlimeMaterial.SetFloat("_Ripple_Density", self.slimeJiggleDensity * 3f);
            self.thisSlimeMaterial.SetFloat("_Amplitude", self.slimeJiggleAmplitude * 3f);
            GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.4f);

            // SFX off of the slimes's audio source
            AudioSource slimeSource = self.GetComponent<AudioSource>();
            AudioClip angrySlimeSFX = IXTweaksAssets.LoadAsset<AudioClip>("Assets/IXTweaks/Assets/SFXs/AngrySlimeGurgle.wav");
            slimeSource.PlayOneShot(angrySlimeSFX);
            WalkieTalkie.TransmitOneShotAudio(slimeSource, angrySlimeSFX, 100f);
            RoundManager.Instance.PlayAudibleNoise(self.transform.position, 10f, 100f, 0, StartOfRound.Instance.hangarDoorsClosed);

            // Then, using LGU drop a slime sample if the player has the provided hunter conditions met
            if (MoreShipUpgrades.Misc.Upgrades.BaseUpgrade.GetActiveUpgrade(Hunter.UPGRADE_NAME) && Hunter.CanHarvest("blob")) {
                Logger.LogInfo("Slime drop loot for hunter");
                ItemManager.Instance.SpawnSample("blob", self.transform.position);
            }
        }
    }


    private void RegisterSpecialItems() {
        // Windex is a spray item cause... I mean it is a spray bottle :p
        Item windex = RegisterItem("Windex.asset", Config.commonItemDropWeight, Config.commonItemMinVal, Config.commonItemMaxVal, "Assets/IXTweaks/Scraps/");
        Utilities.FixMixerGroups(windex.spawnPrefab);

        // Apply spray script and populate it appropriately
        FakeSprayItem script = windex.spawnPrefab.AddComponent<FakeSprayItem>();
        script.grabbable = true;
        script.grabbableToEnemies = true;
        script.itemProperties = windex;

        script.fullSprayEffect = script.transform.GetChild(2);
        script.emptySprayEffect = script.transform.GetChild(2).GetChild(1).GetComponent<ParticleSystem>();
        script.spraySFXr = script.GetComponent<AudioSource>();
        Logger.LogDebug(string.Join(", ", IXTweaksAssets.GetAllAssetNames()));
        script.spraySFX = IXTweaksAssets.LoadAsset<AudioClip>("Assets/IXTweaks/Assets/SFXs/WeedKillerSpray.mp3");
        script.emptySFX = IXTweaksAssets.LoadAsset<AudioClip>("Assets/IXTweaks/Assets/SFXs/WeedKillerEmpty.mp3");

        // Scissors is a powerful weapon dropped by the clay surgeon
        //Item scissors = RegisterHunterDrop("Scissors.asset", "Barber", "Assets/IXTweaks/Samples/");
        //Utilities.FixMixerGroups(scissors.spawnPrefab);

        //Scissors sci = scissors.spawnPrefab.AddComponent<Scissors>();
        //Destroy(scissors.spawnPrefab.GetComponent<MonsterSample>());
        //sci.grabbable = true;
        //sci.grabbableToEnemies = true;
        //sci.itemProperties = scissors;

        //sci.sfx = sci.GetComponent<AudioSource>();
        //Logger.LogDebug(sci.sfx);
        //sci.anim = sci.transform.GetChild(1).GetComponent<Animator>();
        //Logger.LogDebug(sci.anim);
        //sci.snipSFX = IXTweaksAssets.LoadAsset<AudioClip>("Assets/IXTweaks/Assets/SFXs/Snip.mp3");
    }

    private Item RegisterHunterDrop(string itemName, string monsterName, int min, int max, int level, string pathToItem="Assets/IXTweaks/Samples/")
    {
        // loads asset from the asset bundle we provided
        Item item = IXTweaksAssets.LoadAsset<Item>(pathToItem + itemName);
        // Apply min and max
        item.minValue = min;
        item.maxValue = max;
        if (item == null)
            Logger.LogError($"Failed to load {itemName} from IXTweaksAssets");
        else {
            HunterSamples.RegisterSample(item, monsterName, level, true, true, 100);
            Utilities.FixMixerGroups(item.spawnPrefab);

            return item;
        }

        return null;
    }

    private Item RegisterItem(string itemName, int weight, int min, int max, string pathToItem="Assets/IXTweaks/Scraps/")
    {
        // loads asset from the asset bundle we provided
        Item item = IXTweaksAssets.LoadAsset<Item>(pathToItem + itemName);
        // Apply min and max
        item.minValue = min;
        item.maxValue = max;
        if (item == null)
            Logger.LogError($"Failed to load {itemName} from IXTweaksAssets");
        else
        {
            Items.RegisterScrap(item, weight, Levels.LevelTypes.All);
            NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
            Utilities.FixMixerGroups(item.spawnPrefab);

            // Provided we imported into LethalLib successfully, we can return our item
            return item;
        }

        // Return null if the item is imported unsucessfully
        return null;
    }

    private void RegisterRPCs() {
        Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (Type type in types) {
            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (MethodInfo method in methods) {
                object[] attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0) {
                    method.Invoke(null, null);
                }
            }
        }
    }
}
