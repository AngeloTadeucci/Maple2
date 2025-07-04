namespace Maple2.Model.Enum;

public enum SessionState {
    Disconnected = 0,
    ChangeMap = 1, // Moving between maps
    ChangeChannel = 2, // Changing channels
    Connected = 3,
}
