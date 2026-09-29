using Microsoft.AspNetCore.Mvc.Rendering;

namespace ToolLogix.Areas.Identity.Pages.Account.Manage;

public static class ManageNavPages
{
    public static string Index => nameof(Index);
    public static string Email => nameof(Email);
    public static string ChangePassword => nameof(ChangePassword);
    public static string ExternalLogins => nameof(ExternalLogins);
    public static string PersonalData => nameof(PersonalData);
    public static string TwoFactorAuthentication => nameof(TwoFactorAuthentication);

    public static string IndexNavClass(ViewContext viewContext) =>
        PageNavClass(viewContext, Index);

    public static string EmailNavClass(ViewContext viewContext) =>
        PageNavClass(viewContext, Email);

    public static string ChangePasswordNavClass(ViewContext viewContext) =>
        PageNavClass(viewContext, ChangePassword);

    public static string ExternalLoginsNavClass(ViewContext viewContext) =>
        PageNavClass(viewContext, ExternalLogins);

    public static string PersonalDataNavClass(ViewContext viewContext) =>
        PageNavClass(viewContext, PersonalData);

    public static string TwoFactorAuthenticationNavClass(ViewContext viewContext) =>
        PageNavClass(viewContext, TwoFactorAuthentication);

    private static string? PageNavClass(
        ViewContext viewContext,
        string pageName)
    {
        var activePage = viewContext.ViewData["ActivePage"]?.ToString();

        return string.Equals(
            activePage,
            pageName,
            StringComparison.OrdinalIgnoreCase)
                ? "active"
                : null;
    }
}
