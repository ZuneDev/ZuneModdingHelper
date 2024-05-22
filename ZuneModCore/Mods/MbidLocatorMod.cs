using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ZuneModCore.Mods;

public class MbidLocatorMod : Mod
{
    public const string ZUNE_ALBUM_ARTIST_MEDIA_ID_NAME = "ZuneAlbumArtistMediaID";
    public const string ZUNE_ALBUM_MEDIA_ID_NAME = "ZuneAlbumMediaID";
    public const string ZUNE_MEDIA_ID_NAME = "ZuneMediaID";
    public const string ZUNE_COLLECTION_ID_NAME = "ZuneCollectionID";

    private static readonly string[] KNOWN_EXTS = [".mp3", ".mp4", ".m4a", ".wav"];

    private const string Description = "Puts MusicBrainz IDs added by MusicBrainz Picard where the Zune " +
        "software can use it to show additional information provided by Community Webservices.\r\n" +
        "Note that this will only have an effect if you have used MusicBrainz Picard on your music library.";

    private const string Author = "Joshua \"Yoshi\" Askharoun";

    public override ModMetadata Metadata => new(nameof(MbidLocatorMod), "MusicBrainz ID Locator",
        Description, Author, new(1, 0));

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
                        PrivateData = new(new Guid(tag.MusicBrainzReleaseArtistId).ToByteArray())
                    };
                    id3v2.AddFrame(albumArtistFrame);
                }

                if (tag.MusicBrainzReleaseId != null)
                {
                    foreach (var oldAlbumFrame in frames.Where(f => f.Owner == ZUNE_ALBUM_MEDIA_ID_NAME).ToList())
                        id3v2.RemoveFrame(oldAlbumFrame);

                    TagLib.Id3v2.PrivateFrame albumFrame = new(ZUNE_ALBUM_MEDIA_ID_NAME)
                    {
                        PrivateData = new(new Guid(tag.MusicBrainzReleaseId).ToByteArray())
                    };
                    id3v2.AddFrame(albumFrame);
                }

                if (tag.MusicBrainzTrackId != null)
                {
                    foreach (var oldTrackFrame in frames.Where(f => f.Owner == ZUNE_MEDIA_ID_NAME).ToList())
                        id3v2.RemoveFrame(oldTrackFrame);

                    TagLib.Id3v2.PrivateFrame trackFrame = new(ZUNE_MEDIA_ID_NAME)
                    {
                        PrivateData = new(new Guid(tag.MusicBrainzTrackId).ToByteArray())
                    };
                    id3v2.AddFrame(trackFrame);
                }
            }
        }

        tfile.Save();
    }
}
