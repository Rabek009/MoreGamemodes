using HarmonyLib;
using AmongUs.GameOptions;
using UnityEngine;

namespace MoreGamemodes
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerPatch
    {
        public static void Postfix(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (player == null) return;
            if (!Main.GameStarted) return;
            switch (Options.CurrentGamemode)
            {
                case Gamemodes.HideAndSeek:
                    if (Main.Impostors.Contains(player.PlayerId))
                    {
                        if (!Options.HnSImpostorsCanVent.GetBool())
                        {
                            __instance.ImpostorVentButton.SetDisabled();
                            __instance.ImpostorVentButton.ToggleVisible(false);
                        }
                        if (!Options.HnSImpostorsCanCloseDoors.GetBool())
                        {
                            __instance.SabotageButton.SetDisabled();
                            __instance.SabotageButton.ToggleVisible(false);
                        }
                    }
                    __instance.ReportButton.SetDisabled();
                    __instance.ReportButton.ToggleVisible(false);
                    break;

                case Gamemodes.ShiftAndSeek:
                    if (Main.Impostors.Contains(player.PlayerId))
                    {
                        if (!Options.SnSImpostorsCanVent.GetBool())
                        {
                            __instance.ImpostorVentButton.SetDisabled();
                            __instance.ImpostorVentButton.ToggleVisible(false);
                        }
                        if (!Options.SnSImpostorsCanCloseDoors.GetBool())
                        {
                            __instance.SabotageButton.SetDisabled();
                            __instance.SabotageButton.ToggleVisible(false);
                        }
                    }
                    __instance.ReportButton.SetDisabled();
                    __instance.ReportButton.ToggleVisible(false);
                    break;
                case Gamemodes.BombTag:
                    __instance.ImpostorVentButton.SetDisabled();
                    __instance.ImpostorVentButton.ToggleVisible(false);
                    __instance.ReportButton.SetDisabled();
                    __instance.ReportButton.ToggleVisible(false);
                    __instance.SabotageButton.SetDisabled();
                    __instance.SabotageButton.ToggleVisible(false);
                    if (!PlayerControl.LocalPlayer.Data.IsDead)
                        __instance.AbilityButton.OverrideText("Explosion");
                    if (PlayerControl.LocalPlayer.HasBomb())
                    {
                        __instance.KillButton.ToggleVisible(true);
                        __instance.KillButton.OverrideText("Bomb");
                    }
                    else
                    {
                        __instance.ReportButton.SetDisabled();
                        __instance.KillButton.ToggleVisible(false);
                    }
                    if (player.Data.IsDead)
                    {
                        __instance.KillButton.SetDisabled();
                        __instance.KillButton.ToggleVisible(false);
                    }
                    break;
                case Gamemodes.RandomItems:
                    if (player.Data.Role.Role == RoleTypes.GuardianAngel)
                    {
                        if (Main.HackTimer > 1f)
                        {
                            __instance.AbilityButton.SetDisabled();
                            __instance.AbilityButton.ToggleVisible(false);
                        }
                    }
                    if (player.Data.Role.Role == RoleTypes.ImpostorGhost)
                    {
                        if (Main.HackTimer > 1f && Options.HackAffectsImpostors.GetBool())
                        {
                            __instance.SabotageButton.SetDisabled();
                            __instance.SabotageButton.ToggleVisible(false);
                        }
                    }
                    if (!player.Data.Role.IsSimpleRole)
                    {
                        if (Main.HackTimer > 1f && (Main.Impostors.Contains(player.PlayerId) == false || Options.HackAffectsImpostors.GetBool()))
                        {
                            __instance.AbilityButton.SetDisabled();
                            __instance.AbilityButton.ToggleVisible(false);
                        }
                    }
                    if (!Main.GameStarted || player.GetItem() == Items.None || player.GetItem() == Items.Stop || Main.HackTimer > 1f)
                    {
                        __instance.PetButton.SetDisabled();
                        __instance.PetButton.ToggleVisible(false);
                    }
                    if (Main.HackTimer > 1f && (Main.Impostors.Contains(player.PlayerId) == false || Options.HackAffectsImpostors.GetBool()))
                    {
                        __instance.AbilityButton.SetDisabled();
                        __instance.AbilityButton.ToggleVisible(false);
                        __instance.ImpostorVentButton.SetDisabled();
                        __instance.ImpostorVentButton.ToggleVisible(false);
                        __instance.KillButton.SetDisabled();
                        __instance.KillButton.ToggleVisible(false);
                        __instance.ReportButton.SetDisabled();
                        __instance.ReportButton.ToggleVisible(false);
                        __instance.SabotageButton.SetDisabled();
                        __instance.SabotageButton.ToggleVisible(false);
                        __instance.UseButton.SetDisabled();
                        __instance.UseButton.ToggleVisible(false);
                    }
                    if (__instance.PetButton.isActiveAndEnabled)
                    {
                        switch (player.GetItem())
                        {
                            case Items.TimeSlower:
                                __instance.PetButton.OverrideText("Slow Time");
                                break;
                            case Items.Knowlegde:
                                __instance.PetButton.OverrideText("Reveal");
                                break;
                            case Items.Shield:
                                __instance.PetButton.OverrideText("Shield");
                                break;
                            case Items.Gun:
                                __instance.PetButton.OverrideText("Kill");
                                break;
                            case Items.Illusion:
                                __instance.PetButton.OverrideText("Manipulate");
                                break;
                            case Items.TimeSpeeder:
                                __instance.PetButton.OverrideText("Speed Time");
                                break;
                            case Items.Flash:
                                __instance.PetButton.OverrideText("Flash");
                                break;
                            case Items.Hack:
                                __instance.PetButton.OverrideText("Hack");
                                break;
                            case Items.Camouflage:
                                __instance.PetButton.OverrideText("Camouflage");
                                break;
                            case Items.MultiTeleport:
                                __instance.PetButton.OverrideText("Multi TP");
                                break;
                            case Items.Teleport:
                                __instance.PetButton.OverrideText("Teleport");
                                break;
                            case Items.Button:
                                __instance.PetButton.OverrideText("Emergency");
                                break;
                            case Items.Finder:
                                __instance.PetButton.OverrideText("Find");
                                break;
                            case Items.Rope:
                                __instance.PetButton.OverrideText("Pull");
                                break;
                        }
                    }
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    class ShowSabotageMapPatch
    {
        public static void Postfix(MapBehaviour __instance)
        {
            if ((Options.CurrentGamemode == Gamemodes.HideAndSeek && !Options.HnSImpostorsCanCloseDoors.GetBool()) || (Options.CurrentGamemode == Gamemodes.ShiftAndSeek && !Options.SnSImpostorsCanCloseDoors.GetBool()) ||
                Options.CurrentGamemode == Gamemodes.BombTag)
            {
                __instance.Close();
                __instance.ShowNormalMap();
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ToggleHighlight))]
    class ToggleHighlightPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (Options.CurrentGamemode == Gamemodes.BombTag && PlayerControl.LocalPlayer.HasBomb())
            {
                __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Color.gray);
            }
        }
    }
}