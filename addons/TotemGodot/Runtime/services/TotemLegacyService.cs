using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.Networking;
using TotemEntities;
using TotemConsts;
using Nethereum.Signer;
using TotemUtils;
using Godot;
using Newtonsoft.Json;
using GodotUtils;
using System.Threading.Tasks;
//using UnityWebRequest = System.Net.HttpWebRequest;

namespace TotemServices
{
	public partial class TotemLegacyService : MonoBehaviour
	{

		#region Resoponse Models

		[Serializable]
		private class LegacyRepsonse
		{
			public int total;
			public int limit;
			public int offset;
			public LegacyRecord[] results;
		}

		[Serializable]
		private class LegacyRecord
		{
			public string assetId;
			public string gameId;
			public string timestamp;
			public string data;
		}

		[Serializable]
		private class LegacyRequest
		{
			public string playerAddress;
			public string assetId;
			public string gameAddress;
			public string data;
		}

		#endregion

		private int requestLimit = 20; 

		#region Requests


		public async Task GetAssetLegacy(string assetId, string assetType, string gameAddress, string publicKey,
			UnityAction<List<TotemLegacyRecord>> onSuccess, UnityAction<string> onFailure = null)
		{
			var key = new EthECKey(TotemUtils.Convert.HexToByteArray(publicKey), false);
			string address = key.GetPublicAddress();
			
			//StartCoroutine(GetAssetLegacyCoroutine(assetId, assetType, gameAddress, address, 0, new List<TotemLegacyRecord>(), onSuccess, onFailure));
			await GetAssetLegacyCoroutine(assetId, assetType, gameAddress, address, 0, new List<TotemLegacyRecord>(), onSuccess, onFailure);
		}

		//private IEnumerator GetAssetLegacyCoroutine(string assetId, string assetType, string gameAddress, string playerAddress, int offset, 
		private async Task GetAssetLegacyCoroutine(string assetId, string assetType, string gameAddress, string playerAddress, int offset, 
			List<TotemLegacyRecord> data,
			UnityAction<List<TotemLegacyRecord>> onSuccess, 
			UnityAction<string> onFailure)
		{
			string url = ServicesEnv.AssetLegacyServicesUrl +
				$"/{assetType}?playerAddress={playerAddress}&assetId={assetId}&gameAddress={gameAddress}&limit={requestLimit}&offset={offset}";
			GD.Print(url);
			UnityWebRequest www = UnityWebRequest.Get(url);
			AddChild(www);
			//yield return www.SendWebRequest;
			await Task.Run(www.SendWebRequest);
			//www.SendWebRequest();
			
			GD.Print("1");
			if (www.result != HttpRequest.Result.Success)
			{
				GD.Print("2");
				GD.PrintErr("TotemLegacyService- Failed to get legecy records: " + www.downloadHandler.text);
				onFailure?.Invoke(www.error);
			}
			else
			{
				GD.Print("3");
				GD.Print(www.downloadHandler.text);
				GD.Print("4");
				LegacyRepsonse response = JsonConvert.DeserializeObject<LegacyRepsonse>(www.downloadHandler.text);
				foreach (var record in response.results)
				{
					string decodedData = record.data;
					try
					{
						decodedData = System.Text.Encoding.UTF8.GetString(TotemUtils.Convert.DecodeBase64(record.data));
					}
					catch (Exception e)
					{
						GD.PrintErr($"TotemLegacyService- Failed to decode data (assetId: {record.assetId}): " + e.Message);
					}
					TotemLegacyRecord legacy = new TotemLegacyRecord(TotemEnums.LegacyRecordTypeEnum.Achievement, record.assetId, record.gameId, 
						decodedData, record.timestamp);
					data.Add(legacy);
				}

				if (response.total > data.Count)
				{
					await GetAssetLegacyCoroutine(assetId, assetType, gameAddress, playerAddress, data.Count, data, onSuccess, onFailure);
				}
				else
				{
					onSuccess.Invoke(data);
				}

			}

			www.Dispose();
		}



		public async Task AddAssetLegacy(TotemLegacyRecord legacy, string assetType, string publicKey,
			UnityAction onSuccess = null, UnityAction<string> onFailure = null)
		{
			var key = new EthECKey(TotemUtils.Convert.HexToByteArray(publicKey), false);
			string address = key.GetPublicAddress();

			await AddAssetLegacyCoroutine(legacy, assetType, address, onSuccess, onFailure);
		}

		private async Task AddAssetLegacyCoroutine(TotemLegacyRecord legacy, string assetType, string playerAddress,
			UnityAction onSuccess = null, UnityAction<string> onFailure = null)
		{
			string url = ServicesEnv.AssetLegacyServicesUrl + $"/{assetType}";

			string base64Data = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(legacy.data));

			LegacyRequest request = new LegacyRequest
			{
				playerAddress = playerAddress,
				assetId = legacy.assetId,
				gameAddress = legacy.gameAddress,
				data = base64Data
			};
			GD.Print("post request data");
			GD.Print(playerAddress);
			GD.Print(legacy.assetId);
			GD.Print(legacy.gameAddress);
			GD.Print(base64Data);
			GD.Print("post request data");

			UnityWebRequest www = WebUtils.CreateRequestJson(url, JsonConvert.SerializeObject(request));
			AddChild(www);
			await www.SendWebRequest();
			if (www.result != HttpRequest.Result.Success)
			{
				GD.PrintErr("TotemLegacyService- Failed to add legacy: " + www.error);
				GD.Print(www.downloadHandler.text);
				onFailure?.Invoke(www.error);
			}
			else
			{
				onSuccess?.Invoke();
			}

			www.Dispose();

		}

		#endregion
	}
}
