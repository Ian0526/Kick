using System.Collections;
using System.Reflection;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Kick.Util
{
    public static class PlayerUtils
    {

        private static readonly string[] KickMessages =
        {
            "i am an incel and deserve to be kicked",
            "something smells like an electrical fire",
            "i have a low i q",
            "i wonder why i spend so much time ruining fun for others",
            "everyone i'm so sorry my prefrontal cortex hasn't fully developed yet",
            "i have no friends",
            "five four and a half three three and a half two two and a half one one and a half one and a quarter one and an eighth"
        };

        public static void Kill(PlayerAvatar avatar)
        {
            if (avatar == null) return;
            avatar.photonView.RPC("PlayerDeathRPC", RpcTarget.All, 0);
            avatar.photonView.RPC("SetDisabledRPC", RpcTarget.All);
        }

        public static void KickPlayer(Player target)
        {
            var levelGen = Object.FindObjectOfType<LevelGenerator>();
            if (levelGen == null)
            {
                Debug.LogError("[Kick] LevelGenerator not found.");
                return;
            }
            for (int i = 0; i < 5000; i++)
            {
                levelGen.PhotonView.RPC("ItemSetup", target);
                levelGen.PhotonView.RPC("NavMeshSetupRPC", target);
            }
        }

        public static IEnumerator KickWithMessage(Player target)
        {
            PlayerAvatar avatar = GameDirector.instance?.PlayerList?.Find(p => p?.photonView?.Owner == target);
            if (avatar == null)
            {
                Debug.LogError("[Kick] Could not find avatar to send funny message before kick.");
                yield break;
            }

            string message = $"{GetRandomKickMessage()}";
            avatar.photonView.RPC("ChatMessageSendRPC", RpcTarget.All, message, false);

            yield return new WaitForSeconds(5f);

            KickPlayer(target);
        }

        public static IEnumerator BallStateCoroutine(PlayerAvatar avatar)
        {
            yield return new WaitForSeconds(0.2f);
            PhotonView pv = GetTumblePhotonView(avatar);
            while (avatar != null && avatar.tumble != null && Kick.Data.KickStorage.BallCoroutines.ContainsKey(avatar))
            {
                pv.RPC("TumbleRequestRPC", RpcTarget.All, true, false);
                yield return new WaitForSeconds(0.2f);
            }
        }

        public static string GetPlayerName(PlayerAvatar avatar)
        {
            if (avatar == null) return "Unknown";
            var field = typeof(PlayerAvatar).GetField("playerName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(avatar) as string ?? "Unknown";
        }

        public static PhotonView GetTumblePhotonView(PlayerAvatar avatar)
        {
            var field = avatar.tumble?.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(avatar.tumble) as PhotonView;
        }

        private static string GetRandomKickMessage()
        {
            int index = Random.Range(0, KickMessages.Length);
            return KickMessages[index];
        }
    }
}
