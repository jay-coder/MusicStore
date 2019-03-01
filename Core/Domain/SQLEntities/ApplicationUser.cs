using JayCoder.MusicStore.Core.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;

namespace JayCoder.MusicStore.Core.Domain.SQLEntities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public int Gender { get; set; }
        public string Avatar { get; set; }
        public int Language { get; set; }
        public bool Enabled { get; set; }
        public EnumUserType UserType { get; set; }
    }
}
