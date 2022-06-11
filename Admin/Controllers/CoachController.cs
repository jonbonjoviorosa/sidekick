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
using Sidekick.Model.Specialty;
using Sidekick.Model.Gym;
using Sidekick.Model.Language;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sidekick.Model.Nationality;
using ClosedXML.Excel;
using System.IO;

namespace Sidekick.Admin.Controllers
{
    public class CoachController : BaseController
    {
        private IMainHttpClient MainHTTPClient { get; }
        public AdminUserContext AdminUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public CoachController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
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
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Coach/GetCoaches"));

                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    IEnumerable<CoachViewModel> returnList = JsonConvert.DeserializeObject<IEnumerable<CoachViewModel>>(returnRes.Payload.ToString());

                    ViewBag.Current = "User Management";
                    return View(returnList.Any() ? returnList.OrderByDescending(r => r.DateCreated.Value).ToList() : returnList);
                }
            }
            return RedirectToAction("Logout", "Home");
        }
        public IActionResult Add()
        {
            var viewModel = new CoachRenderViewModel();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var gymResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Gym/Get/"));
                var specialtyResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Specialty/Get/"));
                var languageResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Language/Get/"));
                var nationalityResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Nationality/Get/"));
                var areaResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/GetAreas"));
                if (!IsTokenInvalidUsingResponse(specialtyResponse, "Unathorized access.")
                   && !IsTokenInvalidUsingResponse(gymResponse, "Unathorized access."))
                {
                    var specialties = JsonConvert.DeserializeObject<IEnumerable<Specialty>>(specialtyResponse.Payload.ToString());
                    var gyms = gymResponse != null ? JsonConvert.DeserializeObject<IEnumerable<Gym>>(gymResponse.Payload.ToString()) : new List<Gym>();
                    var languages = languageResponse != null ? JsonConvert.DeserializeObject<IEnumerable<Language>>(languageResponse.Payload.ToString()) : new List<Language>();
                    var nationalities = nationalityResponse != null ? JsonConvert.DeserializeObject<IEnumerable<Nationality>>(nationalityResponse.Payload.ToString()) : new List<Nationality>();
                    var areas = areaResponse != null ? JsonConvert.DeserializeObject<List<Area>>(areaResponse.Payload.ToString()) : new List<Area>();
                    var gymList = new List<SelectListItem>();

                    if (gyms.Any())
                    {
                        foreach (var gym in gyms)
                        {
                            gymList.Add(new SelectListItem
                            {
                                Value = gym.GymId.ToString(),
                                Text = gym.GymName
                            });
                        }
                    }

                    var nationalityList = new List<SelectListItem>();
                    nationalityList.Add(new SelectListItem
                    {
                        Text = "Select Nationality",
                        Value = Guid.Empty.ToString()
                    });

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

                    var languagesList = new List<SelectListItem>();

                    if (languages.Any())
                    {
                        foreach (var language in languages)
                        {
                            languagesList.Add(new SelectListItem
                            {
                                Value = language.LanguageId.ToString(),
                                Text = language._Language
                            });
                        }
                    }

                    var specialtyList = new List<SelectListItem>();

                    if (specialties.Any())
                    {
                        foreach (var specialty in specialties)
                        {
                            specialtyList.Add(new SelectListItem
                            {
                                Value = specialty.SpecialtyId.ToString(),
                                Text = specialty.Name
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

                    ViewBag.Specialty = specialtyList;
                    ViewBag.Gyms = gymList;
                    ViewBag.Languages = languagesList;
                    ViewBag.Nationalities = nationalityList;
                    ViewBag.Areas = areaList;
                    return View(viewModel);
                }
            }
            ViewBag.Current = "User Management";
            return View();
        }

        public IActionResult Edit(Guid u)
        {
            var viewModel = new CoachRenderViewModel();
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var retrieveResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest($"User/GetCoachUserProfile/{u}"));
                var gymResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Gym/Get/"));
                var specialtyResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Specialty/Get/"));
                var languageResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Language/Get/"));
                var nationalityResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Nationality/Get/"));
                var areaResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Facility/GetAreas"));
                if (!IsTokenInvalidUsingResponse(specialtyResponse, "Unathorized access.")
                   && !IsTokenInvalidUsingResponse(gymResponse, "Unathorized access.")
                   && !IsTokenInvalidUsingResponse(retrieveResponse, "Unauthorized access"))
                {
                    var specialties = JsonConvert.DeserializeObject<IEnumerable<Specialty>>(specialtyResponse.Payload.ToString());
                    var gyms = gymResponse != null ? JsonConvert.DeserializeObject<IEnumerable<Gym>>(gymResponse.Payload.ToString()) : new List<Gym>();
                    var languages = languageResponse != null ? JsonConvert.DeserializeObject<IEnumerable<Language>>(languageResponse.Payload.ToString()) : new List<Language>();
                    var nationalities = nationalityResponse != null ? JsonConvert.DeserializeObject<IEnumerable<Nationality>>(nationalityResponse.Payload.ToString()) : new List<Nationality>();
                    var areas = areaResponse != null ? JsonConvert.DeserializeObject<List<Area>>(areaResponse.Payload.ToString()) : new List<Area>();
                    viewModel = retrieveResponse != null ? JsonConvert.DeserializeObject<CoachRenderViewModel>(retrieveResponse.Payload.ToString()) : new CoachRenderViewModel();
                    var gymList = new List<SelectListItem>();

                    if (gyms.Any())
                    {
                        foreach (var gym in gyms)
                        {
                            gymList.Add(new SelectListItem
                            {
                                Value = gym.GymId.ToString(),
                                Text = gym.GymName
                            });
                        }
                    }

                    var nationalityList = new List<SelectListItem>();
                    nationalityList.Add(new SelectListItem
                    {
                        Text = "Select Nationality",
                        Value = Guid.Empty.ToString()
                    });

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

                    var languagesList = new List<SelectListItem>();

                    if (languages.Any())
                    {
                        foreach (var language in languages)
                        {
                            languagesList.Add(new SelectListItem
                            {
                                Value = language.LanguageId.ToString(),
                                Text = language._Language
                            });
                        }
                    }

                    var specialtyList = new List<SelectListItem>();

                    if (specialties.Any())
                    {
                        foreach (var specialty in specialties)
                        {
                            specialtyList.Add(new SelectListItem
                            {
                                Value = specialty.SpecialtyId.ToString(),
                                Text = specialty.Name
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

                    ViewBag.Specialty = specialtyList;
                    ViewBag.Gyms = gymList;
                    ViewBag.Languages = languagesList;
                    ViewBag.Nationalities = nationalityList;
                    ViewBag.Areas = areaList;
                    viewModel.UserId = u;
                    return View(viewModel);
                }
            }

            ViewBag.Current = "User Management";
            return View();
        }

        [HttpPost]
        public IActionResult AddOrEditUserCoach([FromBody] CoachRenderViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var saveUserResponse = new APIResponse();
                var userResponse = new User();
                if (viewModel.UserId == Guid.Empty)
                {
                    var user = new UserRegistration
                    {
                        FirstName = viewModel.FirstName,
                        LastName = viewModel.LastName,
                        Email = viewModel.Email,
                        Password = viewModel.Password,
                        UserType = EUserType.NORMALANDCOACH,
                        DevicePlatform = EDevicePlatform.Web,
                        MobileNumber = viewModel.MobileNumber,
                    };

                    saveUserResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("User/Register", user));
                    if(saveUserResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = saveUserResponse.Message, Status = saveUserResponse.Status });
                    }
                    userResponse = saveUserResponse != null ? JsonConvert.DeserializeObject<User>(saveUserResponse.Payload.ToString()) : new User();
                }

                var uploadProfileImg = new UserProfile
                {
                    UserId = viewModel.UserId == Guid.Empty ? userResponse.UserId : viewModel.UserId,
                    ImageUrl = viewModel.ImageUrl,
                    Description = viewModel.Description,
                    NationalityId = viewModel.NationalityId.Value,
                    FirstName = viewModel.FirstName,
                    LastName = viewModel.LastName,
                    Email = viewModel.Email,
                    MobileNumber = viewModel.MobileNumber,
                    IsActive = viewModel.IsActive,
                    Password = viewModel.IsPasswordEdit == true ? viewModel.Password : string.Empty
                };

                var updateProfileResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("User/UpdateUserProfile", uploadProfileImg));
                if (updateProfileResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var coachProfile = new CoachUpdateProfile
                    {
                        Name = $"{viewModel.FirstName} {viewModel.LastName}",
                        Languages = viewModel.LanguageIds,
                        Specialties = viewModel.SpecialtyIds,
                        GymsAccess = viewModel.GymIds,
                        UserID = viewModel.UserId == Guid.Empty ? userResponse.UserId : viewModel.UserId,
                        Description = viewModel.Description,
                        IsActive = viewModel.IsActive
                    };

                    var coachResponse = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("Coach/BecomeACoach", coachProfile));
                    if (coachResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return Json(new APIResponse { StatusCode = coachResponse.StatusCode, ModelError = coachResponse.ModelError, Payload = coachResponse.Payload });
                    }
                }


                return Json(new APIResponse { StatusCode = viewModel.UserId == Guid.Empty ? saveUserResponse.StatusCode : updateProfileResponse.StatusCode, 
                    Payload = viewModel.UserId == Guid.Empty ? saveUserResponse.Payload : updateProfileResponse.Payload, 
                    ModelError = viewModel.UserId == Guid.Empty ? saveUserResponse.ModelError : updateProfileResponse.ModelError,
                    Message = viewModel.UserId == Guid.Empty ? "Successfully Added a Coach" : "Coach Updated Successfully"
                });
            }

            var customErrors = ModelState.Values.SelectMany(x => x.Errors);
            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = customErrors.FirstOrDefault().ErrorMessage, ModelError = ModelState.Errors() });
        }

        [HttpPost]
        public IActionResult Status([FromBody] ChangeStatus userId)
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var response = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.PostHttpClientRequest("User/ChangeStatus", userId));

                if (base.IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                return Json(response);
            }
            return RedirectToAction("Logout", "Home");
        }

        public IActionResult Export()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(MainHTTPClient.GetHttpClientRequest("Coach/GetCoaches"));
                IEnumerable<CoachViewModel> returnList = new List<CoachViewModel>();
                if (!IsTokenInvalidUsingResponse(returnRes, "Unathorized access."))
                {
                    returnList = JsonConvert.DeserializeObject<IEnumerable<CoachViewModel>>(returnRes.Payload.ToString());
                    returnList = returnList.Any() ? returnList.OrderByDescending(r => r.DateCreated.Value).ToList() : returnList;
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Coach User Reports");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "Name";
                    worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 2).Value = "Email";
                    worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 3).Value = "Mobile Number";
                    worksheet.Cell(currentRow, 3).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 4).Value = "Date Created";
                    worksheet.Cell(currentRow, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                    //worksheet.Cell(currentRow, 5).Value = "Last Coaching";
                    //worksheet.Cell(currentRow, 5).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Cell(currentRow, 5).Value = "Status";
                    worksheet.Cell(currentRow, 5).Style.Fill.BackgroundColor = XLColor.Yellow;

                    foreach (var coach in returnList)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = coach.ProfileName;
                        worksheet.Cell(currentRow, 2).Value = coach.Email;
                        worksheet.Cell(currentRow, 3).Value = coach.MobileNo;
                        worksheet.Cell(currentRow, 4).Value = coach.DateCreated.GetValueOrDefault().ToString("dd/MM/yyyy");
                        //worksheet.Cell(currentRow, 5).Value = coach.LastCoachingDate;
                        if (coach.Status == "Active")
                        {
                            worksheet.Cell(currentRow, 5).Value = "ACTIVE";
                            worksheet.Cell(currentRow, 5).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        }
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
                        $"Coaches - {DateTime.Now}.xlsx");
                }
            }

            return View();
        }
    }
}
