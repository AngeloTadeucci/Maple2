using Maple2.Model.Metadata;

namespace Maple2.Model.Game;

public class Ride {
    public readonly int OwnerId; // ObjectId of owner.
    public readonly RideMetadata Metadata;
    public readonly RideOnAction Action;
    public readonly int[] Passengers;

    public Ride(int ownerId, RideMetadata metadata, RideOnAction action) {
        OwnerId = ownerId;
        Metadata = metadata;
        Action = action;
        Passengers = new int[metadata.Basic.Passengers];
    }
}
