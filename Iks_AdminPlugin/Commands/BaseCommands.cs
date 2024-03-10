using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Iks_Admin_Api;
using Microsoft.Extensions.Localization;

namespace Iks_Admin.Commands;

public class BaseCommands
{
    AdminApi AdminApi;
    PluginConfig Config;
    IStringLocalizer Localizer;
    private BaseFuntions bf;

    public List<CCSPlayerController> HidenPlayers = new();
    

    public BaseCommands(IStringLocalizer Localizer, PluginConfig config, AdminApi api)
    {
        AdminApi = api;
        Config = config;
        this.Localizer = Localizer;
        bf = new BaseFuntions(Localizer);
    }

    public void OnAdminaddCmd(CCSPlayerController? controller, CommandInfo info)
    {
        if (!AdminApi.HaveFlag(controller, "adminManage"))
        {
            AdminApi.HaveNotAccess(info);
            return;
        }
        var args = XHelper.GetArgsFromCommandLine(info.GetCommandString);
        if (args.Count < 6)
        {
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_adminadd <sid> <name> <flags/-> <immunity/-1(for group)> <group_id/-1> <time> <server_id/ - (ALL SERVERS)>");
            return;
        }

        string sid = args[0];
        if (XHelper.GetIdentityType($"#{sid}") != "sid")
        {
            AdminApi.SendMessage(info, Localizer["NOTIFY_IncorrectSid"]);
        }

        string adminsid = controller == null ? "CONSOLE" : controller.SteamID.ToString();
        string name = args[1];
        string flags = args[2] == "-" ? "" : args[2];
        int immunity = Int32.Parse(args[3]);
        int group_id = Int32.Parse(args[4]);
        int time = Int32.Parse(args[5]);
        string server_id = args.Count < 7 ? Iks_Admin.ConfigNow.ServerId : args[6] == "-" ? "" : args[6];
        long end = time == 0 ? 0 : DateTimeOffset.UtcNow.ToUnixTimeSeconds() + time * 60;

        Task.Run(async () =>
        {
            await AdminApi.AddAdmin(adminsid, sid, name, flags, immunity, group_id, end, server_id);

        });
    }
    
    public void OnAdmindelCmd(CCSPlayerController? controller, CommandInfo info)
    {
        if (!AdminApi.HaveFlag(controller, "adminManage"))
        {
            AdminApi.HaveNotAccess(info);
            return;
        }
        var args = XHelper.GetArgsFromCommandLine(info.GetCommandString);
        if (args.Count < 1)
        {
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_admindel <sid>");
            return;
        }

        string sid = args[0];
        if (XHelper.GetIdentityType($"#{sid}") != "sid")
        {
            AdminApi.SendMessage(info, Localizer["NOTIFY_IncorrectSid"]);
        }

        string adminsid = controller == null ? "CONSOLE" : controller.SteamID.ToString();

        Task.Run(async () =>
        {
            await AdminApi.DelAdmin(adminsid, sid);
        });
    }
    
    // Команды на выдачу
    
    public void OnSlayCmd(CCSPlayerController? controller, CommandInfo info)
    {
        if (!AdminApi.HaveFlag(controller, "slay"))
        {
            AdminApi.HaveNotAccess(info);
            return;
        }
        
        var args = XHelper.GetArgsFromCommandLine(info.GetCommandString);

        if (args.Count < 1)
        {
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_slay <#uid/#sid/name>");
            return;
        }
        
        
        var target = XHelper.GetPlayerFromArg(args[0]);

        if (target == null)
        {
            AdminApi.SendMessage(Localizer["PlayerNotFinded"]);
            return;
        }

        bf.SlayPlayer(controller, target);
        if (controller == null)
        {
            AdminApi.SendMessage(info, $"Player {target.PlayerName} slayed!");
        }
    }
    
