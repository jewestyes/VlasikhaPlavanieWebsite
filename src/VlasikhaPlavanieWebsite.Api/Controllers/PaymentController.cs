using Microsoft.AspNetCore.Mvc;
using VlasikhaPlavanieWebsite.ViewModels;
using VlasikhaPlavanieWebsite.Application.Interfaces;
using VlasikhaPlavanieWebsite.Infrastructure.Exceptions.Payment;
using VlasikhaPlavanieWebsite.Controllers;

public class PaymentController : BaseController
{
	private readonly IPaymentService _paymentService;
	private readonly ILogger<PaymentController> _logger;

	public PaymentController(IPaymentService paymentService, IHomeService homeService, ILogger<PaymentController> logger)
		: base(homeService)
{
		_paymentService = paymentService;
		_logger = logger;
	}

	[HttpGet("payment/success")]
	public IActionResult PaymentSuccess()
	{
		return View();
	}

	[HttpGet("payment/failure")]
	public IActionResult PaymentFailure()
	{
		return View();
	}

	[HttpGet]
	public async Task<IActionResult> Payment(string orderId)
	{
		var paymentViewModel = new PaymentViewModel();

		try
		{
			paymentViewModel = await _paymentService.GetPaymentInfoAsync(orderId);
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning(ex, "Payment info not found for {OrderId}", orderId);
			return NotFound(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while preparing payment for {OrderId}", orderId);
			return StatusCode(500, "Internal server error");
		}

		return View(paymentViewModel);
	}

	[HttpPost]
	public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
	{
		if (!ModelState.IsValid)
			return View("Payment", model);

		try
		{
			var redirectUrl = await _paymentService.InitializePaymentAsync(model);
			return Redirect(redirectUrl);
		}
		catch (PaymentAmountMismatchException ex)
		{
			_logger.LogWarning(ex, ex.Message);
			ModelState.AddModelError("", "Сумма платежа не соответствует ожидаемой.");
			return View("Payment", model);
		}
		catch (TokenGenerationException ex)
		{
			_logger.LogError(ex, "Не удалось сгенерировать токен для OrderId={OrderId}", model.OrderId);
			ViewBag.ErrorMessage = "Ошибка генерации платёжного токена. Пожалуйста, попробуйте позже.";
			return View("PaymentFailure");
		}
		catch (PaymentInitializationException ex)
		{
			_logger.LogWarning(ex, "Ошибка инициализации платежа: {Code} / {Msg}", ex.ErrorCode, ex.Message);
			ViewBag.ErrorMessage = $"Ошибка при инициализации платежа: {ex.Message}";
			return View("PaymentFailure");
		}
		catch (PaymentApiException ex)
		{
			_logger.LogError(ex, "Ошибка связи с платёжным API: {Status} {Content}", ex.StatusCode, ex.ResponseContent);
			ViewBag.ErrorMessage = "Не удалось связаться с платёжным сервисом. Попробуйте позже.";
			return View("PaymentFailure");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Неожиданная ошибка при оплате OrderId={OrderId}", model.OrderId);
			ViewBag.ErrorMessage = "Внутренняя ошибка сервера при оплате. Попробуйте позже.";
			return View("PaymentFailure");
		}
	}
}