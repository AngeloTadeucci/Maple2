using System.Security.Cryptography;
using Maple2.Tools;

namespace Maple2.File.Ingest.Utils;

public static class NavmeshHash {
    public static bool HasValidHash(string filename) {
        string hashPath = Path.Combine(Paths.NAVMESH_HASH_DIR, $"{filename}-hash");

        if (!System.IO.File.Exists(hashPath)) {
            return false;
        }

        string currentHash = System.IO.File.ReadAllText(hashPath);
        string newHash = GetHash(filename);

        return currentHash.Equals(newHash);
    }

    public static void WriteHash(string filename) {
        string hashPath = Path.Combine(Paths.NAVMESH_HASH_DIR, $"{filename}-hash");

        string newHash = GetHash(filename);

        System.IO.File.WriteAllText(hashPath, newHash);
    }

    private static string GetHash(string filename) {
        string filepath = Path.Combine(Paths.NAVMESH_DIR, $"{filename}.navmesh");

        if (!System.IO.File.Exists(filepath)) {
            return "";
        }

        using MD5 md5 = MD5.Create();
        using FileStream stream = System.IO.File.OpenRead(filepath);

        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}