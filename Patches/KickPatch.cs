﻿using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(ShopManager), "Awake")]
public static class KickPatch
{
    private static void Postfix(ShopManager __instance)
    {
        if (__instance == null)
        {
            Debug.LogError("[KickPatch] ShopManager instance is null! Cannot patch.");
            return;
        }

        if (__instance.gameObject.GetComponent<KickGUI>() == null)
        {
            __instance.gameObject.AddComponent<KickGUI>();
            if (KickPlugin.componentAddedOnce == false)
            {
                Debug.Log("[KickPatch] Kick initialized. (This may pop up several times as ShopManager instances are destroyed, this is not a bug).");
                KickPlugin.componentAddedOnce = true;
            }
        }
    }
}