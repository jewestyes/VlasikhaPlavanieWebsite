using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Serilog;
using System.Threading.Tasks;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;

namespace VlasikhaPlavanieWebsite.Controllers
{
    
    public class AdminController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly string _filesDirectory;

        public AdminController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<HomeController> logger)
        {
            _logger = logger;
			_webHostEnvironment = webHostEnvironment;
            _signInManager = signInManager;
            _userManager = userManager;
			_context = context;
			_filesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Admin/Login")]
        public IActionResult Login()
        {
            return View();
        }


		[HttpPost]
		[Route("Admin/Login")]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
			{
				Log.Warning("Invalid model state for login attempt. Email: {Email}, ModelState: {ModelStateErrors}",
							model.Email,
							string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
				return View(model);
			}

			try
			{
				Log.Information("Login attempt for user with email: {Email}", model.Email);

				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user == null)
				{
					Log.Warning("No user found with email: {Email}", model.Email);
					ModelState.AddModelError("", "Invalid login attempt");
					return View(model);
				}

				if (!await _userManager.IsInRoleAsync(user, "Admin"))
				{
					Log.Warning("User {Email} is not in role Admin", model.Email);
					ModelState.AddModelError("", "Invalid login attempt");
					return View(model);
				}

				var checkPasswordResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
				if (!checkPasswordResult.Succeeded)
				{
					Log.Warning("Password verification failed for user: {Email}", model.Email);
					ModelState.AddModelError("", "Invalid login attempt");
					return View(model);
				}

				HttpContext.Session.Clear();
				await _signInManager.SignInAsync(user, false);
				Log.Information("User {Email} successfully signed in", model.Email);

				return RedirectToAction("Index", "Admin");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "An error occurred while attempting to log in user: {Email}", model.Email);
				ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
				return View(model);
			}
		}


		[HttpGet]
		[Route("Admin/Logout")]
		public async Task<IActionResult> Logout()
		{
			var userName = User.Identity?.Name ?? "Unknown user";
			Log.Information("User {UserName} is attempting to log out.", userName);

			await _signInManager.SignOutAsync();
			Log.Information("User {UserName} has successfully logged out.", userName);

			foreach (var cookie in Request.Cookies.Keys)
			{
				Log.Information("Deleting cookie: {CookieName} for user {UserName}", cookie, userName);
				Response.Cookies.Delete(cookie);
			}

			Log.Information("All cookies deleted for user {UserName}.", userName);

			return RedirectToAction("Index", "Home");
		}

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("Admin/Index")]
        public async Task<IActionResult> Index()
        {
            var query = from p in _context.Participants
                        join o in _context.Orders on p.OrderId equals o.Id into po
                        from order in po.DefaultIfEmpty()
                        join d in _context.Disciplines on p.Id equals d.ParticipantId into pd
                        from discipline in pd.DefaultIfEmpty()
                        join rs in _context.RegistrationStage on order.RegistrationStageId equals rs.Id into ors
                        from regStage in ors.DefaultIfEmpty()
                        select new ParticipantOrderViewModel
                        {
                            LastName = p.LastName,
                            FirstName = p.FirstName,
                            MiddleName = p.MiddleName,
                            BirthDate = p.BirthDate,
                            Gender = p.Gender,
                            CityOrTeam = p.CityOrTeam,
                            Rank = p.Rank,
                            Phone = p.Phone,
                            CreatedAt = order.CreatedAt,
                            Email = p.Email,
                            DisciplineName = discipline != null ? discipline.Name : null,
                            Distance = discipline != null ? discipline.Distance : null,
                            EntryTime = discipline != null ? discipline.EntryTime : null,
                            OrderNumber = order != null ? order.OrderNumber : null,
                            Amount = order != null ? order.Amount : 0m,
                            RegistrationStageName = regStage != null ? regStage.StageName : "Неизвестный этап"
                        };

            var result = await query.ToListAsync();
            return View(result);
        }



        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("Admin/DownloadParticipantsExcelByStage")]
        public async Task<IActionResult> DownloadParticipantsExcelByStage(string stageName)
        {
            if (string.IsNullOrEmpty(stageName))
            {
                return BadRequest("Не указан этап регистрации.");
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Выборка участников для указанного этапа
            var query = from p in _context.Participants
                        join o in _context.Orders on p.OrderId equals o.Id into po
                        from order in po.DefaultIfEmpty()
                        join d in _context.Disciplines on p.Id equals d.ParticipantId into pd
                        from discipline in pd.DefaultIfEmpty()
                        join rs in _context.RegistrationStage on order.RegistrationStageId equals rs.Id
                        where rs.StageName == stageName
                        select new ParticipantOrderViewModel
                        {
                            LastName = p.LastName,
                            FirstName = p.FirstName,
                            MiddleName = p.MiddleName,
                            BirthDate = p.BirthDate,
                            Gender = p.Gender,
                            CityOrTeam = p.CityOrTeam,
                            Rank = p.Rank,
                            Phone = p.Phone,
                            CreatedAt = order.CreatedAt,
                            Email = p.Email,
                            DisciplineName = discipline != null ? discipline.Name : null,
                            Distance = discipline != null ? discipline.Distance : null,
                            EntryTime = discipline != null ? discipline.EntryTime : null,
                            OrderNumber = order != null ? order.OrderNumber : null,
                            Amount = order != null ? order.Amount : 0m,
                            RegistrationStageName = rs.StageName
                        };

            var participants = await query.ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Participants_" + stageName);

                // Заголовки колонок
                worksheet.Cells[1, 1].Value = "Дата заказа";
                worksheet.Cells[1, 2].Value = "Фамилия";
                worksheet.Cells[1, 3].Value = "Имя";
                worksheet.Cells[1, 4].Value = "Отчество";
                worksheet.Cells[1, 5].Value = "Дата рождения";
                worksheet.Cells[1, 6].Value = "Пол";
                worksheet.Cells[1, 7].Value = "Город/Команда";
                worksheet.Cells[1, 8].Value = "Разряд";
                worksheet.Cells[1, 9].Value = "Телефон";
                worksheet.Cells[1, 10].Value = "Email";
                worksheet.Cells[1, 11].Value = "Дисциплина";
                worksheet.Cells[1, 12].Value = "Дистанция";
                worksheet.Cells[1, 13].Value = "Заявочное время";

                // Добавление данных
                for (int i = 0; i < participants.Count; i++)
                {
                    var participant = participants[i];
                    worksheet.Cells[i + 2, 1].Value = participant.CreatedAt.AddHours(3).ToString("dd.MM.yyyy HH:mm");
                    worksheet.Cells[i + 2, 2].Value = participant.LastName;
                    worksheet.Cells[i + 2, 3].Value = participant.FirstName;
                    worksheet.Cells[i + 2, 4].Value = participant.MiddleName;
                    worksheet.Cells[i + 2, 5].Value = participant.BirthDate.ToString("dd.MM.yyyy");
                    worksheet.Cells[i + 2, 6].Value = participant.Gender;
                    worksheet.Cells[i + 2, 7].Value = participant.CityOrTeam;
                    worksheet.Cells[i + 2, 8].Value = participant.Rank;
                    worksheet.Cells[i + 2, 9].Value = participant.Phone;
                    worksheet.Cells[i + 2, 10].Value = participant.Email;
                    worksheet.Cells[i + 2, 11].Value = participant.DisciplineName;
                    worksheet.Cells[i + 2, 12].Value = participant.Distance;
                    worksheet.Cells[i + 2, 13].Value = participant.EntryTime;
                }

                using (var range = worksheet.Cells[1, 1, 1, 13])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Participants_{stageName}_{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(stream, contentType, fileName);
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
		[Route("Admin/EditFiles")]
        public async Task<IActionResult> EditFiles()
		{
			try
			{
                var fileMappings = await _context.FileMappings
                                .Select(f => new
                                {
                                    f.ButtonName,
                                    FileName = f.FileName ?? "#",
                                    FilePath = f.FilePath ?? "#"
                                })
                                .ToListAsync();
                
                var buttonFiles = fileMappings.ToDictionary(
                    f => f.ButtonName,
                    f => f.FilePath
                );

                foreach (var key in buttonFiles.Keys.ToList())
                {
                    var relativePath = buttonFiles[key];

                    if (!relativePath.StartsWith("http"))
                    {
                        var fileName = Path.GetFileName(relativePath);
                        var fileDirectory = Path.Combine("Files", fileName);

                        var absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, "Files", fileName);

                        if (System.IO.File.Exists(absolutePath))
                        {
                            buttonFiles[key] = Url.Content($"~/Files/{fileName}");
                        }
                        else
                        {
                            buttonFiles[key] = "#";
                        }
                    }
                }

                return View(buttonFiles);
            }
			catch (Exception ex)
			{
                _logger.LogError($"[HomeController] [Index] ERROR: {ex.Message}");
                return View(new Dictionary<string, string>());
			}
		}


        [Authorize(Roles = "Admin")]
        [HttpPost]
		[Route("Admin/EditFiles")]
		public async Task<IActionResult> EditFiles(string buttonName, IFormFile newFile)
		{
			if (newFile != null && newFile.Length > 0)
			{
				var extension = Path.GetExtension(newFile.FileName);
				var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".xls", "txt" };

				if (!allowedExtensions.Contains(extension.ToLower()))
				{
					ModelState.AddModelError("", "Недопустимый формат файла.");
					return RedirectToAction("EditFiles");
				}

				var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "Files");

				if (!Directory.Exists(uploadPath))
				{
					Directory.CreateDirectory(uploadPath);
				}

				var fileName = $"{buttonName}_{DateTime.Now.Ticks}{extension}";
				var filePath = Path.Combine(uploadPath, fileName);

				var fileMapping = await _context.FileMappings.FirstOrDefaultAsync(f => f.ButtonName == buttonName);

				if (fileMapping != null && !string.IsNullOrEmpty(fileMapping.FilePath))
				{
					var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, fileMapping.FilePath);
					if (System.IO.File.Exists(oldFilePath))
					{
						System.IO.File.Delete(oldFilePath);
					}
				}

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await newFile.CopyToAsync(stream);
				}

				if (fileMapping == null)
				{
					fileMapping = new FileMapping
					{
						ButtonName = buttonName,
						FileName = newFile.FileName,
						FilePath = Path.Combine("Files", fileName)
					};
					_context.FileMappings.Add(fileMapping);
				}
				else
				{
					fileMapping.FileName = newFile.FileName;
					fileMapping.FilePath = Path.Combine("Files", fileName);
					_context.FileMappings.Update(fileMapping);
				}

				await _context.SaveChangesAsync();
			}

			return RedirectToAction("EditFiles");
		}


        [Authorize(Roles = "Admin")]
        [HttpPost]
		[Route("Admin/DeleteFile")]
		public async Task<IActionResult> DeleteFile(string buttonName)
		{
			if (string.IsNullOrEmpty(buttonName))
			{
				return BadRequest("ButtonName не может быть пустым.");
			}

			var fileMapping = await _context.FileMappings.FirstOrDefaultAsync(f => f.ButtonName == buttonName);

			if (fileMapping != null)
			{
				if (!string.IsNullOrEmpty(fileMapping.FilePath) && System.IO.File.Exists(fileMapping.FilePath))
				{
					System.IO.File.Delete(fileMapping.FilePath);
				}

				fileMapping.FilePath = "#";
				fileMapping.FileName = "#";
				_context.FileMappings.Update(fileMapping);

				await _context.SaveChangesAsync();
			}

			return RedirectToAction("EditFiles");
		}

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("Admin/ManageStages")]
        public async Task<IActionResult> ManageStages()
        {
            var stages = await _context.RegistrationStage.ToListAsync();
            var model = new ManageStagesViewModel
            {
                Stages = stages,
                NewStage = new RegistrationStage()
            };
            return View(model);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Admin/CreateStage")]
        public async Task<IActionResult> CreateStage(ManageStagesViewModel model)
        {
            ModelState.Remove("Stages");
            if (ModelState.IsValid)
            {
                model.NewStage.IsOpen = false;
                _context.RegistrationStage.Add(model.NewStage);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageStages");
            }
            model.Stages = await _context.RegistrationStage.ToListAsync();
            return View("ManageStages", model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Admin/ChangeStageStatus")]
        public async Task<IActionResult> ChangeStageStatus(int id, bool isOpen)
        {
            var stage = await _context.RegistrationStage.FindAsync(id);
            if (stage != null)
            {
                stage.IsOpen = isOpen;
                if (!isOpen)
                {
                    stage.RegistrationEndDate = DateTime.UtcNow.AddHours(3);
                }
                else
                {
                    stage.RegistrationEndDate = null;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageStages");
        }
    }
}
