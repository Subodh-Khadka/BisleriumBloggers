using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Bislerium
{
    public interface IAuthService
    {
        Task<string> Login(LoginVm loginVm);
        Task Logout();
        
    }
}
    