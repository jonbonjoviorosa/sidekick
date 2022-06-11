using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sidekick.Model;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Model.Player;
using Sidekick.Model.Nationality;
using Microsoft.AspNetCore.Mvc.Rendering;
using ClosedXML.Excel;
using System.IO;

namespace Sidekick.Admin.Controllers
{
    public class PlayerController : BaseController
    {
        private IMainHttpClient MainHTTPClient { get; }
        public AdminUserContext AdminUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public PlayerController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
        {
            MainHTTPClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            AdminUCtxt = _session.GetObjectFromJson<AdminUserContext>("adminUserContext");
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPlayer/All"));

                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    IEnumerable<FacilityPlayer> returnList = JsonConvert.DeserializeObject<IEnumerable<FacilityPlayer>>(returnRes.Payload.ToString());

                    ViewBag.Current = "User Management";
                    return View(returnList.Any() ? returnList.GroupBy(x => x.UserId).Select(g => g.First()).OrderByDescending(r => r.DateCreated.Value).ToList() : returnList);
                }
            }
            return RedirectToAction("Logout", "Home");
        }

        [HttpGet]
        public IActionResult Add()
        {
            var viewModel = new PlayerViewModel();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var nationalityResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Nationality/Get/"));
                var areaResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/GetAreas"));
                if (!IsTokenInvalidUsingResponse(nationalityResponse, "Unathorized access.")
                 && !IsTokenInvalidUsingResponse(areaResponse, "Unathorized access."))
                {
                    var nationalities = nationalityResponse != null ? JsonConvert.DeserializeObject<IEnumerable<Nationality>>(nationalityResponse.Payload.ToString()) : new List<Nationality>();
                    var areas = areaResponse != null ? JsonConvert.DeserializeObject<List<Area>>(areaResponse.Payload.ToString()) : new List<Area>();

                    var nationalityList = new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Text = "Select Nationality",
                            Value = Guid.Empty.ToString()
                        }
                    };

                    if (nationalities.Any())
                    {
                        foreach (var nationality in nationalities)
                        {
                            nationalityList.Add(new SelectListItem
                            {
                                Value = nationality.NationalityId.ToString(),
                                Text = nationality._Nationality
                            });
                        }
                    }

                    var areaList = new List<SelectListItem>()
                    {
                        new SelectListItem
                        {
                            Value = default,
                            Text = "Select Area"
                        }
                    };

                    if (areas.Any())
                    {
                        foreach (var area in areas)
                        {
                            areaList.Add(new SelectListItem
                            {
                                Value = area.AreaId.ToString(),
                                Text = area.AreaName
                            });
                        }
                    }

                    ViewBag.DefaultIcon = MConf.WebApiBaseUrl.Replace("api/", "") + "resources/Defaults/User_Default_Logo.jpg";
                    ViewBag.Nationalities = nationalityList;
                    ViewBag.Areas = areaList;
                    return View(viewModel);
                }
            }
            ViewBag.Current = "User Management";
            return View();
        }

        [HttpPost]
        public IActionResult AddOrEditUserPlayer(PlayerViewModel player)
        {
            //Password is optional for edit
            if (player.UserId != Guid.Empty)
            {
                foreach (var modelState in ModelState)
                {
                    if (modelState.Key == "Password")
                    {
                        modelState.Value.ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var saveUserResponse = new APIResponse();
                var userResponse = new User();
                if (player.UserId == Guid.Empty)
                {
                    var user = new UserRegistration
                    {
                        FirstName = player.FirstName,
                        LastName = player.LastName,
                        Email = player.Email,
                        Password = player.Password,
                        UserType = EUserType.NORMAL,
                        DevicePlatform = EDevicePlatform.Web,
                        MobileNumber = player.MobileNumber,
                        ImageUrl = player.ImageUrl,
                        NationalityId = player.NationalityId,
                        AreaId = player.AreaId
                    };

                    saveUserResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("User/Register", user));
                    if (saveUserResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        ViewBag.ModalTitle = saveUserResponse.StatusCode == System.Net.HttpStatusCode.OK ? saveUserResponse.Status : saveUserResponse.Status;
                        ViewBag.ModalMessage = saveUserResponse.StatusCode == System.Net.HttpStatusCode.OK ? saveUserResponse.Message : saveUserResponse.Message;
                        ViewBag.ShowModal = saveUserResponse.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";

                        return View("Index");
                    }

                    userResponse = saveUserResponse != null ? JsonConvert.DeserializeObject<User>(saveUserResponse.Payload.ToString()) : new User();
                }

                var uploadProfileImg = new UserProfile
                {
                    UserId = player.UserId == Guid.Empty ? userResponse.UserId : player.UserId,
                    ImageUrl = player.ImageUrl,
                    NationalityId = player.NationalityId.Value,
                    FirstName = player.FirstName,
                    LastName = player.LastName,
                    Email = player.Email,
                    MobileNumber = player.MobileNumber,
                    //IsActive = player.IsActive,
                    Password = string.IsNullOrWhiteSpace(player.Password) ? string.Empty  : player.Password
                };

                var updateProfileResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("User/UpdateUserProfile", uploadProfileImg));
                if (updateProfileResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var facilityPlayer = new PlayerViewModel
                    {
                        UserId = player.UserId == Guid.Empty ? userResponse.UserId : player.UserId,
                        FirstName = player.FirstName,
                        LastName = player.LastName,
                        Email = player.Email,
                        Password = player.Password,
                        UserType = EUserType.NORMAL,
                        DevicePlatform = EDevicePlatform.Web,
                        MobileNumber = player.MobileNumber,
                        ImageUrl = player.ImageUrl,
                        NationalityId = player.NationalityId,
                        AreaId = player.AreaId
                    };

                    var addOrEditPlayerResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FacilityPlayer/AddOrEditPlayer", facilityPlayer));
                    ViewBag.ModalTitle = addOrEditPlayerResponse.StatusCode == System.Net.HttpStatusCode.OK ? addOrEditPlayerResponse.Status : addOrEditPlayerResponse.Status;
                    ViewBag.ModalMessage = addOrEditPlayerResponse.StatusCode == System.Net.HttpStatusCode.OK ? addOrEditPlayerResponse.Message : addOrEditPlayerResponse.Message;
                    ViewBag.ShowModal = addOrEditPlayerResponse.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";

                    return View("Index");
                }
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult Edit(Guid u)
        {
            var viewModel = new PlayerViewModel();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var nationalityResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Nationality/Get/"));
                var areaResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/GetAreas"));
                var retrieveResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest($"FacilityPlayer/GetPlayer/{u}"));
                if (!IsTokenInvalidUsingResponse(nationalityResponse, "Unathorized access.")
                 && !IsTokenInvalidUsingResponse(areaResponse, "Unathorized access.")
                 && !IsTokenInvalidUsingResponse(retrieveResponse, "Unauthorized accesss"))
                {
                    var nationalities = nationalityResponse != null ? JsonConvert.DeserializeObject<IEnumerable<Nationality>>(nationalityResponse.Payload.ToString()) : new List<Nationality>();
                    var areas = areaResponse != null ? JsonConvert.DeserializeObject<List<Area>>(areaResponse.Payload.ToString()) : new List<Area>();
                    viewModel = retrieveResponse != null ? JsonConvert.DeserializeObject<PlayerViewModel>(retrieveResponse.Payload.ToString()) : new PlayerViewModel();
                    var nationalityList = new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Text = "Select Nationality",
                            Value = Guid.Empty.ToString()
                        }
                    };

                    if (nationalities.Any())
                    {
                        foreach (var nationality in nationalities)
                        {
                            nationalityList.Add(new SelectListItem
                            {
                                Value = nationality.NationalityId.ToString(),
                                Text = nationality._Nationality
                            });
                        }
                    }

                    var areaList = new List<SelectListItem>()
                    {
                        new SelectListItem
                        {
                            Value = default,
                            Text = "Select Area"
                        }
                    };

                    if (areas.Any())
                    {
                        foreach (var area in areas)
                        {
                            areaList.Add(new SelectListItem
                            {
                                Value = area.AreaId.ToString(),
                                Text = area.AreaName
                            });
                        }
                    }

                    ViewBag.Nationalities = nationalityList;
                    ViewBag.Areas = areaList;
                    return View(viewModel);
                }
            }
            ViewBag.Current = "User Management";
            return View();
        }

        [HttpPost]
        public IActionResult Status([FromBody] ChangeStatus userId)
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("FacilityPlayer/ChangeStatus", userId));

                if (base.IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                return Json(response);
            }
            return RedirectToAction("Logout", "Home");
        }

        public IActionResult Export()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("FacilityPlayer/All"));
                IEnumerable<FacilityPlayer> returnList = new List<FacilityPlayer>();
                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    returnList = JsonConvert.DeserializeObject<IEnumerable<FacilityPlayer>>(returnRes.Payload.ToString());
                    returnList = returnList.Any() ? returnList.Where(r => r.FacilityId == Guid.Empty).OrderByDescending(r => r.DateCreated.Value).ToList() : returnList;
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Player User Reports");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Name";
                    worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 2).Value = "Email";
                    worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 3).Value = "Mobile Number";
                    worksheet.Cell(currentRow, 3).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 4).Value = "Date Created";
                    worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 5).Value = "Last Booking";
                    worksheet.Cell(currentRow, 5).Style.Fill.BackgroundColor = XLColor.Yellow;
                    //worksheet.Cell(currentRow, 6).Value = "Status";
                    //worksheet.Cell(currentRow, 6).Style.Fill.BackgroundColor = XLColor.Yellow;

                    foreach (var player in returnList)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = $"{player.FirstName} {player.LastName}";
                        worksheet.Cell(currentRow, 2).Value = player.Email;
                        worksheet.Cell(currentRow, 3).Value = player.ContactNumber;
                        worksheet.Cell(currentRow, 4).Value = player.DateCreated.GetValueOrDefault().ToString("dd/MM/yyyy");
                        worksheet.Cell(currentRow, 5).Value = player.LastBooking;
                        //if (player.IsEnabled.Value)
                        //{
                        //    worksheet.Cell(currentRow, 6).Value = "ACTIVE";
                        //    worksheet.Cell(currentRow, 6).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        //}
                        //else
                        //{
                        //    worksheet.Cell(currentRow, 6).Value = "INACTIVE";
                        //    worksheet.Cell(currentRow, 6).Style.Fill.BackgroundColor = XLColor.LightGray;
                        //}
                    }

                    using var stream = new MemoryStream();
                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Players - {DateTime.Now}.xlsx");
                }
            }

            return View();
        }
    }
}
