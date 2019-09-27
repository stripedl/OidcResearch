using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace TestOidcIdp
{
	public class RequestResponseLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

		public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task Invoke(HttpContext context)
		{
			//First, get the incoming request
			var request = await FormatRequest(context.Request);

			_logger.LogInformation("Received request: {request}", request);
			
			//Copy a pointer to the original response body stream
			var originalBodyStream = context.Response.Body;

			//Create a new memory stream...
			using (var responseBody = new MemoryStream())
			{
				//...and use that for the temporary response body
				context.Response.Body = responseBody;

				//Continue down the Middleware pipeline, eventually returning to this class
				await _next(context);

				//Format the response from the server
				var response = await FormatResponse(context.Response);

				//TODO: Save log to chosen datastore
				_logger.LogInformation("Received response: {response}", response);

				//Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
				await responseBody.CopyToAsync(originalBodyStream);
			}
		}

		private async Task<string> FormatRequest(HttpRequest request)
		{
			var body = request.Body;

			//This line allows us to set the reader for the request back at the beginning of its stream.
			request.EnableRewind();

			////We now need to read the request stream.  First, we create a new byte[] with the same length as the request stream...
			//var buffer = new byte[Convert.ToInt32(request.ContentLength)];

			////...Then we copy the entire request stream into the new buffer.
			//await request.Body.ReadAsync(buffer, 0, buffer.Length);

			request.EnableBuffering();
			// Read the stream as text
			var bodyAsText = await new System.IO.StreamReader(request.Body).ReadToEndAsync();
			// Set the position of the stream to 0 to enable rereading
			request.Body.Position = 0;

			var headers = request.Headers.ToList();
			string headersString = "";
			foreach (var h in headers)
			{
				headersString = headersString + $"{h.Key}:{h.Value}\n";
			}

			//..and finally, assign the read body back to the request body, which is allowed because of EnableRewind()
			//request.Body = body;

			return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {headers} {bodyAsText}";
		}

		private async Task<string> FormatResponse(HttpResponse response)
		{
			//We need to read the response stream from the beginning...
			response.Body.Seek(0, SeekOrigin.Begin);

			//...and copy it into a string
			string text = await new StreamReader(response.Body).ReadToEndAsync();

			//We need to reset the reader for the response so that the client can read it.

			var headers = response.Headers.ToList();
			string headersString = "";
			foreach (var h in headers)
			{
				headersString = headersString + $"{h.Key}:{h.Value}\n";
			}

			response.Body.Position = 0;

			//Return the string for the response, including the status code (e.g. 200, 404, 401, etc.)
			return $"{response.StatusCode}: {text} {headersString}";
		}
	}

	class SerilogMiddleware
	{
		const string MessageTemplate =
			"HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

		static readonly Serilog.ILogger Log = Serilog.Log.ForContext<SerilogMiddleware>();

		readonly RequestDelegate _next;

		public SerilogMiddleware(RequestDelegate next)
		{
			if (next == null) throw new ArgumentNullException(nameof(next));
			_next = next;
		}

		public async Task Invoke(HttpContext httpContext)
		{
			if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

			var sw = Stopwatch.StartNew();
			try
			{
				await _next(httpContext);
				sw.Stop();

				var statusCode = httpContext.Response?.StatusCode;
				var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;

				var log = level == LogEventLevel.Error ? LogForErrorContext(httpContext) : Log;
				log.Write(level, MessageTemplate, httpContext.Request.Method, httpContext.Request.Path, statusCode, sw.Elapsed.TotalMilliseconds);
			}
			// Never caught, because `LogException()` returns false.
			catch (Exception ex) when (LogException(httpContext, sw, ex)) { }
		}

		static bool LogException(HttpContext httpContext, Stopwatch sw, Exception ex)
		{
			sw.Stop();

			LogForErrorContext(httpContext)
				.Error(ex, MessageTemplate, httpContext.Request.Method, httpContext.Request.Path, 500, sw.Elapsed.TotalMilliseconds);

			return false;
		}

		static ILogger LogForErrorContext(HttpContext httpContext)
		{
			var request = httpContext.Request;

			var result = Log
				.ForContext("RequestHeaders", request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), destructureObjects: true)
				.ForContext("RequestHost", request.Host)
				.ForContext("RequestProtocol", request.Protocol);

			if (request.HasFormContentType)
				result = result.ForContext("RequestForm", request.Form.ToDictionary(v => v.Key, v => v.Value.ToString()));

			return result;
		}
	}
}
