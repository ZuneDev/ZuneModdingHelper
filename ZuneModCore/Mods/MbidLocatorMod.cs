using ATL;
using MediaFoundation;
using MediaFoundation.Misc;
using OwlCore.AbstractUI.Models;
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
        MF.Startup().ThrowExceptionOnError();

        var sourceResolver = MF.CreateSourceResolver();

        sourceResolver.CreateObjectFromURL(file.FullName,
            MFResolution.MediaSource | MFResolution.Read | MFResolution.ContentDoesNotHaveToMatchExtensionOrMimeType,
            null, out IMFMediaSource mediaSource)
            .ThrowExceptionOnError();

        mediaSource.CreatePresentationDescriptor(out var presentationDescriptor)
            .ThrowExceptionOnError();

        var hr = MF.GetService<IMFMetadataProvider>(mediaSource, MFServices.MF_METADATA_PROVIDER_SERVICE, out var metadataProvider);
        if (hr.Succeeded())
        {
            metadataProvider.GetMFMetadata(presentationDescriptor, 0, 0, out var metadata)
                .ThrowExceptionOnError();

            metadata.GetAllPropertyNames(out var propNames)
                .ThrowExceptionOnError();

            foreach (var propName in propNames)
            {
                PropVariant value = new();
                metadata.GetProperty(propName, value).ThrowExceptionOnError();
                System.Diagnostics.Debug.WriteLine($"{propName} = ({value.GetVariantType()}){value}");
            }
        }
        else
        {
            MF.GetService<IPropertyStore>(mediaSource, MFServices.MF_PROPERTY_HANDLER_SERVICE, out var propertyStore)
                .ThrowExceptionOnError();

            propertyStore.GetCount(out var propCount)
                .ThrowExceptionOnError();

            List<(Guid setId, int propId, PropVariant value)> props = new(propCount);

            for (int i = 0; i < propCount; i++)
            {
                PropertyKey key = new();
                PropVariant value = new();

                propertyStore.GetAt(i, key);
                propertyStore.GetValue(key, value);

                props.Add((key.fmtid, key.pID, value));
            }

            var organizedProps = props
                .OrderBy(prop => prop.setId)
                .ThenBy(prop => prop.propId);
            foreach (var prop in organizedProps)
                System.Diagnostics.Debug.WriteLine($"{prop.setId} {prop.propId} = ({prop.value.GetVariantType()}){prop.value}");
        }

        return;
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
