using Photon.Pun;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Kick.Util;

public class KickGUI : MonoBehaviourPunCallbacks
{
    private bool showGUI;
    private bool isTextFieldFocused;
    private readonly Dictionary<PlayerAvatar, Coroutine> ballCoroutines = new Dictionary<PlayerAvatar, Coroutine>();
    private GUIStyle buttonStyle;

    private void Update()
    {
        if (!isTextFieldFocused && Keyboard.current.f2Key.wasPressedThisFrame)
        {
            showGUI = !showGUI;
            Debug.Log($"[Kick] GUI toggled: {(showGUI ? "Shown" : "Hidden")}");
        }
    }

    private void OnGUI()
    {
        if (!showGUI) return;

        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14 };

        GUILayout.BeginArea(new Rect(10f, 10f, Screen.width - 20, 1200f), "Kick Menu", GUI.skin.window);
        GUILayout.Label("Select an action for a player:");

        foreach (var player in GameDirector.instance.PlayerList)
        {
            string playerName = PlayerUtils.GetPlayerName(player);

            GUILayout.BeginHorizontal();
            DrawButton($"Kill {playerName}", () => PlayerUtils.Kill(player));

            string ballLabel = Kick.Data.KickStorage.BallCoroutines.ContainsKey(player) ? $"Stop Ball {playerName}" : $"Force Ball {playerName}";
            DrawButton(ballLabel, () => PlayerUtils.BallStateCoroutine(player));

            DrawButton($"Kick {playerName}", () => PlayerUtils.KickPlayer(player.photonView.Owner));
            DrawButton($"Funny Kick {playerName}", () => StartCoroutine(PlayerUtils.KickWithMessage(player.photonView.Owner)));
            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }

    private void DrawButton(string label, System.Action action)
    {
        float width = buttonStyle.CalcSize(new GUIContent(label)).x + 20f;
        if (GUILayout.Button(label, GUILayout.Width(width)))
            action.Invoke();
    }
}