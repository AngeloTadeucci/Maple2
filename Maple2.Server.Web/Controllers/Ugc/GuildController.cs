using System.IO;
using Maple2.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maple2.Server.Web.Controllers.Ugc;

[Route("/guildmark/ms2/01/")]
public class GuildController : ControllerBase {

    [HttpGet("{guildId}/{uid}.png")]
    public IResult GetGuildEmblem(long guildId, string uid) {
        string fullPath = $"{Paths.WEB_DATA_DIR}/guildmark/{guildId}/{uid}.png";
        if (!System.IO.File.Exists(fullPath)) {
            return Results.NotFound();
        }

        FileStream guildMark = System.IO.File.OpenRead(fullPath);
        return Results.File(guildMark, contentType: "image/png");
    }

    [HttpGet("{guildId}/banner/{uid}.png")]
    public IResult GetGuildBanner(long guildId, string uid) {
        string fullPath = $"{Paths.WEB_DATA_DIR}/guildmark/{guildId}/banner/{uid}.png";
        if (!System.IO.File.Exists(fullPath)) {
            return Results.NotFound();
        }

        FileStream guildBanner = System.IO.File.OpenRead(fullPath);
        return Results.File(guildBanner, contentType: "image/png");
    }
}
