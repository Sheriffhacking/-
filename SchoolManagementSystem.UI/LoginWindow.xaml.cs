using System.Windows;
using SchoolManagementSystem.BLL;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.UI
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService auth = new AuthService();

        public static User CurrentUser { get; private set; } // 👈 Session بسيط

        public LoginWindow()
        {
            InitializeComponent();
        }
        private void UpdatePassword_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text?.Trim();
            string oldPass = txtOldPassword.Password;
            string newPass = txtNewPassword.Password;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(oldPass) ||
                string.IsNullOrWhiteSpace(newPass))
            {
                MessageBox.Show("الرجاء تعبئة جميع الحقول");
                return;
            }

            try
            {
                var user = auth.Login(username, oldPass);

                if (user == null)
                {
                    MessageBox.Show("كلمة المرور القديمة غير صحيحة");
                    return;
                }

                auth.ChangePassword(user.UserId, newPass);

                MessageBox.Show("تم تحديث كلمة المرور بنجاح");

                txtOldPassword.Clear();
                txtNewPassword.Clear();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("خطأ: " + ex.Message);
            }
        }
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text?.Trim();
            string password = txtPassword.Password;

            // 1. Validation
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("الرجاء إدخال اسم المستخدم");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("الرجاء إدخال كلمة المرور");
                txtPassword.Focus();
                return;
            }

            try
            {
                // 2. Login check
                User user = auth.Login(username, password);

                if (user != null)
                {
                    CurrentUser = user; // حفظ المستخدم الحالي

                    MessageBox.Show($"مرحباً {user.Username}");

                    MainDashboard dashboard = new MainDashboard();
                    dashboard.Show();

                    this.Close();
                }
                else
                {
                    MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة");
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء تسجيل الدخول: " + ex.Message);
            }
        }
    }
}