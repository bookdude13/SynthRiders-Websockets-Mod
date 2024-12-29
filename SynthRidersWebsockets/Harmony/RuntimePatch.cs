using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;

namespace SynthRidersWebsockets.Harmony
{
    internal class RuntimePatch
    {
        public static void PatchAll()
        {
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("dev.kk964.websockets");
            harmony.PatchAll();

            foreach (var method in harmony.GetPatchedMethods())
            {
                WebsocketMod.Instance.LoggerInstance.Msg($"[Websocket] Successfully patched \"{method.Name}\"");
            }
        }
    }

    public class ReflectionUtils
    {
        public static object GetValue(object obj, string field)
        {
            if (obj == null) return null;
            if (field == null) return null;
            Type type = obj.GetType();
            FieldInfo info = type.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            if (info == null) return null;
            return info.GetValue(obj);
        }
    }
}

[HarmonyPatch(typeof(GameControlManager), "Awake")]
public class GameControlManagerPatch
{
    [HarmonyPostfix]
    public static void PostFix()
    {
        SynthRidersWebsockets.WebsocketMod.Instance.GameManagerInit();
    }
}

[HarmonyPatch(typeof(GameControlManager), "ReturnToMenu")]
public class GameControlManagerReturnToMenuPatch
{
    [HarmonyPostfix]
    public static void PostFix()
    {
        SynthRidersWebsockets.WebsocketMod.Instance.EmitReturnToMenuEvent();
    }
}

// For capturing the player's current health, as the original LifeBarHelper.GetScalePercent()
// doesn't seem to return the correct value anymore (always zero)
[HarmonyPatch(typeof(Game_ScoreManager), "UpdateHealthBar")]
public class GameScoreManagerUpdateHealthBarPatch
{
    [HarmonyPostfix]
    public static void PostFix(float health)
    {
        SynthRidersWebsockets.WebsocketMod.Instance.UpdateHealth(health);
    }
}
