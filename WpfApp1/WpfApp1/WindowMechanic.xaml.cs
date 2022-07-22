using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для WindowMechanic.xaml
    /// </summary>
    public partial class WindowMechanic : Window
    {
        private Appl changeAppl = null;
        private Repair currentRepair = null;
        

        public WindowMechanic()
        {
            InitializeComponent();
            Title = "Механик: " + MainWindow.userName;
            ResizeMode = ResizeMode.NoResize;
            main.Visibility = Visibility.Visible;
            InitializeElements();
        }

        private void InitializeElements()
        {
            getApplication();
            DataGridApplication.ItemsSource = lstApplication;
            cbCategory.ItemsSource = lstCategory;
            cbCategory.DisplayMemberPath = "Name";
            cbService.ItemsSource = lstServ;
            cbService.DisplayMemberPath = "Name";
            dgServiceChange.ItemsSource = lstServices;
            dgPartChange.ItemsSource = lstGivenParts;
        }

        public class Repair
        {
            public int Id { get; }
            public string Worker_name { get; }
            public string Defects { get; set; }

            public Repair(int id, string workerName, string defects)
            {
                Id = id;
                Worker_name = workerName;
                Defects = defects;
            }
        }

        public class Appl
        {
            public int Id { get; }
            public string Client_name;
            public string Client_telephone;
            public string Car_name;
            public string Car_number;
            public string Date_start;
            public string Date_end;
            public string Comment { get; }
            public string Status { get; set; }

            public string getClient { get => Client_name; }
            public string getCar { get => Car_name + " [" + Car_number + "]"; }

            public Appl(int id, string clientName, string clientTelephone, string carName, string carNumber,
                string dateStart, string dateEnd, string comment, string status)
            {
                Id = id;
                Client_name = clientName;
                Client_telephone = clientTelephone;
                Car_name = carName;
                Car_number = carNumber;
                Date_start = dateStart;
                Date_end = dateEnd;
                Comment = comment;
                Status = status;
            }
        };
        public ObservableCollection<Appl> lstApplication = new ObservableCollection<Appl>();

        public struct Service
        {
            public string Category { get; }
            public string Name { get; }
            public int Price { get; }

            public Service(string category, string name, int price)
            {
                Category = category;
                Name = name;
                Price = price;
            }
        }
        public ObservableCollection<Service> lstServices = new ObservableCollection<Service>();

        public struct Category
        {
            public int Id { get; }
            public string Name { get; }

            public Category(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }
        public ObservableCollection<Category> lstCategory = new ObservableCollection<Category>();

        public struct Serv
        {
            public int Id { get; }
            public string Name { get; }
            public int Price { get; }

            public Serv(int id, string name, int price)
            {
                Id = id;
                Name = name;
                Price = price;
            }
        }
        public ObservableCollection<Serv> lstServ = new ObservableCollection<Serv>();

        public struct GivenPart
        {
            public string Name { get; }
            public int Price { get; }
            public int Quantity { get; }

            public GivenPart(string name, int price, int quantity)
            {
                Name = name;
                Price = price;
                Quantity = quantity;
            }
        }
        public ObservableCollection<GivenPart> lstGivenParts = new ObservableCollection<GivenPart>();

        private void getApplication()
        {
            if (lstApplication.Count > 0) lstApplication.Clear();
            RestResponse response = MainWindow.Get_Request("/getApplicationForMechanic");
            if (response.IsSuccessful)
            {
                dynamic resData = JsonConvert.DeserializeObject(response.Content);
                resData = resData["result"];
                for (int i = 0; i < resData.Count; i++)
                {
                    lstApplication.Add(new Appl(
                        (int)resData[i]["Id"],
                        (string)resData[i]["Full_name"],
                        (string)resData[i]["Telephone"],
                        (string)resData[i]["Car"],
                        (string)resData[i]["Number"],
                        (string)resData[i]["Date_start"],
                        (string)resData[i]["Date_end"],
                        (string)resData[i]["Comment"],
                        (string)resData[i]["Status"]
                        ));
                }
            }
            else if (response.StatusCode == 0) errorConnect("Главное окно");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Главное окно", response.Content);
            else errorResponse("Главное окно", response.Content);
        }

        private void ButtonClickLogout(object sender, RoutedEventArgs e)
        {
            MainWindow.Get_Request("/logout");
            MainWindow.userName = "";
            Application.Current.Windows[0].Show();
            Close();
        }
        private void ButtonClickCancelChange(object sender, RoutedEventArgs e)
        {
            changeAppl = null;

            lbWorkerChange.Visibility = Visibility.Collapsed;
            btnMainChange.Visibility = Visibility.Visible;
            tbDiagnisticChange.IsReadOnly = false;
            lbServiceChange.Visibility = Visibility.Collapsed;
            dgServiceChange.Visibility = Visibility.Collapsed;
            lbPriceChange.Visibility = Visibility.Collapsed;
            lbPartChange.Visibility = Visibility.Collapsed;
            btnAddServiceChange.Visibility = Visibility.Collapsed;
            dgPartChange.Visibility = Visibility.Collapsed;
            lbPricePartChange.Visibility = Visibility.Collapsed;
            lbAllPricehange.Visibility = Visibility.Collapsed;

            FormChangeApplication.Visibility = Visibility.Collapsed;
            main.Visibility = Visibility.Visible;
        }
        private void ButtonClickCancelAddService(object sender, RoutedEventArgs e)
        {
            if (cbService.SelectedIndex != -1 || cbCategory.SelectedIndex != -1)
            {
                var result = MessageBox.Show("Некоторые поля заполнены. Вы уверены, что хотите покинуть данную форму? Данные будут утеряны.",
                    "Закрытие формы", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }
            cbService.SelectedIndex = -1;
            cbCategory.SelectedIndex = -1;

            FormAddService.Visibility = Visibility.Collapsed;
            FormChangeApplication.Visibility = Visibility.Visible;
        }

        private void ButtonClickUpdateApplication(object sender, RoutedEventArgs e)
        {
            getApplication();
        }
        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            changeAppl = (Appl)row.Item;

            lbStatusChange.Content = "Статус: " + changeAppl.Status;
            lbIdChange.Content = "Заявка на ремонт №" + changeAppl.Id;
            lbDateStartChange.Content = "Дата оформления: " + changeAppl.Date_start;
            lbDateEndChange.Content = "Дата завершения: " + changeAppl.Date_end;
            lbClientChange.Content = "Клиент: " + changeAppl.Client_name;
            lbTelephoneChange.Content = "Телефон: " + changeAppl.Client_telephone;
            lbCarChange.Content = "Автомобиль: [" + changeAppl.Car_number + "] " + changeAppl.Car_name;
            tbCommentChange.Text = changeAppl.Comment;

            if (changeAppl.Status == "Диагностика" || changeAppl.Status == "Ремонт")
            {
                RestResponse response = MainWindow.Post_Request("/getRepair", new { id = changeAppl.Id });
                if (response.IsSuccessful)
                {
                    dynamic resData = JsonConvert.DeserializeObject(response.Content);
                    resData = resData["result"];
                    currentRepair = new Repair((int)resData["Id"], (string)resData["Full_name"], (string)resData["Defects"]);
                }
                else if (response.StatusCode == 0) errorConnect("Окно заявки");
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Окно заявки", response.Content);
                else errorResponse("Окно заявки", response.Content);

                if (currentRepair.Defects.Length == 0)
                {
                    lbDiagnosticChange.Content = "Неисправности (ЗАПОЛНИТЬ!):";
                    btnMainChange.Content = "Отправить";
                }
                else
                {
                    lbDiagnosticChange.Content = "Неисправности:";
                    tbDiagnisticChange.IsReadOnly = true;
                    tbDiagnisticChange.Text = currentRepair.Defects;
                    if(changeAppl.Status == "Диагностика") btnMainChange.Visibility = Visibility.Collapsed;
                }
            }
            if (changeAppl.Status == "Ремонт")
            {
                if (lstServices.Count > 0) lstServices.Clear();
                RestResponse response = MainWindow.Post_Request("/getRepairServices", new { repairId = currentRepair.Id });
                if (response.IsSuccessful)
                {
                    dynamic resData = JsonConvert.DeserializeObject(response.Content);
                    resData = resData["result"];
                    int sum = 0;
                    foreach (var data in resData)
                    {
                        sum += (int)data["Price"];
                        lstServices.Add(new Service((string)data["Category"], (string)data["Name"], (int)data["Price"]));
                    }
                    lbPriceChange.Content = "Стоимость услуг: " + sum.ToString() + " руб.";
                }
                else if (response.StatusCode == 0) errorConnect("Окно заявки");
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Окно заявки", response.Content);
                else errorResponse("Окно заявки", response.Content);

                if (lstGivenParts.Count > 0) lstGivenParts.Clear();
                response = MainWindow.Post_Request("/getGivenPart", new { repairId = currentRepair.Id });
                if (response.IsSuccessful)
                {
                    dynamic resData = JsonConvert.DeserializeObject(response.Content);
                    resData = resData["result"];
                    int sum = 0;
                    foreach (var data in resData)
                    {
                        sum += (int)data["Price"] * (int)data["Quantity"];
                        lstGivenParts.Add(new GivenPart((string)data["Name"], (int)data["Price"], (int)data["Quantity"]));
                    }
                    lbPricePartChange.Content = "Стоимость запчастей: " + sum.ToString() + " руб.";
                }
                else if (response.StatusCode == 0) errorConnect("Окно заявки");
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Окно заявки", response.Content);
                else errorResponse("Окно заявки", response.Content);

                btnMainChange.Content = "Завершить ремонт";

                lbServiceChange.Visibility = Visibility.Visible;
                dgServiceChange.Visibility = Visibility.Visible;
                lbPriceChange.Visibility = Visibility.Visible;
                lbPartChange.Visibility = Visibility.Visible;
                btnAddServiceChange.Visibility = Visibility.Visible;
                dgPartChange.Visibility = Visibility.Visible;
                lbPricePartChange.Visibility = Visibility.Visible;
            }
            else if(changeAppl.Status != "Диагностика") btnMainChange.Visibility = Visibility.Collapsed;

            main.Visibility = Visibility.Collapsed;
            FormChangeApplication.Visibility = Visibility.Visible;
        }
        private void ButtonClickMainChange(object sender, RoutedEventArgs e)
        {
            RestResponse response;
            if (changeAppl.Status == "Диагностика")
            {
                string defects = tbDiagnisticChange.Text;
                if(defects.Trim() == "")
                {
                    MessageBox.Show("Не заполнено поле с неисправностями.", "Главное окно", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                response = MainWindow.Post_Request("/addDefects", new { repairId = currentRepair.Id, defects });
                if (response.IsSuccessful)
                {
                    currentRepair.Defects = defects;
                    tbDiagnisticChange.IsReadOnly = true;
                }
                else if (response.StatusCode == 0) { errorConnect("Окно заявки"); return; }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) { notAuthorize("Окно заявки", response.Content); return; }
                else { errorResponse("Окно заявки", response.Content); return; }

                DataGridApplication.Items.Refresh();
                btnMainChange.Visibility = Visibility.Collapsed;
            }
            else if (changeAppl.Status == "Ремонт")
            {
                changeAppl.Status = "Готово";
                btnMainChange.Visibility = Visibility.Collapsed;
                getApplication();
            }
            lbStatusChange.Content = "Статус: " + changeAppl.Status;
            response = MainWindow.Post_Request("/setStatus", new { status = changeAppl.Status, id = changeAppl.Id });
            if (response.IsSuccessful) return;
            else if (response.StatusCode == 0) errorConnect("Окно заявки");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Окно заявки", response.Content);
            else errorResponse("Окно заявки", response.Content);
        }
        private void btnAddServiceChange_Click(object sender, RoutedEventArgs e)
        {
            if (lstCategory.Count > 0) lstCategory.Clear();
            RestResponse response = MainWindow.Get_Request("/getAllCategory");
            if (response.IsSuccessful)
            {
                dynamic resData = JsonConvert.DeserializeObject(response.Content);
                resData = resData["result"];
                for (int i = 0; i < resData.Count; i++)
                {
                    lstCategory.Add(new Category((int)resData[i]["Id"], (string)resData[i]["Name"]));
                }
            }
            else if (response.StatusCode == 0) errorConnect("Добавление услуги");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Добавление услуги", response.Content);
            else errorResponse("Добавление услуги", response.Content);

            FormChangeApplication.Visibility = Visibility.Collapsed;
            FormAddService.Visibility = Visibility.Visible;
        }

        private void OnlyNumber(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && e.Text != "\b") e.Handled = true;
        }
        private void NumberCar(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && e.Text != "\b")
            {
                char inp = e.Text[0];
                if (inp < 'А' || inp > 'Я') e.Handled = true;
            }
        }

        private void errorConnect(string caption)
        {
            MessageBox.Show("Отсуствует подключение к серверу", caption);
            MainWindow.userName = "";
            Application.Current.Windows[0].Show();
            Close();
        }
        private void notAuthorize(string caption, string responseContent)
        {
            dynamic resData = JsonConvert.DeserializeObject(responseContent);
            MessageBox.Show((string)resData["message"], caption, MessageBoxButton.OK, MessageBoxImage.Information);
            MainWindow.userName = "";
            Application.Current.Windows[0].Show();
            Close();
        }
        private void errorResponse(string caption, string responseContent)
        {
            dynamic resData = JsonConvert.DeserializeObject(responseContent);
            MessageBox.Show((string)resData["message"], caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void cbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCategory.SelectedIndex < 0) return;
            int categoryId = ((Category)cbCategory.SelectedItem).Id;
            if (lstServ.Count > 0) lstServ.Clear();
            RestResponse response = MainWindow.Post_Request("/getServiceByCategory", new { categoryId });
            if (response.IsSuccessful)
            {
                dynamic resData = JsonConvert.DeserializeObject(response.Content);
                resData = resData["result"];
                for (int i = 0; i < resData.Count; i++)
                {
                    lstServ.Add(new Serv((int)resData[i]["Id"], (string)resData[i]["Name"], (int)resData[i]["Price"]));
                }
            }
            else if (response.StatusCode == 0) errorConnect("Добавление услуги");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Добавление услуги", response.Content);
            else errorResponse("Добавление услуги", response.Content);

        }

        private void ButtonClickAddService(object sender, RoutedEventArgs e)
        {
            if (cbCategory.SelectedIndex == -1 || cbService.SelectedIndex == -1)
            {
                MessageBox.Show("Не заполнены некоторые поля. Попробуйте снова.", "Добавление услуги", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string category = ((Category)cbCategory.SelectedItem).Name;
            int serviceId = ((Serv)cbService.SelectedItem).Id;

            RestResponse response = MainWindow.Post_Request("/addService", new { serviceId, repairId = currentRepair.Id });
            if (response.IsSuccessful)
            {
                MessageBox.Show("Услуга успешно добавлена.", "Добавление услуги");
                lstServices.Add(new Service(category, ((Serv)cbService.SelectedItem).Name, ((Serv)cbService.SelectedItem).Price));
                cbCategory.SelectedIndex = -1;
                cbService.SelectedIndex = -1;
                lstServ.Clear();
                int sum = 0;
                foreach (var elem in lstServices) sum += elem.Price;
                lbPricePartChange.Content = "Стоимость запчастей: " + sum + " руб.";
            }
            else if (response.StatusCode == 0) errorConnect("Добавление услуги");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Добавление услуги", response.Content);
            else errorResponse("Добавление услуги", response.Content);
        }
    }
}
