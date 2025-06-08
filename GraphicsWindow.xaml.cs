using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Data;
using System.Linq;
using System.Windows;

namespace WpfApp1
{
    public partial class GraphicsWindow : Window
    {
        public GraphicsWindow(DataTable records)
        {
            InitializeComponent();

            // Group by date and count entrances
            var entranceCounts = records.AsEnumerable()
                .GroupBy(row => Convert.ToDateTime(row["AccessTime"]).Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            var values = new ChartValues<int>(entranceCounts.Select(x => x.Count));
            var labels = entranceCounts.Select(x => x.Date.ToString("yyyy-MM-dd")).ToArray();

            myChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Entrances",
                    Values = values
                }
            };

            myChart.AxisX.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Date",
                Labels = labels
            });

            myChart.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Count"
            });
        }
    }
}
