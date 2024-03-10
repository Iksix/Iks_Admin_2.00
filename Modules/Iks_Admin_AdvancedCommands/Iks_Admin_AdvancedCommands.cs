using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Iks_Admin_Api;
using Microsoft.Extensions.Logging;

namespace Iks_Admin_AdvancedCommands;

public class Iks_Admin_AdvancedCommands : BasePlugin
{
    public override string ModuleName { get; } = "Iks_Admin_AdvancedCommands";
    public override string ModuleVersion { get; } = "1.0.0";
    public override string ModuleAuthor { get; } = "iks";

    private List<ModuleMenuOption> addedOptionsToPlayersMenu = new();

    public static IIks_Admin_Api? api;

    public static PluginCapability<IIks_Admin_Api> AdminApiCapability = new PluginCapability<IIks_Admin_Api>("iksadmin:core");

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        api = AdminApiCapability.Get();
        if (api == null)
        {
            Logger.LogError("Api not finded :(");
            return;
        }

        var option = new RenameOption(this, "rename", Localizer["MENUOPTION_Rename"], (p, _) => {

            ChatMenu menu = new ChatMenu($" {api.Localizer["PluginTag"]} " + Localizer["MENUTITLE_Rename"]);
            var admin = api.GetAdmin(p);
            if (admin == null)
            {
                api.HaveNotAccess(p);
            }
            foreach (var target in XHelper.GetOnlinePlayers())
            {
                var isTargetAdmin = api.GetAdmin(target);
                if (isTargetAdmin != null)
                {
                    if (isTargetAdmin.SteamId != p.SteamID.ToString() && isTargetAdmin.Immunity >= admin.Immunity)
                    {
                        continue;
                    }
                }

                menu.AddMenuOption(target.PlayerName, (p, _) => {
                    api.SendMessage(Localizer["NOTIFY_WriteName"]);
                    api.NextSayActions.Add(p, (p, msg) => {
                        api.SendMessageToServer(Localizer["SERVER_Rename"].ToString()
                        .Replace("{name}", target.PlayerName)
                        .Replace("{new}", msg)
                        );
                        target.PlayerName = msg;
                        Utilities.SetStateChanged(target, "CBasePlayerController", "m_iszPlayerName");
                    });
                });
            }

            MenuManager.OpenChatMenu(p, menu);
            
        });
        api.PlayersMenuOptions!.Add(option);
        addedOptionsToPlayersMenu.Add(option);
        api.LogMessage("[ADVA] Rename option added!");
        //api.PlayersMenuOptions.Add();
    }

    public override void Unload(bool hotReload)
    {
        foreach (var option in addedOptionsToPlayersMenu)
        {
            api!.PlayersMenuOptions!.Remove(option);
        }
    }

    [ConsoleCommand("css_admins")]
    public void OnAdminsCmd(CCSPlayerController? controller, CommandInfo info)
    {
        var admins = api!.Admins;
        if (controller == null)
        {
            foreach (var admin in admins!)
            {
                string end = admin.End == 0 ? Localizer["Never"] : XHelper.GetDateStringFromUTC((ulong)admin.End);
                info.ReplyToCommand("===================");
                info.ReplyToCommand($"Name: {admin.Name}");
                info.ReplyToCommand($"SteamId: {admin.SteamId}");
                info.ReplyToCommand($"Flags: {admin.Flags}");
                info.ReplyToCommand($"Immunity: {admin.Immunity}");
                info.ReplyToCommand($"End: {end}");
                info.ReplyToCommand($"GroupName: {admin.GroupName}");
                info.ReplyToCommand($"GroupId: {admin.GroupId}");
                info.ReplyToCommand($"ServerId: {admin.ServerId}");
                info.ReplyToCommand("===================");
            }
            
            return;
        }

        OpenAdminsMenu(controller);
        
    }

    public void OpenAdminsMenu(CCSPlayerController controller)
    {
        var admins = api!.Admins;
        ChatMenu menu = new ChatMenu($" {api.Localizer["PluginTag"]} " + Localizer["MENUTITLE_Admins"]);

        menu.AddMenuOption(api.Localizer["MENUOPTION_Close"], (p, _) => {
            MenuManager.CloseActiveMenu(p);
            api.SendMessage(api.Localizer["NOTIFY_MenuClosed"]);
        });
            
        foreach (var admin in admins)
        {
            string end = admin.End == 0 ? Localizer["Never"] : XHelper.GetDateStringFromUTC((ulong)admin.End);
            menu.AddMenuOption(admin.Name, (p, _) => {
                api.SendMessage(p, Localizer["TARGET_AdminInfo"].ToString()
                .Replace("{name}", admin.Name)
                .Replace("{sid}", admin.SteamId)
                .Replace("{flags}", admin.Flags)
                .Replace("{immunity}", admin.Immunity.ToString())
                .Replace("{end}", end)
                .Replace("{group_id}", admin.GroupId.ToString())
                .Replace("{group}", admin.GroupName)
                .Replace("{server_id}", admin.ServerId)
                );
                OpenAdminsMenu(p);
            });
        }

        MenuManager.OpenChatMenu(controller, menu);
    }
}

public class RenameOption : ModuleMenuOption
{
    public RenameOption(BasePlugin plugin, string OptionAccess, string OptionTitle, Action<CCSPlayerController, ChatMenuOption> SelectAction) : base(plugin, OptionAccess, OptionTitle, SelectAction)
    {
        
    }
}

