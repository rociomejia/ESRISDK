using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System.IO;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext =Mymap;


            AuthenticationManager.Current.ChallengeHandler =
                new Esri.ArcGISRuntime.Toolkit.  .UI(Dispatcher);
            
        //    FeatureLayer schoolLayer = new FeatureLayer(new Uri("https://services3.arcgis.com/nvI0fYFeERFykmSz/arcgis/rest/services/Active_Schools/FeatureServer/0"));
          //  Mymap.OperationalLayers.Add(schoolLayer);
            //mapView.Map = Mymap;                     

        }
        public Map Mymap { get; } = CreateMap();

        private static Map CreateMap()
        {
            if (File.Exists("map.json")) 
            {
                var savedMap = Map.FromJson(File.ReadAllText("map.json"));
                savedMap.InitialViewpoint = new Viewpoint(51.05011, -114.08, 4000);
                return savedMap;
            }
            var map = new Map(Basemap.CreateStreetsVector())
            {
                InitialViewpoint = new Viewpoint(51.05011, -114.08, 4000)
            };
            var data = new FeatureCollection();
            data.Tables.Add(new FeatureCollectionTable(
                new Field[] { new Field(FieldType.Text, "Text", null, 50) },
                GeometryType.Point, SpatialReferences.Wgs84)
            { 
                Renderer = new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red  , 20))
            });
            map.OperationalLayers.Add(new FeatureCollectionLayer(data))  ;
            return map;
        }

     private async void mapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            var table = Mymap.OperationalLayers.OfType<FeatureCollectionLayer>().First().FeatureCollection.Tables.First();
                var feature = table.CreateFeature();
            feature.Geometry = e.Location;
            feature.Attributes["Text"] = $"{e.Location.X},{e.Location.Y}";
            try
            {
                await table.AddFeatureAsync(feature);
            }
            catch { }

        }
        protected override void OnClosing(CancelEventArgs e)
        {
            string json = Mymap.ToJson();

            File.WriteAllText("map.json",json);

            base.OnClosing(e);
        }

        private const string PortalUrl = "https://www.arcgis.com/sharing/rest/";
        
        private async void Publish_Click(object sender,RoutedEventArgs e)
        {
            if (Mymap.Item != null)
            {
                await Mymap.SaveAsync();
            }
            else
            {
                var credential = await AuthenticationManager.Current.ChallengeHandler.CreateCredentialAsync(
                    new CredentialRequestInfo()
                    {
                        AuthenticationType = AuthenticationType.Token,
                        ServiceUri = new Uri(PortalUrl)

                    });
                var portal = await ArcGISPortal.CreateAsync(new Uri(PortalUrl), credential);
                
                await Mymap.SaveAsAsync (portal, null, "Dev",  DateTime.Now.ToString(), new string[] { "tag1", "tag2" } );
            }
        }

    }
}
