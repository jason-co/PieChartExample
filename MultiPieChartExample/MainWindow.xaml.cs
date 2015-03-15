using System.Collections.ObjectModel;

namespace MultiPieChartExample
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded( object sender, System.Windows.RoutedEventArgs e )
		{
			chart1.DataList = new ObservableCollection<double>();
			chart1.DataList.Add(25);
			chart1.DataList.Add(100);

			chart2.DataList = new ObservableCollection<double>();
			chart2.DataList.Add(25);
			chart2.DataList.Add(100);
			chart2.DataList.Add(100);

			chart3.DataList = new ObservableCollection<double>();
			chart3.DataList.Add(0.5);
			chart3.DataList.Add(0.2);
			chart3.DataList.Add(0.1);

			chart4.DataList = new ObservableCollection<double>();
			chart4.DataList.Add(10000);
			chart4.DataList.Add(20000);
			chart4.DataList.Add(15000);
			chart4.DataList.Add(30000);

			chart5.DataList = new ObservableCollection<double>();
			chart5.DataList.Add(10);
			chart5.DataList.Add(5);
			chart5.DataList.Add(20);
			chart5.DataList.Add(30);
			chart5.DataList.Add(15);

			chart6.DataList = new ObservableCollection<double>();
			chart6.DataList.Add(40);
			chart6.DataList.Add(40);
			chart6.DataList.Add(60);
			chart6.DataList.Add(70);
			chart6.DataList.Add(100);
		}
	}
}
