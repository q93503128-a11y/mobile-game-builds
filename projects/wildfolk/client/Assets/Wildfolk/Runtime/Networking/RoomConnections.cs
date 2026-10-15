using System;
using System.Net;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

namespace Wildfolk
{
    public enum ConnectionRoute { Internet,LocalNetwork }
    public interface IRoomConnection
    {
        Task<string> Host(UnityTransport transport,int maximumPlayers);
        Task Join(UnityTransport transport,string destination);
    }
    public sealed class RelayRoomConnection : IRoomConnection
    {
        public static string AuthenticationProfile="default";
        private static async Task Initialize()
        {
            if(UnityServices.State!=ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync(new InitializationOptions().SetProfile(AuthenticationProfile));
            if(!AuthenticationService.Instance.IsSignedIn)await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        public static bool ValidCode(string code)
        {if(string.IsNullOrWhiteSpace(code) || code.Length<6 || code.Length>12)return false;foreach(var c in code)if(!(c>='A' && c<='Z' || c>='0' && c<='9'))return false;return true;}
        public async Task<string> Host(UnityTransport transport,int maximumPlayers)
        {
            await Initialize();var allocation=await RelayService.Instance.CreateAllocationAsync(maximumPlayers-1);
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation,"dtls"));
            return await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        public async Task Join(UnityTransport transport,string destination)
        {
            await Initialize();var allocation=await RelayService.Instance.JoinAllocationAsync(destination);
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation,"dtls"));
        }
    }
    public sealed class LocalRoomConnection : IRoomConnection
    {
        public const ushort Port=7777;
        public static bool TryAddress(string text,out string address,out ushort port)
        {
            address=null;port=Port;if(string.IsNullOrWhiteSpace(text))return false;
            var parts=text.Trim().Split(':');if(parts.Length>2 || !IPAddress.TryParse(parts[0],out var ip) || ip.AddressFamily!=System.Net.Sockets.AddressFamily.InterNetwork)return false;
            if(parts.Length==2 && (!ushort.TryParse(parts[1],out port)||port==0))return false;address=ip.ToString();return true;
        }
        public Task<string> Host(UnityTransport transport,int maximumPlayers)
        {transport.SetConnectionData("127.0.0.1",Port,"0.0.0.0");return Task.FromResult(string.Empty);}
        public Task Join(UnityTransport transport,string destination)
        {if(!TryAddress(destination,out var address,out var port))throw new ArgumentException();transport.SetConnectionData(address,port);return Task.CompletedTask;}
    }
}