    public void OnBanCmd(CCSPlayerController? controller, CommandInfo info)
    {
        // Проверка на флаг
        if (!AdminApi.HaveFlag(controller, "ban"))
        {
            AdminApi.HaveNotAccess(info);
            return;
        }
        
        // Получение аргументов
        var args = XHelper.GetArgsFromCommandLine(info.GetCommandString);
        
        // Вывод о использовании команды в случае нехватки аргументов
        if (args.Count < 3)
        {
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_ban <#uid/#sid/name> <time> <reason> <name if needed>");
            return;
        }
        
        // Попытка получения цели
        var target = XHelper.GetPlayerFromArg(args[0]);

        // Если цель не найдена и аргументом был передан ник сообщаем
        if (target == null && !args[0].StartsWith("#"))
        {
            AdminApi.SendMessage(info, Localizer["PlayerNotFinded"]);
            return;
        }
        
        // Регистрация аргументов функции
        string adminSid = XHelper.GetAdminSid(controller);
        int time = Int32.Parse(args[1]);
        string reason = args[2];
        string ip = target == null ? "Undefined" : target!.IpAddress!;
        string sid = "";
        if (target != null) sid = target.SteamID.ToString();
        
        if (sid == "") sid = args[0].Replace("#", "");
        
        string name = string.Join(" ", args.Skip(3)).Trim();
        if (name.Trim() == "")
        {
            name = target == null ? "Undefined" : target.PlayerName;
        }
        
        // Если цель не найдена, и аргументом переданно не стим айди возвращаем с сообщением об использовании команды
        if (XHelper.GetIdentityType(args[0]) != "sid" && target == null)
        {
            AdminApi.SendMessage(info, Localizer["IncorrectSid"]);
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_ban <#uid/#sid/name> <time> <reason> <name if needed>");
            return;
        }
        
        // Проверка на наличие у цели иммунитета выше чем у контроллера
        var isTargetAdmin = AdminApi.GetAdmin(sid);
        if (isTargetAdmin != null && controller != null)
        {
            if (isTargetAdmin.Immunity >= AdminApi.GetAdmin(controller).Immunity)
            {
                AdminApi.SendMessage(info, Localizer["NOTIFY_PlayerHaveBiggerImmunity"].ToString()
                    .Replace("{admin}", isTargetAdmin.Name)
                );
                return;
            }
        }
        
        // Выполняем функцию
        bf.BanPlayer(name, sid, ip, adminSid, time, reason);
    }
    
    public void OnGagCmd(CCSPlayerController? controller, CommandInfo info)
    {
        // Проверка на флаг
        if (!AdminApi.HaveFlag(controller, "gag"))
        {
            AdminApi.HaveNotAccess(info);
            return;
        }
        
        // Получение аргументов
        var args = XHelper.GetArgsFromCommandLine(info.GetCommandString);
        
        // Вывод о использовании команды в случае нехватки аргументов
        if (args.Count < 3)
        {
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_gag <#uid/#sid/name> <time> <reason> <name if needed>");
            return;
        }
        
        // Попытка получения цели
        var target = XHelper.GetPlayerFromArg(args[0]);

        // Если цель не найдена и аргументом был передан ник сообщаем
        if (target == null && !args[0].StartsWith("#"))
        {
            AdminApi.SendMessage(info, Localizer["PlayerNotFinded"]);
            return;
        }
        
        // Регистрация аргументов функции
        string adminSid = XHelper.GetAdminSid(controller);
        int time = Int32.Parse(args[1]);
        string reason = args[2];
        string sid = "";
        if (target != null) sid = target.SteamID.ToString();
        
        if (sid == "") sid = args[0].Replace("#", "");
        
        string name = string.Join(" ", args.Skip(3)).Trim();
        if (name.Trim() == "")
        {
            name = target == null ? "Undefined" : target.PlayerName;
        }
        
        // Если цель не найдена, и аргументом переданно не стим айди возвращаем с сообщением об использовании команды
        if (XHelper.GetIdentityType(args[0]) != "sid" && target == null)
        {
            AdminApi.SendMessage(info, Localizer["IncorrectSid"]);
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_ban <#uid/#sid/name> <time> <reason> <name if needed>");
            return;
        }
        
        // Проверка на наличие у цели иммунитета выше чем у контроллера
        var isTargetAdmin = AdminApi.GetAdmin(sid);
        if (isTargetAdmin != null && controller != null)
        {
            if (isTargetAdmin.Immunity >= AdminApi.GetAdmin(controller).Immunity)
            {
                AdminApi.SendMessage(info, Localizer["NOTIFY_PlayerHaveBiggerImmunity"].ToString()
                    .Replace("{admin}", isTargetAdmin.Name)
                );
                return;
            }
        }
        
        // Выполняем функцию
        bf.GagPlayer(name, sid, adminSid, time, reason);
    }
    
