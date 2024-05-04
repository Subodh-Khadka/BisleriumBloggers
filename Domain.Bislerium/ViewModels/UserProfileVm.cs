using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Bislerium.ViewModels
{
    public class UserProfileVm
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }

    }
}
