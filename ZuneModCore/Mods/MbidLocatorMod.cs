using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ZuneModCore.Mods;

public class MbidLocatorModFactory : DIModFactoryBase<MbidLocatorMod>
{
    public override ModMetadata Metadata { get; } = new()
    {
        Id = nameof(MbidLocatorMod),
        Title = "Music Tag Fixer",
        Author = "Joshua \"Yoshi\" Askharoun",
        Version = new(2, 0),
        Description = "Fixes music tags to improve Zune compatibility and enable Marketplace features.",
        ExtendedDescription = "• If your library is tagged with MusicBrainz Picard, " +
            "this places MBIDs where Zune can use them for Marketplace features.\r\n" +
            "• Converts ID3v2.4 to ID3v2.3.",
    };
}

public class MbidLocatorMod(ModMetadata metadata) : Mod(metadata)
{
    public const string ZUNE_ALBUM_ARTIST_MEDIA_ID_NAME = "ZuneAlbumArtistMediaID";
    public const string ZUNE_ALBUM_MEDIA_ID_NAME = "ZuneAlbumMediaID";
    public const string ZUNE_MEDIA_ID_NAME = "ZuneMediaID";
    public const string ZUNE_COLLECTION_ID_NAME = "ZuneCollectionID";

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
                    UpdateMbidInFile(file);

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

        if (errorString.Length != 0)
            return errorString;
        else
            return null;
    }

    public override Task<string?> Reset()
    {
        return Task.FromResult<string?>(null);
    }

    public static void UpdateMbidInFile(FileInfo file)
    {
        var tfile = TagLib.File.Create(file.FullName);

        if (tfile.Tag is TagLib.Asf.Tag asfTag)
        {
            var albumArtistIdDesc = asfTag.ExtendedContentDescriptionObject
                .FirstOrDefault(cd => cd.Name == ZUNE_ALBUM_ARTIST_MEDIA_ID_NAME);
            if (albumArtistIdDesc == null && asfTag.MusicBrainzReleaseArtistId != null)
            {
                asfTag.ExtendedContentDescriptionObject.AddDescriptor(new TagLib.Asf.ContentDescriptor(
                    ZUNE_ALBUM_ARTIST_MEDIA_ID_NAME, asfTag.MusicBrainzReleaseArtistId));
            }

            var albumIdDesc = asfTag.ExtendedContentDescriptionObject
                .FirstOrDefault(cd => cd.Name == ZUNE_ALBUM_MEDIA_ID_NAME);
            if (albumIdDesc == null && asfTag.MusicBrainzReleaseId != null)
            {
                asfTag.ExtendedContentDescriptionObject.AddDescriptor(new TagLib.Asf.ContentDescriptor(
                    ZUNE_ALBUM_MEDIA_ID_NAME, asfTag.MusicBrainzReleaseId));
            }

            var trackIdDesc = asfTag.ExtendedContentDescriptionObject
                .FirstOrDefault(cd => cd.Name == ZUNE_MEDIA_ID_NAME);
            if (trackIdDesc == null && asfTag.MusicBrainzTrackId != null)
            {
                asfTag.ExtendedContentDescriptionObject.AddDescriptor(new TagLib.Asf.ContentDescriptor(
                    ZUNE_MEDIA_ID_NAME, asfTag.MusicBrainzTrackId));
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
                    foreach (var oldAlbumArtistFrame in frames.Where(f => f.Owner == ZUNE_ALBUM_ARTIST_MEDIA_ID_NAME).ToList())
                        id3v2.RemoveFrame(oldAlbumArtistFrame);

                    TagLib.Id3v2.PrivateFrame albumArtistFrame = new(ZUNE_ALBUM_ARTIST_MEDIA_ID_NAME)
                    {
                        PrivateData = new(EncodeMbidTag(tag.MusicBrainzReleaseArtistId))
                    };
                    id3v2.AddFrame(albumArtistFrame);
                }

                if (tag.MusicBrainzReleaseId != null)
                {
                    foreach (var oldAlbumFrame in frames.Where(f => f.Owner == ZUNE_ALBUM_MEDIA_ID_NAME).ToList())
                        id3v2.RemoveFrame(oldAlbumFrame);

                    TagLib.Id3v2.PrivateFrame albumFrame = new(ZUNE_ALBUM_MEDIA_ID_NAME)
                    {
                        PrivateData = new(EncodeMbidTag(tag.MusicBrainzReleaseId))
                    };
                    id3v2.AddFrame(albumFrame);
                }

                if (tag.MusicBrainzTrackId != null)
                {
                    foreach (var oldTrackFrame in frames.Where(f => f.Owner == ZUNE_MEDIA_ID_NAME).ToList())
                        id3v2.RemoveFrame(oldTrackFrame);

                    TagLib.Id3v2.PrivateFrame trackFrame = new(ZUNE_MEDIA_ID_NAME)
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
}
