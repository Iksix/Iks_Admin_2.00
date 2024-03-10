using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Iks_Admin_Api;
using Microsoft.Extensions.Localization;

namespace Iks_Admin.Menus;

public class AdminMenu
{
    public AdminApi adminApi = Iks_Admin.adminApi;
    private IStringLocalizer Localizer;
    private PluginConfig Config = Iks_Admin.ConfigNow;

    public List<ControllerParams> GetTargets()
    {
        List<ControllerParams> targets = new List<ControllerParams>();
        var players = XHelper.GetOnlinePlayers();
        foreach (var p in players)
        {
            if (!XHelper.IsControllerValid(p)) continue;
            targets.Add(XHelper.GetControllerParams(p));
        }

        return targets;
    }

    public AdminMenu(IStringLocalizer Localizer)
    {
        this.Localizer = Localizer;
    }

    public string AdminMenuName(string str)
    {
        return $" {Localizer["PluginTag"]} {Localizer[$"MENUTITLE_{str}"]}";
    }
    public string AdminMenuOption(string str)
    {
        return $" {Localizer[$"MENUOPTION_{str}"]}";
    }
    public void OpenAdminMenu(CCSPlayerController controller)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Main"));

        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;

        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        if (adminApi.HaveOptionAccess(admin, "players"))
        {
            menu.AddMenuOption(AdminMenuOption("Players"), (p, _) =>
            {
                OpenPlayersManageMenu(p);
            });
        }

        if (adminApi.HaveOptionAccess(admin, "blocks"))
        {
            menu.AddMenuOption(AdminMenuOption("Blocks"), (p, _) =>
            {
                OpenBlocksMenu(p);
            });
        }

        foreach (var option in adminApi.BaseMenuOptions!)
        {
            if (adminApi.HaveOptionAccess(admin, option.OptionAccess))
            {
                menu.AddMenuOption(option.OptionTitle, (p, _) =>
                {
                    option.SelectAction.Invoke(p, _);
                });
            }
        }
        

