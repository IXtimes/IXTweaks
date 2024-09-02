using System;
//using System.Numerics;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using MonoMod.Cil;
using GameNetcodeStuff;
using Mono.Cecil.Cil;

namespace IXTweaks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("com.siguard.csync", "4.1.0")]
[BepInDependency(LethalLib.Plugin.ModGUID)] 
[LobbyCompatibility(CompatibilityLevel.ClientOnly, VersionStrictness.None)]
public class IXTweaks : BaseUnityPlugin
{
    public static IXTweaks Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static new IXTweaksConfig? Config;

    private void Awake() {
        Logger = base.Logger;
        Instance = this;

        // Load config
        Config = new IXTweaksConfig(base.Config);

        // Add patches
        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch() {

        Logger.LogDebug("Patching...");

        // Add patch for kill on sprint depletion
        On.GameNetcodeStuff.PlayerControllerB.Update += MyPatch;
        // Add patch for emoting whenever
        On.GameNetcodeStuff.PlayerControllerB.CheckConditionsForEmote += EmotePatch;

        // IL patch for jump depending on crouch state
        IL.GameNetcodeStuff.PlayerControllerB.Jump_performed += IL_JumpPatch;
        // IL patch for slimes being dicks
        IL.BlobAI.OnCollideWithPlayer += IL_BlobPatch;

        Logger.LogDebug("Finished patching!");
    }

    private static void IL_BlobPatch(ILContext il)
    {
        // Create a cursor to trace to the IL that we want to make modifications at
        ILCursor c = new(il);

        // Find this IL which is where we want to place our cursor
        c.GotoNext(
            MoveType.After, // We specifically want to place the cursor AFTER this instruction, since we are replacing it
            x => x.MatchLdarg(0), // Match: 'IL_0022: ldarg.0' since we are replacing it
            x => x.MatchLdfld<BlobAI>(nameof(BlobAI.angeredTimer)), // Match: 'IL_0023: ldfld float32 BlobAI::angeredTimer'
            x => x.MatchLdcR4(0.0f), // Match 'IL_0028: ldc.r4 0.0'
            x => x.MatchBgeUn(out _) // Match 'IL_002d: bge.un.s IL_0030', with out '_' we specify for ANY matching arg
        );

        // With the instruction found and the cursor placed AFTER it, we can replace its opcode with the appropriate one!
        // 'IL_002d: bgt.un.s IL_0030'
        c.Previous.OpCode = OpCodes.Bgt_Un_S;

        // Debug to show the insertion worked
        Logger.LogInfo(il.ToString());
    }


    private static void IL_JumpPatch(ILContext il)
    {
        // Create a cursor to trace to the IL that we want to make modifications at
        ILCursor c = new(il);

        // Find this IL which is where we want to place our cursor BEFORE
        c.GotoNext(
            x => x.MatchLdarg(0), // Match: 'IL_00b5: ldarg.0'
            x => x.MatchLdcR4(0.0f), // Match 'IL_00b6: ldc.r4 0.0'
            x => x.MatchStfld<PlayerControllerB>(nameof(PlayerControllerB.playerSlidingTimer))
        );

        // With the position found, we can insert C# logic directly and it will get compiled to IL properly
        // First, we need to load 'this' onto the stack so we can perform this task appropriately
        c.Emit(OpCodes.Ldarg_0); // Load arg 0, 'this', onto the stack
        // Now we can inject our C# code
        c.EmitDelegate<Action<PlayerControllerB>>((self) =>
            {
                Logger.LogInfo("Hello from C# code in IL!");

                if (self.isSprinting)
                    self.jumpForce = 30f;
                else
                    self.jumpForce = 13f; // this is the default value of jumpForce
            }
        );

        // Debug to show the insertion worked
        Logger.LogInfo(il.ToString());
    }


    private static bool EmotePatch(On.GameNetcodeStuff.PlayerControllerB.orig_CheckConditionsForEmote orig, GameNetcodeStuff.PlayerControllerB self)
    {
        bool originalResult = orig(self);
        Logger.LogInfo("Would emoting be normally allowed? " + originalResult);

        return true;
    }


    private static void MyPatch(On.GameNetcodeStuff.PlayerControllerB.orig_Update orig, GameNetcodeStuff.PlayerControllerB self) {
        // Original code behavior
        orig(self);

        // Check if the player is exhausted
        if (self.isExhausted)
            // Kill the player >:)
            self.KillPlayer(Vector3.zero);
    }


    internal static void Unpatch() {
        Logger.LogDebug("Unpatching...");

        Logger.LogDebug("Finished unpatching!");
    }
}
