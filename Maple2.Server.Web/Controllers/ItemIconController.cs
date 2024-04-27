using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maple2.Server.Web.Controllers;

[Route("/itemicon/ms2/01/")]
public class ItemIconController : ControllerBase {
    private static readonly string SolutionDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
    private static readonly string RootDir = Path.Combine(SolutionDir, "Maple2.Server.Web/Data");

    [HttpGet("{itemId}/{uid}.png")]
    public IResult GetItem(long itemId, string uid) {
        string fullPath = $"{RootDir}/itemicon/{itemId}/{uid}.png";
        if (!System.IO.File.Exists(fullPath)) {
            return Results.BadRequest();
        }

        FileStream itemIcon = System.IO.File.OpenRead(fullPath);
        return Results.File(itemIcon, contentType: "image/png");
    }
}
