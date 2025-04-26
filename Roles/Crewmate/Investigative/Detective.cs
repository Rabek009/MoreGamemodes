// namespace MoreGamemodes
// {
//     public class Detective : CustomRole
//     {
//         public Detective(PlayerControl player)
//         {
//             Role = CustomRoles.Detective;
//             BaseRole = BaseRoles.Crewmate;
//             Player = player;
//             Utils.SetupRoleInfo(this);
//             AbilityUses = -1;
//             TimeSinceLastFootprints = 0f;
//         }

//         public float TimeSinceLastFootprints;

//         public static OptionItem Chance;
//         public static OptionItem Count;
//         public static OptionItem FootprintsDuration;
//         public static OptionItem FootprintsInterval;
//         public static OptionItem AnonymousFootprints;
//         public static void SetupOptionItem()
//         {
//             Chance = RoleOptionItem.Create(100400, CustomRoles.Detective, TabGroup.CrewmateRoles, false);
//             Count = IntegerOptionItem.Create(100401, "Max", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
//                 .SetParent(Chance);
//             FootprintsDuration = FloatOptionItem.Create(100402, "Footprints duration", new(1f, 30f, 0.5f), 5f, TabGroup.CrewmateRoles, false)
//                 .SetParent(Chance)
//                 .SetValueFormat(OptionFormat.Seconds);
//             FootprintsInterval = FloatOptionItem.Create(100403, "Footprints interval", new(0.1f, 2.5f, 0.1f), 0.5f, TabGroup.CrewmateRoles, false)
//                 .SetParent(Chance)
//                 .SetValueFormat(OptionFormat.Seconds);
//             AnonymousFootprints = BooleanOptionItem.Create(100404, "Anonymous footprints", false, TabGroup.CrewmateRoles, false)
//                 .SetParent(Chance);
//             Options.RolesChance[CustomRoles.Detective] = Chance;
//             Options.RolesCount[CustomRoles.Detective] = Count;
//         }
//     }
// }