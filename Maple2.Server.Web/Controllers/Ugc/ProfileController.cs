using System.IO;
using Maple2.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maple2.Server.Web.Controllers.Ugc;

[Route("/data/profiles")]
public class ProfileController : ControllerBase {

    [HttpGet("avatar/{characterId:long}/{hash}.png")]
    public IResult GetAvatar(long characterId, string hash) {
        string fullPath = $"{Paths.WEB_DATA_DIR}/profiles/{characterId}/{hash}.png";
        if (!System.IO.File.Exists(fullPath)) {
            return Results.NotFound();
        }

        FileStream profileImage = System.IO.File.OpenRead(fullPath);
        return Results.File(profileImage, contentType: "image/png");
    }
}
