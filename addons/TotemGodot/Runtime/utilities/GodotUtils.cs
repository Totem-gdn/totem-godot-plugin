using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using TotemServices.DNA;
namespace GodotUtils
{
	//public delegate void UnityAction();
	//public delegate void UnityAction<T0>(T0 arg0);
	public partial class MonoBehaviour : Node
	{
		public async Task StartCoroutine(IEnumerator enumerator)
		{
			while (enumerator.MoveNext())
			{
				await Task.Yield();
			}
			/*Task.Run(async () =>
			{
				while (enumerator.MoveNext())
				{
					await Task.Yield();
				}
			});*/
		}
		virtual public void Start() { }
		virtual public void Update() { }
		public override void _Ready()
		{
			Start();
		}
		public override void _Process(double delta)
		{
			Update();

		}
	}
	/*public class CustomYieldInstruction : IEnumerator
	{

	}*/
	public class DownloadHandler
	{
		public string text;
	}
	public class UploadHandler
	{
		public string text;
		public UploadHandler() { text = ""; }
		public UploadHandler(string body) { text = body; }
	}
	public partial class UnityWebRequest : Node
	{
		System.Net.Http.HttpClient client;
		private string _MethodName;
		private string url;
		public string error { get; private set; }
		public HttpRequest.Result result { get; private set; }
		private Dictionary<string, string> customHeaders;
		public DownloadHandler downloadHandler { get; private set; }
		public UploadHandler uploadHandler { get; set; }

		public bool complete { get; private set; }

		public UnityWebRequest(string url, string method) : base()
		{
			this.url = url;
			_MethodName = method;
			//AddChild(request);
			//request.RequestCompleted += OnRequestCompleted;
			downloadHandler = new DownloadHandler();
			uploadHandler = new UploadHandler();
			error = string.Empty;
			customHeaders = new Dictionary<string, string>();
			complete = false;
		}
		public static UnityWebRequest Get(string url)
		{
			UnityWebRequest instance = new UnityWebRequest(url, "GET");
			return instance;
		}
		public async Task SendWebRequest()
		{
			client = new System.Net.Http.HttpClient();
			try
			{
				HttpResponseMessage response;
				if (_MethodName == "POST")
				{
					// Send the POST request
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
					request.Content = new StringContent(uploadHandler.text, System.Text.Encoding.UTF8, customHeaders["Content-Type"]);
					response = await client.SendAsync(request);
					//GD.Print(request.Content.ToString());
				}
				else
				{
					// Send the GET request
					response = await client.GetAsync(url);
				}

				// Check the response status code
				if (response.IsSuccessStatusCode)
				{
					// Read the response content as a string
					string responseBody = await response.Content.ReadAsStringAsync();
					downloadHandler.text = responseBody;
					//GD.Print("responseBody" + responseBody);
				}
				else
				{
					//GD.PrintErr(response.ReasonPhrase);
					//GD.PrintErr(response.ToString());
					GD.PrintErr($"Request failed with status code {response.StatusCode}");
					GD.PrintErr($"URL: {url}");
					GD.PrintErr($"Method: {_MethodName}");
				}
			}
			catch (Exception e)
			{
				GD.PrintErr($"Request failed with error: {e.Message}");
			}
			finally
			{
				client.Dispose();
			}
		}
		private void OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
		{
			var json = new Json();
			json.Parse(body.GetStringFromUtf8());
			downloadHandler.text = json.Data.AsString();
			GD.Print("OnRequestCompleted: " + downloadHandler.text);
			GD.Print("size of byte: " + body.Length);
			if(body.Length != 0) complete = true;
		}
		public void SetRequestHeader(string key, string value)
		{
			customHeaders.Add(key,value);
		}

	}
	public class CustomYieldInstruction : IEnumerator
	{
		public object Current => throw new NotImplementedException();

		public bool MoveNext()
		{
			throw new NotImplementedException();
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}
	}
}
