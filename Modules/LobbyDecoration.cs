using TMPro;
using UnityEngine;
using HarmonyLib;
using InnerNet;

namespace MoreGamemodes;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public class LobbyStartPatch
{
    private static GameObject Paint;
    private static GameObject Decorations;
    private static bool FirstDecorationsLoad = true;

    private static bool IsInLobby = AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined;
    public static void Postfix()
    {
        float waitTime = 0f;
        if (FirstDecorationsLoad)
            waitTime = 0.25f;
        else
            waitTime = 0.05f;
        if (Main.EnableCustomDecorations.Value)
        {
            _ = new LateTask(() =>
            {
                var Dropship = GameObject.Find("SmallBox");
                if (Dropship != null)
                {
                    Decorations = Object.Instantiate(Dropship, Object.FindAnyObjectByType<LobbyBehaviour>().transform);
                    Decorations.name = "Lobby_Decorations";
                    Decorations.transform.DestroyChildren();
                    Object.Destroy(Decorations.GetComponent<PolygonCollider2D>());
                    Decorations.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("MoreGamemodes.Resources.MoreGamemodes-Logo.png", 60f);
                    Decorations.transform.SetSiblingIndex(1);
                    Decorations.transform.localPosition = new(0.05f, 0.8334f);
                    FirstDecorationsLoad = false;
                }
            }, waitTime, "Dropship Decorations");
        }
        _ = new LateTask(() =>
        {
            if (!IsInLobby || Paint != null) return;
            
            var LeftBox = GameObject.Find("Leftbox");
            if (LeftBox != null)
            {
                Paint = Object.Instantiate(LeftBox, LeftBox.transform.parent.transform);
                Paint.name = "Lobby Paint";
                Paint.transform.localPosition = new Vector3(0.042f, -2.59f, -10.5f);
                SpriteRenderer renderer = Paint.GetComponent<SpriteRenderer>();
                renderer.sprite = Utils.LoadSprite("TOHE.Resources.Images.LobbyPaint.png", 290f);
            }
        }, 3f, "LobbyPaint");
    }
}