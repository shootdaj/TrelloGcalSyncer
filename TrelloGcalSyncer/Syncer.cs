using System;
using System.IO;
using System.Linq;
using System.Threading;
using Anshul.Utilities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Manatee.Trello;
using Manatee.Trello.ManateeJson;
using Manatee.Trello.WebApi;
using Refigure;

namespace TrelloGcalSyncer
{
	public class Syncer
	{
		private string DailyTasksBoardID => Config.Get("DailyTasksBoardID");
		private const string TODOListName = "TODO";
		private const string CommentFlagText = "Added to GCal";
		private const string ApplicationName = "TrelloGcalSyncer";
		private const string CalendarID = "primary";

		private UserCredential _gcalCredential;
		private CalendarService _calendarService;

		public Syncer()
		{
			InitGCalService();
			InitTrelloAPI();
		}

		public void Sync()
		{
			var board = new Board(DailyTasksBoardID);
			var todo = board.Lists.Single(x => x.Name == TODOListName);

			todo.Cards.ToList().ForEach(card =>
			{
				if (card.Comments.All(x => x.Data.Text.ToString() != CommentFlagText))
				{
					AddCardToGCal(card);
				}
			});
		}

		public void AddCardToGCal(Card card)
		{
			var @event = new Event()
			{
				Summary = card.Name,
				Description = card.Description,
				Start = new EventDateTime()
				{
					Date =  card.CreationDate.Date.ToString("yyyy-MM-dd")
				},
				End = new EventDateTime()
				{
					Date = card.CreationDate.Date.ToString("yyyy-MM-dd")
				}
			};
			var createRequest = _calendarService.Events.Insert(@event, CalendarID);

			createRequest.Execute();

			card.Comments.Add(CommentFlagText);
		}

		private void InitGCalService()
		{
			// If modifying these scopes, delete your previously saved credentials
			// at ~/.credentials/calendar-dotnet-quickstart.json
			string[] scopes = { CalendarService.Scope.Calendar };

			var clientSecretFile = Extensions.MergePath(AppDomain.CurrentDomain.BaseDirectory, "clientsecret.json");

			using (var stream = new FileStream(clientSecretFile, FileMode.Open, FileAccess.Read))
			{
				var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				credPath = Path.Combine(credPath, ".credentials/calendar-trello-gcal-syncer.json");

				_gcalCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
					GoogleClientSecrets.Load(stream).Secrets,
					scopes,
					"user",
					CancellationToken.None,
					new FileDataStore(credPath, true)).Result;
				Console.WriteLine("Credential file saved to: " + credPath);
			}

			// Create Google Calendar API service.
			_calendarService = new CalendarService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = _gcalCredential,
				ApplicationName = ApplicationName,
			});
		}

		//	// Define parameters of request.
		//	EventsResource.ListRequest request = service.Events.List("primary");
		//	request.TimeMin = DateTime.Now;
		//		request.ShowDeleted = false;
		//		request.SingleEvents = true;
		//		request.MaxResults = 10;
		//		request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

		//		// List events.
		//		Events events = request.Execute();
		//	Console.WriteLine("Upcoming events:");
		//		if (events.Items != null && events.Items.Count > 0)
		//		{
		//			foreach (var eventItem in events.Items)
		//			{
		//				string when = eventItem.Start.DateTime.ToString();
		//				if (String.IsNullOrEmpty(when))
		//				{
		//					when = eventItem.Start.Date;
		//				}
		//Console.WriteLine("{0} ({1})", eventItem.Summary, when);
		//			}
		//		}
		//		else
		//		{
		//			Console.WriteLine("No upcoming events found.");
		//		}
		//		Console.Read();

		public void InitTrelloAPI()
		{
			var serializer = new ManateeSerializer();
			TrelloConfiguration.Serializer = serializer;
			TrelloConfiguration.Deserializer = serializer;
			TrelloConfiguration.JsonFactory = new ManateeFactory();
			TrelloConfiguration.RestClientProvider = new WebApiClientProvider();
			TrelloAuthorization.Default.AppKey = Config.Get("TrelloAppKey");
			TrelloAuthorization.Default.UserToken = Config.Get("TrelloAppToken");
		}
	}
}
