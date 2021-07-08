using AutoMapper;
using MusCat.Core.Entities;
using MusCat.Infrastructure.Services;
using MusCat.ViewModels.Entities;
using Prism.Services.Dialogs;
using System.IO;
using System.Linq;
using System.Windows;

namespace MusCat.Util
{
    class MoveAlbumService
    {
        private readonly IDialogService _dialogService;

        public AlbumViewModel AlbumToMove { get; private set; }
        public PerformerViewModel InitialPerformer { get; private set; }

        public MoveAlbumService(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public void BeginMoveAlbum(AlbumViewModel album, PerformerViewModel performer)
        {
            if (album is null)
            {
                return;
            }

            AlbumToMove = album;
            InitialPerformer = performer;

            MessageBox.Show("Now select the performer and click the 'Move album here' button");
        }

        public void MoveAlbum(PerformerViewModel performerNew)
        {
            InitialPerformer.Albums.Remove(AlbumToMove);

            AlbumToMove.Performer = Mapper.Map<Performer>(performerNew);
            AlbumToMove.LocateImagePath();
        }

        public void MoveAlbumFiles(PerformerViewModel performerNew)
        {
            var album = Mapper.Map<Album>(AlbumToMove);
            var performer = Mapper.Map<Performer>(performerNew);

            var initialAlbum = Mapper.Map<Album>(AlbumToMove);
            initialAlbum.Performer = Mapper.Map<Performer>(InitialPerformer);

            var pathlist = FileLocator.MakePerformerImagePathlist(performer);
            var path = string.Empty;

            if (pathlist.Count == 1)
            {
                path = Path.GetDirectoryName(pathlist.First());
            }
            else
            {
                var parameters = new DialogParameters
                {
                    { "options", pathlist.Select(p => Path.GetDirectoryName(p)) }
                };

                _dialogService.ShowDialog("ChoiceWindow", parameters, r =>
                {
                    path = r.Parameters.GetValue<string>("choice");
                });

                Directory.CreateDirectory(path);
            }

            // move album cover image file

            var albumPath = FileLocator.GetAlbumImagePath(initialAlbum);
            if (albumPath != string.Empty)
            {
                File.Move(albumPath, $"{path}\\{Path.GetFileName(albumPath)}");

                AlbumToMove.LocateImagePath();
            }

            // move folder with song files

            var albumFolder = FileLocator.FindAlbumPath(initialAlbum);
            if (albumFolder != string.Empty)
            {
                var destinationFolder = $"{Path.GetDirectoryName(path)}\\{new DirectoryInfo(albumFolder).Name}";

                if (Path.GetPathRoot(albumFolder) == Path.GetPathRoot(path))
                {
                    Directory.Move(albumFolder, destinationFolder);
                }

                // !!!!! .NET, are you kidding me? ((( 

                else
                {
                    Directory.CreateDirectory(destinationFolder);

                    foreach (string dir in Directory.GetDirectories(albumFolder, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(Path.Combine(destinationFolder, dir.Substring(albumFolder.Length + 1)));
                    }

                    foreach (string file in Directory.GetFiles(albumFolder, "*", SearchOption.AllDirectories))
                    {
                        File.Copy(file, Path.Combine(destinationFolder, file.Substring(albumFolder.Length + 1)));
                    }

                    Directory.Delete(albumFolder, true);
                }
            }
        }

        public void EndMoveAlbum()
        {
            AlbumToMove = null;
            InitialPerformer = null;
        }
    }
}
