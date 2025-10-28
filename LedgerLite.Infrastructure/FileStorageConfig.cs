using System.IO;

namespace LedgerLite.Infrastructure;

public record FileStorageConfig(string DataDirectory = "data", string ExportDirectory = "exports")
{
    public static FileStorageConfig Default => new();

    public FileStorageConfig()
        : this("data", "exports")
    {
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(ExportDirectory);
    }
}
