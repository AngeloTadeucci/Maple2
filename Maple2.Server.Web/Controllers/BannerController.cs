using System.IO;
using Maple2.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maple2.Server.Web.Controllers;

[Route("/banner/ms2/01/")]
public class BannerController : ControllerBase {

    [HttpGet("{bannerId}/{fileHash}.m2u")]
    public IResult GetBanner(long bannerId, string fileHash) {
        string fullPath = Path.Combine(Paths.WEB_DATA_DIR, "banner", bannerId.ToString(), $"{fileHash}.m2u");
        if (!System.IO.File.Exists(fullPath)) {
            return Results.NotFound();
        }

        using FileStream banner = System.IO.File.OpenRead(fullPath);
        return Results.File(banner, contentType: "application/octet-stream");
    }
}
