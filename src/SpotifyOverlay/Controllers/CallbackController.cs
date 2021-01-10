using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SpotifyOverlay.Classes;

namespace SpotifyOverlay.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        private readonly SpotifyService spotifyService;

        public CallbackController(SpotifyService spotifyService)
        {
            this.spotifyService = spotifyService;
        }

        // GET
        public async Task<RedirectResult> Index(string code)
        {
            // give our code to spotifyService
            // wait for it to swap
            var success = await spotifyService.SwapCodeForTokenAsync(code);

            if (success)
            {
                // redirect to index
                return Redirect("/");
            }
            else
            {
                // error page?               
            }
            return null;
        }
    }
}