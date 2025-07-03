using System.Diagnostics;
using System.Numerics;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class HomeActionPacket {
    private enum HomeActionCommand : byte {
        Alarm = 4,
        Survey = 5,
        PortalCube = 6,
        Ball = 7,
        Roll = 11,
        NoticeCube = 12,
    }

    private enum SurveyCommand : byte {
        Message = 0,
        Question = 2,
        AddOption = 3,
        Start = 4,
        Answer = 5,
        End = 6,
    }

    private enum BallCommand : byte {
        Add = 0,
        Remove = 1,
        Update = 2,
        Hit = 3,
    }

    public static ByteWriter SendCubePortalSettings(PlotCube cube, List<string> otherPortalsNames) {
        Debug.Assert(cube.Interact?.PortalSettings != null, nameof(cube.Interact.PortalSettings) + " != null");

        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.PortalCube);
        pWriter.WriteByte();
        pWriter.Write<Vector3B>(cube.Position);
        pWriter.WriteClass<CubePortalSettings>(cube.Interact.PortalSettings);
        pWriter.WriteInt(otherPortalsNames.Count);
        foreach (string portalName in otherPortalsNames) {
            pWriter.WriteUnicodeString(portalName);
        }

        return pWriter;
    }

    public static ByteWriter SendCubeNoticeSettings(PlotCube cube, bool editing = false) {
        Debug.Assert(cube.Interact?.NoticeSettings != null, nameof(cube.Interact.NoticeSettings) + " != null");

        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.NoticeCube);
        pWriter.WriteBool(editing);
        pWriter.Write<Vector3B>(cube.Position);
        pWriter.WriteClass<CubeNoticeSettings>(cube.Interact.NoticeSettings);

        return pWriter;
    }

    public static ByteWriter HostAlarm(string message) {
        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.Alarm);
        pWriter.WriteByte();
        pWriter.WriteInt();
        pWriter.WriteUnicodeString(message);

        return pWriter;
    }

    public static ByteWriter SurveyMessage() {
        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.Survey);
        pWriter.Write<SurveyCommand>(SurveyCommand.Message);

        return pWriter;
    }

    public static ByteWriter SurveyQuestion(HomeSurvey survey) {
        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.Survey);
        pWriter.Write<SurveyCommand>(SurveyCommand.Question);
        pWriter.WriteUnicodeString(survey.Question);
        pWriter.WriteBool(survey.Public);

        return pWriter;
    }

    public static ByteWriter SurveyAddOption(HomeSurvey survey, bool success = true) {
        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.Survey);
        pWriter.Write<SurveyCommand>(SurveyCommand.AddOption);
        pWriter.WriteUnicodeString(survey.Question);
        pWriter.WriteBool(survey.Public);
        pWriter.WriteUnicodeString(survey.Options.Keys.Last());
        pWriter.WriteBool(success);

        return pWriter;
    }

    public static ByteWriter SurveyStart(HomeSurvey survey) {
        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.Survey);
        pWriter.Write<SurveyCommand>(SurveyCommand.Start);
        pWriter.WriteLong(survey.OwnerId);
        pWriter.WriteLong(survey.Id);
        pWriter.WriteBool(survey.Public);
        pWriter.WriteUnicodeString(survey.Question);
        pWriter.WriteByte((byte) survey.Options.Count);
        foreach (string option in survey.Options.Keys) {
            pWriter.WriteUnicodeString(option);
        }

        return pWriter;
    }

    public static ByteWriter SurveyAnswer(string name) {
        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.Survey);
        pWriter.Write<SurveyCommand>(SurveyCommand.Answer);
        pWriter.WriteUnicodeString(name);

        return pWriter;
    }

    public static ByteWriter SurveyEnd(HomeSurvey survey) {
        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.Survey);
        pWriter.Write<SurveyCommand>(SurveyCommand.End);
        pWriter.WriteLong(survey.Id);
        pWriter.WriteBool(survey.Public);
        pWriter.WriteUnicodeString(survey.Question);
        pWriter.WriteByte((byte) survey.Options.Count);
        foreach (KeyValuePair<string, List<string>> option in survey.Options) {
            pWriter.WriteUnicodeString(option.Key);
            pWriter.WriteByte((byte) option.Value.Count);

            if (!survey.Public) {
                continue;
            }

            foreach (string name in option.Value) {
                pWriter.WriteUnicodeString(name);
            }
        }

        pWriter.WriteByte((byte) survey.AvailableCharacters.Count);
        if (!survey.Public) {
            return pWriter;
        }

        foreach (string name in survey.AvailableCharacters) {
            pWriter.WriteUnicodeString(name);
        }

        return pWriter;
    }

    public static ByteWriter Roll(Character character, int number) {
        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.Roll);
        pWriter.WriteLong(character.AccountId);
        pWriter.WriteUnicodeString(character.Name);
        pWriter.WriteByte();
        pWriter.WriteByte(1);
        pWriter.WriteInt(1);
        pWriter.Write(StringCode.s_ugcmap_fun_roll);
        pWriter.WriteInt(2); // number of strings
        pWriter.WriteUnicodeString(character.Name);
        pWriter.WriteUnicodeString(number.ToString());

        return pWriter;
    }

    public static ByteWriter AddBall(FieldGuideObject guideObject) {
        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.Ball);
        pWriter.Write<BallCommand>(BallCommand.Add);
        pWriter.WriteInt(guideObject.ObjectId);
        pWriter.WriteLong(guideObject.CharacterId);
        pWriter.Write<Vector3>(guideObject.Position);
        guideObject.Value.WriteTo(pWriter);

        return pWriter;
    }

    public static ByteWriter RemoveBall(FieldGuideObject guideObject) {
        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.Ball);
        pWriter.Write<BallCommand>(BallCommand.Remove);
        pWriter.WriteInt(guideObject.ObjectId);

        return pWriter;
    }

    public static ByteWriter UpdateBall(FieldGuideObject guideObject, Vector3 velocity1, Vector3 velocity2) {
        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.Ball);
        pWriter.Write<BallCommand>(BallCommand.Update);
        pWriter.WriteInt(guideObject.ObjectId);
        pWriter.WriteLong(guideObject.CharacterId);
        pWriter.Write<Vector3>(guideObject.Position);
        pWriter.Write<Vector3>(velocity1);
        pWriter.Write<Vector3>(velocity2);

        return pWriter;
    }

    public static ByteWriter HitBall(FieldGuideObject guideObject, Vector3 velocity) {
        var pWriter = Packet.Of(SendOp.HomeAction);
        pWriter.Write<HomeActionCommand>(HomeActionCommand.Ball);
        pWriter.Write<BallCommand>(BallCommand.Hit);
        pWriter.WriteInt(guideObject.ObjectId);
        pWriter.WriteLong(guideObject.CharacterId);
        pWriter.Write<Vector3>(guideObject.Position);
        pWriter.Write<Vector3>(velocity);

        return pWriter;
    }
}
