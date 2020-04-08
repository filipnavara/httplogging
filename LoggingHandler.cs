using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HttpLogging
{
	public class LoggingHandler : DelegatingHandler
	{
		Utf8JsonWriter jsonWriter;

		public LoggingHandler(HttpMessageHandler innerHandler, Stream logStream)
			: base(innerHandler)
		{
			this.jsonWriter = new Utf8JsonWriter(logStream, new JsonWriterOptions { Indented = true });
			this.jsonWriter.WriteStartObject();
			this.jsonWriter.WriteStartObject("log");
			this.jsonWriter.WriteString("version", "1.2");
			this.jsonWriter.WriteStartObject("creator");
			this.jsonWriter.WriteString("name", "LoggingHandler");
			this.jsonWriter.WriteString("version", "1.0");
			this.jsonWriter.WriteEndObject();
			this.jsonWriter.WriteStartArray("entries");
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.jsonWriter != null)
			{
				this.jsonWriter.WriteEndArray();
				this.jsonWriter.WriteEndObject();
				this.jsonWriter.WriteEndObject();
				this.jsonWriter.Dispose();
				this.jsonWriter = null;
			}
			base.Dispose(disposing);
		}

		private static void WriteHeaders(Utf8JsonWriter jsonWriter, HttpHeaders httpHeaders)
		{
			foreach (var header in httpHeaders)
			{
				foreach (var headerValue in header.Value)
				{
					jsonWriter.WriteStartObject();
					jsonWriter.WriteString("name", header.Key);
					jsonWriter.WriteString("value", headerValue);
					jsonWriter.WriteEndObject();
				}
			}
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			this.jsonWriter.WriteStartObject();
			this.jsonWriter.WriteString("startedDateTime", DateTime.UtcNow);

			this.jsonWriter.WriteStartObject("request");
			this.jsonWriter.WriteString("method", request.Method.Method);
			this.jsonWriter.WriteString("url", request.RequestUri.OriginalString);
			this.jsonWriter.WriteString("httpVersion", "HTTP/" + request.Version.ToString(2));
			this.jsonWriter.WriteStartArray("cookies");
			// FIXME
			this.jsonWriter.WriteEndArray();
			this.jsonWriter.WriteStartArray("headers");
			WriteHeaders(this.jsonWriter, request.Headers);
			if (request.Content != null)
				WriteHeaders(this.jsonWriter, request.Content.Headers);
			this.jsonWriter.WriteEndArray();
			this.jsonWriter.WriteStartArray("queryString");
			// FIXME
			this.jsonWriter.WriteEndArray();
			if (request.Content != null)
			{
				this.jsonWriter.WriteStartObject("postData");
				this.jsonWriter.WriteString("mimeType", request.Content.Headers.ContentType.ToString());
				this.jsonWriter.WriteStartArray("params");
				// FIXME
				this.jsonWriter.WriteEndArray();
				this.jsonWriter.WriteString("text", await request.Content.ReadAsStringAsync());
				this.jsonWriter.WriteEndObject();
			}
			this.jsonWriter.WriteNumber("headersSize", 0); // FIXME
			this.jsonWriter.WriteNumber("bodySize", 0); // FIXME
			this.jsonWriter.WriteEndObject();

			HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

			this.jsonWriter.WriteStartObject("response");
			this.jsonWriter.WriteNumber("status", (int)response.StatusCode);
			this.jsonWriter.WriteString("statusText", response.ReasonPhrase);
			this.jsonWriter.WriteString("httpVersion", "HTTP/" + response.Version.ToString(2));
			this.jsonWriter.WriteStartArray("cookies");
			// TODO
			this.jsonWriter.WriteEndArray();
			this.jsonWriter.WriteStartArray("headers");
			WriteHeaders(this.jsonWriter, response.Headers);
			if (response.Content != null)
				WriteHeaders(this.jsonWriter, response.Content.Headers);
			this.jsonWriter.WriteEndArray();
			this.jsonWriter.WriteStartObject("content");
			if (response.Content != null)
			{
				this.jsonWriter.WriteNumber("size", 0); // FIXME
				this.jsonWriter.WriteNumber("compression", 0); // FIXME
				this.jsonWriter.WriteString("mimeType", response.Content.Headers.ContentType.ToString());
				this.jsonWriter.WriteString("text", await response.Content.ReadAsStringAsync());
			}
			this.jsonWriter.WriteEndObject();
			this.jsonWriter.WriteString("redirectURL", response.Headers.Location?.OriginalString ?? "");
			this.jsonWriter.WriteNumber("headersSize", 0); // FIXME
			this.jsonWriter.WriteNumber("bodySize", 0); // FIXME
			this.jsonWriter.WriteEndObject();

			this.jsonWriter.WriteStartObject("cache");
			this.jsonWriter.WriteEndObject();

			this.jsonWriter.WriteStartObject("timings");
			this.jsonWriter.WriteNumber("send", 0); // FIXME
			this.jsonWriter.WriteNumber("wait", 0); // FIXME
			this.jsonWriter.WriteNumber("receive", 0); // FIXME
			this.jsonWriter.WriteEndObject();

			this.jsonWriter.WriteNumber("time", 0);

			this.jsonWriter.WriteEndObject();

			return response;
		}
	}
}
