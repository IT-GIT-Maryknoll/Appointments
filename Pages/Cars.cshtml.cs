using Appointments.Database.Dto;
using Appointments.Database.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace Appointments.Pages
{
    public class CarsModel (ILogger<CarsModel> logger, IConfiguration configuration, ICarsRepository carsData) : PageModel
    {
            private readonly ILogger<CarsModel> _logger = logger;
            private readonly IConfiguration _configuration = configuration;
            private readonly ICarsRepository _carsData = carsData;

            public required UserDto UserInfo;
            public List<CarDto> lstCars = new List<CarDto>();

            public void OnGet()
            {
                string sMsg = "";

                UserInfo = GetUserInfo(GetUserName());
                lstCars = _carsData.LoadCars(out sMsg);

            }
            private string GetUserName()
            {
                if (HttpContext == null) return null;

                var userName = HttpContext.User.Identity.Name;
                string sUserConfig = configuration.GetValue<string>("UName");
                if (sUserConfig != null && sUserConfig.Trim().Length > 0) userName = sUserConfig;
                if (string.IsNullOrEmpty(userName) || userName.Length < 0)
                {
                    return "";
                }
                else
                {
                    var arrName = userName.Split("\\");
                    return arrName[1];
                }
            }
            public UserDto GetUserInfo(string sUser)
            {
                if (UserInfo is object) return UserInfo;
                using (var context = new PrincipalContext(ContextType.Domain))
                {
                    var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, sUser);
                    if (userPrincipal != null)
                    {
                        DirectoryEntry? directoryEntry = userPrincipal.GetUnderlyingObject() as DirectoryEntry;
                        UserDto oRet = new UserDto();
                        oRet.sAMAccountName = sUser;
                        oRet.FirstName = userPrincipal.GivenName;
                        oRet.MI = userPrincipal.MiddleName;
                        oRet.LastName = userPrincipal.Surname;
                        oRet.DisplayName = userPrincipal.DisplayName;
                        oRet.MbrEmpIndicator = directoryEntry.Properties["company"].Value == null ? "" : directoryEntry.Properties["company"].Value.ToString();
                        oRet.MbrEmpNumber = userPrincipal.EmployeeId;
                        if (oRet.MbrEmpNumber == null) oRet.MbrEmpNumber = (directoryEntry.Properties["title"].Value == null ? "" : directoryEntry.Properties["title"].Value.ToString());
                        oRet.EmailAddress = userPrincipal.EmailAddress;
                        oRet.ADSDescription = userPrincipal.Description;
                        oRet.msExchHideFromAddressLists = (directoryEntry.Properties["msExchHideFromAddressLists"].Value == null ? false : directoryEntry.Properties["msExchHideFromAddressLists"].Value.ToString().ToLower().Equals("true"));
                        if (oRet.msExchHideFromAddressLists) oRet.EmailAddress = "";

                        UserInfo = oRet;
                        return oRet;
                    }
                }
                return null;
            }
            public JsonResult OnGetGetCarById(int id)
            {
                string sMsg = "";
                CarDto oRet = _carsData.GetCarById(id, out sMsg);
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
                else TempData["Message"] += sMsg;
                sMsg = "";
                return new JsonResult(oRet);
            }
            public JsonResult OnGetDeleteCarById(int id)
            {
                string sMsg = "";
                bool bRet = _carsData.DeleteCar(id, out sMsg);
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
                else TempData["Message"] += sMsg;
                sMsg = "";
                return new JsonResult(bRet);
            }
            public JsonResult OnGetSaveCar(string carInfo, string carMake, int carNum, string carModel, string carType, bool isNew)
            {
                string sMsg = "";
                CarDto car = new CarDto();
                car.CarInfo = carInfo; car.CarMake = carMake; car.CarNum = carNum; car.CarModel = carModel ; car.Type = carType; 
                bool bRet = _carsData.SaveCar(car, isNew, out sMsg);
                if ((TempData["Message"] == null || (TempData["Message"]?.ToString()?.Length ?? 0) == 0) && sMsg != null && sMsg.Trim().Length > 0) TempData["Message"] = sMsg;
                else TempData["Message"] += sMsg;
                sMsg = "";
                return new JsonResult(bRet);
            }
        }
    }
