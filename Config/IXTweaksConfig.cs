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
    [DataMember] public SyncedEntry<Int32> slimeSampleMinVal;
    [DataMember] public SyncedEntry<Int32> slimeSampleMaxVal;

    public IXTweaksConfig(ConfigFile cfg) : base("IXtimes.IXTweaks") {
        ConfigManager.Register(this);

        slimeSampleMinVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Slime", "MinSampleValue"),
            125,
            new ConfigDescription("The minimum value that the sample dropped from the slime (Hydrogelen?) is worth")
        );
        slimeSampleMaxVal = cfg.BindSyncedEntry(
            new ConfigDefinition("Hunter: Slime", "MaxSampleValue"),
            220,
            new ConfigDescription("The maximum value that the sample dropped from the slime (Hydrogelen?) is worth")
        );
    }
}