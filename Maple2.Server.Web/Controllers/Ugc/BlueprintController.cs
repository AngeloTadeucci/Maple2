using System.IO;
using Maple2.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maple2.Server.Web.Controllers.Ugc;

[Route("/blueprint/ms2/01/")]
public class BlueprintController : ControllerBase {

    [HttpGet("{blueprintId}/{ugcUid}.png")]
    public IResult GetBlueprint(long blueprintId, string ugcUid) {
        string fullPath = Path.Combine(Paths.WEB_DATA_DIR, "blueprint", blueprintId.ToString(), $"{ugcUid}.png");
        if (!System.IO.File.Exists(fullPath)) {
            return Results.NotFound();
        }

        FileStream blueprint = System.IO.File.OpenRead(fullPath);
        return Results.File(blueprint, contentType: "image/png");
    }
}
