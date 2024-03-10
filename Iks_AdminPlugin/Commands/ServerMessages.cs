using Iks_Admin_Api;
using Microsoft.Extensions.Localization;

namespace Iks_Admin.Commands;

public class ServerMessages
{
    private IStringLocalizer Localizer;
    private AdminApi AdminApi;
    

    public ServerMessages(AdminApi api, IStringLocalizer localizer)
    {
        AdminApi = api;
        Localizer = localizer;
    }
}