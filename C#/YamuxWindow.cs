using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Newtonsoft.Json.Linq;
using Pango;
using Application = Gtk.Application;
using Task = System.Threading.Tasks.Task;
using Thread = System.Threading.Thread;
using UI = Gtk.Builder.ObjectAttribute;
using Window = Gtk.Window;

namespace Yamux
{
    class YamuxWindow : Window
    {
        [UI] private Dialog DonateWindow = null;
        [UI] private Window AboutWindow = null;

        [UI] private Button AboutGitHubProject = null;
        [UI] private Button AboutGitHubAuthor = null;
        [UI] private Button AboutTelegramChannel = null;
        [UI] private Button AboutDonateMe = null;
        [UI] private Button KofiDonate = null;
        [UI] private Button CloseAboutWindow = null;
        [UI] private Button CloseDonateWindow = null;

        [UI] private Button AboutProgram = null;
        [UI] private SearchEntry SearchMusic = null;
        [UI] private Box ResultBox = null;
        [UI] private Label IfNoResult = null;
        private VBox _bestBox = new VBox();

        public YamuxWindow() : this(new Builder("Yamux.glade"))
        {
        }

        private YamuxWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            DeleteEvent += Window_DeleteEvent;
            AboutProgram.Clicked += ShowAboutWindow;
            AboutDonateMe.Clicked += ShowDonateWindow;
            SearchMusic.SearchChanged += SearchChangedOutput;
            SetDefaultIconFromFile("Svg/icon.svg");
        }

        private void ShowAboutWindow(object sender, EventArgs a)
        {
            AboutWindow.ShowAll();
            AboutWindow.Deletable = false;

            CloseAboutWindow.Clicked += HideAboutWindow;
            AboutGitHubProject.Clicked += ClickAboutGitHubProject;
            AboutGitHubAuthor.Clicked += ClickAboutGitHubAuthor;
            AboutTelegramChannel.Clicked += ClickTelegramChannel;
        }

