using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Model;
using Sidekick.Model.Gym;
using Sidekick.Model.SetupConfiguration;
using Sidekick.Model.SetupConfiguration.Goals;
using Sidekick.Model.SetupConfiguration.Level;
using Sidekick.Model.SetupConfiguration.Size;
using Sidekick.Model.Specialty;
using System;
using System.Collections.Generic;

namespace Sidekick.Admin.Controllers
{
    public class SetupController : BaseController
    {
        private IMainHttpClient AdminHttpClient { get; }
        public AdminUserContext AdminUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public SetupController(IMainHttpClient _mhttpc,
            IHttpContextAccessor httpContextAccessor,
            ConfigMaster _conf)
        {
            AdminHttpClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            AdminUCtxt = _session.GetObjectFromJson<AdminUserContext>("adminUserContext");
        }

        /// <summary>
        ///  Index page for all Setup Configuration
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            if (IsUserLoggedIn(AdminUCtxt))
            {
                var surfaceResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Surface/Get/"));
                var locationResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Location/Get/"));
                var teamSizeResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Size/Get/"));
                var sportsResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Sport/Get/"));
                var specialtyResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Specialty/Get/"));
                var goalResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Goal/Get/"));
                var gymResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Gym/Get/"));
                var levelResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Level/Get/"));
                var roleResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Facility/FacilityUserTypes/"));
                var areaResponse = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.GetHttpClientRequest("Facility/GetAreas"));

