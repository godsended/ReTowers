using Mirror;
using System;

namespace Core.Contracts
{
    public struct AuthDto : NetworkMessage
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string LastLoginTime { get; set; }
        public bool IsGuest { get; set; }
    }
}