        private void ShowDonateWindow(object sender, EventArgs a)
        {
            DonateWindow.ShowAll();
            DonateWindow.Deletable = false;
            CloseDonateWindow.Clicked += HideDonateWindow;
            KofiDonate.Clicked += ClickKofiDonate;
        }
        async private void SearchChangedOutput(object sender, EventArgs a)
        {
            string text = SearchMusic.Text;
            JToken root = "{}";
            await Task.Run(() =>
            {
                Thread.Sleep(2000);
                if (text == SearchMusic.Text && !string.IsNullOrEmpty(SearchMusic.Text) && !string.IsNullOrEmpty(text))
                {
                    Console.WriteLine(text);
                    JObject resultSearch = YandexMusicApi.Default.Search(text);
                    root = resultSearch.Last.Last.Root;
                    root = root.SelectToken("result");
                }
            });
            ShowResultSearch(root, text);
        }
        async private void ShowResultSearch(JToken root, string text)
        {
            if (text == SearchMusic.Text && !string.IsNullOrEmpty(SearchMusic.Text) && !string.IsNullOrEmpty(text))
            {
                Dictionary<string, List<string>> result = Yamux.GetTrack(root);
                if (root.Count() > 6)
                {
                    _bestBox.Destroy();
                    _bestBox = new VBox();
                    ResultBox.Add(_bestBox);
                    await Task.Run(() =>
                    {
                        IfNoResult.Text = "";
                        string typeBest = root["best"]["type"].ToString();

                        switch (typeBest)
                        {
                            case "artist":
                                typeBest = "Артист";
                                break;
                            case "track":
                                typeBest = "Трек";
                                break;
                            case "playlist":
                                typeBest = "Плейлист";
                                break;
                            case "podcast":
                                typeBest = "Выпуски подкастов";
                                break;
                            case "album":
                                typeBest = "Альбом";
                                break;
                        }

                        Dictionary<string, List<string>> artist = Yamux.GetArtist(root);
                        Dictionary<string, List<string>> track = Yamux.GetTrack(root);
                        Dictionary<string, List<string>> podcast = Yamux.GetPodcast(root);
                        Dictionary<string, List<string>> playlist = Yamux.GetPlaylist(root);
                        List<string> artistName = artist["name"];
                        List<string> artistCoverUri = artist["coverUri"];
                        List<string> trackName = track["name"];
                        List<string> trackCoverUri = track["coverUri"];
                        List<string> podcastName = podcast["name"];
                        List<string> podcastCoverUri = podcast["coverUri"];
                        List<string> playlistName = playlist["name"];
                        List<string> playlistCoverUri = playlist["coverUri"];

                        HBox artistBox = Yamux.CreateBoxResultSearch(artistName, artistCoverUri);
                        HBox trackBox = Yamux.CreateBoxResultSearch(trackName, trackCoverUri);
                        HBox podcastBox = Yamux.CreateBoxResultSearch(podcastName, podcastCoverUri);
                        HBox playlistBox = Yamux.CreateBoxResultSearch(playlistName, playlistCoverUri);

                        ScrolledWindow scrolledArtist = new ScrolledWindow();
                        ScrolledWindow scrolledTrack = new ScrolledWindow();
                        ScrolledWindow scrolledPodcast = new ScrolledWindow();
                        ScrolledWindow scrolledPlaylist = new ScrolledWindow();
                        scrolledArtist.PropagateNaturalHeight = true;
                        scrolledArtist.PropagateNaturalWidth = true;
                        scrolledTrack.PropagateNaturalHeight = true;
                        scrolledTrack.PropagateNaturalWidth = true;
                        scrolledPodcast.PropagateNaturalHeight = true;
                        scrolledPodcast.PropagateNaturalWidth = true;
                        scrolledPlaylist.PropagateNaturalHeight = true;
                        scrolledPlaylist.PropagateNaturalWidth = true;

                        Viewport viewportArtist = new Viewport();
                        Viewport viewportTrack = new Viewport();
                        Viewport viewportPodcast = new Viewport();
                        Viewport viewportPlaylist = new Viewport();

                        Label artistLabel = new Label(typeBest);
                        FontDescription tpfartist = new FontDescription();
                        tpfartist.Size = 12288;
                        artistLabel.ModifyFont(tpfartist);

                        Label trackLabel = new Label("Треки");
                        FontDescription tpftrack = new FontDescription();
                        tpftrack.Size = 12288;
                        trackLabel.ModifyFont(tpftrack);

                        Label podcastLabel = new Label("Выпуски подкастов");
                        FontDescription tpfpodcast = new FontDescription();
                        tpfpodcast.Size = 12288;
                        podcastLabel.ModifyFont(tpfpodcast);

                        Label playlistLabel = new Label("Плейлисты");
                        FontDescription tpfplaylist = new FontDescription();
                        tpfplaylist.Size = 12288;
                        playlistLabel.ModifyFont(tpfplaylist);

                        scrolledArtist.Add(viewportArtist);
                        viewportArtist.Add(artistBox);
                        scrolledTrack.Add(viewportTrack); 
                        viewportTrack.Add(trackBox);
                        scrolledPodcast.Add(viewportPodcast);
                        viewportPodcast.Add(podcastBox);
                        scrolledPlaylist.Add(viewportPlaylist);
                        viewportPlaylist.Add(playlistBox);

                        _bestBox.Add(artistLabel);
                        _bestBox.Add(scrolledArtist);
                        _bestBox.Add(trackLabel);
                        _bestBox.Add(scrolledTrack);
                        _bestBox.Add(podcastLabel);
                        _bestBox.Add(scrolledPodcast);
                        _bestBox.Add(playlistLabel);
                        _bestBox.Add(scrolledPlaylist);
                    });
                    ResultBox.ShowAll();
                    _bestBox.ShowAll();
                }
                else
                {
                    _bestBox.Destroy();
                    IfNoResult.Text = "Нет результата😢";
                }
            }
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
        private void HideAboutWindow(object sender, EventArgs a)
        {
            AboutWindow.Hide();
        }
        private void HideDonateWindow(object sender, EventArgs a)
        {
            DonateWindow.Hide();
        }
        private void ClickAboutGitHubProject(object sender, EventArgs a)
        {
            Yamux.OpenLinkToWebBrowser("https://github.com/KirMozor/Yamux");
        }
        private void ClickAboutGitHubAuthor(object sender, EventArgs a)
        {
            Yamux.OpenLinkToWebBrowser("https://github.com/KirMozor");
        }
        private void ClickTelegramChannel(object sender, EventArgs a)
        {
            Yamux.OpenLinkToWebBrowser("https://t.me/kirmozor");
        }

        private void ClickKofiDonate(object sender, EventArgs a)
        {
            Yamux.OpenLinkToWebBrowser("https://ko-fi.com/kirmozor");
        }
    }
}