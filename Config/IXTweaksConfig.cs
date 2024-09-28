using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using CSync.Lib;
using CSync;
using HarmonyLib;
using System.Runtime.Serialization;
using CSync.Util;

namespace IXTweaks;

class IXTweaksConfig : SyncedConfig<IXTweaksConfig> {
    // ---------- HUNTER CONFIG ------------
    [DataMember] public SyncedEntry<Int32> slimeHunterLevel;
    [DataMember] public SyncedEntry<Int32> slimeSampleMinVal;
    [DataMember] public SyncedEntry<Int32> slimeSampleMaxVal;

    [DataMember] public SyncedEntry<Int32> nutcrackerHunterLevel;
    [DataMember] public SyncedEntry<Int32> nutcrackerSampleMinVal;
    [DataMember] public SyncedEntry<Int32> nutcrackerSampleMaxVal;

    [DataMember] public SyncedEntry<Int32> springHitPoints;
    [DataMember] public SyncedEntry<Int32> springHunterLevel;
    [DataMember] public SyncedEntry<Int32> springSampleMinVal;
    [DataMember] public SyncedEntry<Int32> springSampleMaxVal;

    [DataMember] public SyncedEntry<Int32> jesterHitPoints;
    [DataMember] public SyncedEntry<Int32> jesterHunterLevel;
    [DataMember] public SyncedEntry<Int32> jesterSampleMinVal;
    [DataMember] public SyncedEntry<Int32> jesterSampleMaxVal;

    [DataMember] public SyncedEntry<Int32> butlerHunterLevel;
    [DataMember] public SyncedEntry<Int32> butlerSampleMinVal;
    [DataMember] public SyncedEntry<Int32> butlerSampleMaxVal;

    [DataMember] public SyncedEntry<Int32> maskedHunterLevel;
    [DataMember] public SyncedEntry<Int32> maskedSampleMinVal;
    [DataMember] public SyncedEntry<Int32> maskedSampleMaxVal;

    [DataMember] public SyncedEntry<Int32> pufferHitPoints;

    // ---------- CUSTOM DROP CONFIG ------------
    [DataMember] public SyncedEntry<Int32> commonItemDropWeight;
    [DataMember] public SyncedEntry<Int32> commonItemMinVal;
    [DataMember] public SyncedEntry<Int32> commonItemMaxVal;

    [DataMember] public SyncedEntry<Int32> rareItemDropWeight;
    [DataMember] public SyncedEntry<Int32> rareItemMinVal;
    [DataMember] public SyncedEntry<Int32> rareItemMaxVal;
    
    [DataMember] public SyncedEntry<Int32> legendaryItemDropWeight;
    [DataMember] public SyncedEntry<Int32> legendaryItemMinVal;
    [DataMember] public SyncedEntry<Int32> legendaryItemMaxVal;

