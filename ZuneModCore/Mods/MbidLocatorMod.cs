using ATL;
﻿using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ZuneModCore.Mods;

public class MbidLocatorModFactory : DIModFactoryBase<MbidLocatorMod>
{
    private const string Description = "Puts MusicBrainz IDs added by MusicBrainz Picard where the Zune " +
        "software can use it to show additional information provided by Community Webservices.\r\n" +
        "Note that this will only have an effect if you have used MusicBrainz Picard on your music library.";

    private const string Author = "Joshua \"Yoshi\" Askharoun";

    public override ModMetadata Metadata => new(nameof(MbidLocatorMod), "MusicBrainz ID Locator",
        Description, Author, new(1, 0));
}

public class MbidLocatorMod(ModMetadata metadata) : Mod(metadata)
{
    public const string ZUNE_ALBUMARTIST_MEDIAID_KEY = "ZuneAlbumArtistMediaID";
    public const string ZUNE_ALBUM_MEDIAID_KEY = "ZuneAlbumMediaID";
    public const string ZUNE_MEDIAID_KEY = "ZuneMediaID";

    private const string MUSICBRAINZ_ARTISTID_KEY = "MusicBrainz Artist Id";
    private const string MUSICBRAINZ_ALBUMARTISTID_KEY = "MusicBrainz Album Artist Id";
    private const string MUSICBRAINZ_ALBUMID_KEY = "MusicBrainz Album Id";
    private const string MUSICBRAINZ_TRACKID_KEY = "MusicBrainz Track Id";

    private static readonly string[] KNOWN_EXTS = [".mp3", ".mp4", ".m4a", ".wav"];

