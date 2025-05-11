using System.Net;

public class PaymentApiException : Exception
{
	public HttpStatusCode StatusCode { get; }
	public string ResponseContent { get; }

	public PaymentApiException(HttpStatusCode statusCode, string responseContent)
		: base($"Tinkoff API error {statusCode}: {responseContent}")
	{
		StatusCode = statusCode;
		ResponseContent = responseContent;
	}
}