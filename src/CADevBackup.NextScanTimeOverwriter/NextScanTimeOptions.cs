namespace CADevBackup.NextScanTimeOverwriter;

internal class NextScanTimeOptions
{
    public const string SectionName = "NextScanTime";

    public TimeSpan MinNextScanTimeFromUTCNow { get; set; }
    public TimeSpan SLAInterval { get; set; }
}