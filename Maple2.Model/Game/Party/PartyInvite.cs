namespace Maple2.Model.Game.Party;

public class PartyInvite {
    public enum Response : byte {
        Accept = 1,
        RejectInvite = 9,
        RejectTimeout = 12,
    }
}
