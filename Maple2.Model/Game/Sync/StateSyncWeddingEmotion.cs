using Maple2.PacketLib.Tools;

namespace Maple2.Model.Game;

// gosWeddingEmotion
public class StateSyncWeddingEmotion : StateSync {
    public bool UnknownWeddingEmotionBool1;
    public bool UnknownWeddingEmotionBool2;
    public int UnknownWeddingEmotionInt1;
    public int UnknownWeddingEmotionInt2;
    public bool UnknownWeddingEmotionBool3;

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteBool(UnknownWeddingEmotionBool1);
        writer.WriteBool(UnknownWeddingEmotionBool2);
        if (UnknownWeddingEmotionBool1) {
            writer.WriteInt(UnknownWeddingEmotionInt1);
        }
        if (UnknownWeddingEmotionBool2) {
            writer.WriteInt(UnknownWeddingEmotionInt2);
            writer.WriteBool(UnknownWeddingEmotionBool3);
        }
    }

    public override void ReadFrom(IByteReader reader) {
        base.ReadFrom(reader);
        UnknownWeddingEmotionBool1 = reader.ReadBool();
        UnknownWeddingEmotionBool2 = reader.ReadBool();
        if (UnknownWeddingEmotionBool1) {
            UnknownWeddingEmotionInt1 = reader.ReadInt();
        }
        if (UnknownWeddingEmotionBool2) {
            UnknownWeddingEmotionInt2 = reader.ReadInt();
            UnknownWeddingEmotionBool3 = reader.ReadBool();
        }
    }
}