    public override AbstractUICollection? GetDefaultOptionsUI()
    {
        AbstractUICollection optionsUi = new(nameof(MbidLocatorMod))
        {
            new AbstractTextBox("folderBox", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)),
            new AbstractBoolean("recursiveBox", "Search recursively")
        };
        optionsUi.Title = "select music";
        optionsUi.Subtitle = "CHOOSE YOUR MUSIC FOLDER.";
        return optionsUi;
    }

    public override IReadOnlyList<Type>? DependentMods => null;

    public override async Task<string?> Apply()
    {
        // Use user choices from AbstractUI
        string folderPath = ((AbstractTextBox)OptionsUI![0]).Value;
        bool recursive = ((AbstractBoolean)OptionsUI[1]).State;
        string errorString = string.Empty;

        // Verify that the folder exists
        DirectoryInfo folder = new(folderPath);
        if (!folder.Exists)
        {
            return $"The folder '{folder.FullName}' does not exist.";
        }

        foreach (FileInfo file in folder.GetFiles("*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
        {
            try
            {
                if (KNOWN_EXTS.Contains(file.Extension))
                    //UpdateMbidInFile(file);
                    await UpdateMbidInFileAsync(file);

#if DEBUG
                System.Diagnostics.Debug.WriteLine(file);
#endif
            }
            catch (Exception ex)
            {
                errorString += ex.Message + "\r\n";
                continue;
            }
        }

        return errorString.Length != 0
            ? errorString : null;
    }

    public override Task<string?> Reset()
    {
        return Task.FromResult<string?>(null);
    }

    public static async Task UpdateMbidInFileAsync(FileInfo file)
    {
        Track track = new(file.FullName);

        if (track.AudioFormat.IsValidMimeType("audio/mp4"))
        {
            // TODO: Verify that Zune Media IDs in MP4 tags are supposed to be strings and not bytes like in ID3v2 tags

            if (track.AdditionalFields.TryGetValue(MUSICBRAINZ_ALBUMARTISTID_KEY, out var artistId))
                track.AdditionalFields[ZUNE_ALBUMARTIST_MEDIAID_KEY] = artistId;

            if (track.AdditionalFields.TryGetValue(MUSICBRAINZ_ALBUMID_KEY, out var albumId))
                track.AdditionalFields[ZUNE_ALBUM_MEDIAID_KEY] = albumId;

            if (track.AdditionalFields.TryGetValue(MUSICBRAINZ_TRACKID_KEY, out var trackId))
                track.AdditionalFields[ZUNE_MEDIAID_KEY] = trackId;
        }
        else if (track.AudioFormat.IsValidMimeType("audio/mpeg"))
        {
            const string PRIV_FRAME_PREFIX = "PRIV.";

            if (track.AdditionalFields.TryGetValue(MUSICBRAINZ_ALBUMARTISTID_KEY, out var artistId))
            {
                var newTag = EncodeMbidTagAsByteString(artistId);
                if (track.AdditionalFields.TryGetValue(PRIV_FRAME_PREFIX + ZUNE_ALBUMARTIST_MEDIAID_KEY, out var oldTag))
                    CommunityToolkit.Diagnostics.Guard.IsEqualTo(newTag, oldTag);
            }

            if (track.AdditionalFields.TryGetValue(MUSICBRAINZ_ALBUMID_KEY, out var albumId))
            {
                var newTag = EncodeMbidTagAsByteString(albumId);
                if (track.AdditionalFields.TryGetValue(PRIV_FRAME_PREFIX + ZUNE_ALBUM_MEDIAID_KEY, out var oldTag))
                    CommunityToolkit.Diagnostics.Guard.IsEqualTo(newTag, oldTag);
            }

            if (track.AdditionalFields.TryGetValue("UFID", out var ufid))
            {
                // The UFID field may contain various IDs, but we only want MusicBrainz
                var ufidParts = ufid.Split('\0');
                if (ufidParts[0].Equals("http://musicbrainz.org", StringComparison.Ordinal))
                {
                    var mbid = ufidParts[1];
                    var newTag = EncodeMbidTagAsByteString(mbid);
                    if (track.AdditionalFields.TryGetValue(PRIV_FRAME_PREFIX + ZUNE_MEDIAID_KEY, out var oldTag))
                        CommunityToolkit.Diagnostics.Guard.IsEqualTo(newTag, oldTag);
                }
            }
        }
        else
        {
            return;
        }

        // Force sub-version to be ID3v2.3, otherwise Zune won't see the tags
        Settings.ID3v2_tagSubVersion = 3;

        //await track.SaveAsync();
    }

    public static void UpdateMbidInFile(FileInfo file)
    {
        var tfile = TagLib.File.Create(file.FullName);

        if (tfile.Tag is TagLib.Asf.Tag asfTag)
        {
            var albumArtistIdDesc = asfTag.ExtendedContentDescriptionObject
                .FirstOrDefault(cd => cd.Name == ZUNE_ALBUMARTIST_MEDIAID_KEY);
            if (albumArtistIdDesc == null && asfTag.MusicBrainzReleaseArtistId != null)
            {
                asfTag.ExtendedContentDescriptionObject.AddDescriptor(new TagLib.Asf.ContentDescriptor(
                    ZUNE_ALBUMARTIST_MEDIAID_KEY, asfTag.MusicBrainzReleaseArtistId));
            }

            var albumIdDesc = asfTag.ExtendedContentDescriptionObject
                .FirstOrDefault(cd => cd.Name == ZUNE_ALBUM_MEDIAID_KEY);
            if (albumIdDesc == null && asfTag.MusicBrainzReleaseId != null)
            {
                asfTag.ExtendedContentDescriptionObject.AddDescriptor(new TagLib.Asf.ContentDescriptor(
                    ZUNE_ALBUM_MEDIAID_KEY, asfTag.MusicBrainzReleaseId));
            }

            var trackIdDesc = asfTag.ExtendedContentDescriptionObject
                .FirstOrDefault(cd => cd.Name == ZUNE_MEDIAID_KEY);
            if (trackIdDesc == null && asfTag.MusicBrainzTrackId != null)
            {
                asfTag.ExtendedContentDescriptionObject.AddDescriptor(new TagLib.Asf.ContentDescriptor(
                    ZUNE_MEDIAID_KEY, asfTag.MusicBrainzTrackId));
            }
        }
        else if (tfile.Tag is TagLib.NonContainer.Tag ncTag)
        {
            var tag = ncTag.Tags.FirstOrDefault(t => t.TagTypes == TagLib.TagTypes.Id3v2);
            if (tag is TagLib.Id3v2.Tag id3v2)
            {
                var frames = id3v2.GetFrames<TagLib.Id3v2.PrivateFrame>();

                // Note the use of .ToList(): this is because calling RemoveFrame() will
                // modify the underlying collection, causing an exception to be thrown
                // when attempting the next iteration of the for loop

                if (tag.MusicBrainzReleaseArtistId != null)
                {
                    foreach (var oldAlbumArtistFrame in frames.Where(f => f.Owner == ZUNE_ALBUMARTIST_MEDIAID_KEY).ToList())
                        id3v2.RemoveFrame(oldAlbumArtistFrame);

                    TagLib.Id3v2.PrivateFrame albumArtistFrame = new(ZUNE_ALBUMARTIST_MEDIAID_KEY)
                    {
                        PrivateData = new(EncodeMbidTag(tag.MusicBrainzReleaseArtistId))
                    };
                    id3v2.AddFrame(albumArtistFrame);
                }

                if (tag.MusicBrainzReleaseId != null)
                {
                    foreach (var oldAlbumFrame in frames.Where(f => f.Owner == ZUNE_ALBUM_MEDIAID_KEY).ToList())
                        id3v2.RemoveFrame(oldAlbumFrame);

                    TagLib.Id3v2.PrivateFrame albumFrame = new(ZUNE_ALBUM_MEDIAID_KEY)
                    {
                        PrivateData = new(EncodeMbidTag(tag.MusicBrainzReleaseId))
                    };
                    id3v2.AddFrame(albumFrame);
                }

                if (tag.MusicBrainzTrackId != null)
                {
                    foreach (var oldTrackFrame in frames.Where(f => f.Owner == ZUNE_MEDIAID_KEY).ToList())
                        id3v2.RemoveFrame(oldTrackFrame);

                    TagLib.Id3v2.PrivateFrame trackFrame = new(ZUNE_MEDIAID_KEY)
                    {
                        PrivateData = new(EncodeMbidTag(tag.MusicBrainzTrackId))
                    };
                    id3v2.AddFrame(trackFrame);
                }
            }
        }

        tfile.Save();
    }

    private static byte[] EncodeMbidTag(string mbidTag)
    {
        // MBID tags may have more than one ID in them.
        // So far, I've only seen forward slashes used as delimiters,
        // but we might as well cover all the bases.
        mbidTag = mbidTag.Split(['/', '\\', ',', ';']).First();
        return new Guid(mbidTag).ToByteArray();
    }

    private static string EncodeMbidTagAsByteString(string mbidTag)
    {
        var guidBytes = EncodeMbidTag(mbidTag);
        return System.Text.Encoding.Latin1.GetString(guidBytes);
    }
}
