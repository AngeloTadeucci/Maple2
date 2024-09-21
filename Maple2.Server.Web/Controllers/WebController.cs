﻿using System;
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
using Serilog.Core;

namespace Maple2.Server.Web.Controllers;

[Route("")]
public class WebController : ControllerBase {

    private readonly WebStorage webStorage;

    public WebController(WebStorage webStorage) {
        this.webStorage = webStorage;
    }

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
        long accountId = packet.ReadLong();
        long characterId = packet.ReadLong();
        long ugcUid = packet.ReadLong();
        int id = packet.ReadInt(); // item id, guild id, others?
        packet.ReadInt();
        packet.ReadLong();

        byte[] fileBytes = packet.ReadBytes(packet.Available);

        Log.Logger.Debug("Upload: type={Type}, accountId={AccountId}, characterId={CharacterId}, ugcUid={UgcUid}, id={Id}", type, accountId, characterId, ugcUid, id);

        UgcResource? resource = null;
        if (ugcUid != 0) {
            using WebStorage.Request db = webStorage.Context();
            resource = db.GetUgc(ugcUid);
            if (resource == null) {
                return Results.NotFound($"{ugcUid} does not exist.");
            }
        }

        if (type is UgcType.ItemIcon or UgcType.Item or UgcType.BlueprintIcon or UgcType.LayoutBlueprint && resource == null) {
            return Results.BadRequest("Invalid UGC resource.");
        }

