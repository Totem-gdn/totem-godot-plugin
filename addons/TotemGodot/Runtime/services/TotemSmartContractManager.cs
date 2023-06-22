using System.Collections;
using System.Collections.Generic;
using System.Numerics;
//using UnityEngine;
//using UnityEngine.Events;
//using Nethereum.Unity.Rpc;
using TotemEntities;
using TotemEntities.DNA;
using TotemServices.DNA;
using TotemUtils;
using TotemConsts;
using Godot;
using GodotUtils;
using System.Threading.Tasks;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using Nethereum.Contracts.Extensions;
using Nethereum.RPC.Eth.DTOs;

namespace TotemServices
{
    public partial class TotemSmartContractManager : MonoBehaviour
    {
        private Dictionary<object, BigInteger> assetIdTable;
        public override void _Ready()
        {
            assetIdTable = new Dictionary<object, BigInteger>();
        }
        private void Awake()
        {
            assetIdTable = new Dictionary<object, BigInteger>();
        }
        public async void GetAvatars<T>(TotemUser user, TotemDNAFilter filter, UnityAction<List<T>> onComplete) where T : new()
        {
            //await StartCoroutine(GetAvatarsCoroutine(user.PublicKey, filter, onComplete));
            await GetAvatarsCoroutine(user.PublicKey, filter, onComplete);
        }
        private async Task GetAvatarsCoroutine<T>(string publicKey, TotemDNAFilter filter, UnityAction<List<T>> onCompelte) where T : new()
        {
            var avatarsList = new List<T>();

            var key = new EthECKey(TotemUtils.Convert.HexToByteArray(publicKey), false);
            string address = key.GetPublicAddress();
            
            /*
            var balanceQuery = new QueryUnityRequest<BalanceOfFunction, BalanceOfOutputDTO>(ServicesEnv.SmartContractUrl, address);
            yield return balanceQuery.Query(new BalanceOfFunction() { Owner = address }, ServicesEnv.SmartContractAvatars);
            var ownedAvatarsCount = balanceQuery.Result.ReturnValue1;
            */

            var account = new Account(address);
            var web3 = new Web3(account, ServicesEnv.SmartContractUrl);

            var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
            var balance = await balanceHandler.QueryDeserializingToObjectAsync<BalanceOfOutputDTO>(new BalanceOfFunction() { Owner = address }, ServicesEnv.SmartContractAvatars);
            var ownedAvatarsCount = balance.ReturnValue1;
            
            for (int i = 0; i < ownedAvatarsCount; i++)
            {
                /*
                var tokenIdRequest = new QueryUnityRequest<TokenOfOwnerByIndexFunction, TokenOfOwnerByIndexOutputDTO>(ServicesEnv.SmartContractUrl, address);
                yield return tokenIdRequest.Query(new TokenOfOwnerByIndexFunction() { Owner = address, Index = i }, ServicesEnv.SmartContractAvatars);
                */
                var tokenIdHandler = web3.Eth.GetContractQueryHandler<TokenOfOwnerByIndexFunction>();
                var tokenIdRequest = await tokenIdHandler.QueryDeserializingToObjectAsync<TokenOfOwnerByIndexOutputDTO>(new TokenOfOwnerByIndexFunction() { Owner = address, Index = i }, ServicesEnv.SmartContractAvatars);
                /*
                var avatarRequest = new QueryUnityRequest<TokenURIFunction, TokenURIOutputDTO>(ServicesEnv.SmartContractUrl, address);
                var tokenId = tokenIdRequest.Result.ReturnValue1;
                yield return avatarRequest.Query(new TokenURIFunction() { TokenId = tokenIdRequest.Result.ReturnValue1 }, ServicesEnv.SmartContractAvatars);
                */
                var avaterHandler = web3.Eth.GetContractQueryHandler<TokenURIFunction>();
                var tokenId = tokenIdRequest.ReturnValue1;
                var avatarRequest = await avaterHandler.QueryDeserializingToObjectAsync<TokenURIOutputDTO>(new TokenURIFunction() { TokenId = tokenIdRequest.ReturnValue1 }, ServicesEnv.SmartContractAvatars);
                
                string binaryDna = TotemUtils.Convert.HexStringToBinary(avatarRequest/*.Result*/.ReturnValue1);

                var avatar = filter.FilterDNA<T>(binaryDna);
                avatarsList.Add(avatar);
                AddAssetToIdTable(avatar, tokenId);
            }

            onCompelte?.Invoke(avatarsList);
        }


        public async void GetAvatar<T>(TotemUser user, TotemDNAFilter filter, BigInteger assetId, UnityAction<T> onComplete) where T : new()
        {
            //StartCoroutine(GetAvatarCoroutine(user.PublicKey, filter, assetId, onComplete));
            await GetAvatarCoroutine(user.PublicKey, filter, assetId, onComplete);
        }

        private async Task GetAvatarCoroutine<T>(string publicKey, TotemDNAFilter filter, BigInteger assetId, UnityAction<T> onCompelte) where T : new()
        {
            var key = new EthECKey(TotemUtils.Convert.HexToByteArray(publicKey), false);
            string address = key.GetPublicAddress();

            var account = new Account(address);
            var web3 = new Web3(account, ServicesEnv.SmartContractUrl);

            //var avatarRequest = new QueryUnityRequest<TokenURIFunction, TokenURIOutputDTO>(ServicesEnv.SmartContractUrl, address);
            //yield return avatarRequest.Query(new TokenURIFunction() { TokenId = tokenId }, ServicesEnv.SmartContractAvatars);
            var tokenId = assetId;
            var avatarHandler = web3.Eth.GetContractQueryHandler<TokenURIFunction>();
            var avatarRequest = await avatarHandler.QueryDeserializingToObjectAsync<TokenURIOutputDTO>(new TokenURIFunction() { TokenId = tokenId }, ServicesEnv.SmartContractAvatars);
            

            string binaryDna = TotemUtils.Convert.HexStringToBinary(avatarRequest/*.Result*/.ReturnValue1);

            var avatar = filter.FilterDNA<T>(binaryDna);
            AddAssetToIdTable(avatar, tokenId);

            onCompelte?.Invoke(avatar);
        }



