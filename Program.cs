using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpLogging
{
	class Program
	{
		static async Task Main(string[] args)
		{
			using var clientHandler = new HttpClientHandler();
			using var log = File.OpenWrite("log.har");
			using var loggingHandler = new LoggingHandler(clientHandler, log);
			using var client = new HttpClient(loggingHandler);

			var result = await client.GetAsync("https://microsoft.com");
		}
	}
}
