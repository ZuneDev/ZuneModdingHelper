using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZuneModCore.Win32;

public static partial class HostsEditor
{
    private static readonly string _hostsPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\drivers\etc\hosts");

    [GeneratedRegex(@"(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\s+(?<name>[\w.-]+?)(\s+|$)")]
    private static partial Regex HostsEntryRegex();

    public static async Task AddOrUpdateHostnamesWithIPAsync(IEnumerable<HostsEntry> newEntries)
    {
        var unhandledEntries = newEntries
            .ToDictionary(e => e.Hostname, e => e.IPAddress);

        var hostLines = await ReadHostsFileAsync();

        // Update existing entries
        Regex rx = HostsEntryRegex();
        for (int l = 0; l < hostLines.Count; l++)
        {
            var line = hostLines[l].Trim();

            // Ignore comments and empty lines
            if (line.Length <= 0 || line[0] is '#')
                continue;

            var match = rx.Match(line);
            if (!match.Success)
                continue;

            var hostname = match.Groups["name"].Value;
            var ip = match.Groups["ip"].Value;

            // Ignore entries we've already handled
            if (!unhandledEntries.TryGetValue(hostname, out var newIP))
                continue;

            // Avoid changing lines when we don't have to. This
            // preserves trivia such as whitespace and comments when possible
            if (IPAddress.Parse(ip) == newIP)
                continue;

            // Reconstruct entry with new IP address
            hostLines[l] = $"{newIP} {hostname}";

            // Track that we handled this domain
            unhandledEntries.Remove(hostname);
        }

        // Add any entries that didn't already exist
        if (unhandledEntries.Count > 0)
        {
            foreach (var (hostname, ip) in unhandledEntries)
                hostLines.Add($"{ip} {hostname}");
        }

        await WriteHostsFileAsync(hostLines);
    }

    private static async Task<List<string>> ReadHostsFileAsync()
    {
        var lines = await File.ReadAllLinesAsync(_hostsPath);
        return lines.ToList();
    }

    private static async Task WriteHostsFileAsync(List<string> lines)
    {
        await File.WriteAllLinesAsync(_hostsPath, lines);
    }
}

public class HostsEntry(IPAddress ipAddress, string hostname, string? originalString = null)
    : IEquatable<HostsEntry>
{
    private IPAddress _ipAddress = ipAddress;
    private string _hostname = hostname;
    private string? _originalString = originalString;

    public IPAddress IPAddress
    {
        get => _ipAddress;
        set
        {
            _originalString = null;
            _ipAddress = value;
        }
    }

    public string Hostname
    {
        get => _hostname;
        set
        {
            _originalString = null;
            _hostname = value;
        }
    }

    public bool Equals(HostsEntry? other)
    {
        if (other is null)
            return false;

        return IPAddress.Equals(other.IPAddress)
            && Hostname.Equals(other.Hostname, StringComparison.InvariantCulture);
    }

    public override bool Equals(object obj) => Equals(obj as HostsEntry);

    public override string ToString() => _originalString ?? $"{IPAddress} {Hostname}";

    public override int GetHashCode() => (IPAddress, Hostname).GetHashCode();
}