        public async void GetItems<T>(TotemUser user, TotemDNAFilter filter, UnityAction<List<T>> onCompelte) where T : new()
        {
            //StartCoroutine(GetItemsCoroutine<T>(user.PublicKey, filter, onCompelte));
            await GetItemsCoroutine(user.PublicKey, filter, onCompelte);
        }

        private async Task GetItemsCoroutine<T>(string publicKey, TotemDNAFilter filter, UnityAction<List<T>> onCompelte) where T : new()
        {
            var itemsList = new List<T>();

            var key = new EthECKey(TotemUtils.Convert.HexToByteArray(publicKey), false);
            string address = key.GetPublicAddress();
            var account = new Account(address);
            var web3 = new Web3(account, ServicesEnv.SmartContractUrl);

            var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
            var balanceQuery = await balanceHandler.QueryDeserializingToObjectAsync<BalanceOfOutputDTO>(new BalanceOfFunction() { Owner = address }, ServicesEnv.SmartContractItems);
            
            //var balanceQuery = new QueryUnityRequest<BalanceOfFunction, BalanceOfOutputDTO>(ServicesEnv.SmartContractUrl, address);
            //yield return balanceQuery.Query(new BalanceOfFunction() { Owner = address }, ServicesEnv.SmartContractItems);

            var ownedItemsCount = balanceQuery/*.Result*/.ReturnValue1;
            for (int i = 0; i < ownedItemsCount; i++)
            {
                //var tokenIdRequest = new QueryUnityRequest<TokenOfOwnerByIndexFunction, TokenOfOwnerByIndexOutputDTO>(ServicesEnv.SmartContractUrl, address);
                //yield return tokenIdRequest.Query(new TokenOfOwnerByIndexFunction() { Owner = address, Index = i }, ServicesEnv.SmartContractItems);
                var tokenIdHandler = web3.Eth.GetContractQueryHandler<TokenOfOwnerByIndexFunction>();
                var tokenIdRequest = await tokenIdHandler.QueryDeserializingToObjectAsync<TokenOfOwnerByIndexOutputDTO>(new TokenOfOwnerByIndexFunction() { Owner = address, Index = i }, ServicesEnv.SmartContractItems);

                var tokenId = tokenIdRequest/*.Result*/.ReturnValue1;

                //var itemRequest = new QueryUnityRequest<TokenURIFunction, TokenURIOutputDTO>(ServicesEnv.SmartContractUrl, address);
                //yield return itemRequest.Query(new TokenURIFunction() { TokenId = tokenId }, ServicesEnv.SmartContractItems);
                var itemHandler = web3.Eth.GetContractQueryHandler<TokenURIFunction>();
                var itemRequest = await itemHandler.QueryDeserializingToObjectAsync<TokenURIOutputDTO>(new TokenURIFunction() { TokenId = tokenId }, ServicesEnv.SmartContractItems);

                string binaryDna = TotemUtils.Convert.HexStringToBinary(itemRequest/*.Result*/.ReturnValue1);
                var item = filter.FilterDNA<T>(binaryDna);
                itemsList.Add(item);
                AddAssetToIdTable(item, tokenId);
            }
            onCompelte.Invoke(itemsList);
        }

        public async void GetItem<T>(TotemUser user, TotemDNAFilter filter, BigInteger assetId, UnityAction<T> onComplete) where T : new()
        {
            //StartCoroutine(GetItemCoroutine(user.PublicKey, filter, assetId, onComplete));
            await GetItemCoroutine(user.PublicKey, filter, assetId, onComplete);
        }

        private async Task GetItemCoroutine<T>(string publicKey, TotemDNAFilter filter, BigInteger assetId, UnityAction<T> onCompelte) where T : new()
        {
            var key = new EthECKey(TotemUtils.Convert.HexToByteArray(publicKey), false);
            string address = key.GetPublicAddress();
            var account = new Account(address);
            var web3 = new Web3(account, ServicesEnv.SmartContractUrl);


            var tokenId = assetId;

            //var itemRequest = new QueryUnityRequest<TokenURIFunction, TokenURIOutputDTO>(ServicesEnv.SmartContractUrl, address);
            //yield return itemRequest.Query(new TokenURIFunction() { TokenId = tokenId }, ServicesEnv.SmartContractItems);
            var itemHandler = web3.Eth.GetContractQueryHandler<TokenURIFunction>();
            var itemRequest = await itemHandler.QueryDeserializingToObjectAsync<TokenURIOutputDTO>(new TokenURIFunction() { TokenId = tokenId }, ServicesEnv.SmartContractItems);

            string binaryDna = TotemUtils.Convert.HexStringToBinary(itemRequest/*.Result*/.ReturnValue1);

            var item = filter.FilterDNA<T>(binaryDna);
            AddAssetToIdTable(item, tokenId);

            onCompelte?.Invoke(item);
        }


        private void AddAssetToIdTable(object asset, BigInteger id)
        {
            assetIdTable.Add(asset, id);
        }

        public BigInteger GetAssetId(object asset)
        {
            if (asset == null)
            {
                return -1;
            }

            if (!assetIdTable.ContainsKey(asset))
                return -1;

            return assetIdTable[asset];
        }

    }
}
