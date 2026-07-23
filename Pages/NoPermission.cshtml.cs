using Appointments.Database.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointments.Pages
{
    public class NoPermissionModel : BasePageModel
    {
        public NoPermissionModel(ILogger<NoPermissionModel> logger, IConfiguration configuration, IPermissionsRepository permissionsRepository) : base(logger, configuration, permissionsRepository)
        {
        }

        public void OnGet()
        {
            OnGetBase();
        }
    }
}