    public void OnKickCmd(CCSPlayerController? controller, CommandInfo info)
    {
        // Проверка на флаг
        if (!AdminApi.HaveFlag(controller, "kick"))
        {
            AdminApi.HaveNotAccess(info);
            return;
        }
        
        // Получение аргументов
        var args = XHelper.GetArgsFromCommandLine(info.GetCommandString);
        
        // Вывод о использовании команды в случае нехватки аргументов
        if (args.Count < 3)
        {
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_kick <#uid/#sid/name> <reason>");
            return;
        }
        
        // Попытка получения цели
        var target = XHelper.GetPlayerFromArg(args[0]);

        // Если цель не найдена и аргументом был передан ник сообщаем
        if (target == null)
        {
            AdminApi.SendMessage(info, Localizer["PlayerNotFinded"]);
            return;
        }
        
        // Регистрация аргументов функции
        string reason = args[1];
        
        
        // Проверка на наличие у цели иммунитета выше чем у контроллера
        var isTargetAdmin = AdminApi.GetAdmin(target.SteamID.ToString());
        if (isTargetAdmin != null && controller != null)
        {
            if (isTargetAdmin.Immunity >= AdminApi.GetAdmin(controller).Immunity)
            {
                AdminApi.SendMessage(info, Localizer["NOTIFY_PlayerHaveBiggerImmunity"].ToString()
                    .Replace("{admin}", isTargetAdmin.Name)
                );
                return;
            }
        }
        
        // Выполняем функцию
        AdminApi.KickPlayer(controller, target.SteamID.ToString(), reason);
    }
    
    public void OnSwitchteamCmd(CCSPlayerController? controller, CommandInfo info)
    {
        // Проверка на флаг
        if (!AdminApi.HaveFlag(controller, "switchteam"))
        {
            AdminApi.HaveNotAccess(info);
            return;
        }
        
        // Получение аргументов
        var args = XHelper.GetArgsFromCommandLine(info.GetCommandString);
        
        // Вывод о использовании команды в случае нехватки аргументов
        if (args.Count < 3)
        {
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_switchteam <#uid/#sid/name> <t/ct>");
            return;
        }
        
        // Попытка получения цели
        var target = XHelper.GetPlayerFromArg(args[0]);

        // Если цель не найдена и аргументом был передан ник сообщаем
        if (target == null)
        {
            AdminApi.SendMessage(info, Localizer["PlayerNotFinded"]);
            return;
        }

        string adminsid = controller == null ? "CONSOLE" : controller.SteamID.ToString();
        
        // Регистрация аргументов функции
        CsTeam? newTeam = args[1].ToLower() switch
        {
            "t" => CsTeam.Terrorist,
            "ct" => CsTeam.CounterTerrorist,
            _ => null
        };

        if (newTeam == null)
        {
            AdminApi.SendMessage(info, AdminApi.Localizer["NOTIFY_IncorrectTeam"]);
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_switchteam <#uid/#sid/name> <t/ct>");
            return;
        }
        
        
        // Проверка на наличие у цели иммунитета выше чем у контроллера
        var isTargetAdmin = AdminApi.GetAdmin(target.SteamID.ToString());
        if (isTargetAdmin != null && controller != null)
        {
            if (isTargetAdmin.Immunity >= AdminApi.GetAdmin(controller).Immunity)
            {
                AdminApi.SendMessage(info, Localizer["NOTIFY_PlayerHaveBiggerImmunity"].ToString()
                    .Replace("{admin}", isTargetAdmin.Name)
                );
                return;
            }
        }
        string OldTeam = target.TeamNum switch
        {
            1 => "SPEC",
            2 => "T",
            3 => "CT",
            0 => "NONE",
            _ => "NONE"
        };
        AdminApi.CommandUsed(controller, "css_switchteam", $"{OldTeam} {args[1].ToUpper()}");

        // Выполняем функцию
        target.SwitchTeam((CsTeam)newTeam);

        string serverMessage = AdminApi.Localizer["SERVER_SwitchTeam"].ToString()
            .Replace("{admin}", Iks_Admin.Messages.GetAdminName(adminsid))
            .Replace("{team}", args[1].ToUpper());
        
        string targetMessage = AdminApi.Localizer["TARGET_SwitchTeam"].ToString()
            .Replace("{admin}", Iks_Admin.Messages.GetAdminName(adminsid))
            .Replace("{team}", args[1].ToUpper());
        
        
        AdminApi.SendMessageToServer(serverMessage);
        AdminApi.SendMessage(target, targetMessage);
    }
    
