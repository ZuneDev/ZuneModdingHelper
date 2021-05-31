using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ZuneModCore.Mods
{
    public class MbidLocatorMod : Mod
    {
        public const string ZUNE_ALBUM_ARTIST_MEDIA_ID_NAME = "ZuneAlbumArtistMediaID";
        public const string ZUNE_ALBUM_MEDIA_ID_NAME = "ZuneAlbumMediaID";
        public const string ZUNE_MEDIA_ID_NAME = "ZuneMediaID";
        public const string ZUNE_COLLECTION_ID_NAME = "ZuneCollectionID";

        public override string Id => nameof(FeaturesOverrideMod);

        public override string Title => "MusicBrainz ID Locator";

        public override string Description => "Puts MusicBrainz IDs added by MusicBrainz Picard where the Zune " +
            "software can use it to show additional information provided by Community Webservices.\r\n" +
            "Note that this will only have an effect if you have used MusicBrainz Picard on your music library.";

        public override string Author => "Joshua \"Yoshi\" Askharoun";

        public override AbstractUIElementGroup OptionsUI => new(nameof(MbidLocatorMod))
        {
            Title = "Select music folder:",
            Items =
            {
                new AbstractTextBox("folderBox"),
                new AbstractBooleanUIElement("recursiveBox", "Search recursively")
            }
        };

        public override IReadOnlyList<Type>? DependentMods => null;

        public override async Task<string?> Apply()
        {
            // TODO: Use user choices from AbstractUI
            string folderPath = ((AbstractTextBox)OptionsUI.Items[0]).Value;
            bool recursive = ((AbstractBooleanUIElement)OptionsUI.Items[1]).State;
            string errorString = string.Empty;

            // If the user didn't enter a folder, default to the "My Music" user folder
            if (string.IsNullOrEmpty(folderPath))
                folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

            // Verify that the folder exists
            DirectoryInfo folder = new(folderPath);
            if (!folder.Exists)
            {
                return $"The folder '{folder.FullName}' does not exist.";
            }

            foreach (FileInfo file in folder.EnumerateFiles("*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                try
                {
                    UpdateMbidInFile(file);
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

                    var albumArtistFrame = frames.FirstOrDefault(f => f.Owner == ZUNE_ALBUM_ARTIST_MEDIA_ID_NAME);
                    if (albumArtistFrame == null && tag.MusicBrainzReleaseArtistId != null)
                    {
                        albumArtistFrame = new TagLib.Id3v2.PrivateFrame(ZUNE_ALBUM_ARTIST_MEDIA_ID_NAME);
                        albumArtistFrame.PrivateData = new TagLib.ByteVector(
                            new Guid(tag.MusicBrainzReleaseArtistId).ToByteArray());
                        id3v2.AddFrame(albumArtistFrame);
                    }

                    var albumFrame = frames.FirstOrDefault(f => f.Owner == ZUNE_ALBUM_MEDIA_ID_NAME);
                    if (albumFrame == null && tag.MusicBrainzReleaseId != null)
                    {
                        albumFrame = new TagLib.Id3v2.PrivateFrame(ZUNE_ALBUM_MEDIA_ID_NAME);
                        albumFrame.PrivateData = new TagLib.ByteVector(
                            new Guid(tag.MusicBrainzReleaseId).ToByteArray());
                        id3v2.AddFrame(albumFrame);
                    }

                    var trackFrame = frames.FirstOrDefault(f => f.Owner == ZUNE_MEDIA_ID_NAME);
                    if (trackFrame == null && tag.MusicBrainzTrackId != null)
                    {
                        trackFrame = new TagLib.Id3v2.PrivateFrame(ZUNE_MEDIA_ID_NAME);
                        trackFrame.PrivateData = new TagLib.ByteVector(
                            new Guid(tag.MusicBrainzTrackId).ToByteArray());
                        id3v2.AddFrame(trackFrame);
                    }
                }
            }

            tfile.Save();
        }
    }
}
