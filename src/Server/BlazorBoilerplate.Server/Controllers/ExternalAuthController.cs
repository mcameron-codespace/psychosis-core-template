using BlazorBoilerplate.Infrastructure.Server;
using BlazorBoilerplate.Server.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Security.Claims;
using System.Security.Principal;

namespace BlazorBoilerplate.Server.Controllers
{
    [OpenApiIgnore]
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalAuthController : ControllerBase
    {
        private readonly IExternalAuthManager _externalAuthManager;

        public ExternalAuthController(IExternalAuthManager externalAuthManager)
        {
            _externalAuthManager = externalAuthManager;
        }

        [HttpGet("challenge/{provider}")]
        [HttpGet("challenge/{provider}/{returnUrl?}")]
        [AllowAnonymous]
        public async Task<IActionResult> Challenge(string provider, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            // validate returnUrl - implement whitelist-based validation for enhanced security
            if (!IsValidReturnUrl(returnUrl))
            {
                // Log potential open redirect attack attempt
                var logger = HttpContext.RequestServices.GetService<ILogger<ExternalAuthController>>();
                logger?.LogWarning("Potential open redirect attack detected. Rejected return URL: {ReturnUrl} from IP: {RemoteIP}", 
                    returnUrl, 
                    HttpContext.Connection.RemoteIpAddress?.ToString());
                
                // Redirect to safe default location instead of throwing exception
                return RedirectToAction("Index", "Home");
            }

            if (AccountOptions.WindowsAuthenticationSchemeName == provider)
            {
                // windows authentication needs special handling
                return await ProcessWindowsLoginAsync(returnUrl);
            }
            else
            {
                // start challenge and roundtrip the return URL and scheme 
                var props = new AuthenticationProperties
                {
                    RedirectUri = Url.Action(nameof(ExternalSignIn)),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", provider },
                    }
                };

                return Challenge(props, provider);
            }
        }

        private async Task<IActionResult> ProcessWindowsLoginAsync(string returnUrl)
        {
            // see if windows auth has already been requested and succeeded
            var result = await HttpContext.AuthenticateAsync(AccountOptions.WindowsAuthenticationSchemeName);
            if (result?.Principal is WindowsPrincipal wp)
            {
                // we will issue the external cookie and then redirect the
                // user back to the external callback, in essence, treating windows
                // auth the same as any other external authentication mechanism
                var props = new AuthenticationProperties()
                {
                    RedirectUri = Url.Action("Callback"),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", AccountOptions.WindowsAuthenticationSchemeName },
                    }
                };

#pragma warning disable CA1416 // Validate platform compatibility
                var id = new ClaimsIdentity(AccountOptions.WindowsAuthenticationSchemeName);
                id.AddClaim(new Claim(ClaimTypes.NameIdentifier, wp.FindFirst(ClaimTypes.PrimarySid).Value));
                id.AddClaim(new Claim(ClaimTypes.Name, wp.Identity.Name));

                // add the groups as claims -- be careful if the number of groups is too large
                if (AccountOptions.IncludeWindowsGroups)
                {
                    var wi = wp.Identity as WindowsIdentity;
                    var groups = wi.Groups.Translate(typeof(NTAccount));
                    var roles = groups.Select(x => new Claim(ClaimTypes.Role, x.Value));
                    id.AddClaims(roles);
                }
#pragma warning restore CA1416 // Validate platform compatibility

                await HttpContext.SignInAsync(
                    IdentityConstants.ExternalScheme,
                    new ClaimsPrincipal(id),
                    props);

                return Redirect(props.RedirectUri);
            }
            else
            {
                // trigger windows auth
                // since windows auth don't support the redirect uri,
                // this URL is re-triggered when we call challenge
                return Challenge(AccountOptions.WindowsAuthenticationSchemeName);
            }
        }


        [HttpGet("ExternalSignIn", Name = "ExternalSignIn")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalSignIn()
        => Redirect(await _externalAuthManager.ExternalSignIn(HttpContext));

        /// <summary>
        /// Validates return URL against open redirect attacks using multiple validation layers
        /// </summary>
        /// <param name="returnUrl">The return URL to validate</param>
        /// <returns>True if the URL is safe, false otherwise</returns>
        private bool IsValidReturnUrl(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
                return false;

            // Layer 1: Check if it's a local URL (built-in ASP.NET Core validation)
            if (Url.IsLocalUrl(returnUrl))
                return true;

            // Layer 2: Prevent protocol-based attacks (javascript:, data:, etc.)
            if (returnUrl.Contains(":", StringComparison.OrdinalIgnoreCase))
            {
                var scheme = returnUrl.Split(':')[0].ToLowerInvariant();
                // Block dangerous schemes
                if (new[] { "javascript", "data", "vbscript", "file" }.Contains(scheme))
                    return false;
            }

            // Layer 3: Prevent double-encoding attacks
            if (returnUrl.Contains("%2f") || returnUrl.Contains("%5c"))
            {
                try
                {
                    var decoded = Uri.UnescapeDataString(returnUrl);
                    // Check if decoding reveals a dangerous URL
                    if (!Url.IsLocalUrl(decoded) && !decoded.Contains(Request.Host.Value, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
                catch
                {
                    // If decoding fails, reject the URL
                    return false;
                }
            }

            // Layer 4: Ensure the URL belongs to our host
            if (returnUrl.StartsWith("/") == false && 
                returnUrl.Contains(Request.Host.Value, StringComparison.OrdinalIgnoreCase) == false)
                return false;

            // Layer 5: Block URLs with embedded credentials (user:pass@host)
            if (returnUrl.Contains("@"))
                return false;

            return true;
        }
    }
}