        return type switch {
            UgcType.ProfileAvatar => UploadProfileAvatar(fileBytes, characterId),
            UgcType.Item or UgcType.Mount or UgcType.Furniture => UploadItem(fileBytes, id, ugcUid, resource!),
            UgcType.ItemIcon => UploadItemIcon(fileBytes, id, ugcUid, resource!),
            UgcType.Banner => UploadBanner(fileBytes, id, ugcUid),
            UgcType.GuildEmblem => HandleGuildEmblem(fileBytes, id, ugcUid),
            UgcType.GuildBanner => HandleGuildBanner(fileBytes, id, ugcUid),
            UgcType.BlueprintIcon => HandleBlueprintIcon(fileBytes, ugcUid, resource!),
            UgcType.LayoutBlueprint => HandleBlueprintPreview(fileBytes, ugcUid, resource!),
            _ => HandleUnknownMode(type),
        };
    }

    private static IResult UploadProfileAvatar(byte[] fileBytes, long characterId) {
        string filePath = Path.Combine(Paths.WEB_DATA_DIR, "profiles", characterId.ToString());
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
        System.IO.File.WriteAllBytes(Path.Combine(filePath, $"{uniqueFileName}.png"), fileBytes);
        return Results.Text($"0,data/profiles/avatar/{characterId}/{uniqueFileName}.png");
    }

    private IResult UploadItem(byte[] fileBytes, int itemId, long ugcId, UgcResource resource) {
        string filePath = Path.Combine(Paths.WEB_DATA_DIR, "items", itemId.ToString());
        try {
            Directory.CreateDirectory(filePath);
        } catch (Exception ex) {
            Log.Error(ex, "Failed preparing directory: {Path}", filePath);
            return Results.Problem("Internal Server Error", statusCode: 500);
        }
        using WebStorage.Request db = webStorage.Context();
        string ugcPath = $"item/ms2/01/{itemId}/{resource.Id}.m2u";

        db.UpdatePath(ugcId, ugcPath);
        System.IO.File.WriteAllBytes(Path.Combine(filePath, $"{resource.Id}.m2u"), fileBytes);
        return Results.Text($"0,{ugcPath}");
    }

    private IResult UploadItemIcon(byte[] fileBytes, int itemId, long ugcId, UgcResource resource) {
        string filePath = Path.Combine(Paths.WEB_DATA_DIR, "itemicon", itemId.ToString());
        try {
            Directory.CreateDirectory(filePath);
        } catch (Exception ex) {
            Log.Error(ex, "Failed preparing directory: {Path}", filePath);
            return Results.Problem("Internal Server Error", statusCode: 500);
        }
        //TODO: Verify that the item exists in the database
        string ugcPath = $"itemicon/ms2/01/{itemId}/{resource.Id}.png";

        System.IO.File.WriteAllBytes(Path.Combine(filePath, $"{ugcId}.png"), fileBytes);
        return Results.Text($"0,{ugcPath}");
    }

    private IResult UploadBanner(byte[] fileBytes, int bannerId, long ugcId) {
        string filePath = Path.Combine(Paths.WEB_DATA_DIR, "banner", bannerId.ToString());
        try {
            Directory.CreateDirectory(filePath);
        } catch (Exception ex) {
            Log.Error(ex, "Failed preparing directory: {Path}", filePath);
            return Results.Problem("Internal Server Error", statusCode: 500);
        }
        using WebStorage.Request db = webStorage.Context();
        string ugcPath = $"banner/ms2/01/{bannerId}/{ugcId}.m2u";

        db.UpdatePath(ugcId, ugcPath);

        System.IO.File.WriteAllBytes(Path.Combine(filePath, $"{ugcId}.m2u"), fileBytes);
        return Results.Text($"0,{ugcPath}");
    }

    private IResult HandleGuildEmblem(byte[] fileBytes, int guildId, long ugcId) {
        string filePath = Path.Combine(Paths.WEB_DATA_DIR, "guildmark", guildId.ToString());
        try {
            Directory.CreateDirectory(filePath);
        } catch (Exception ex) {
            Log.Error(ex, "Failed preparing directory: {Path}", filePath);
            return Results.Problem("Internal Server Error", statusCode: 500);
        }
        using WebStorage.Request db = webStorage.Context();
        string ugcPath = $"guildmark/ms2/01/{guildId}/{ugcId}.png";

        db.UpdatePath(ugcId, ugcPath);

        System.IO.File.WriteAllBytes(Path.Combine(filePath, $"{ugcId}.png"), fileBytes);
        return Results.Text($"0,{ugcPath}");
    }

    private IResult HandleGuildBanner(byte[] fileBytes, int guildId, long ugcId) {
        string filePath = Path.Combine(Paths.WEB_DATA_DIR, "guildmark", guildId.ToString(), "banner");
        try {
            Directory.CreateDirectory(filePath);
        } catch (Exception ex) {
            Log.Error(ex, "Failed preparing directory: {Path}", filePath);
            return Results.Problem("Internal Server Error", statusCode: 500);
        }
        using WebStorage.Request db = webStorage.Context();
        string ugcPath = $"guildmark/ms2/01/{guildId}/banner/{ugcId}.png";

        db.UpdatePath(ugcId, ugcPath);

        System.IO.File.WriteAllBytes(Path.Combine(filePath, $"{ugcId}.png"), fileBytes);
        return Results.Text($"0,{ugcPath}");
    }

    private IResult HandleBlueprintIcon(byte[] fileBytes, long ugcUid, UgcResource resource) {
        string filePath = Path.Combine(Paths.WEB_DATA_DIR, "blueprint", ugcUid.ToString());
        try {
            Directory.CreateDirectory(filePath);
        } catch (Exception ex) {
            Log.Error(ex, "Failed preparing directory: {Path}", filePath);
            return Results.Problem("Internal Server Error", statusCode: 500);
        }

        string ugcPath = $"blueprint/ms2/01/{ugcUid}/{resource.Id}_icon.png";

        System.IO.File.WriteAllBytes(Path.Combine(filePath, $"{resource.Id}_icon.png"), fileBytes);
        return Results.Text($"0,{ugcPath}");
    }

    private IResult HandleBlueprintPreview(byte[] fileBytes, long ugcUid, UgcResource resource) {
        string filePath = Path.Combine(Paths.WEB_DATA_DIR, "blueprint", ugcUid.ToString());
        try {
            Directory.CreateDirectory(filePath);
        } catch (Exception ex) {
            Log.Error(ex, "Failed preparing directory: {Path}", filePath);
            return Results.Problem("Internal Server Error", statusCode: 500);
        }
        using WebStorage.Request db = webStorage.Context();
        string ugcPath = $"blueprint/ms2/01/{ugcUid}/{resource.Id}.png";

        db.UpdatePath(ugcUid, ugcPath);

        System.IO.File.WriteAllBytes(Path.Combine(filePath, $"{resource.Id}.png"), fileBytes);
        return Results.Text($"0,{ugcPath}");
    }


    private static IResult HandleUnknownMode(UgcType mode) {
        Log.Logger.Warning("Invalid upload mode: {Mode}", mode);
        return Results.BadRequest($"Invalid upload mode: {mode}");
    }
}
