using Maple2.Model.Metadata;

namespace Maple2.Model.Game;

public class Ride(int ownerId, RideMetadata metadata, RideOnAction action) {
    public readonly int OwnerId = ownerId; // ObjectId of owner.
    public readonly RideMetadata Metadata = metadata;
    public readonly RideOnAction Action = action;
    public readonly int[] Passengers = new int[metadata.Basic.Passengers];

}
