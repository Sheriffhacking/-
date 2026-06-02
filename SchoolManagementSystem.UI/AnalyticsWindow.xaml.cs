using SchoolManagementSystem.BLL;
using System;
using System.Windows;

namespace SchoolManagementSystem.UI
{
    public partial class AnalyticsWindow : Window
    {
        private readonly AnalyticsService service = new AnalyticsService();

        public AnalyticsWindow()
        {
            InitializeComponent();
            LoadSchoolSummary();
        }

        private void TopStudents_Click(object sender, RoutedEventArgs e)
        {
            dgAnalytics.ItemsSource = service.GetTopStudents();
        }

        private void WeakStudents_Click(object sender, RoutedEventArgs e)
        {
            dgAnalytics.ItemsSource = service.GetWeakStudents();
        }

        private void ClassRanking_Click(object sender, RoutedEventArgs e)
        {
            dgAnalytics.ItemsSource = service.GetClassRanking();
        }

        private void SubjectRanking_Click(object sender, RoutedEventArgs e)
        {
            dgAnalytics.ItemsSource = service.GetSubjectRanking();
        }

        private void SchoolSummary_Click(object sender, RoutedEventArgs e)
        {
            LoadSchoolSummary();
            dgAnalytics.ItemsSource = new[] { service.GetSchoolSummary() };
        }

        private void LoadSchoolSummary()
        {
            var data = service.GetSchoolSummary();

            txtAvg.Text = data.SchoolAverage.ToString("0.##");
            txtMax.Text = data.MaxScore.ToString("0.##");
            txtMin.Text = data.MinScore == 0 ? "—" : data.MinScore.ToString("0.##");
        }
    }
}