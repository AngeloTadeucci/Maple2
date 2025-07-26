namespace Maple2.Server.DebugGame.Graphics.Assets;

//public enum AssetType

public class AssetManager {
    public AssetManager? Parent { get; init; }

    public AssetManager(AssetManager? parent = null) {
        Parent = parent;
    }


}