    public void OnChangeteamCmd(CCSPlayerController? controller, CommandInfo info)
    {
        // Проверка на флаг
        if (!AdminApi.HaveFlag(controller, "changeteam"))
        {
            AdminApi.HaveNotAccess(info);
            return;
        }
        
        // Получение аргументов
        var args = XHelper.GetArgsFromCommandLine(info.GetCommandString);
        
        // Вывод о использовании команды в случае нехватки аргументов
        if (args.Count < 3)
        {
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_changeteam <#uid/#sid/name> <t/ct/spec>");
            return;
        }
        
        // Попытка получения цели
        var target = XHelper.GetPlayerFromArg(args[0]);

        // Если цель не найдена и аргументом был передан ник сообщаем
        if (target == null)
        {
            AdminApi.SendMessage(info, Localizer["PlayerNotFinded"]);
            return;
        }

        string adminsid = controller == null ? "CONSOLE" : controller.SteamID.ToString();
        
        // Регистрация аргументов функции
        CsTeam? newTeam = args[1].ToLower() switch
        {
            "t" => CsTeam.Terrorist,
            "ct" => CsTeam.CounterTerrorist,
            "spec" => CsTeam.Spectator,
            _ => null
        };

        if (newTeam == null)
        {
            AdminApi.SendMessage(info, AdminApi.Localizer["NOTIFY_IncorrectTeam"]);
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_changeteam <#uid/#sid/name> <t/ct/spec>");
            return;
        }
        
        
        // Проверка на наличие у цели иммунитета выше чем у контроллера
        var isTargetAdmin = AdminApi.GetAdmin(target.SteamID.ToString());
        if (isTargetAdmin != null && controller != null)
        {
            if (isTargetAdmin.Immunity >= AdminApi.GetAdmin(controller).Immunity)
            {
                AdminApi.SendMessage(info, Localizer["NOTIFY_PlayerHaveBiggerImmunity"].ToString()
                    .Replace("{admin}", isTargetAdmin.Name)
                );
                return;
            }
        }
        
        string OldTeam = target.TeamNum switch
        {
            1 => "SPEC",
            2 => "T",
            3 => "CT",
            0 => "NONE",
            _ => "NONE"
        };
        AdminApi.CommandUsed(controller, "css_switchteam", $"{OldTeam} {args[1].ToUpper()}");
        
        // Выполняем функцию
        target.ChangeTeam((CsTeam)newTeam);

        string serverMessage = AdminApi.Localizer["SERVER_ChangeTeam"].ToString()
            .Replace("{admin}", Iks_Admin.Messages.GetAdminName(adminsid))
            .Replace("{team}", args[1].ToUpper());
        
        string targetMessage = AdminApi.Localizer["TARGET_ChangeTeam"].ToString()
            .Replace("{admin}", Iks_Admin.Messages.GetAdminName(adminsid))
            .Replace("{team}", args[1].ToUpper());
        
        
        
        AdminApi.SendMessageToServer(serverMessage);
        AdminApi.SendMessage(target, targetMessage);
    }
    
    public void OnMuteCmd(CCSPlayerController? controller, CommandInfo info)
    {
        // Проверка на флаг
        if (!AdminApi.HaveFlag(controller, "mute"))
        {
            AdminApi.HaveNotAccess(info);
            return;
        }
        
        // Получение аргументов
        var args = XHelper.GetArgsFromCommandLine(info.GetCommandString);
        
        // Вывод о использовании команды в случае нехватки аргументов
        if (args.Count < 3)
        {
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_mute <#uid/#sid/name> <time> <reason> <name if needed>");
            return;
        }
        
        // Попытка получения цели
        var target = XHelper.GetPlayerFromArg(args[0]);

        // Если цель не найдена и аргументом был передан ник сообщаем
        if (target == null && !args[0].StartsWith("#"))
        {
            AdminApi.SendMessage(info, Localizer["PlayerNotFinded"]);
            return;
        }
        
        // Регистрация аргументов функции
        string adminSid = XHelper.GetAdminSid(controller);
        int time = Int32.Parse(args[1]);
        string reason = args[2];
        string sid = "";
        if (target != null) sid = target.SteamID.ToString();
        
        if (sid == "") sid = args[0].Replace("#", "");
        
        string name = string.Join(" ", args.Skip(3)).Trim();
        if (name.Trim() == "")
        {
            name = target == null ? "Undefined" : target.PlayerName;
        }
        
        // Если цель не найдена, и аргументом переданно не стим айди возвращаем с сообщением об использовании команды
        if (XHelper.GetIdentityType(args[0]) != "sid" && target == null)
        {
            AdminApi.SendMessage(info, Localizer["IncorrectSid"]);
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_ban <#uid/#sid/name> <time> <reason> <name if needed>");
            return;
        }
        
        // Проверка на наличие у цели иммунитета выше чем у контроллера
        var isTargetAdmin = AdminApi.GetAdmin(sid);
        if (isTargetAdmin != null && controller != null)
        {
            if (isTargetAdmin.Immunity >= AdminApi.GetAdmin(controller).Immunity)
            {
                AdminApi.SendMessage(info, Localizer["NOTIFY_PlayerHaveBiggerImmunity"].ToString()
                    .Replace("{admin}", isTargetAdmin.Name)
                );
                return;
            }
        }
        
        // Выполняем функцию
        bf.MutePlayer(name, sid, adminSid, time, reason);
    }
    
