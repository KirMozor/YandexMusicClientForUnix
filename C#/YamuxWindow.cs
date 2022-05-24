using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Gdk;
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
        [UI] private SearchEntry SearchMusic = null;
        [UI] private Box ResultBox = null;
        [UI] private Label IfNoResult = null;
        private VBox BestBox = new VBox();
        public YamuxWindow() : this(new Builder("Yamux.glade"))
        {
        }

        private YamuxWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            DeleteEvent += Window_DeleteEvent;
            SearchMusic.SearchChanged += SearchChangedOutput;
            SetDefaultIconFromFile("Svg/icon.svg");
        }

        async void SearchChangedOutput(object sender, EventArgs a)
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
                    //Console.WriteLine(root);
                }
            });
            ShowResultSearch(root, text);
        }

        private void ShowResultSearch(JToken root, string text)
        {
            if (text == SearchMusic.Text && !string.IsNullOrEmpty(SearchMusic.Text) && !string.IsNullOrEmpty(text))
            {
                if (root.Count() > 6)
                {
                    File.Delete("s.jpg");
                    BestBox.Destroy();
                    BestBox = new VBox();
                    ResultBox.Add(BestBox);
                    
                    IfNoResult.Text = "";
                    Dictionary<string, string> Best = GetBest(root);
                    string typeBest = Best["type"];
                    string nameBest = Best["name"];
                    
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
                            typeBest = "Подкасты";
                            break;
                        case "album":
                            typeBest = "Альбом";
                            break;
                    }

                    Label TypeBestLabel = new Label(typeBest);
                    FontDescription tpfTypeBest = new FontDescription();
                    tpfTypeBest.Size = 12288;
                    TypeBestLabel.ModifyFont(tpfTypeBest);
                    
                    BestBox.Add(TypeBestLabel);

                    ResultBox.ShowAll();
                    BestBox.ShowAll();
                }
                else
                {
                    BestBox.Destroy();
                    IfNoResult.Text = "Нет результата😢";
                }
            }
        }
        
        private Dictionary<string, string> GetBest(JToken root)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            root = root.SelectToken("best");
            Console.WriteLine(root);
            result.Add("type", root.SelectToken("type").ToString());

            try
            {
                result.Add("uriCover", root.SelectToken("result").SelectToken("cover").SelectToken("uri").ToString());
            }
            catch (NullReferenceException)
            {
                result.Add("uriCover", root.SelectToken("result").SelectToken("coverUri").ToString());
            }
            
            try
            {
                result.Add("name", root.SelectToken("result").SelectToken("name").ToString());
            }
            catch (NullReferenceException)
            {
                result.Add("name", root.SelectToken("result").SelectToken("title").ToString());
            }
            
            try
            {
                result.Add("id", root.SelectToken("result").SelectToken("id").ToString());
            }
            catch (NullReferenceException)
            {
                result.Add("uid", root.SelectToken("result").SelectToken("uid").ToString());
                result.Add("kind", root.SelectToken("result").SelectToken("kind").ToString());
            }

            return result;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}