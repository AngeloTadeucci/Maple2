using System;
using System.IO;
using System.Threading.Tasks;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Maple2.Server.Web.Controllers;

[Route("")]
public class WebController(WebStorage webStorage) : ControllerBase {

    [HttpPost("urq.aspx")]
    public async Task<IResult> Upload() {
        Stream bodyStream = Request.Body;
        var memoryStream = new MemoryStream();
        await bodyStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0; // reset position to beginning of stream before returning
        if (memoryStream.Length == 0) {
            return Results.BadRequest("Request was empty");
        }

        IByteReader packet = new ByteReader(memoryStream.ToArray());
        packet.ReadInt();
        var type = (UgcType) packet.ReadInt();
        packet.ReadLong();
        long characterId = packet.ReadLong();
        long ugcUid = packet.ReadLong();
        int id = packet.ReadInt(); // item id, guild id, others?
        packet.ReadInt();
        packet.ReadLong();

        byte[] fileBytes = packet.ReadBytes(packet.Available);

        UgcResource? resource = null;
        if (ugcUid != 0) {
            using WebStorage.Request db = webStorage.Context();
            resource = db.GetUgc(ugcUid);
            if (resource == null) {
                return Results.NotFound($"{ugcUid} does not exist.");
            }

            if (type != UgcType.ItemIcon && !string.IsNullOrEmpty(resource.Path)) {
                if (System.IO.File.Exists(resource.Path)) {
                    return Results.Conflict("resource already exists.");
                }

                await System.IO.File.WriteAllBytesAsync(resource.Path, fileBytes);
                return Results.Text($"0,{resource.Path}");
            }
        }
        if (resource == null) {
            return Results.BadRequest("Invalid resource.");
        }

        return type switch {
            UgcType.ProfileAvatar => UploadProfileAvatar(fileBytes, characterId),
            UgcType.Item or UgcType.Mount or UgcType.Furniture => UploadItem(fileBytes, id, ugcUid, resource),
            UgcType.ItemIcon => UploadItemIcon(fileBytes, id, ugcUid, resource),
            _ => HandleUnknownMode(type),
        };
    }

    private static IResult UploadProfileAvatar(byte[] fileBytes, long characterId) {
        string filePath = $"{Paths.WEB_DATA_DIR}/profiles/{characterId}/";
        try {
            // Deleting old files in the character folder
            if (Path.Exists(filePath)) {
                Directory.Delete(filePath, true);
            }
            Directory.CreateDirectory(filePath);
        } catch (Exception ex) {
            Log.Error(ex, "Failed preparing directory: {Path}", filePath);
            return Results.Problem("Internal Server Error", statusCode: 500);
        }

        string uniqueFileName = Guid.NewGuid().ToString();
        System.IO.File.WriteAllBytes($"{filePath}/{uniqueFileName}.png", fileBytes);
        return Results.Text($"0,data/profiles/avatar/{characterId}/{uniqueFileName}.png");
    }

    private IResult UploadItem(byte[] fileBytes, int itemId, long ugcId, UgcResource resource) {
        string filePath = $"{Paths.WEB_DATA_DIR}/items/{itemId}/";
        try {
            Directory.CreateDirectory(filePath);
        } catch (Exception ex) {
            Log.Error(ex, "Failed preparing directory: {Path}", filePath);
            return Results.Problem("Internal Server Error", statusCode: 500);
        }
        using WebStorage.Request db = webStorage.Context();
        string ugcPath = $"item/ms2/01/{itemId}/{resource.Id}.m2u";

        db.UpdatePath(ugcId, ugcPath);
        System.IO.File.WriteAllBytes($"{filePath}/{resource.Id}.m2u", fileBytes);
        return Results.Text($"0,{ugcPath}");
    }

    private IResult UploadItemIcon(byte[] fileBytes, int itemId, long ugcId, UgcResource resource) {
        string filePath = $"{Paths.WEB_DATA_DIR}/itemicon/{itemId}/";
        try {
            Directory.CreateDirectory(filePath);
        } catch (Exception ex) {
            Log.Error(ex, "Failed preparing directory: {Path}", filePath);
            return Results.Problem("Internal Server Error", statusCode: 500);
        }
        //TODO: Verify that the item exists in the database
        string ugcPath = $"itemicon/ms2/01/{itemId}/{resource.Id}.png";

        System.IO.File.WriteAllBytes($"{filePath}/{ugcId}.png", fileBytes);
        return Results.Text($"0,{ugcPath}");
    }

    private static IResult HandleUnknownMode(UgcType mode) {
        Log.Logger.Warning("Invalid upload mode: {Mode}", mode);
        return Results.BadRequest($"Invalid upload mode: {mode}");
    }
}
