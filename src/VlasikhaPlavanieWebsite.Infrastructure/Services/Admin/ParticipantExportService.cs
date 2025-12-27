using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using VlasikhaPlavanieWebsite.Data;
using VlasikhaPlavanieWebsite.Models;
using VlasikhaPlavanieWebsite.ViewModels;

namespace VlasikhaPlavanieWebsite.Infrastructure.Services.Admin
{
	public class ParticipantExportService : IParticipantExportService
	{
		private readonly ApplicationDbContext _applicationDbContext;
		public ParticipantExportService(ApplicationDbContext applicationDbContext)
		{
			_applicationDbContext = applicationDbContext;
		}

		public async Task<Stream> ExportBycompetitionAsync(string competitionName)
		{
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

			// Выборка участников для указанного соревнования
			var query = from p in _applicationDbContext.Participants
						join o in _applicationDbContext.Orders on p.OrderId equals o.Id into po
						from order in po.DefaultIfEmpty()
						join d in _applicationDbContext.Disciplines on p.Id equals d.ParticipantId into pd
						from discipline in pd.DefaultIfEmpty()
                                                join rs in _applicationDbContext.Competitions on order.CompetitionId equals rs.Id
                                                where rs.Name == competitionName && order != null && order.Status == OrderStatus.Paid
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
                                                        OrderId = order.Id,
                                                        CreatedAt = order.CreatedAt,
                                                        Email = p.Email,
                                                        DisciplineName = discipline != null ? discipline.Name : null,
                                                        Distance = discipline != null ? discipline.Distance : null,
							EntryTime = discipline != null ? discipline.EntryTime : null,
							OrderNumber = order != null ? order.OrderNumber : null,
							Amount = order != null ? order.Amount : 0m,
							RegistrationCompetitionName = rs.Name
						};

			var participants = await query.ToListAsync();

			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add("Participants_" + competitionName);

                                // Заголовки колонок
                                worksheet.Cells[1, 1].Value = "Дата заказа";
                                worksheet.Cells[1, 2].Value = "OrderId";
                                worksheet.Cells[1, 3].Value = "Фамилия";
                                worksheet.Cells[1, 4].Value = "Имя";
                                worksheet.Cells[1, 5].Value = "Отчество";
                                worksheet.Cells[1, 6].Value = "Дата рождения";
                                worksheet.Cells[1, 7].Value = "Пол";
                                worksheet.Cells[1, 8].Value = "Город/Команда";
                                worksheet.Cells[1, 9].Value = "Разряд";
                                worksheet.Cells[1, 10].Value = "Телефон";
                                worksheet.Cells[1, 11].Value = "Email";
                                worksheet.Cells[1, 12].Value = "Дисциплина";
                                worksheet.Cells[1, 13].Value = "Дистанция";
                                worksheet.Cells[1, 14].Value = "Заявочное время";

				// Добавление данных
                                for (int i = 0; i < participants.Count; i++)
                                {
                                        var participant = participants[i];
                                        worksheet.Cells[i + 2, 1].Value = participant.CreatedAt.AddHours(3).ToString("dd.MM.yyyy HH:mm");
                                        worksheet.Cells[i + 2, 2].Value = participant.OrderId;
                                        worksheet.Cells[i + 2, 3].Value = participant.LastName;
                                        worksheet.Cells[i + 2, 4].Value = participant.FirstName;
                                        worksheet.Cells[i + 2, 5].Value = participant.MiddleName;
                                        worksheet.Cells[i + 2, 6].Value = participant.BirthDate.ToString("dd.MM.yyyy");
                                        worksheet.Cells[i + 2, 7].Value = participant.Gender;
                                        worksheet.Cells[i + 2, 8].Value = participant.CityOrTeam;
                                        worksheet.Cells[i + 2, 9].Value = participant.Rank;
                                        worksheet.Cells[i + 2, 10].Value = participant.Phone;
                                        worksheet.Cells[i + 2, 11].Value = participant.Email;
                                        worksheet.Cells[i + 2, 12].Value = participant.DisciplineName;
                                        worksheet.Cells[i + 2, 13].Value = participant.Distance;
                                        worksheet.Cells[i + 2, 14].Value = participant.EntryTime;
                                }

                                using (var range = worksheet.Cells[1, 1, 1, 14])
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

				return stream;
			}
		}
	}
}