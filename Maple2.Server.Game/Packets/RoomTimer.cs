using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple2.Server.Game.Packets;

public static class RoomTimer {
    private enum Command : byte {
        FieldEnter = 0,
        Modify = 1,
        Unknown2 = 2, // identical to 0
        Unknown3 = 3,
        Unknown4 = 4, // sets to zero
        Unknown5 = 5, // sets to zero
    }
}