    public IXTweaksConfig(ConfigFile cfg) : base("IXtimes.IXTweaks") {
        ConfigManager.Register(this);

        slimeHunterLevel = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Slime", "SlimeHunterLevel"),
            3,
            new ConfigDescription("The minimum hunter level needed in LGU to have the slime (Hygrodere) drop its sample")
        );
        slimeSampleMinVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Slime", "MinSlimeSampleValue"),
            125,
            new ConfigDescription("The minimum value that the sample dropped from the slime (Hygrodere) is worth")
        );
        slimeSampleMaxVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Slime", "MaxSlimeSampleValue"),
            220,
            new ConfigDescription("The maximum value that the sample dropped from the slime (Hygrodere) is worth")
        );

        nutcrackerHunterLevel = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Nutcracker", "NutcrackerHunterLevel"),
            3,
            new ConfigDescription("The minimum hunter level needed in LGU to have the nutcracker drop its sample")
        );
        nutcrackerSampleMinVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Nutcracker", "MinNutcrackerSampleValue"),
            100,
            new ConfigDescription("The minimum value that the sample dropped from the nutcracker is worth")
        );
        nutcrackerSampleMaxVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Nutcracker", "MaxNutcrackerSampleValue"),
            150,
            new ConfigDescription("The maximum value that the sample dropped from the nutcracker is worth")
        );

        springHitPoints = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Springhead", "SpringHitPoints"),
            3,
            new ConfigDescription("The amount of hits needed to kill a springhead (total # of hits irrespective of damage)")
        );
        springHunterLevel = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Springhead", "SpringHunterLevel"),
            4,
            new ConfigDescription("The minimum hunter level needed in LGU to have the springhead drop its sample")
        );
        springSampleMinVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Springhead", "MinSpringSampleValue"),
            100,
            new ConfigDescription("The minimum value that the sample dropped from the springhead is worth")
        );
        springSampleMaxVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Springhead", "MaxSpringSampleValue"),
            150,
            new ConfigDescription("The maximum value that the sample dropped from the springhead is worth")
        );

        jesterHitPoints = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Jester", "JesterHitPoints"),
            5,
            new ConfigDescription("The amount of hits needed to kill a jester (total # of hits irrespective of damage)")
        );
        jesterHunterLevel = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Jester", "JesterHunterLevel"),
            4,
            new ConfigDescription("The minimum hunter level needed in LGU to have the jester drop its sample")
        );
        jesterSampleMinVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Jester", "MinJesterSampleValue"),
            125,
            new ConfigDescription("The minimum value that the sample dropped from the jester is worth")
        );
        jesterSampleMaxVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Jester", "MaxJesterSampleValue"),
            300,
            new ConfigDescription("The maximum value that the sample dropped from the jester is worth")
        );

        maskedHunterLevel = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Masked", "MaskedHunterLevel"),
            2,
            new ConfigDescription("The minimum hunter level needed in LGU to have the masked drop its sample")
        );
        maskedSampleMinVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Masked", "MinMaskedSampleValue"),
            60,
            new ConfigDescription("The minimum value that the sample dropped from the masked is worth")
        );
        maskedSampleMaxVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Masked", "MaxMaskedSampleValue"),
            100,
            new ConfigDescription("The maximum value that the sample dropped from the masked is worth")
        );

        butlerHunterLevel = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Butler", "ButlerHunterLevel"),
            3,
            new ConfigDescription("The minimum hunter level needed in LGU to have the butler drop its sample")
        );
        butlerSampleMinVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Butler", "MinButlerSampleValue"),
            100,
            new ConfigDescription("The minimum value that the sample dropped from the butler is worth")
        );
        butlerSampleMaxVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Butler", "MaxButlerSampleValue"),
            150,
            new ConfigDescription("The maximum value that the sample dropped from the butler is worth")
        );

        pufferHitPoints = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Spore Lizard", "PufferHitPoints"),
            4,
            new ConfigDescription("The amount of damage needed to kill a spore lizard")
        );

        commonItemDropWeight = cfg.BindSyncedEntry(
            new ConfigDefinition("TBS Drops", "CommonDropWeight"),
            30,
            new ConfigDescription("The drop weight assigned to custom common items to be placed in the facility")
        );
        commonItemMinVal = cfg.BindSyncedEntry(
            new ConfigDefinition("TBS Drops", "MinCommonValue"),
            30,
            new ConfigDescription("The minimum value that is assigned to common items")
        );
        commonItemMaxVal = cfg.BindSyncedEntry(
            new ConfigDefinition("TBS Drops", "MaxCommonValue"),
            60,
            new ConfigDescription("The maximum value that is assigned to common items")
        );
        rareItemDropWeight = cfg.BindSyncedEntry(
            new ConfigDefinition("TBS Drops", "RareDropWeight"),
            10,
            new ConfigDescription("The drop weight assigned to custom rare items to be placed in the facility")
        );
        rareItemMinVal = cfg.BindSyncedEntry(
            new ConfigDefinition("TBS Drops", "MinRareValue"),
            60,
            new ConfigDescription("The minimum value that is assigned to rare items")
        );
        rareItemMaxVal = cfg.BindSyncedEntry(
            new ConfigDefinition("TBS Drops", "MaxRareValue"),
            120,
            new ConfigDescription("The maximum value that is assigned to rare items")
        );
        legendaryItemDropWeight = cfg.BindSyncedEntry(
            new ConfigDefinition("TBS Drops", "LegendaryDropWeight"),
            5,
            new ConfigDescription("The drop weight assigned to custom legendary items to be placed in the facility")
        );
        legendaryItemMinVal = cfg.BindSyncedEntry(
            new ConfigDefinition("TBS Drops", "MinLegendaryValue"),
            120,
            new ConfigDescription("The minimum value that is assigned to legendary items")
        );
        legendaryItemMaxVal = cfg.BindSyncedEntry(
            new ConfigDefinition("TBS Drops", "MaxLegendaryValue"),
            240,
            new ConfigDescription("The maximum value that is assigned to legendary items")
        );
    }
}