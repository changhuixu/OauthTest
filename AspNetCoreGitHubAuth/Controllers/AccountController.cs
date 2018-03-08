using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreGitHubAuth.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly ClaimsPrincipal _principal;

        public AccountController(IPrincipal principal)
        {
            _principal = principal as ClaimsPrincipal;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties() { RedirectUri = returnUrl });
        }

        [HttpGet]
        [Authorize]
        public IActionResult Me()
        {
           // https://davidpine.net/blog/principal-architecture-changes/
            var dobClaim = _principal?.FindFirst(ClaimTypes.Name);
            var a = User;
          //  var b = Thread.CurrentPrincipal.Identity;
            var c = ClaimsPrincipal.Current;
            return Ok(User.Identity.Name);
        }
    }
}