    // Команды на снятие наказаний
    
    public void OnUnBanCmd(CCSPlayerController? controller, CommandInfo info)
    {
        // Проверка на флаг
        if (!AdminApi.HaveFlag(controller, "unban"))
        {
            AdminApi.HaveNotAccess(info);
            return;
        }
        
        // Получение аргументов
        var args = XHelper.GetArgsFromCommandLine(info.GetCommandString);
        // Вывод о использовании команды в случае нехватки аргументов
        if (args.Count < 1)
        {
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_unban <sid/ip>");
            return;
        }
        string arg = args[0];
        string adminSid = controller == null ? "CONSOLE" : controller.SteamID.ToString();
        
        
        
        // Выполняем функцию
        bf.UnBanPlayer(arg, adminSid);
    }
    public void OnUnGagCmd(CCSPlayerController? controller, CommandInfo info)
    {
        // Проверка на флаг
        if (!AdminApi.HaveFlag(controller, "ungag"))
        {
            AdminApi.HaveNotAccess(info);
            return;
        }
        
        
        // Получение аргументов
        var args = XHelper.GetArgsFromCommandLine(info.GetCommandString);
        // Вывод о использовании команды в случае нехватки аргументов
        if (args.Count < 1)
        {
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_ungag <#sid/#uid/name>");
            return;
        }
        
        string arg = args[0];
        string adminSid = controller == null ? "CONSOLE" : controller.SteamID.ToString();

        var target = XHelper.GetPlayerFromArg(arg);
        
        string farg = arg;
        if (target != null)
        {
            farg = target.SteamID.ToString();
        }
            
        
        
        // Выполняем функцию
        bf.UnGagPlayer(farg, adminSid);
    }
    
    public void OnUnMuteCmd(CCSPlayerController? controller, CommandInfo info)
    {
        // Проверка на флаг
        if (!AdminApi.HaveFlag(controller, "unmute"))
        {
            AdminApi.HaveNotAccess(info);
            return;
        }
        
        
        // Получение аргументов
        var args = XHelper.GetArgsFromCommandLine(info.GetCommandString);
        // Вывод о использовании команды в случае нехватки аргументов
        if (args.Count < 1)
        {
            AdminApi.SendMessage(info, "Command usage:");
            AdminApi.SendMessage(info, "css_unmute <#sid/#uid/name>");
            return;
        }
        
        string arg = args[0];
        string adminSid = controller == null ? "CONSOLE" : controller.SteamID.ToString();

        var target = XHelper.GetPlayerFromArg(arg);
        
        string farg = arg;
        if (target != null)
        {
            farg = target.SteamID.ToString();
        }
            
        
        
        // Выполняем функцию
        bf.UnMutePlayer(farg, adminSid);
    }

    public void OnAdminCmd(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller == null) return;
        var admin = AdminApi.GetAdmin(controller);
        if (admin == null)
        {
            AdminApi.HaveNotAccess(info);
            return;
        }
        Iks_Admin.AdminMenu.OpenAdminMenu(controller);
    }
    
    public void OnHideCmd(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller == null)
        {
            AdminApi.SendMessage(info, controller, "This command use only from client");
            return;
        }
        // Проверка на флаг
        if (!AdminApi.HaveFlag(controller, "hide"))
        {
            AdminApi.HaveNotAccess(info);
            return;
        }

        if (HidenPlayers.Contains(controller))
        {
            controller.ChangeTeam(CsTeam.Spectator);
            AdminApi.SendMessage(controller, Localizer["NOTIFY_OffHide"]);
            HidenPlayers.Remove(controller);
            AdminApi.CommandUsed(controller, "css_hide", "off");
        }
        else
        {
            HidenPlayers.Add(controller);
            Server.ExecuteCommand("sv_disable_teamselect_menu 1");
            AdminApi._plugin.AddTimer(0.5f, () =>
            {
                controller.CommitSuicide(true, true);
                controller.ChangeTeam(CsTeam.None);
                AdminApi.SendMessage(controller, Localizer["NOTIFY_OnHide"]);
                AdminApi._plugin.AddTimer(0.5f, () =>
                {
                    Server.ExecuteCommand("sv_disable_teamselect_menu 0");
                });
            });
            AdminApi.CommandUsed(controller, "css_hide", "on");
        }
        
    }
}