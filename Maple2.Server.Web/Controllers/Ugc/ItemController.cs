using System.IO;
using Maple2.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maple2.Server.Web.Controllers.Ugc;

[Route("/item/ms2/01/")]
public class ItemController : ControllerBase {

    [HttpGet("{itemId}/{uid}.m2u")]
    public IResult GetItem(long itemId, string uid) {
        string fullPath = $"{Paths.WEB_DATA_DIR}/items/{itemId}/{uid}.m2u";
        if (!System.IO.File.Exists(fullPath)) {
            return Results.NotFound();
        }

        FileStream item = System.IO.File.OpenRead(fullPath);
        return Results.File(item, contentType: "application/octet-stream");
    }
}