                    if (!IsTokenInvalidUsingResponse(surfaceResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(locationResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(teamSizeResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(sportsResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(specialtyResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(goalResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(gymResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(levelResponse, "Unathorized access.")
                    && !IsTokenInvalidUsingResponse(roleResponse, "Unathorized access."))
                {
                    var surface = JsonConvert.DeserializeObject<IEnumerable<Surface>>(surfaceResponse.Payload.ToString());
                    var location = JsonConvert.DeserializeObject<IEnumerable<Location>>(locationResponse.Payload.ToString());
                    var teamSize = JsonConvert.DeserializeObject<IEnumerable<TeamSize>>(teamSizeResponse.Payload.ToString());
                    var sports = JsonConvert.DeserializeObject<IEnumerable<Sport>>(sportsResponse.Payload.ToString());
                    var specialty = JsonConvert.DeserializeObject<IEnumerable<Specialty>>(specialtyResponse.Payload.ToString());
                    var goal = JsonConvert.DeserializeObject<IEnumerable<Goal>>(goalResponse.Payload.ToString());
                    var gym = JsonConvert.DeserializeObject<IEnumerable<Gym>>(gymResponse.Payload.ToString());
                    var level = JsonConvert.DeserializeObject<IEnumerable<Level>>(levelResponse.Payload.ToString());
                    var role = JsonConvert.DeserializeObject<IEnumerable<FacilityUserType>>(roleResponse.Payload.ToString());
                    var areas = areaResponse != null ? JsonConvert.DeserializeObject<List<Area>>(areaResponse.Payload.ToString()) : new List<Area>();

                    var setupConfigurations = new SetupConfiguration
                    {
                        Surfaces = surface,
                        Locations = location,
                        TeamSize = teamSize,
                        Sports = sports,
                        Specialties = specialty,
                        Goals = goal,
                        Gyms = gym,
                        Levels = level,
                        FacilityUserTypes = role
                    };

                    ViewBag.Current = "Setup";
                    ViewBag.Areas = areas;
                    return View(setupConfigurations);
                }
            }
            ViewBag.Current = "Setup";
            return View();
        }

        #region AddOrEdit SetupConfiguration
        /// <summary>
        ///  Adds or Edit a Surface
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddOrEditSurface(Surface surface)
        {
            if (!string.IsNullOrWhiteSpace(surface.Name) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Surface/AddOrEdit/", surface));

                    if (base.IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "true";
                    }

                    else
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "false";
                    }

                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        ///  Adds or Edit a Location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddOrEditLocation(Location location)
        {
            if (!string.IsNullOrWhiteSpace(location.Name) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Location/AddOrEdit/", location));

                    if (base.IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "true";
                    }

                    else
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "false";
                    }

                    ViewBag.Current = "Setup";
                    return View("Index");

                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        ///  Adds or Edits a Team Size
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddOrEditTeamSize(TeamSize size)
        {
            if (!string.IsNullOrWhiteSpace(size.SizeName) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Size/AddOrEdit/", size));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "true";
                    }

                    else
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "false";
                    }

                    ViewBag.Current = "Setup";
                    return View("Index");

                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        ///  Adds a Sport
        /// </summary>
        /// <param name="sport"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddSport(SportDto sport)
        {
            if (!string.IsNullOrWhiteSpace(sport.Name) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Sport/Add/", sport));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "true";
                    }

                    else
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "false";
                    }

                    ViewBag.Current = "Setup";
                    return View("Index");

                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        ///  Edits a Sport
        /// </summary>
        /// <param name="sport"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditSport(SportDto sport)
        {
            if (!string.IsNullOrWhiteSpace(sport.Name) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Sport/Edit/", sport));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "true";
                    }

                    else
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "false";
                    }

                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        ///  Adds or Edits a Specialty
        /// </summary>
        /// <param name="specialty"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddOrEditSpecialty(Specialty specialty)
        {
            if (!string.IsNullOrWhiteSpace(specialty.Name) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Specialty/AddOrEdit/", specialty));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "true";
                    }

                    else
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "false";
                    }

                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        ///  Adds or Edits a Goal
        /// </summary>
        /// <param name="goal"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddOrEditGoal(Goal goal)
        {
            if (!string.IsNullOrWhiteSpace(goal.Name) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Goal/AddOrEdit/", goal));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "true";
                    }

                    else
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "false";
                    }

                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        ///  Adds or Edits a Gym
        /// </summary>
        /// <param name="gym"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddOrEditGym(Gym gym)
        {
            if (!string.IsNullOrWhiteSpace(gym.GymName) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Gym/AddOrEdit/", gym));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "true";
                    }

                    else
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "false";
                    }

                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        /// Adds or Edit a Level
        /// </summary>
        /// <param name="goal"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddOrEditLevel(Level level)
        {
            if (!string.IsNullOrWhiteSpace(level.Name) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Level/AddOrEdit/", level));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "true";
                    }

                    else
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "false";
                    }

                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        /// Adds or Edit a Role
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddOrEditRole(FacilityUserType role)
        {
            if (ModelState.IsValid)
            {
                APIResponse returnRes = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Facility/FacilityUserTypes", role));

                if (base.IsTokenInvalidUsingResponse(returnRes, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                ViewBag.ModalTitle = returnRes.Message;

                if (returnRes.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ViewBag.ShowModal = "true";
                }

                else
                {
                    ViewBag.ShowModal = "false";
                }

                ViewBag.Current = "Setup";
                return View("Index");

            }
            else
            {
                return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Invalid Object!", ModelError = ModelState.Errors() });
            }
        }
        #endregion

        #region DeleteSetupConfiguration
        /// <summary>
        /// Set the record IsEnabled false for that sportId
        /// </summary>
        /// <param name="sportId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteSport(Guid sportId)
        {
            if (!string.IsNullOrWhiteSpace(sportId.ToString()) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    APIResponse response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest($"Sport/Delete/{sportId}", sportId));

                    if (base.IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    if(response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "true";
                    }
                    else
                    {
                        ViewBag.ModalTitle = response.Status;
                        ViewBag.ModalMessage = response.Message;
                        ViewBag.ShowModal = "false";
                    }

                    ViewBag.Current = "Setup";
                    return View("Index");

                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        /// Set the record IsEnabled false for that surfaceId
        /// </summary>
        /// <param name="surfaceId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteSurface(Guid surfaceId)
        {
            if (!string.IsNullOrWhiteSpace(surfaceId.ToString()) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest($"Surface/Delete/{surfaceId}", surfaceId));

                    if (base.IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                    ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                    ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        /// Set the record IsEnabled false for that locationId
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteLocation(Guid locationId)
        {
            if (!string.IsNullOrWhiteSpace(locationId.ToString()) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest($"Location/Delete/{locationId}", locationId));

                    if (base.IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                    ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                    ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        /// Set the record IsEnabled false for that sizeId
        /// </summary>
        /// <param name="sizeId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteSize(Guid sizeId)
        {
            if (!string.IsNullOrWhiteSpace(sizeId.ToString()) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest($"Size/Delete/{sizeId}", sizeId));

                    if (base.IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                    ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                    ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        /// Set the record IsEnabled false for that gymId
        /// </summary>
        /// <param name="gymId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteGym(Guid gymId)
        {
            if (!string.IsNullOrWhiteSpace(gymId.ToString()) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest($"Gym/Delete/{gymId}", gymId));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                    ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                    ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        /// Set the record IsEnabled false for that specialtyId
        /// </summary>
        /// <param name="specialtyId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteSpecialty(Guid specialtyId)
        {
            if (!string.IsNullOrWhiteSpace(specialtyId.ToString()) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest($"Specialty/Delete/{specialtyId}", specialtyId));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                    ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                    ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        /// Set the record IsEnabled false for that goalId
        /// </summary>
        /// <param name="goalId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteGoal(Guid goalId)
        {
            if (!string.IsNullOrWhiteSpace(goalId.ToString()) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest($"Goal/Delete/{goalId}", goalId));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                    ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                    ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        /// Set the record IsEnabled false for that levelId
        /// </summary>
        /// <param name="levelId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteLevel(Guid levelId)
        {
            if (!string.IsNullOrWhiteSpace(levelId.ToString()) && ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest($"Level/Delete/{levelId}", levelId));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                    ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                    ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Name is Required", ModelError = ModelState.Errors() });
        }

        /// <summary>
        /// Set the record IsEnabled false for that levelId
        /// </summary>
        /// <param name="facilityRoleId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteRole(FacilityUserType role)
        {
            if (ModelState.IsValid)
            {
                if (IsUserLoggedIn(AdminUCtxt))
                {
                    var response = JsonConvert.DeserializeObject<APIResponse>(AdminHttpClient.PostHttpClientRequest("Facility/FacilityUserTypes/Delete", role));

                    if (IsTokenInvalidUsingResponse(response, "Unathorized access.")) { return RedirectToAction("Logout", "Home"); }

                    ViewBag.ModalTitle = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Status : response.Status;
                    ViewBag.ModalMessage = response.StatusCode == System.Net.HttpStatusCode.OK ? response.Message : response.Message;
                    ViewBag.ShowModal = response.StatusCode == System.Net.HttpStatusCode.OK ? "true" : "false";
                    ViewBag.Current = "Setup";
                    return View("Index");
                }
            }

            return Json(new APIResponse { StatusCode = System.Net.HttpStatusCode.BadRequest, Message = "Invalid Object!", ModelError = ModelState.Errors() });
        }
        #endregion
    }
}
