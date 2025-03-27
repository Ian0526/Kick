using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using System.Reflection;
using Photon.Realtime;



public class KickSpawnGUI : MonoBehaviourPunCallbacks
{

    private bool showKickGUI = false;
    private bool isTextFieldFocused = false;
    private Dictionary<PlayerAvatar, Coroutine> ballStateCoroutines = new Dictionary<PlayerAvatar, Coroutine>();

    private float maxNameWidth = 0f;
    private float baseMenuWidth = 600f;
    private GUIStyle textStyle;

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!isTextFieldFocused && Keyboard.current.f2Key.wasPressedThisFrame)
        {
            showKickGUI = !showKickGUI;
            Debug.Log($"[Kick] Kick GUI toggled: {(showKickGUI ? "Shown" : "Hidden")}");
        }
    }

    private float GetMaxNameWidth()
    {
        float maxWidth = 0f;
        foreach (PlayerAvatar p in GameDirector.instance.PlayerList)
        {
            string playerName = GetPlayerName(p);
            if (!string.IsNullOrEmpty(playerName))
            {
                Vector2 textSize = textStyle.CalcSize(new GUIContent($"Kick {playerName}"));
                maxWidth = Mathf.Max(maxWidth, textSize.x);
            }
        }
        return maxWidth;
    }

    private void OnGUI()
    {
        if (!showKickGUI) return;

        if (textStyle == null)
            textStyle = new GUIStyle(GUI.skin.button) { fontSize = 14 };

        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, 1200), "Kick Menu", GUI.skin.window);
        GUILayout.Label("Select an action for a player:");

        foreach (PlayerAvatar p in GameDirector.instance.PlayerList)
        {
            string playerName = GetPlayerName(p);

            string killLabel = $"Kill {playerName}";
            string ballLabel = ballStateCoroutines.ContainsKey(p) ? $"Stop Ball {playerName}" : $"Force Ball {playerName}";
            string kickLabel = $"Kick {playerName}";

            float killWidth = textStyle.CalcSize(new GUIContent(killLabel)).x + 20f;
            float ballWidth = textStyle.CalcSize(new GUIContent(ballLabel)).x + 20f;
            float kickWidth = textStyle.CalcSize(new GUIContent(kickLabel)).x + 20f;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(killLabel, GUILayout.Width(killWidth))) AttemptKill(p);

            if (ballStateCoroutines.ContainsKey(p))
            {
                if (GUILayout.Button(ballLabel, GUILayout.Width(ballWidth))) StopBallState(p);
            }
            else
            {
                if (GUILayout.Button(ballLabel, GUILayout.Width(ballWidth))) StartBallState(p);
            }

            if (GUILayout.Button(kickLabel, GUILayout.Width(kickWidth))) MakePlayersClientSad(p.photonView.Owner);

            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }

    private void AttemptKill(PlayerAvatar targetAvatar)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[Kick] You are not the MasterClient, cannot execute kill!");
            return;
        }

        if (targetAvatar == null)
        {
            Debug.LogError("[Kick] Target avatar not found.");
            return;
        }

        targetAvatar.photonView.RPC("PlayerDeathRPC", RpcTarget.All, 0);
        targetAvatar.photonView.RPC("SetDisabledRPC", RpcTarget.All);
    }

    private void StartBallState(PlayerAvatar targetAvatar)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[Kick] You are not the MasterClient, cannot force ball state!");
            return;
        }

        if (targetAvatar == null)
        {
            Debug.LogError("[Kick] Target avatar not found.");
            return;
        }

        if (ballStateCoroutines.ContainsKey(targetAvatar))
        {
            Debug.Log($"[Kick] Ball state already active for {GetPlayerName(targetAvatar)}");
            return;
        }

        FieldInfo field = targetAvatar.tumble.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (field != null)
        {
            PhotonView pv = field.GetValue(targetAvatar.tumble) as PhotonView;
            Coroutine c = StartCoroutine(ContinuouslyForceBallState(targetAvatar, pv));
            ballStateCoroutines[targetAvatar] = c;
            Debug.Log($"[Kick] Started ball state for {GetPlayerName(targetAvatar)}");
        }
        else
        {
            Debug.LogError("[Kick] PhotonView field not found on target.");
        }
    }

    private void StopBallState(PlayerAvatar targetAvatar)
    {
        if (ballStateCoroutines.ContainsKey(targetAvatar))
        {
            StopCoroutine(ballStateCoroutines[targetAvatar]);
            ballStateCoroutines.Remove(targetAvatar);
            Debug.Log($"[Kick] Stopped ball state for {GetPlayerName(targetAvatar)}");
        }
    }

    private IEnumerator ContinuouslyForceBallState(PlayerAvatar targetAvatar, PhotonView photonView)
    {
        yield return new WaitForSeconds(0.2f);
        while (true)
        {
            if (!ballStateCoroutines.ContainsKey(targetAvatar) || targetAvatar == null || targetAvatar.tumble == null)
            {
                Debug.Log($"[Kick] Target avatar {GetPlayerName(targetAvatar)} no longer found. Stopping ball state.");
                yield break;
            }

            photonView.RPC("TumbleRequestRPC", RpcTarget.All, true, false);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void MakePlayersClientSad(Player targetPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[Kick] You are not the MasterClient, cannot perform this action!");
            return;
        }

        LevelGenerator lg = FindObjectOfType<LevelGenerator>();

        if (lg == null)
        {
            Debug.LogError("[KickExploit] Could not find LevelGenerator PhotonView.");
            return;
        }

        // https://media.tenor.com/CRXCax5A5vgAAAAM/kots-straal.gif
        // Forgive me God, this is awful
        // I tried using Reflection to access SendOperation to forcefully send
        // a SetMasterClient change (causes a quit), but Photon blocks this (anything over 200).
        // I tried Closing the user's connection, but there are loose ends.
        // I tried manipulating various RPC methods to find which may trigger the client to thinking
        // the game has ended, but this also caused glitched states with no DC.

        // I've only been left with one option
        const int spamCount = 5000;
        for (int i = 0; i < spamCount; i++)
        {
            int fakeItemId = UnityEngine.Random.Range(0, 9999);
            lg.PhotonView.RPC("ItemSetup", targetPlayer);
            lg.PhotonView.RPC("NavMeshSetupRPC", targetPlayer);
        }
    }


    private string GetPlayerName(PlayerAvatar targetAvatar)
    {
        if (targetAvatar == null)
            return "Unknown";

        FieldInfo field = typeof(PlayerAvatar).GetField("playerName", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        return field != null ? (string)field.GetValue(targetAvatar) : "Unknown";
    }
}
