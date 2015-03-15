using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PieCharts
{
	/// <summary>
	/// Interaction logic for MultiPieChart.xaml
	/// </summary>
	[TemplatePart( Name = "PART_PieChart", Type = typeof( MultiPieChart ) )]
	public partial class MultiPieChart
	{
		private const string DefaultDataBrush = "#939496";

		private Image _pieChartImage;

		#region dependency properties

		public static readonly DependencyProperty SizeProperty =
		  DependencyProperty.Register( "Size", typeof( double ), typeof( MultiPieChart ), new PropertyMetadata( 100.0, OnPiePropertyChanged ) );

		public static readonly DependencyProperty InnerPieSliceFillProperty =
		   DependencyProperty.Register( "InnerPieSliceFill", typeof( Brush ), typeof( MultiPieChart ), new PropertyMetadata( CreateBrush( "#939496" ), OnPiePropertyChanged ) );

		public static readonly DependencyProperty OuterPieSliceFillProperty =
		   DependencyProperty.Register( "OuterPieSliceFill", typeof( Brush ), typeof( MultiPieChart ), new PropertyMetadata( CreateBrush( "#D0D1D3" ), OnPiePropertyChanged ) );

		public static readonly DependencyProperty DataListProperty =
		   DependencyProperty.Register( "DataList", typeof( IList<double> ), typeof( MultiPieChart ), new PropertyMetadata( null, OnDataListPropertyChanged ) );

		public static readonly DependencyProperty DataBrushesProperty =
		   DependencyProperty.Register( "DataBrushes", typeof( MultiPieChartBrushCollection ), typeof( MultiPieChart ), new PropertyMetadata( new MultiPieChartBrushCollection(), OnPiePropertyChanged ) );

		public double Size
		{
			get { return (double)GetValue( SizeProperty ); }
			set { SetValue( SizeProperty, value ); }
		}

		public Brush InnerPieSliceFill
		{
			get { return (Brush)GetValue( InnerPieSliceFillProperty ); }
			set { SetValue( InnerPieSliceFillProperty, value ); }
		}

		public Brush OuterPieSliceFill
		{
			get { return (Brush)GetValue( OuterPieSliceFillProperty ); }
			set { SetValue( OuterPieSliceFillProperty, value ); }
		}

		public IList<double> DataList
		{
			get { return (IList<double>)GetValue( DataListProperty ); }
			set { SetValue( DataListProperty, value ); }
		}

		public MultiPieChartBrushCollection DataBrushes
		{
			get { return (MultiPieChartBrushCollection)GetValue( DataBrushesProperty ); }
			set { SetValue( DataBrushesProperty, value ); }
		}

		#endregion

		#region constructor

		public MultiPieChart()
		{
			InitializeComponent();
		}

		#endregion

		#region overrides

		public override void OnApplyTemplate()
		{
			_pieChartImage = (Image)Template.FindName( "PART_PieChart", this );

			var collection = DataList as INotifyCollectionChanged;
			if ( collection != null )
			{
				collection.CollectionChanged += DataListCollectionChanged;
			}

			CreatePieChart();
		}

		#endregion

		#region events

		private static void OnPiePropertyChanged( DependencyObject dep, DependencyPropertyChangedEventArgs ev )
		{
			var chart = (MultiPieChart)dep;

			if ( chart.IsInitialized )
			{
				chart.CreatePieChart();
			}
		}

		private static void OnDataListPropertyChanged( DependencyObject dep, DependencyPropertyChangedEventArgs ev )
		{
			var chart = (MultiPieChart)dep;

			var collection = ev.OldValue as INotifyCollectionChanged;
			if ( collection != null )
			{
				collection.CollectionChanged -= chart.DataListCollectionChanged;
			}

			collection = ev.NewValue as INotifyCollectionChanged;
			if ( collection != null )
			{
				collection.CollectionChanged += chart.DataListCollectionChanged;
			}
		}


		private void DataListCollectionChanged( object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs )
		{
			CreatePieChart();
		}

		#endregion

		#region private methods

		private void CreatePieChart()
		{
			if ( _pieChartImage != null )
			{
				if ( !double.IsNaN( Size ) && DataList != null && DataList.Any() )
				{
					_pieChartImage.Width = _pieChartImage.Height = Width = Height = Size;

					var di = new DrawingImage();
					_pieChartImage.Source = di;

					var dg = new DrawingGroup();
					di.Drawing = dg;

					if ( DataList.Count > 1 )
					{
						var total = DataList.Sum();
						var startPoint = new Point( Width / 2, 0 );
						double radians = 0;

						for ( int i = 0; i < DataList.Count; i++ )
						{
							var data = DataList[i];
							var dataBrush = GetBrushFromList( i );
							var percentage = data / total;

							Point endPoint;
							var angle = 360 * percentage;

							if ( i + 1 == DataList.Count )
							{
								endPoint = new Point( Width / 2, 0 );
							}
							else
							{
								radians += ( Math.PI / 180 ) * angle;
								var endPointX = Math.Sin( radians ) * Height / 2 + Height / 2;
								var endPointY = Width / 2 - Math.Cos( radians ) * Width / 2;
								endPoint = new Point( endPointX, endPointY );
							}

							dg.Children.Add( CreatePathGeometry( dataBrush, startPoint, endPoint, angle > 180 ) );

							startPoint = endPoint;
						}
					}
					else
					{
						dg.Children.Add( CreateEllipseGeometry( GetBrushFromList( 0 ) ) );
					}
				}
				else
				{
					_pieChartImage.Source = null;
				}
			}
		}

		private GeometryDrawing CreatePathGeometry( Brush brush, Point startPoint, Point arcPoint, bool isLargeArc )
		{
			/*
			 * <GeometryDrawing Brush="@Brush">
				  <GeometryDrawing.Geometry>
					 <PathGeometry>
						<PathFigure StartPoint="@Size/2">
						   <PathFigure.Segments>
							  <LineSegment Point="@startPoint"/>
							  <ArcSegment Point="@arcPoint"  SweepDirection="Clockwise" Size="@Size/2"/>
							  <LineSegment Point="@Size/2"/>
						   </PathFigure.Segments>
						</PathFigure>
					 </PathGeometry>
				  </GeometryDrawing.Geometry>
			   </GeometryDrawing>
			 * */

			var midPoint = new Point( Width / 2, Height / 2 );

			var drawing = new GeometryDrawing { Brush = brush };
			var pathGeometry = new PathGeometry();
			var pathFigure = new PathFigure { StartPoint = midPoint };

			var ls1 = new LineSegment( startPoint, false );
			var arc = new ArcSegment
			{
				SweepDirection = SweepDirection.Clockwise,
				Size = new Size( Width / 2, Height / 2 ),
				Point = arcPoint,
				IsLargeArc = isLargeArc
			};
			var ls2 = new LineSegment( midPoint, false );

			drawing.Geometry = pathGeometry;
			pathGeometry.Figures.Add( pathFigure );

			pathFigure.Segments.Add( ls1 );
			pathFigure.Segments.Add( arc );
			pathFigure.Segments.Add( ls2 );

			return drawing;
		}

		private GeometryDrawing CreateEllipseGeometry( Brush brush )
		{
			var midPoint = new Point( Width / 2, Height / 2 );

			var drawing = new GeometryDrawing { Brush = brush };
			var ellipse = new EllipseGeometry( midPoint, Size / 2, Size / 2 );

			drawing.Geometry = ellipse;

			return drawing;
		}

		private static SolidColorBrush CreateBrush( string brush )
		{
			var color = ColorConverter.ConvertFromString( brush );
			if ( color != null )
			{
				return new SolidColorBrush( (Color)color );
			}

			return null;
		}

		private Brush GetBrushFromList( int index )
		{
			if ( DataBrushes == null || !DataBrushes.Any() )
			{
				return CreateBrush( DefaultDataBrush );
			}
			else
			{
				var modIndex = index % DataBrushes.Count;
				return DataBrushes[modIndex];
			}
		}

		#endregion
	}
}
