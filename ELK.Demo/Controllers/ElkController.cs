using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;

namespace ELK.Demo.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ElkController : ControllerBase
	{
		private readonly ElasticClient _client;

		public ElkController(
			ElasticClient client)
		{
			_client = client;
		}

		[HttpGet]
		public IActionResult Get()
		{
			return Ok(Data.AuthorBooks);
		}

		#region Index

		[HttpGet]
		[Route("indexOne")]
		public async Task<IActionResult> IndexOneItem()
		{
			var authorBook = Data.AuthorBooks.FirstOrDefault();

			var result = await _client.IndexAsync(authorBook, idx => idx.Index("authorbooks"));
			// https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/debug-information.html
			return Ok(result.DebugInformation);
		}

		// Why bad ? Here's why https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/indexing-documents.html#_multiple_documents_with_bulk
		[HttpGet]
		[Route("bulkIndexBad")]
		public async Task<IActionResult> IndexBulkBad()
		{
			var bulkIndexResponse = await _client.BulkAsync(b => b
				.Index("authorbooks")
				.IndexMany(Data.AuthorBooks));
			return Ok(bulkIndexResponse.DebugInformation);
		}

		[HttpGet]
		[Route("bulkIndexBetter")]
		public IActionResult IndexBulkBetter()
		{
			var results = new string[] { };
			// https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/indexing-documents.html#_multiple_documents_with_bulkallobservable_helper
			_client.BulkAll(Data.AuthorBooks,
					b
						=> b.Index("authorbooks")
							.BackOffTime("30s") // time to wait between retries
							.BackOffRetries(2) // max amount to retry
							.RefreshOnCompleted() // make sure to index all documents before client can read said documents
							.Size(200))
				.Wait(TimeSpan.FromMinutes(15), response => // this will block main thread (not in core though)
				{
					results = response.Items.Select(r => r.Result).ToArray();
				}); // items per bulk request

			return Ok(results);
		}

		#endregion


		#region Search

		// Find out more ways to search on https://www.elastic.co/guide/en/elasticsearch/reference/current/getting-started-search.html

		[HttpGet]
		[Route("queryOne")]
		public async Task<IActionResult> QueryOne()
		{
			var bookAuthor = await _client
				.GetAsync<AuthorBook>(1, // document query
					index => index.Index("authorbooks"));

			return Ok(bookAuthor.Source);
		}

		[HttpGet]
		[Route("queryMany")]
		public async Task<IActionResult> QueryMany()
		{
			var results = await _client.SearchAsync<AuthorBook>(
				s =>
					s.Index("authorbooks")
						.Query(q
							=> q.Match(
								_ =>
									_.Field(_ => _.Name)
										.Query("Huy").Fuzziness(Fuzziness.EditDistance(2)))));

			return Ok(results.DebugInformation);
		}

		[HttpGet]
		[Route("queryMany2")]
		public async Task<IActionResult> QueryMany2()
		{
			var request = new SearchRequest
			{
				From = 0,
				Size = 10,
				Query = new MatchQuery {Field = "name", Query = "Huy", Fuzziness = Fuzziness.EditDistance(2)}
			};

			var results = await _client.SearchAsync<AuthorBook>(request);
			return Ok(results.DebugInformation);
		}


		[HttpGet]
		[Route("queryMany3")]
		public async Task<IActionResult> QueryMany3()
		{
			// altsource is currently using raw query
			var request = @"{
								
									""match"": { 
										""name"": { 
											""query"": ""Huy"",
											""fuzziness"": 2
										}
									}
							
							}";

			var results = await _client
				.SearchAsync<AuthorBook>(s
					=> s.Index("authorbooks").Query(q => q.Raw(request)));
			return Ok(results.DebugInformation);
		}

		#endregion
	}
}