using System;
using System.Collections.Generic;
using System.Collections;
//using UnityEngine;
//using UnityEngine.Networking;
using Nethereum.Signer;
using TotemEnums;
using TotemConsts;
using TotemEntities;
using Newtonsoft.Json;
using Godot;
using GodotUtils;
using System.Threading.Tasks;

namespace TotemServices
{
	public delegate void UnityAction();
	public delegate void UnityAction<T0>(T0 arg0);
	public partial class TotemAnalytics : MonoBehaviour
	{
		#region Models
		public class MixpanelEvent
		{
			[JsonProperty("event")]
			public string _event;
			public MixpanelEventProperties properties;
		}

		public class MixpanelEventProperties
		{
			public long time;

			[JsonProperty("$insert_id")]
			public long insertId;

			public string distinct_id;
			public string user_id;
			public string username;
			public string game_id;
			public string token;
			public string source;
			public string source_ver;
		}
		#endregion

		private string sourceVersion;

		public override void _Ready()
		{
			sourceVersion = "5.3.0"; //TODO: Retreive plugin version at runtime
		}

		public async Task RecordAction(TotemServicesAction action, string gameId, TotemUser user, string userEmail)
		{
#if! UNITY_EDITOR && !UNITY_WEBGL
			var key = new EthECKey(TotemUtils.Convert.HexToByteArray(user.PublicKey), false);
			string address = key.GetPublicAddress();

			await RecordActionCoroutine(action, gameId, address, userEmail);
#endif
		}

		private async Task RecordActionCoroutine(TotemServicesAction action, string gameId, string userAddress, string userEmail)
		{
			string trackJson = GenerateTrackJson(action, gameId, userAddress, userEmail);

			var www = new UnityWebRequest(ServicesEnv.AnalyticsUrl, "POST");
			AddChild(www);
			//byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(trackJson);
			//www.uploadHandler = new UploadHandlerRaw(bodyRaw);
			www.uploadHandler = new UploadHandler(trackJson);
			//www.downloadHandler = new DownloadHandlerBuffer();
			www.SetRequestHeader("Content-Type", "application/json");

			await Task.Run(www.SendWebRequest);
			if (www.result != HttpRequest.Result.Success)
			{
				GD.PrintErr("TotemAnalytics- Failed to record an action: " + www.error);
			}

			www.Dispose();
		}


		private string GenerateTrackJson(TotemServicesAction action, string gameId, string userId, string userEmail)
		{
			long unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

			List<MixpanelEvent> events = new List<MixpanelEvent>();
			MixpanelEvent mpEvent = new MixpanelEvent()
			{
				_event = action.ToString(),
				properties = new MixpanelEventProperties()
				{
					time = unixTimestamp,
					token = ServicesEnv.AnalyticsToken,
					distinct_id = userId,
					user_id = userId,
					username = userEmail,
					source = "unity_plugin",
					source_ver = sourceVersion,
					game_id = gameId,
					insertId = unixTimestamp
				}
			};
			events.Add(mpEvent);

			return JsonConvert.SerializeObject(events);
		}
	}
}
