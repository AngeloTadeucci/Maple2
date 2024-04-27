using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maple2.Server.Web.Controllers;

[Route("/item/ms2/01/")]
public class ItemController : ControllerBase {
    private static readonly string SolutionDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
    private static readonly string RootDir = Path.Combine(SolutionDir, "Maple2.Server.Web/Data");

    [HttpGet("{itemId}/{uid}.m2u")]
    public IResult GetItem(long itemId, string uid) {
        string fullPath = $"{RootDir}/items/{itemId}/{uid}.m2u";
        if (!System.IO.File.Exists(fullPath)) {
            return Results.BadRequest();
        }

        FileStream item = System.IO.File.OpenRead(fullPath);
        return Results.File(item, contentType: "application/octet-stream");
    }
}
