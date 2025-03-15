using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public interface IFieldProperty : IByteSerializable {
    public FieldProperty Type { get; }
}

public class FieldPropertyGravity : IFieldProperty, IByteDeserializable {
    public FieldProperty Type => FieldProperty.Gravity;

    public float Gravity { get; private set; }

    public FieldPropertyGravity(float gravity) {
        Gravity = gravity;
    }

    public void WriteTo(IByteWriter writer) {
        writer.Write<FieldProperty>(Type);
        writer.WriteFloat(Gravity);
    }

    public void ReadFrom(IByteReader reader) {
        reader.ReadByte();
        Gravity = reader.ReadFloat();
    }
}

public class FieldPropertyMusicConcert : IFieldProperty {
    public FieldProperty Type => FieldProperty.MusicConcert;

    public long CharacterId { get; init; }
    public int Unknown { get; init; } // ServerTicks?

    public void WriteTo(IByteWriter writer) {
        writer.Write<FieldProperty>(Type);
        writer.WriteLong(CharacterId);
        writer.WriteInt(Unknown);
    }
}

public class FieldPropertyHidePlayer : IFieldProperty {
    public FieldProperty Type => FieldProperty.HidePlayer;

    public void WriteTo(IByteWriter writer) {
        writer.Write<FieldProperty>(Type);
    }
}

public class FieldPropertyLockPlayer : IFieldProperty {
    public FieldProperty Type => FieldProperty.LockPlayer;

    public void WriteTo(IByteWriter writer) {
        writer.Write<FieldProperty>(Type);
    }
}

public class FieldPropertyUserTagSymbol : IFieldProperty {
    public FieldProperty Type => FieldProperty.UserTagSymbol;

    public required string Symbol1 { get; init; }
    public required string Symbol2 { get; init; }

    public void WriteTo(IByteWriter writer) {
        writer.Write<FieldProperty>(Type);
        writer.WriteUnicodeString(Symbol1);
        writer.WriteUnicodeString(Symbol2);
    }
}

public class FieldPropertySightRange : IFieldProperty {
    public FieldProperty Type => FieldProperty.SightRange;

    public float Range { get; set; } = 450;
    public float[] Fades { get; init; } = new float[3];
    public bool Unknown { get; set; }
    public byte Opacity { get; set; }
    public bool Opaque { get; set; } = false;

    public void WriteTo(IByteWriter writer) {
        writer.Write<FieldProperty>(Type);
        writer.WriteFloat(Range);
        foreach (float fade in Fades) {
            writer.WriteFloat(fade);
        }
        writer.WriteBool(Unknown);
        writer.WriteByte(Opacity);
        writer.WriteBool(Opaque);
    }
}

public class FieldPropertyWeather : IFieldProperty {
    public FieldProperty Type => FieldProperty.Weather;

    public WeatherType WeatherType { get; init; }

    public void WriteTo(IByteWriter writer) {
        writer.Write<FieldProperty>(Type);
        writer.Write<WeatherType>(WeatherType);
    }
}

public class FieldPropertyAmbientLight : IFieldProperty {
    public FieldProperty Type => FieldProperty.AmbientLight;

    public Byte3 Color { get; init; }

    public void WriteTo(IByteWriter writer) {
        writer.Write<FieldProperty>(Type);
        writer.Write<Byte3>(Color);
    }
}

public class FieldPropertyDirectionalLight : IFieldProperty {
    public FieldProperty Type => FieldProperty.DirectionalLight;

    public Byte3 DiffuseColor { get; init; }
    public Byte3 SpecularColor { get; init; }

    public void WriteTo(IByteWriter writer) {
        writer.Write<FieldProperty>(Type);
        writer.Write<Byte3>(DiffuseColor);
        writer.Write<Byte3>(SpecularColor);
    }
}

public class FieldPropertyLocalCamera : IFieldProperty {
    public FieldProperty Type => FieldProperty.LocalCamera;

    public bool Enabled { get; init; }

    public void WriteTo(IByteWriter writer) {
        writer.Write<FieldProperty>(Type);
        writer.WriteBool(Enabled);
    }
}

public class FieldPropertyPhotoStudio : IFieldProperty {
    public FieldProperty Type => FieldProperty.PhotoStudio;

    public bool Enabled { get; init; }

    public void WriteTo(IByteWriter writer) {
        writer.Write<FieldProperty>(Type);
        writer.WriteBool(Enabled);
    }
}
