using System.IO;
using Maple2.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maple2.Server.Web.Controllers;

[Route("/system/")]
public class SystemController : ControllerBase {

    [HttpGet("banner/{name}.png")]
    public IResult GetBanner(string name) {
        string fullPath = $"{Paths.WEB_DATA_DIR}/system/banner/{name}.png";
        if (!System.IO.File.Exists(fullPath)) {
            return Results.NotFound();
        }

        FileStream bannerImage = System.IO.File.OpenRead(fullPath);
        return Results.File(bannerImage, contentType: "image/png");
    }
}
