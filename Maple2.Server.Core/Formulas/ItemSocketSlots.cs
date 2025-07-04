namespace Maple2.Server.Core.Formulas;

public static class ItemSocketSlots {
    // This is entirely just a guess. 5% success rate per socket.
    public static byte OpenSocketCount(int maxSlots) {
        byte openSlots = 0;
        for (int i = 0; i < maxSlots; i++) {
            int successValue = Random.Shared.Next(0, 100);
            if (successValue < 95) {
                break;
            }
            openSlots++;
        }
        return openSlots;
    }
}
