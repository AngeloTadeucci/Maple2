using System.IO;
using Maple2.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maple2.Server.Web.Controllers.Ugc;

[Route("/itemicon/ms2/01/")]
public class ItemIconController : ControllerBase {

    [HttpGet("{itemId}/{uid}.png")]
    public IResult GetItem(long itemId, string uid) {
        string fullPath = $"{Paths.WEB_DATA_DIR}/itemicon/{itemId}/{uid}.png";
        if (!System.IO.File.Exists(fullPath)) {
            return Results.NotFound();
        }

        FileStream itemIcon = System.IO.File.OpenRead(fullPath);
        return Results.File(itemIcon, contentType: "image/png");
    }
}
