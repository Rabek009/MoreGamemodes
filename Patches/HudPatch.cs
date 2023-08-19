using HarmonyLib;
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
                    if (PlayerControl.LocalPlayer.HasBomb() && !PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        __instance.KillButton.ToggleVisible(true);
                        __instance.KillButton.OverrideText("Bomb");
                        if (PlayerControl.LocalPlayer.GetClosestPlayer().HasBomb())
                            __instance.KillButton.SetTarget(null);
                    }
                    else
                    {
                        __instance.KillButton.ToggleVisible(false);
                        __instance.KillButton.SetTarget(null);
                    }
                    break;
                case Gamemodes.RandomItems:
                    if (!Main.GameStarted || player.GetItem() == Items.None || player.GetItem() == Items.Stop)
                    {
                        __instance.PetButton.SetDisabled();
                        __instance.PetButton.ToggleVisible(false);
                    }
                    if (__instance.PetButton.isActiveAndEnabled)
                    {
                        switch (player.GetItem())
                        {
                            case Items.TimeSlower:
                                __instance.PetButton.OverrideText("Slow Time");
                                break;
                            case Items.Knowledge:
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
                            case Items.Radar:
                                __instance.PetButton.OverrideText("Radar");
                                break;
                            case Items.Swap:
                                __instance.PetButton.OverrideText("Swap");
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
                            case Items.Bomb:
                                __instance.PetButton.OverrideText("Detonated");
                                break;
                            case Items.Trap:
                                __instance.PetButton.OverrideText("Place");
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
                            case Items.Newsletter:
                                __instance.PetButton.OverrideText("Get Info");
                                break;
                            case Items.Compass:
                                __instance.PetButton.OverrideText("Track");
                                break;
                        }
                    }
                    if (Main.HackTimer > 0f && (!Main.Impostors.Contains(PlayerControl.LocalPlayer.PlayerId) || Options.HackAffectsImpostors.GetBool()))
                    {
                        __instance.PetButton.SetDisabled();
                        __instance.PetButton.ToggleVisible(false);
                        __instance.ImpostorVentButton.SetDisabled();
                        __instance.ImpostorVentButton.ToggleVisible(false);
                        __instance.ReportButton.SetDisabled();
                        __instance.ReportButton.ToggleVisible(false);
                        __instance.SabotageButton.SetDisabled();
                        __instance.SabotageButton.ToggleVisible(false);
                        __instance.KillButton.SetDisabled();
                        __instance.KillButton.ToggleVisible(false);
                        __instance.AbilityButton.SetDisabled();
                        __instance.AbilityButton.ToggleVisible(false);
                        __instance.AdminButton.SetDisabled();
                        __instance.AdminButton.ToggleVisible(false);
                    }
                    break;
                case Gamemodes.BattleRoyale:
                    __instance.ImpostorVentButton.SetDisabled();
                    __instance.ImpostorVentButton.ToggleVisible(false);
                    __instance.ReportButton.SetDisabled();
                    __instance.ReportButton.ToggleVisible(false);
                    __instance.SabotageButton.SetDisabled();
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.KillButton.OverrideText("Attack");
                    break;
                case Gamemodes.Speedrun:
                    __instance.ReportButton.SetDisabled();
                    __instance.ReportButton.ToggleVisible(false);
                    if (Options.CurrentBodyType == SpeedrunBodyTypes.Ghost)
                    {
                        __instance.AbilityButton.SetDisabled();
                        __instance.AbilityButton.ToggleVisible(false);
                    }
                    break;
                case Gamemodes.PaintBattle:
                    __instance.ReportButton.SetDisabled();
                    __instance.ReportButton.ToggleVisible(false);
                    __instance.PetButton.OverrideText("Paint");
                    __instance.AbilityButton.OverrideText("Remaining Time");
                    if (Main.PaintTime == 0f)
                    {
                        __instance.PetButton.SetDisabled();
                        __instance.PetButton.ToggleVisible(false);
                        __instance.AbilityButton.SetDisabled();
                        __instance.AbilityButton.ToggleVisible(false);
                    }
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
    class SetTaskTextPatch
    {
        public static void Postfix(TaskPanelBehaviour __instance, [HarmonyArgument(0)] string str)
        {
            switch (Options.CurrentGamemode)
            {
                case Gamemodes.HideAndSeek:
                    if (Main.Impostors.Contains(PlayerControl.LocalPlayer.PlayerId))
                        __instance.taskText.text = Utils.ColorString(Color.red, "Seeker:\nKill all hiders.");
                    else if (!PlayerControl.LocalPlayer.Data.IsDead)
                        __instance.taskText.text = "Hider:\nDo your tasks and survive.\n\n" + str;
                    break;
                case Gamemodes.ShiftAndSeek:
                    if (Main.Impostors.Contains(PlayerControl.LocalPlayer.PlayerId))
                        __instance.taskText.text = Utils.ColorString(Color.red, "Shifter:\nShift into your victim.");
                    else if (!PlayerControl.LocalPlayer.Data.IsDead)
                        __instance.taskText.text = "Hider:\nHide in vents and do tasks.\n\n" + str;
                    break;
                case Gamemodes.BombTag:
                    if (PlayerControl.LocalPlayer.Data.IsDead)
                        __instance.taskText.text = Utils.ColorString(Color.red, "You're dead. Enjoy the chaos.");
                    else if (PlayerControl.LocalPlayer.HasBomb())
                        __instance.taskText.text = Utils.ColorString(Color.black, "You have bomb!\nGive your bomb away.");
                    else
                        __instance.taskText.text = Utils.ColorString(Color.green, "You haven't bomb!\nDon't get bomb.");
                    break;
                case Gamemodes.BattleRoyale:
                    if (PlayerControl.LocalPlayer.Data.IsDead)
                        __instance.taskText.text = Utils.ColorString(Color.red, "You're dead. Enjoy the chaos.");
                    else
                        __instance.taskText.text = Utils.ColorString(Color.red, "Player:\nKill everyone and survive.");
                    break;
                case Gamemodes.Speedrun:
                    __instance.taskText.text = Utils.ColorString(Color.yellow, "Speedrunner:\nFinish tasks as fast as you can.\n\n") + str;
                    break;
                case Gamemodes.PaintBattle:
                    __instance.taskText.text = Utils.ColorString(Color.gray, "Painter:\nPaint something in theme.\nThe theme is " + Main.Theme + ".");
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
    class ShowNormalMapPatch
    {
        public static void Postfix(MapBehaviour __instance)
        {
            if (Options.CurrentGamemode == Gamemodes.PaintBattle)
                __instance.Close();
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    class ShowSabotageMapPatch
    {
        public static void Postfix(MapBehaviour __instance)
        {
            if ((Options.CurrentGamemode == Gamemodes.HideAndSeek && !Options.HnSImpostorsCanCloseDoors.GetBool()) || (Options.CurrentGamemode == Gamemodes.ShiftAndSeek && !Options.SnSImpostorsCanCloseDoors.GetBool()) ||
                Options.CurrentGamemode == Gamemodes.BombTag || (Options.CurrentGamemode == Gamemodes.RandomItems && Main.HackTimer > 0f && Options.HackAffectsImpostors.GetBool()) ||Options.CurrentGamemode == Gamemodes.BattleRoyale)
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

    [HarmonyPatch(typeof(KillOverlay), nameof(KillOverlay.CoShowOne))]
    class ShowKillAnimationPatch
    {
        public static bool Prefix(KillOverlay __instance, [HarmonyArgument(0)] OverlayAnimation anim)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            return !Main.IsCreatingBody;
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.SetMovement))]
    class SetMovementPatch
    {
        public static bool Prefix(KillAnimation __instance, [HarmonyArgument(0)] GameData.PlayerInfo killer, [HarmonyArgument(1)] GameData.PlayerInfo victim)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            return !Main.IsCreatingBody;
        }
    }
}