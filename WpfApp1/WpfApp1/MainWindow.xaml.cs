using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static RestClient client = new RestClient("http://localhost:3000");
        public static string userName = "";

        public MainWindow()
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            checkConnect();
            Title = "Авторизация";
        }

        private void ButtonClickAuthorization(object sender, RoutedEventArgs e)
        {
            string login = tbLogin.Text;
            string password = pbPassword.Password;
            if (login.Trim() == "" || password.Trim() == "")
            {
                MessageBox.Show("Заполните все поля.", "Авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            object data = new
            {
                login=login,
                password=password
            };
            RestResponse response = Post_Request("/auth", data);
            if (response.IsSuccessful)
            {
                dynamic resData = JsonConvert.DeserializeObject(response.Content);
                userName = (string)resData["name"];
                tbLogin.Text = "";
                pbPassword.Password = "";
                if (resData["roles"] == 1)
                {
                    masterWindow mw = new masterWindow();
                    Hide();
                    mw.Show();
                } 
                else if (resData["roles"] == 2)
                {
                    WindowMechanic wm = new WindowMechanic();
                    Hide();
                    wm.Show();
                }
            } 
            else if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                dynamic resData = JsonConvert.DeserializeObject(response.Content);
                MessageBox.Show((string)resData.message, "Авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
            } 
            else if (response.StatusCode == 0)
            {
                MessageBox.Show("Отсуствует подключение к серверу", "Авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static RestResponse Get_Request(string resource)
        {
            var request = new RestRequest(resource, Method.Get);
            return client.Execute(request);
        }
        public static RestResponse Post_Request(string resource, object data)
        {
            var request = new RestRequest(resource, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddBody(data);
            return client.Execute(request);
        }

        public void onInputPassword(object sender, RoutedEventArgs e)
        {
            waterMarkPassword.Visibility = (pbPassword.Password.Length > 0) ? Visibility.Collapsed : Visibility.Visible;
        }
        public static void checkConnect()
        {
            RestResponse response = Get_Request("/");
            if (!response.IsSuccessful)
            {
                MessageBox.Show("Отсуствует подключение к серверу", "Авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
