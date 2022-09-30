using Microsoft.AspNetCore.Mvc;
using System.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Firebase.Storage;
using Firebase.Auth;
using System.Text;
using pdfapis.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace pdfapis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfConverter : ControllerBase
    {
		// The authentication key (API Key).
		// Get your own by registering at https://app.pdf.co
		const String API_KEY = "mrltibs@gmail.com_5d6b8cd7cd877a4abb86bd9490a8724b325f2d900974331b7ad84e539ddf58ac9cf14bf3";

		// Direct URL of source PDF file.
		// You can also upload your own file into PDF.co and use it as url. Check "Upload File" samples for code snippets: https://github.com/bytescout/pdf-co-api-samples/tree/master/File%20Upload/
		
		// Comma-separated list of page indices (or ranges) to process. Leave empty for all pages. Example: '0,2-5,7-'.
		const string Pages = "";
		// PDF document password. Leave empty for unprotected documents.
		const string Password = "";
		// Destination TXT file name
		const string DestinationFile = @".\result.html";
		// (!) Make asynchronous job
		const bool Async = true;
		// GET: api/<PdfConverter>
		static string CheckJobStatus(string jobId)
		{
			using (WebClient webClient = new WebClient())
			{
				// Set API Key
				webClient.Headers.Add("x-api-key", API_KEY);

				string url = "https://api.pdf.co/v1/job/check?jobid=" + jobId;

				string response = webClient.DownloadString(url);
				JObject json = JObject.Parse(response);

				return Convert.ToString(json["status"]);
			}
		}
		[HttpPost]
        public async Task<ActionResult<LinkModel>> PdftoText(LinkModel model)
        {
			
			/*return new string[] {"how" ,"to"};*/
			// Create standard .NET web client instance
			WebClient webClient = new WebClient();

			// Set API Key
			webClient.Headers.Add("x-api-key", API_KEY);
			
			// URL for `PDF To Text` API call
			string url = "https://api.pdf.co/v1/pdf/convert/to/html";

			// Prepare requests params as JSON
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("name", Path.GetFileName(DestinationFile));
			parameters.Add("password", Password);
			parameters.Add("pages", Pages);
			parameters.Add("url", model.link);
			parameters.Add("async", Async);

			// Convert dictionary of params to JSON
			string jsonPayload = JsonConvert.SerializeObject(parameters);

			try
			{
				// Execute POST request with JSON payload
				string response = webClient.UploadString(url, jsonPayload);
				Console.WriteLine(model.link);
				// Parse JSON response
				JObject json = JObject.Parse(response);
				string resultFileUrl = json["url"].ToString();
				

				
				if (json["error"].ToObject<bool>() == false)
				{
					
					// Asynchronous job ID
					string jobId = json["jobId"].ToString();
					// URL of generated TXT file that will be available after the job completion
					

					// Check the job status in a loop. 
					// If you don't want to pause the main thread you can rework the code 
					// to use a separate thread for the status checking and completion.
					do
					{
						string status = CheckJobStatus(jobId); // Possible statuses: "working", "failed", "aborted", "success".

						// Display timestamp and status (for demo purposes)
						Console.WriteLine(DateTime.Now.ToLongTimeString() + ": " + status);

						if (status == "success")
						{
							// Download TXT file
							webClient.DownloadFile(resultFileUrl, DestinationFile);
							string s = webClient.DownloadString(resultFileUrl);
							Console.WriteLine("Generated TXT file saved as \"{0}\" file.", DestinationFile);
							webClient.Dispose();
							LinkModel m = new LinkModel();
							m.link = resultFileUrl;
							return Ok(m);
							break;
						}
						else if (status == "working")
						{
							// Pause for a few seconds
							Thread.Sleep(5000);
						}
						else
						{
							Console.WriteLine(status);
							break;
						}
					}
					while (true);
				}
				else
				{
					Console.WriteLine(json["message"].ToString());
				}
			}
			catch (WebException e)
			{
				Console.WriteLine(e.StackTrace);
				webClient.Dispose();
				return BadRequest(e.Message);
			}

			webClient.Dispose();
			return BadRequest();

		}

       /* // GET api/<PdfConverter>/5
        [HttpPost("data")]
        public async Task<string> GetAsync(string data)
        {

			var client = new HttpClient();
			var request = new HttpRequestMessage
			{
				Method = HttpMethod.Post,
				RequestUri = new Uri("https://bionic-reading1.p.rapidapi.com/convert"),
				Headers =
	{
		{ "X-RapidAPI-Key", "6018235053msh08a88646e7c8e4fp11422cjsn5ea71d54cbd9" },
		{ "X-RapidAPI-Host", "bionic-reading1.p.rapidapi.com" },
	},
				Content = new FormUrlEncodedContent(new Dictionary<string, string>
	{
		{ "content", ""+data },
		{ "response_type", "html" },
		{ "request_type", "html" },
		{ "fixation", "1" },
		{ "saccade", "10" },
	}),
			};
			using (var response = await client.SendAsync(request))
			{
				//response.EnsureSuccessStatusCode();
				var body = await response.Content.ReadAsStringAsync();
				Console.WriteLine(body);
				toPdf();
				


                return await FirebaseUploadAsync();
            }
			return "none";
        }

		static void toPdf()
		{
			// HTML input
			const string path = @".\result.html";
			//FileStream stream = System.IO.File.ReadAllText(path);
			string inputSample = System.IO.File.ReadAllText(path);

			// Destination PDF file name
			string destinationFile = @".\result.pdf";

			// Create standard .NET web client instance
			WebClient webClient = new WebClient();

			// Set API Key
			webClient.Headers.Add("x-api-key", API_KEY);

			// Set JSON content type
			webClient.Headers.Add("Content-Type", "application/json");

			try
			{
				// Prepare requests params as JSON
				// See documentation: https://apidocs.pdf.co/?#1-json-pdfconvertfromhtml
				Dictionary<string, object> parameters = new Dictionary<string, object>();

				// Input HTML code to be converted. Required. 
				parameters.Add("html", inputSample);

				// Name of resulting file
				parameters.Add("name", Path.GetFileName(destinationFile));

				// Set to css style margins like 10 px or 5px 5px 5px 5px.
				parameters.Add("margins", "5px 5px 5px 5px");

				// Can be Letter, A4, A5, A6 or custom size like 200x200
				parameters.Add("paperSize", "Letter");

				// Set to Portrait or Landscape. Portrait by default.
				parameters.Add("orientation", "Portrait");

				// true by default. Set to false to disbale printing of background.
				parameters.Add("printBackground", true);

				// If large input document, process in async mode by passing true
				parameters.Add("async", true);

				// Set to HTML for header to be applied on every page at the header.
				parameters.Add("header", "");

				// Set to HTML for footer to be applied on every page at the bottom.
				parameters.Add("footer", "");


				// Convert dictionary of params to JSON
				string jsonPayload = JsonConvert.SerializeObject(parameters);


				// Prepare URL for `HTML to PDF` API call
				string url = "https://api.pdf.co/v1/pdf/convert/from/html";

				// Execute POST request with JSON payload
				string response = webClient.UploadString(url, jsonPayload);

				// Parse JSON response
				JObject json = JObject.Parse(response);

				if (json["error"].ToObject<bool>() == false)
				{
					// Asynchronous job ID
					string jobId = json["jobId"].ToString();
					// URL of generated PDF file that will available after the job completion
					string resultFileUrl = json["url"].ToString();

					// Check the job status in a loop. 
					// If you don't want to pause the main thread you can rework the code 
					// to use a separate thread for the status checking and completion.
					do
					{
						string status = CheckJobStatus(jobId); // Possible statuses: "working", "failed", "aborted", "success".

						// Display timestamp and status (for demo purposes)
						Console.WriteLine(DateTime.Now.ToLongTimeString() + ": " + status);

						if (status == "success")
						{
							webClient.Headers.Remove("Content-Type"); // remove the header required for only the previous request

							// Download PDF file
							webClient.DownloadFile(resultFileUrl, destinationFile);

							Console.WriteLine("Generated PDF file saved as \"{0}\" file.", destinationFile);
							break;
						}
						else if (status == "working")
						{
							// Pause for a few seconds
							Thread.Sleep(3000);
						}
						else
						{
							Console.WriteLine(status);
							break;
						}
					}
					while (true);
				}
				else
				{
					Console.WriteLine(json["message"].ToString());
				}
			}
			catch (WebException e)
			{
				Console.WriteLine(e.ToString());
			}

			webClient.Dispose();

		}*/
		static async Task<string> FirebaseUploadAsync()
        {
			const string path = @".\result.html";

			FileStream stream = System.IO.File.OpenRead(path);
			//CONVERT HTML TO PDF

			//authentication
			var auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCRDMrr9fhO9hH1_OFmlv-BOzfsmy8D3Sg"));
			var a = await auth.SignInWithEmailAndPasswordAsync("mrltibs@gmail.com", "Justfortest#123");

			// Constructr FirebaseStorage, path to where you want to upload the file and Put it there
			var task = new FirebaseStorage(
				"bionicwebapp.appspot.com",

				 new FirebaseStorageOptions
				 {
					 AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
					 ThrowOnCancel = true,
				 })
				.Child("bionic")
				.Child("sample.pdf")
				.PutAsync(stream);

			// Track progress of the upload
			task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

			// await the task to wait until upload completes and get the download url
			var downloadUrl = await task;
			return downloadUrl;
		}

	}
}
