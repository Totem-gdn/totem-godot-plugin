using System;
using System.Net;
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

namespace TotemServices
{
	public partial class TotemPayment : MonoBehaviour
	{
		[Serializable]
		private class PaymentLinkRequest
		{
			public string successUrl;
			public string ownerAddress;
			public string imageUrl;
			public bool redirect;
		}

		[Serializable]
		private class PaymentLinkResponse
		{
			public string url;
			public string order_id;
		}



		public async void PurchaseAsset(string assetType, string publicKey, UnityAction<object> onSuccess)
		{
			var key = new EthECKey(TotemUtils.Convert.HexToByteArray(publicKey), false);
			string address = key.GetPublicAddress();

			await PurchaseAssetCoroutine(assetType, address, onSuccess);
		}

		private async Task PurchaseAssetCoroutine(string assetType, string ownerAddress, UnityAction<object> onSuccess)
		{
			PaymentLinkRequest linkRequest = new PaymentLinkRequest()
			{
				ownerAddress = ownerAddress,
				redirect = true,
				successUrl = "https://dev.totem-explorer.com",
				imageUrl = "https://totem-explorer.com/assets/images/avatar-placeholder.webp"
			};

			string url = $"{ServicesEnv.PaymentAPIUrl}/{ServicesEnv.PaymentSystem}/{assetType}/link";

			var www = WebUtils.CreateRequestJson(url, JsonConvert.SerializeObject(linkRequest));
			await Task.Run(www.SendWebRequest);

			if (www.result != HttpRequest.Result.Success)
			{
				GD.PrintErr("TotemPayment- Failed to get payment link: " + www.downloadHandler.text);
			}
			else
			{
				GD.Print("Payment link: " + www.downloadHandler.text);
				PaymentLinkResponse response = JsonConvert.DeserializeObject<PaymentLinkResponse>(www.downloadHandler.text);

			}

		}



			private async void ListenHttpResponse()
		{
			HttpListener httpListener = new HttpListener();
			httpListener.Prefixes.Add(ServicesEnv.HttpListenerUrl);
			httpListener.Start();
			HttpListenerContext context = await httpListener.GetContextAsync();
			HttpListenerRequest req = context.Request;

			GD.Print("Res payment: " + req.Url);

			//onLoginCallback.Invoke(user);

			//HttpListenerResponse response = context.Response;
			//string responseText = Resources.Load<TextAsset>(ServicesEnv.AuthHttpResponseFileName).text;
			//byte[] responseBuffer = System.Text.Encoding.UTF8.GetBytes(responseText);
			//response.ContentLength64 = responseBuffer.Length;
			//var output = response.OutputStream;
			//output.Write(responseBuffer, 0, responseBuffer.Length);

			httpListener.Stop();
		}

	}
}