        MenuManager.OpenChatMenu(controller, menu);
    }

    public void OpenBlocksMenu(CCSPlayerController controller)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Blocks"));
        
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenAdminMenu(controller);
        });
        

        if (adminApi.HaveOptionAccess(admin, "ban"))
        {
            menu.AddMenuOption(AdminMenuOption("Ban"), (p, _) =>
            {
                OpenBanMenu(p, GetTargets());
            });
            menu.AddMenuOption(AdminMenuOption("OfflineBan"), (p, _) =>
            {
                OpenBanMenu(p, Iks_Admin.OfflinePlayers);
            });
        }
        if (adminApi.HaveOptionAccess(admin, "gag"))
        {
            menu.AddMenuOption(AdminMenuOption("Gag"), (p, _) =>
            {
                OpenGagMenu(p, GetTargets());
            });
        }
        if (adminApi.HaveOptionAccess(admin, "ungag"))
        {
            menu.AddMenuOption(AdminMenuOption("UnGag"), (p, _) =>
            {
                OpenUnGagMenu(p);
            });
        }
        if (adminApi.HaveOptionAccess(admin, "mute"))
        {
            menu.AddMenuOption(AdminMenuOption("Mute"), (p, _) =>
            {
                OpenMuteMenu(p, GetTargets());
            });
        }
        if (adminApi.HaveOptionAccess(admin, "unmute"))
        {
            menu.AddMenuOption(AdminMenuOption("UnMute"), (p, _) =>
            {
                OpenUnMuteMenu(p);
            });
        }
        
        foreach (var option in adminApi.BlocksMenuOptions!)
        {
            if (adminApi.HaveOptionAccess(admin, option.OptionAccess))
            {
                menu.AddMenuOption(option.OptionTitle, (p, _) =>
                {
                    option.SelectAction.Invoke(p, _);
                });
            }
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }
    
    public void OpenPlayersManageMenu(CCSPlayerController controller)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Players"));
        
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenAdminMenu(controller);
        });
        
        
        if (adminApi.HaveOptionAccess(admin, "kick"))
        {
            menu.AddMenuOption(AdminMenuOption("Kick"), (p, _) =>
            {
                OpenKickMenu(p, GetTargets());
            });
        }
        if (adminApi.HaveOptionAccess(admin, "slay"))
        {
            menu.AddMenuOption(AdminMenuOption("Slay"), (p, _) =>
            {
                OpenSlayMenu(p);
            });
        }
        
        if (adminApi.HaveOptionAccess(admin, "switchteam"))
        {
            menu.AddMenuOption(AdminMenuOption("Switchteam"), (p, _) =>
            {
                OpenSwitchteamMenu(p);
            });
        }
        if (adminApi.HaveOptionAccess(admin, "changeteam"))
        {
            menu.AddMenuOption(AdminMenuOption("Changeteam"), (p, _) =>
            {
                OpenChangeteamMenu(p);
            });
        }
        
        foreach (var option in adminApi.PlayersMenuOptions!)
        {
            if (adminApi.HaveOptionAccess(admin, option.OptionAccess))
            {
                menu.AddMenuOption(option.OptionTitle, (p, _) =>
                {
                    option.SelectAction.Invoke(p, _);
                });
            }
        }
        
        
        MenuManager.OpenChatMenu(controller, menu);
    }

    public void OpenSlayMenu(CCSPlayerController controller)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Slay"));
        var admin = adminApi.GetAdmin(controller);
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenPlayersManageMenu(controller);
        });
        
        var targets = XHelper.GetOnlinePlayers(true);
        
        foreach (var target in targets)
        {
            var targetAdmin = adminApi.GetAdmin(target);
            if (targetAdmin != null && targetAdmin.SteamId != controller.SteamID.ToString())
            {
                if (targetAdmin.Immunity >= admin.Immunity) continue;
            }

            menu.AddMenuOption(target.PlayerName, (p, _) =>
            {
                target.CommitSuicide(true, true);
                adminApi.Slay(controller, target);
            });
            
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }
    public void OpenChangeteamMenu(CCSPlayerController controller)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Kick"));
        
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenPlayersManageMenu(controller);
        });


        foreach (var player in XHelper.GetOnlinePlayers(true))
        {
            //Отсеиваем ненужных/недоступных игроков
            var p_admin = adminApi.GetAdmin(player.SteamID.ToString());
            if (p_admin != null && player.SteamID.ToString() != controller.SteamID.ToString())
            {
                if (p_admin.Immunity >= admin.Immunity) continue;
            }
            
            menu.AddMenuOption(player.PlayerName, (p, _) =>
            {
                OpenChangeteamMenu2(p, player);
            });
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }
    public void OpenChangeteamMenu2(CCSPlayerController controller, CCSPlayerController target)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Kick"));
        
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenChangeteamMenu(controller);
        });

        string OldTeam = target.TeamNum switch
        {
            1 => "SPEC",
            2 => "T",
            3 => "CT",
            0 => "NONE",
            _ => "NONE"
        };
        menu.AddMenuOption("To CT", (p, _) =>
        {
            target.ChangeTeam(CsTeam.CounterTerrorist);
            adminApi.CommandUsed(p, "css_changeteam", $"{OldTeam} CT", target);
        });
        menu.AddMenuOption("To T", (p, _) =>
        {
            target.ChangeTeam(CsTeam.Terrorist);
            adminApi.CommandUsed(p, "css_changeteam", $"{OldTeam} T", target);
        });
        menu.AddMenuOption("To SPEC", (p, _) =>
        {
            target.ChangeTeam(CsTeam.Spectator);
            adminApi.CommandUsed(p, "css_changeteam", $"{OldTeam} SPEC", target);
        });
        
        MenuManager.OpenChatMenu(controller, menu);
    }
    
    public void OpenSwitchteamMenu(CCSPlayerController controller)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Kick"));
        
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenPlayersManageMenu(controller);
        });


        foreach (var player in XHelper.GetOnlinePlayers(true))
        {
            //Отсеиваем ненужных/недоступных игроков
            var p_admin = adminApi.GetAdmin(player.SteamID.ToString());
            if (p_admin != null && player.SteamID.ToString() != controller.SteamID.ToString())
            {
                if (p_admin.Immunity >= admin.Immunity) continue;
            }
            
            menu.AddMenuOption(player.PlayerName, (p, _) =>
            {
                OpenSwitchteamMenu2(p, player);
            });
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }
    public void OpenSwitchteamMenu2(CCSPlayerController controller, CCSPlayerController target)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Kick"));
        
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenSwitchteamMenu(controller);
        });

        string OldTeam = target.TeamNum switch
        {
            1 => "SPEC",
            2 => "T",
            3 => "CT",
            0 => "NONE",
            _ => "NONE"
        };
        menu.AddMenuOption("To CT", (p, _) =>
        {
            
            target.SwitchTeam(CsTeam.CounterTerrorist);
            adminApi.CommandUsed(p, "css_switchteam", $"{OldTeam} CT", target);
        });
        menu.AddMenuOption("To T", (p, _) =>
        {
            target.SwitchTeam(CsTeam.Terrorist);
            adminApi.CommandUsed(p, "css_switchteam", $"{OldTeam} T", target);
        });
        
        MenuManager.OpenChatMenu(controller, menu);
    }

    public void OpenKickMenu(CCSPlayerController controller, List<ControllerParams> targets)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Kick"));
        
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenPlayersManageMenu(controller);
        });


        foreach (var player in targets)
        {
            //Отсеиваем ненужных/недоступных игроков
            if (player.SteamID == controller.SteamID.ToString()) continue;
            var p_admin = adminApi.GetAdmin(player.SteamID);
            if (p_admin != null)
            {
                if (p_admin.Immunity >= admin.Immunity) continue;
            }
            
            menu.AddMenuOption(player.PlayerName, (p, _) =>
            {
                OpenKickReasonsMenu(controller, player);
            });
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }

    public void OpenKickReasonsMenu(CCSPlayerController controller, ControllerParams target)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Reason"));
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenKickMenu(controller, GetTargets());
        });
        
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;

        foreach (var reason in Config.KickReasons)
        {
            if (reason.StartsWith("$"))
            {
                menu.AddMenuOption(reason.Replace("$", ""), (p, _) =>
                {
                    adminApi.SendMessage(p, Localizer["NOTIFY_WriteReason"]);
                    adminApi.NextSayActions.Add(controller, (p, msg) =>
                    {
                        adminApi.KickPlayer(controller, target.SteamID, msg);
                    });
                });
                continue;
            }

            menu.AddMenuOption(reason, (p, _) =>
            {
                adminApi.KickPlayer(controller, target.SteamID, reason);
            });
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }
    public void OpenBanMenu(CCSPlayerController controller, List<ControllerParams> targets)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Ban"));
        
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenBlocksMenu(controller);
        });

        var players = XHelper.GetOnlinePlayers();

        foreach (var player in targets)
        {
            //Отсеиваем ненужных/недоступных игроков
            if (player.SteamID == controller.SteamID.ToString()) continue;
            var p_admin = adminApi.GetAdmin(player.SteamID);
            if (p_admin != null)
            {
                if (p_admin.Immunity >= admin.Immunity) continue;
            }
            
            menu.AddMenuOption(player.PlayerName, (p, _) =>
            {
                OpenBanReasonsMenu(controller, player);
            });
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }
    
    public void OpenUnGagMenu(CCSPlayerController controller)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("UnGag"));
        
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenBlocksMenu(controller);
        });

        foreach (var gag in adminApi.OnlineGaggedPlayers)
        {
            menu.AddMenuOption(gag.Name, (p, _) =>
            {
                adminApi.UnGagPlayer(gag.Sid, gag.AdminSid);
            });
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }
    public void OpenUnMuteMenu(CCSPlayerController controller)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("UnMute"));
        
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenBlocksMenu(controller);
        });

        foreach (var mute in adminApi.OnlineMutedPlayers)
        {
            menu.AddMenuOption(mute.Name, (p, _) =>
            {
                adminApi.UnMutePlayer(mute.Sid, mute.AdminSid);
            });
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }
    
    public void OpenGagMenu(CCSPlayerController controller, List<ControllerParams> targets)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Gag"));
        
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenBlocksMenu(controller);
        });

        foreach (var player in targets)
        {
            //Отсеиваем ненужных/недоступных игроков
            if (player.SteamID == controller.SteamID.ToString()) continue;
            var p_admin = adminApi.GetAdmin(player.SteamID);
            if (p_admin != null)
            {
                if (p_admin.Immunity >= admin.Immunity) continue;
            }
            
            menu.AddMenuOption(player.PlayerName, (p, _) =>
            {
                OpenGagReasonsMenu(controller, player);
            });
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }
    public void OpenMuteMenu(CCSPlayerController controller, List<ControllerParams> targets)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Mute"));
        
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenBlocksMenu(controller);
        });

        foreach (var player in targets)
        {
            //Отсеиваем ненужных/недоступных игроков
            if (player.SteamID == controller.SteamID.ToString()) continue;
            var p_admin = adminApi.GetAdmin(player.SteamID);
            if (p_admin != null)
            {
                if (p_admin.Immunity >= admin.Immunity) continue;
            }
            
            menu.AddMenuOption(player.PlayerName, (p, _) =>
            {
                OpenMuteReasonsMenu(controller, player);
            });
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }
    
    public void OpenMuteReasonsMenu(CCSPlayerController controller, ControllerParams target)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Reason"));
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenBlocksMenu(p);
        });

        foreach (var reason in Iks_Admin.ConfigNow.MuteReason)
        {
            menu.AddMenuOption(reason.Title, (p, _) =>
            {
                // Если причина с бан таймом, то баним сразу
                if (reason.BanTime != null && reason.BanTime != -1)
                {
                    
                    MenuManager.CloseActiveMenu(controller);
                    int time = (int)reason.BanTime;
                    Iks_Admin.BaseFunc.MutePlayer(
                        target.PlayerName, 
                        target.SteamID, 
                        p.SteamID.ToString(),
                        time,
                        reason.Title
                        );
                    return;
                }
                // Если причина без бантайма переходим к выбору времени
                if (reason.BanTime == null)
                {
                    OpenTimesMenu(p, target, reason.Title, time =>
                    {
                        MenuManager.CloseActiveMenu(controller);
                        Iks_Admin.BaseFunc.MutePlayer(
                            target.PlayerName, 
                            target.SteamID, 
                            p.SteamID.ToString(),
                            time,
                            reason.Title
                        );
                    }, () =>
                    {
                        OpenMuteReasonsMenu(p, target);
                    });
                    return;
                }
                // Если бантайм -1 то Вводим свою причину
                if (reason.BanTime == -1)
                {
                    adminApi.SendMessage(p, Localizer["NOTIFY_WriteReason"]);
                    adminApi.NextSayActions.Add(p, (playerController, s) =>
                    {
                        OpenTimesMenu(p, target, reason.Title, time =>
                        {
                            MenuManager.CloseActiveMenu(controller);
                            Iks_Admin.BaseFunc.MutePlayer(
                                target.PlayerName, 
                                target.SteamID, 
                                p.SteamID.ToString(),
                                time,
                                s
                            );
                        }, () =>
                        {
                            OpenMuteReasonsMenu(p, target);
                        });
                    });
                }
            });
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }
    
    public void OpenGagReasonsMenu(CCSPlayerController controller, ControllerParams target)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Reason"));
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenBlocksMenu(p);
        });

        foreach (var reason in Iks_Admin.ConfigNow.GagReason)
        {
            menu.AddMenuOption(reason.Title, (p, _) =>
            {
                // Если причина с бан таймом, то баним сразу
                if (reason.BanTime != null && reason.BanTime != -1)
                {
                    int time = (int)reason.BanTime;
                    MenuManager.CloseActiveMenu(controller);
                    Iks_Admin.BaseFunc.GagPlayer(
                        target.PlayerName, 
                        target.SteamID, 
                        p.SteamID.ToString(),
                        time,
                        reason.Title
                        );
                    return;
                }
                // Если причина без бантайма переходим к выбору времени
                if (reason.BanTime == null)
                {
                    OpenTimesMenu(p, target, reason.Title, time =>
                    {
                        MenuManager.CloseActiveMenu(controller);
                        Iks_Admin.BaseFunc.GagPlayer(
                            target.PlayerName, 
                            target.SteamID, 
                            p.SteamID.ToString(),
                            time,
                            reason.Title
                        );
                    }, () =>
                    {
                        OpenGagReasonsMenu(p, target);
                    });
                    return;
                }
                // Если бантайм -1 то вводим свою причину
                if (reason.BanTime == -1)
                {
                    adminApi.SendMessage(p, Localizer["NOTIFY_WriteReason"]);
                    adminApi.NextSayActions.Add(p, (playerController, s) =>
                    {
                        OpenTimesMenu(p, target, reason.Title, time =>
                        {
                            MenuManager.CloseActiveMenu(controller);
                            Iks_Admin.BaseFunc.GagPlayer(
                                target.PlayerName, 
                                target.SteamID, 
                                p.SteamID.ToString(),
                                time,
                                s
                            );
                        }, () =>
                        {
                            OpenGagReasonsMenu(p, target);
                        });
                    });
                }
            });
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }
    public void OpenBanReasonsMenu(CCSPlayerController controller, ControllerParams target)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Reason"));
        var admin = adminApi.GetAdmin(controller);
        if (admin == null) return;
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OpenBlocksMenu(p);
        });

        foreach (var reason in Iks_Admin.ConfigNow.BanReasons)
        {
            menu.AddMenuOption(reason.Title, (p, _) =>
            {
                // Если причина с бан таймом, то баним сразу
                if (reason.BanTime != null && reason.BanTime != -1)
                {
                    int time = (int)reason.BanTime;
                    MenuManager.CloseActiveMenu(controller);
                    Iks_Admin.BaseFunc.BanPlayer(
                        target.PlayerName, 
                        target.SteamID, 
                        target.IpAddress, 
                        p.SteamID.ToString(),
                        time,
                        reason.Title
                        );
                    return;
                }
                // Если причина без бантайма переходим к выбору времени
                if (reason.BanTime == null)
                {
                    OpenTimesMenu(p, target, reason.Title, time =>
                    {
                        MenuManager.CloseActiveMenu(controller);
                        Iks_Admin.BaseFunc.BanPlayer(
                            target.PlayerName, 
                            target.SteamID, 
                            target.IpAddress, 
                            p.SteamID.ToString(),
                            time,
                            reason.Title
                        );
                    }, () =>
                    {
                        OpenBanReasonsMenu(p, target);
                    });
                    return;
                }
                // Если бантайм -1 то вводим свою причину
                if (reason.BanTime == -1)
                {
                    adminApi.SendMessage(p, Localizer["NOTIFY_WriteReason"]);
                    adminApi.NextSayActions.Add(p, (playerController, s) =>
                    {
                        OpenTimesMenu(p, target, reason.Title, time =>
                        {
                            MenuManager.CloseActiveMenu(controller);
                            Iks_Admin.BaseFunc.BanPlayer(
                                target.PlayerName, 
                                target.SteamID, 
                                target.IpAddress, 
                                p.SteamID.ToString(),
                                time,
                                s
                            );
                        }, () =>
                        {
                            OpenBanReasonsMenu(p, target);
                        });
                    });
                }
            });
        }
        
        MenuManager.OpenChatMenu(controller, menu);
    }

    public void OpenTimesMenu(CCSPlayerController controller, ControllerParams target, string reason,
        Action<int> OnSelectTime, Action OnBack)
    {
        ChatMenu menu = new ChatMenu(AdminMenuName("Time"));
        
        menu.AddMenuOption(AdminMenuOption("Close"), (p, _) =>
        {
            adminApi.SendMessage(p , Localizer["NOTIFY_MenuClosed"]);
            MenuManager.CloseActiveMenu(p);
        });
        
        menu.AddMenuOption(AdminMenuOption("Back"), (p, _) =>
        {
            OnBack.Invoke();
        });

        foreach (var time in Iks_Admin.ConfigNow.Times)
        {
            if (time.Value == -1)
            {
                menu.AddMenuOption(AdminMenuOption("OwnTime"), (p, _) =>
                {
                    adminApi.SendMessage(p, Localizer["NOTIFY_WriteTime"]);
                    adminApi.NextSayActions.Add(p, (p, msg) =>
                    {
                        int otime;
                        if (Int32.TryParse(msg, out otime))
                        {
                            MenuManager.CloseActiveMenu(controller);
                            OnSelectTime.Invoke(otime);
                        }
                        else
                        {
                            adminApi.SendMessage(p, Localizer["NOTIFY_ErrorTime"]);
                            OpenTimesMenu(p, target, reason, OnSelectTime, OnBack);
                        }
                    });
                });
                continue;
            }
            menu.AddMenuOption(time.Key, (p, _) =>
            {
                MenuManager.CloseActiveMenu(controller);
                OnSelectTime.Invoke(time.Value);
            });
        }
        
        
        
        MenuManager.OpenChatMenu(controller, menu);
    }
}