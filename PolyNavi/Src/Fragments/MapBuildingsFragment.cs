﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.Design.Widget;

using Mapsui.Geometries;
using Mapsui.UI;
using Mapsui.UI.Android;
using Mapsui.Utilities;
using Mapsui.Projection;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui;
using BruTile.Predefined;

using Itinero;
using Itinero.Profiles;

namespace PolyNavi
{
	// TODO Кеширование RouterDB чтобы не загружать ее при каждой загрузке фрагмента
	public class MapBuildingsFragment : Fragment
	{
		private const string RouterDbName = "polytech_map.routerdb";
		private const string Marker_A_Name = "ic_marker_a.png";
		private const string Marker_B_Name = "ic_marker_b.png";

		private View view;

		RouterDb routerDb;
		Router router;
		Profile profile;

		private MapControl mapControl;
		private Map map;
		private ILayer routeLayer;

		private FloatingActionButton fab;
		
		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			view = inflater.Inflate(Resource.Layout.fragment_map_buildings, container, false);

			InitializeRouting();
			InitializeMapControl();

			fab = view.FindViewById<FloatingActionButton>(Resource.Id.new_fab_buildings);
			fab.Click += Fab_Click;

			return view;
		}

		private void InitializeRouting()
		{
			using (var ms = new MemoryStream())
			using (var stream = Activity.BaseContext.Assets.Open(RouterDbName))
			{
				stream.CopyTo(ms);
				ms.Seek(0, SeekOrigin.Begin);
				routerDb = RouterDb.Deserialize(ms);
			}
			router = new Router(routerDb);
			profile = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Shortest();
		}

		private void InitializeMapControl()
		{
			mapControl = view.FindViewById<MapControl>(Resource.Id.mapControl);
			mapControl.RotationLock = false;
			map = mapControl.Map;
			map.CRS = "EPSG:3857";
			//map.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.EsriWorldTopo)));\
			map.Layers.Add(OpenStreetMap.CreateTileLayer());
			routeLayer = new Layer();
			map.Layers.Add(routeLayer);

			Point centerOfPolytech = new Point(30.371144, 60.003675).FromLonLat();
			map.NavigateTo(centerOfPolytech);
			map.NavigateTo(7);
			map.Transformation = new MinimalTransformation();

			Point leftBot = new Point(30.365751, 59.999560).FromLonLat();
			Point rightTop = new Point(30.391848, 60.008916).FromLonLat();
			map.PanLimits = new BoundingBox(leftBot, rightTop);
			map.PanMode = PanMode.KeepCenterWithinExtents;

			map.ZoomLimits = new MinMax(1, 7);

			map.Widgets.Add(new Mapsui.Widgets.ScaleBar.ScaleBarWidget(map) { TextAlignment = Mapsui.Widgets.Alignment.Center, HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Center, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top });
		}

		private ILayer DrawRoute(Route route)
		{
			string layerName = "Itinero Route";
			return new Layer(layerName)
			{
				Name = layerName,
				DataSource = new MemoryProvider(GenerateLinesAndMarkers(route)),
				Style = null,
			};
		}

		private IEnumerable<IFeature> GenerateLinesAndMarkers(Route route)
		{
			LineString line = new LineString
			{
				Vertices = (from c in route.Shape
							select SphericalMercator.FromLonLat(c.Longitude, c.Latitude)).ToList()
			};
			Feature lineFeature = new Feature()
			{
				Geometry = line,
				Styles = new List<IStyle>()
				{
					new SymbolStyle()
					{
						Line = new Pen()
						{
							Color = Color.Blue,
							PenStyle = PenStyle.Solid,
							Width = 7,
							PenStrokeCap = PenStrokeCap.Round,
						},
						Opacity = 0.7f,
					}
				},
			};
			Feature markerStartFeature = new Feature()
			{
				Geometry = route.Stops[0].Coordinate.ToWorld(),
				Styles = new List<IStyle>()
				{
					new SymbolStyle()
					{
						BitmapId = GetBitmapIdForEmbeddedResource(Marker_A_Name),
						SymbolScale = 0.5,
					}
				}
			};
			Feature markerFinishFeature = new Feature()
			{
				Geometry = route.Stops[1].Coordinate.ToWorld(),
				Styles = new List<IStyle>()
				{
					new SymbolStyle()
					{
						BitmapId = GetBitmapIdForEmbeddedResource(Marker_B_Name),
						SymbolScale = 0.5,
					}
				}
			};

			return new List<Feature> { lineFeature, markerStartFeature, markerFinishFeature, };
		}

		private int GetBitmapIdForEmbeddedResource(string resourceName)
		{
			var image = MainApp.GetEmbeddedResourceStream($"Images.{resourceName}");
			return BitmapRegistry.Instance.Register(image);
		}

		private void Fab_Click(object sender, EventArgs e)
		{
			var searchActivity = new Intent(Activity, typeof(MapRouteActivity));
			StartActivityForResult(searchActivity, MainActivity.RequestCode);
		}

		public override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (requestCode == MainActivity.RequestCode)
			{
				if (resultCode == Result.Ok)
				{
					string[] routeNames = data.GetStringArrayExtra("route");
					string startName = routeNames[0];
					string endName = routeNames[1];

					Point s = MainApp.BuildingsDictionary[startName];
					Point e = MainApp.BuildingsDictionary[endName];
					RouterPoint start = router.Resolve(profile, (float)s.X, (float)s.Y);
					RouterPoint end = router.Resolve(profile, (float)e.X, (float)e.Y);

					Route route = router.Calculate(profile, start, end);

					map.Layers.Remove(routeLayer);
					routeLayer = DrawRoute(route);
					map.Layers.Add(routeLayer);
				}
			}
		}
	}
}