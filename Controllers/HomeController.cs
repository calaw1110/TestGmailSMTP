
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using MimeKit;
using System.Diagnostics;
using TestGmailSMTP.Models;

namespace TestGmailSMTP.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly EmailConfiguration _emailConfig;
		private IWebHostEnvironment _hostEnvironment;
		public HomeController(ILogger<HomeController> logger, EmailConfiguration emailConfig, IWebHostEnvironment webHostEnvironment)
		{
			_logger = logger;
			_emailConfig = emailConfig;
			_hostEnvironment = webHostEnvironment;
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult SubmitMail()
		{
			SendEmail();

			return View("Index");
		}
		private bool SendEmail()
		{
			bool sendResult = false;
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("冠智測試", _emailConfig.From));
			message.To.Add(new MailboxAddress("喬欸", "brdgwen01@eri.com.tw"));
			message.Subject = "How you doin?";

			var body = new TextPart("plain")
			{
				Text = @"測試使用MailKit+gmailSMTP寄信"
			};


			// 夾帶附件-圖片測試
			
			List<MimePart> mimeParts = new List<MimePart>();
			string[] img = new string[] { "a", "b", "c", "d", "e" };

			foreach(var item in img)
			{
				string filePath = Path.Combine(_hostEnvironment.ContentRootPath, $"images/{item}.jpg");
				var attachment = new MimePart("image", "jpg")
				{

					Content = new MimeContent(System.IO.File.OpenRead(filePath), ContentEncoding.Default),
					ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
					ContentTransferEncoding = ContentEncoding.Base64,
					FileName = Path.GetFileName(filePath)
				};
				mimeParts.Add(attachment);
			}

			
			var multipart = new Multipart("mixed");
			multipart.Add(body);
			foreach (var item in mimeParts){
				multipart.Add(item);
			}
			message.Body = multipart;
			using (var client = new MailKit.Net.Smtp.SmtpClient())
			{
				try
				{
					client.Connect(_emailConfig.SmtpServer, _emailConfig.Port);
					// 無使用oauth2
					client.AuthenticationMechanisms.Remove("XOAUTH2");
					// 使用帳號及應用程式密碼驗證
					client.Authenticate(_emailConfig.UserName, _emailConfig.Password);
					client.Send(message);
				}
				catch
				{
					throw;
				}
				finally
				{
					client.Disconnect(true);
					client.Dispose();
				}

			}
			return sendResult;
		}
		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}