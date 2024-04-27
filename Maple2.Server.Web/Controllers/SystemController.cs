using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maple2.Server.Web.Controllers;

[Route("/system/")]
public class SystemController : ControllerBase {
    private static readonly string SolutionDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
    private static readonly string RootDir = Path.Combine(SolutionDir, "Maple2.Server.Web/Data");

    [HttpGet("banner/{name}.png")]
    public IResult GetBanner(string name) {
        string fullPath = $"{RootDir}/system/banner/{name}.png";
        if (!System.IO.File.Exists(fullPath)) {
            return Results.NotFound();
        }

        FileStream bannerImage = System.IO.File.OpenRead(fullPath);
        return Results.File(bannerImage, contentType: "image/png");
    }
}
