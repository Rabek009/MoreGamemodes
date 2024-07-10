using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace MoreGamemodes;

public class DevUser(string code = "", string color = "null", string userType = "null", string tag = "null", bool isUp = false, bool isDev = false, bool deBug = false, bool colorCmd = false, bool nameCmd = false, string upName = "未认证用户")
{
    public string Code { get; set; } = code;
    public string Color { get; set; } = color;
    public string UserType { get; set; } = userType;
    public string Tag { get; set; } = tag;
    public bool IsUp { get; set; } = isUp;
    public bool IsDev { get; set; } = isDev;
    public bool DeBug { get; set; } = deBug;
    public bool ColorCmd { get; set; } = colorCmd;
    public bool NameCmd { get; set; } = nameCmd;
    public string UpName { get; set; } = upName;

    public bool HasTag() => Tag != "null";
    //public string GetTag() => Color == "null" ? $"<size=1.2>{Tag}</size>\r\n" : $"<color={Color}><size=1.2>{(Tag == "#Dev" ? Translator.GetString("Developer") : Tag)}</size></color>\r\n";
    public string GetTag()
    {
        string tagColorFilePath = @$"./TOHE-DATA/Tags/SPONSOR_TAGS/{Code}.txt";

        if (Color == "null" || Color == string.Empty) return $"<size=1.2>{Tag}</size>\r\n";
        var startColor = Color.TrimStart('#');

        if (File.Exists(tagColorFilePath))
        {
            var ColorCode = File.ReadAllText(tagColorFilePath);
            if (Utils.CheckColorHex(ColorCode)) startColor = ColorCode;
        }
        string t1;
        t1 = Tag == "#Dev" ? "Developer" : Tag;
        return $"<size=1.2><color=#{startColor}>{t1}</color></size>\r\r\n";
    }
    //public string GetTag() 
    //{
    //    string tagColorFilePath = @$"./TOHE-DATA/Tags/SPONSOR_TAGS/{Code}.txt";

    //    if (Color == "null" || Color == string.Empty) return $"<size=1.2>{Tag}</size>\r\n";
    //    var startColor = "FFFF00";
    //    var endColor = "FFFF00";
    //    var startColor1 = startColor;
    //    var endColor1 = endColor;
    //    if (Color.Split(",").Length == 1)
    //    {
    //        startColor1 = Color.Split(",")[0].TrimStart('#');
    //        endColor1 = startColor1;
    //    }
    //    else if (Color.Split(",").Length == 2)
    //    {
    //         startColor1 = Color.Split(",")[0].TrimStart('#');
    //         endColor1 = Color.Split(",")[1].TrimStart('#');
    //    }
    //    if (File.Exists(tagColorFilePath))
    //    {
    //        var ColorCode = File.ReadAllText(tagColorFilePath);
    //        if (ColorCode.Split(" ").Length == 2)
    //        {
    //            startColor = ColorCode.Split(" ")[0];
    //            endColor = ColorCode.Split(" ")[1];
    //        }
    //        else
    //        {
    //            startColor = startColor1;
    //            endColor = endColor1;
    //        }
    //    }
    //    else
    //    {
    //        startColor = startColor1;
    //        endColor = endColor1;
    //    }
       
    
}

public static class DevManager
{
    private readonly static DevUser DefaultDevUser = new();
    public readonly static List<DevUser> DevUserList = [];
    public static void Init()
    {
        // Dev
         DevUserList.Add(new(code:"wallstate#7631", color:"#00FF00", userType:"",tag:"#Dev", isUp:true, isDev:true, deBug:true, colorCmd:true, upName:"Rabok009"));
         DevUserList.Add(new(code:"exoticbike#7649", color:"#4831D4", userType:"s_cr",tag:"#Dev", isUp:true, isDev:true, deBug:true, colorCmd:true, upName:"GameTechGuides"));
        
    }
    public static bool IsDevUser(this string code) => DevUserList.Any(x => x.Code == code);
    public static DevUser GetDevUser(this string code) => code.IsDevUser() ? DevUserList.Find(x => x.Code == code) : DefaultDevUser;
    public static string GetUserType(this DevUser user)
    {
        string rolename = "Crewmate";

        if (user.UserType != "null" && user.UserType != string.Empty)
        {
            switch (user.UserType)
            {
                case "s_cr":
                    rolename = "<color=#ff0000>Contributor</color>";
                    break;
                case "s_bo":
                    rolename = "<color=#7f00ff>Booster</color>";
                    break;

                default:
                    if (user.UserType.StartsWith("s_"))
                    {
                        rolename = "<color=#ffff00>Sponsor</color>";
                    }
                    else if (user.UserType.StartsWith("t_"))
                    {
                        rolename = "<color=#00ffff>Translator</color>";
                    }
                    break;
            }
        }

        return rolename;
    }
}