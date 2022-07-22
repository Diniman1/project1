using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Логика взаимодействия для masterWindow.xaml
    /// </summary>
    public partial class masterWindow : Window
    {
        private string changeClient = "";
        private string changeCar = "";
        private string typeSort = "All";
        private Appl changeAppl = null;
        private Repair currentRepair = null;
        Dictionary<string, string> sort = new Dictionary<string, string>();
        List<string> lstStatus = new List<string>();

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

            public string getClient{ get => Client_name; }
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

        public struct Worker 
        {
            public int Id { get; set; }
            public string Full_name { get; set; }

            public Worker(int id, string fullName)
            {
                Id = id;
                Full_name = fullName;
            }
        }
        public ObservableCollection<Worker> lstWorkers = new ObservableCollection<Worker>();

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

        public struct SparePart
        {
            public int Id { get; }
            public string Name { get; }
            public int Price { get; }

            public SparePart(int id, string name, int price)
            {
                Id = id;
                Name = name;
                Price = price;
            }
        }
        public ObservableCollection<SparePart> lstSpareParts = new ObservableCollection<SparePart>();

        public class Repair
        {
            public int Id { get; }
            public string Worker_name { get; }
            public string Defects { get; }

            public Repair(int id, string workerName, string defects)
            {
                Id = id;
                Worker_name = workerName;
                Defects = defects;
            }
        }

        public masterWindow()
        {
            InitializeComponent();
            Title = "Мастер: " + MainWindow.userName;
            ResizeMode = ResizeMode.NoResize;
            mainMaster.Visibility = Visibility.Visible;
            InitializeElements();
        }

        private void InitializeElements()
        {
            RestResponse response = MainWindow.Get_Request("/getAllStatus");
            if (response.IsSuccessful)
            {
                dynamic resData = JsonConvert.DeserializeObject(response.Content);
                foreach (var data in resData["result"]) lstStatus.Add((string)data["Name"]);
            }
            else if (response.StatusCode == 0) errorConnect("Главное окно");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Главное окно", response.Content);
            else errorResponse("Главное окно", response.Content);

            sort.Add("Все", "All");
            sort.Add("Текущий месяц", "Month");
            sort.Add("Дата начала", "Date");
            sort.Add("Клиент", "Client");
            sort.Add("Автомобиль", "Car");
            sort.Add("Статус", "Status");

            cbSortMain.ItemsSource = sort;
            cbSortMain.SelectedIndex = 0;
            cbSortMain.DisplayMemberPath = "Key";
            cbStatusMain.ItemsSource = lstStatus;
            cbWorkerChange.ItemsSource = lstWorkers;
            cbWorkerChange.DisplayMemberPath = "Full_name";
            DataGridApplication.ItemsSource = lstApplication;
            dgServiceChange.ItemsSource = lstServices;
            dgPartChange.ItemsSource = lstGivenParts;
            cbPart.ItemsSource = lstSpareParts;
            cbPart.DisplayMemberPath = "Name";
            getApplication();
        }

        private void getApplication(string filter = "")
        {
            if (lstApplication.Count > 0) lstApplication.Clear();
            RestResponse response = MainWindow.Post_Request("/getApplication", new { typeSort, filter });
            if(response.IsSuccessful)
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

        private void ButtonClickAddApplication(object sender, RoutedEventArgs e)
        {
            mainMaster.Visibility = Visibility.Collapsed;
            FormAddApplication.Visibility = Visibility.Visible;
        }
        private void ButtonClickAddClient(object sender, RoutedEventArgs e)
        {
            FormAddApplication.Visibility = Visibility.Collapsed;
            FormAddClient.Visibility = Visibility.Visible;
        }
        private void ButtonClickAddCar(object sender, RoutedEventArgs e)
        {
            FormAddApplication.Visibility = Visibility.Collapsed;
            FormAddCar.Visibility = Visibility.Visible;
        }

        private void ButtonClickCancelApplication(object sender, RoutedEventArgs e)
        {
            if (tbCarAddApplication.Text.Trim() != "" || tbTelAddApplication.Text.Trim() != "" || tbCommentsAddApplication.Text.Trim() != "")
            {
                var result = MessageBox.Show("Некоторые поля заполнены. Вы уверены, что хотите покинуть данную форму? Данные будут утеряны.",
                    "Закрытие формы", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }
            tbCarAddApplication.Text = "";
            tbTelAddApplication.Text = "";
            tbCommentsAddApplication.Text = "";
            if (changeClient != "")
            {
                lbClientAddApplication.Content = "Клиент";
                changeClient = "";
            }
            if (changeCar != "")
            {
                lbCarAddApplication.Content = "Автомобиль";
                changeCar = "";
            }

            FormAddApplication.Visibility = Visibility.Collapsed;
            mainMaster.Visibility = Visibility.Visible;
        }
        private void ButtonClickCancelClient(object sender, RoutedEventArgs e)
        {
            if (tbFNameAddClient.Text.Trim() != "" || tbTelAddClient.Text.Trim() != "" || tbAddressAddClient.Text.Trim() != "")
            {
                var result = MessageBox.Show("Некоторые поля заполнены. Вы уверены, что хотите покинуть данную форму? Данные будут утеряны.",
                    "Закрытие формы", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }

            tbFNameAddClient.Text = "";
            tbTelAddClient.Text = "";
            tbAddressAddClient.Text = "";

            FormAddClient.Visibility = Visibility.Collapsed;
            FormAddApplication.Visibility = Visibility.Visible;
        }
        private void ButtonClickCancelСar(object sender, RoutedEventArgs e)
        {
            if (tbNumberAddCar.Text.Trim() != "" || tbMarkAddCar.Text.Trim()!= "" || tbModelAddCar.Text.Trim() != "" || 
                tbEngineAddCar.Text.Trim() != "" || tbBodyAddCar.Text.Trim()!= "")
            {
                var result = MessageBox.Show("Некоторые поля заполнены. Вы уверены, что хотите покинуть данную форму? Данные будут утеряны.",
                    "Закрытие формы", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }
            tbNumberAddCar.Text = "";
            tbMarkAddCar.Text = "";
            tbModelAddCar.Text = "";
            tbEngineAddCar.Text = "";
            tbBodyAddCar.Text = "";

            FormAddCar.Visibility = Visibility.Collapsed;
            FormAddApplication.Visibility = Visibility.Visible;
        }
        private void ButtonClickCancelChange(object sender, RoutedEventArgs e)
        {
            changeAppl = null;

            cbWorkerChange.Visibility = Visibility.Collapsed;
            lbWorkerChange.Visibility = Visibility.Collapsed;
            lbWorkChange.Visibility = Visibility.Collapsed;
            btnMainChange.Visibility = Visibility.Visible;
            tbDiagnisticChange.Visibility = Visibility.Collapsed;
            lbServiceChange.Visibility = Visibility.Collapsed;
            dgServiceChange.Visibility = Visibility.Collapsed;
            lbPriceChange.Visibility = Visibility.Collapsed;
            lbPartChange.Visibility = Visibility.Collapsed;
            btnAddPartChange.Visibility = Visibility.Collapsed;
            dgPartChange.Visibility = Visibility.Collapsed;
            lbPricePartChange.Visibility = Visibility.Collapsed;
            lbAllPricehange.Visibility = Visibility.Collapsed;

            FormChangeApplication.Visibility = Visibility.Collapsed;
            mainMaster.Visibility = Visibility.Visible;
        }
        private void ButtonClickCancelAddPart(object sender, RoutedEventArgs e)
        {
            if (cbPart.SelectedIndex != -1 || tbQuantity.Text.Length > 0)
            {
                var result = MessageBox.Show("Некоторые поля заполнены. Вы уверены, что хотите покинуть данную форму? Данные будут утеряны.",
                    "Закрытие формы", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }
            cbPart.SelectedIndex = -1;
            tbQuantity.Text = "";

            FormAddPart.Visibility = Visibility.Collapsed;
            FormChangeApplication.Visibility = Visibility.Visible;
        }
        private void ButtonClickLogout(object sender, RoutedEventArgs e)
        {
            MainWindow.Get_Request("/logout");
            MainWindow.userName = "";
            Application.Current.Windows[0].Show();
            Close();
        }

        private void CheckInput(object sender, EventArgs e)
        {
            if (sender == tbTelAddApplication && changeClient.Length > 0 && tbTelAddApplication.Text != changeClient)
            {
                changeClient = "";
                lbClientAddApplication.Content = "Клиент";
            }
            else if (sender == tbCarAddApplication && changeCar.Length > 0 && tbCarAddApplication.Text != changeCar)
            {
                changeCar = "";
                lbCarAddApplication.Content = "Автомобиль";
            }
        }
        private void OnlyNumber(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && e.Text != "\b") e.Handled = true;
        }
        private void NumberCar(object sender, TextCompositionEventArgs e)
        {
            if(!char.IsDigit(e.Text, 0) && e.Text != "\b")
            {
                char inp = e.Text[0];
                if (inp < 'А' || inp > 'Я') e.Handled = true;
            }
            if (sender == tbCarAddApplication && changeCar.Length > 0 && tbCarAddApplication.Text != changeCar)
            {
                changeCar = "";
                lbCarAddApplication.Content = "Автомобиль";
            }
        }

        private void ButtonClickCreateAddApplication(object sender, RoutedEventArgs e)
        {
            string comment = tbCommentsAddApplication.Text;
            if (changeClient == "" || changeCar == "")
            {
                string message = (changeClient == "") ? "Некорректный номер клиента." : "Некорректный гос. номер автомобиля.";
                MessageBox.Show(message + " Введите номер и нажмите найти.", "Добавление заявки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            object data = new { telephone = changeClient, number = changeCar, comment = comment.Trim() };
            RestResponse response = MainWindow.Post_Request("/createApplication", data);
            if (response.IsSuccessful)
            {
                MessageBox.Show("Заявка успешно создана", "Добавление заявки");
                dynamic resData = JsonConvert.DeserializeObject(response.Content);
                lstApplication.Add(new Appl(
                    (int)resData["Id"],
                    lbClientAddApplication.Content.ToString().Substring(8),
                    changeClient,
                    lbCarAddApplication.Content.ToString().Substring(12),
                    changeCar,
                    DateTime.Now.ToString().Replace(" AM", "").Replace(" PM", ""),
                    "Не указано",
                    tbCommentsAddApplication.Text,
                    "Обработка"
                    ));
                tbCommentsAddApplication.Text = "";
                lbCarAddApplication.Content = "Автомобиль";
                lbClientAddApplication.Content = "Клиент";
                tbCarAddApplication.Text = changeCar = "";
                tbTelAddApplication.Text = changeClient = "";
            }
            else if (response.StatusCode == 0) errorConnect("Добавление заявки");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Добавление заявки", response.Content);
            else errorResponse("Добавление заявки", response.Content);
        }
        private void ButtonClickCreateClient(object sender, RoutedEventArgs e)
        {
            string name = tbFNameAddClient.Text;
            string telephone = tbTelAddClient.Text;
            string address = tbAddressAddClient.Text;
            if (name.Trim() == "" || telephone.Trim() == "" || address.Trim() == "" || telephone.Length != 11)
            {
                MessageBox.Show("Неккоректно введены данные в поля.", "Добавление клиента", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            object data = new { name, telephone, address };
            RestResponse response = MainWindow.Post_Request("/createClient", data);
            if (response.IsSuccessful)
            {
                MessageBox.Show("Клиент успешно добавлен!", "Добавление клиента");
                changeClient = telephone;
                tbTelAddApplication.Text = telephone;
                lbClientAddApplication.Content = "Клиент: " + name;
            }
            else if (response.StatusCode == 0) errorConnect("Добавление клиента");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Добавление клиента", response.Content);
            else
            {
                errorResponse("Добавление клиента", response.Content);
                return;
            }
            tbFNameAddClient.Text = "";
            tbTelAddClient.Text = "";
            tbAddressAddClient.Text = "";
        }
        private void ButtonClickCreateCar(object sender, RoutedEventArgs e)
        {
            string number = tbNumberAddCar.Text;
            string mark = tbMarkAddCar.Text;
            string model = tbModelAddCar.Text;
            string engine = tbEngineAddCar.Text;
            string body = tbBodyAddCar.Text;

            if (number.Trim() == "" || number.Length < 8 || number.Length > 9 ||
                mark.Trim() == "" || model.Trim() == "" ||
                engine.Trim() == "" || engine.Length != 17 || body.Trim() == "")
            {
                MessageBox.Show("Некорректный ввод данных полей. Попробуйте снова.", "Добавление автомобиля", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            object data = new { number, mark, model, engine, body };
            RestResponse response = MainWindow.Post_Request("/createCar", data);
            if (response.IsSuccessful)
            {
                MessageBox.Show("Автомобиль успешно добавлен!", "Добавление автомобиля");
                changeCar = number;
                tbCarAddApplication.Text = number;
                lbCarAddApplication.Content = "Автомобиль: " + mark + " " + model;
            }
            else if (response.StatusCode == 0) errorConnect("Добавление автомобиля");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Добавление автомобиля", response.Content);
            else
            {
                errorResponse("Добавление автомобиля", response.Content);
                return;
            }

            tbNumberAddCar.Text = "";
            tbMarkAddCar.Text = "";
            tbModelAddCar.Text = "";
            tbEngineAddCar.Text = "";
            tbBodyAddCar.Text = "";
        }

        private void ButtonClickSearchClient(object sender, RoutedEventArgs e)
        {
            string telephone = tbTelAddApplication.Text;
            if(telephone.Length != 11 || telephone.Trim() == "")
            {
                MessageBox.Show("Неккоректный номер телефона. Измените его и попробуйте снова.", "Добавление заявки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            RestResponse response = MainWindow.Post_Request("/getClient", new { telephone });
            if (response.IsSuccessful)
            {
                dynamic resData = JsonConvert.DeserializeObject(response.Content);
                changeClient = telephone;
                lbClientAddApplication.Content = "Клиент: " + (string)resData["client"]["Full_name"];
            }
            else if (response.StatusCode == 0) errorConnect("Добавление заявки");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Добавление заявки", response.Content);
            else errorResponse("Добавление заявки", response.Content);
        }
        private void ButtonClickSearchCar(object sender, RoutedEventArgs e)
        {
            string number = tbCarAddApplication.Text;
            if (number.Length < 8 || number.Length > 9 || number.Trim() == "")
            {
                MessageBox.Show("Неккоректный гос. номер. Измените его и попробуйте снова.", "Добавление заявки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            RestResponse response = MainWindow.Post_Request("/getCar", new { number });
            if (response.IsSuccessful)
            {
                dynamic resData = JsonConvert.DeserializeObject(response.Content);
                changeCar = number;
                lbCarAddApplication.Content = "Автомобиль: " + (string)resData["car"]["Mark"] + " " + (string)resData["car"]["Model"];
            }
            else if (response.StatusCode == 0) errorConnect("Добавление заявки");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Добавление заявки", response.Content);
            else errorResponse("Добавление заявки", response.Content);
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

        private void cbSortMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            typeSort = ((KeyValuePair<string, string>)e.AddedItems[0]).Value;
            switch (cbSortMain.SelectedIndex) 
            {
                case 2: { dpMain.Visibility = Visibility.Visible; break; }
                case 3:
                    {
                        tbClientMain.Visibility = Visibility.Visible;
                        lbClientMainWtM.Visibility = Visibility.Visible;
                        break;
                    }
                case 4:
                    {
                        tbCarMain.Visibility = Visibility.Visible;
                        lbCarMainWtM.Visibility = Visibility.Visible;
                        break;
                    }
                case 5: { cbStatusMain.Visibility = Visibility.Visible; break; }
            }
            if (cbSortMain.SelectedIndex != 2 && dpMain.Visibility == Visibility.Visible)
            {
                dpMain.Text = "";
                dpMain.Visibility = Visibility.Collapsed;
            }
            if (cbSortMain.SelectedIndex != 3 && tbClientMain.Visibility == Visibility.Visible)
            {
                tbClientMain.Text = "";
                tbClientMain.Visibility = Visibility.Collapsed;
                lbClientMainWtM.Visibility = Visibility.Collapsed;
            }
            if (cbSortMain.SelectedIndex != 4 && tbCarMain.Visibility == Visibility.Visible)
            {
                tbCarMain.Text = "";
                tbCarMain.Visibility = Visibility.Collapsed;
                lbCarMainWtM.Visibility = Visibility.Collapsed;
            }
            if (cbSortMain.SelectedIndex != 5 && cbStatusMain.Visibility == Visibility.Visible)
            {
                cbStatusMain.SelectedIndex = -1;
                cbStatusMain.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonClickSortApplication(object sender, RoutedEventArgs e)
        {
            string filter = "";
            switch (typeSort) 
            {
                case "Status":
                    {
                        filter = (string)cbStatusMain.Items[cbStatusMain.SelectedIndex];
                        break;
                    }
                case "Client":
                    {
                        filter = tbClientMain.Text;
                        if(filter.Trim() == "" || filter.Length != 11)
                        {
                            MessageBox.Show("Неккоректный номер телефона.", "Главное окно", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        break;
                    }
                case "Car":
                    {
                        filter = tbCarMain.Text;
                        if (filter.Trim() == "" || filter.Length < 8 || filter.Length > 9)
                        {
                            MessageBox.Show("Неккоректный гос. номер автомобиля.", "Главное окно", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        break;
                    }
                case "Date":
                    {
                        string[] part = dpMain.Text.Split('/');
                        filter = part[2] + '-' + part[0] + '-' + part[1];
                        break;
                    }
            }
            getApplication(filter);
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
            lbDiagnosticChange.Content = "Диагностика не проведена.";

            if (changeAppl.Status == "Обработка") btnMainChange.Content = "Принять";
            if (changeAppl.Status == "Принято")
            {
                if (lstWorkers.Count > 0) lstWorkers.Clear();
                getAllWorkers();

                cbWorkerChange.Visibility = Visibility.Visible;
                lbWorkChange.Visibility = Visibility.Visible;

                btnMainChange.Content = "Назначить";
            }
            if (changeAppl.Status == "Диагностика" || changeAppl.Status == "Ремонт" || changeAppl.Status == "Готово" || changeAppl.Status == "Завершено")
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

                if (changeAppl.Status == "Диагностика") btnMainChange.Visibility = Visibility.Collapsed;

                lbWorkerChange.Visibility = Visibility.Visible;
                lbWorkerChange.Content = "Механик: " + currentRepair.Worker_name;
                if(currentRepair.Defects.Length > 0)
                {
                    lbDiagnosticChange.Content = "Неисправности";
                    tbDiagnisticChange.Text = currentRepair.Defects;
                    tbDiagnisticChange.Visibility = Visibility.Visible;
                    if (changeAppl.Status == "Диагностика")
                    {
                        btnMainChange.Visibility = Visibility.Visible;
                        btnMainChange.Content = "Начать ремонт";
                    }
                }
            }
            if (changeAppl.Status == "Ремонт" || changeAppl.Status == "Готово" || changeAppl.Status == "Завершено")
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

                if(changeAppl.Status == "Ремонт") btnMainChange.Visibility = Visibility.Collapsed;

                lbServiceChange.Visibility = Visibility.Visible;
                dgServiceChange.Visibility = Visibility.Visible;
                lbPriceChange.Visibility = Visibility.Visible;
                lbPartChange.Visibility = Visibility.Visible;
                if (changeAppl.Status == "Ремонт") btnAddPartChange.Visibility = Visibility.Visible;
                dgPartChange.Visibility = Visibility.Visible;
                lbPricePartChange.Visibility = Visibility.Visible;
            }
            if (changeAppl.Status == "Готово" || changeAppl.Status == "Завершено")
            {
                if (changeAppl.Status == "Готово") btnMainChange.Content = "Закрыть ремонт";
                int sum = 0;
                foreach (var elem in lstServices) sum += elem.Price;
                foreach (var elem in lstGivenParts) sum += elem.Price * elem.Quantity;
                lbAllPricehange.Content = "Итого к оплате: " + sum + " руб.";
                lbAllPricehange.Visibility = Visibility.Visible;
            }
            if (changeAppl.Status == "Завершено") btnMainChange.Visibility = Visibility.Collapsed;

            mainMaster.Visibility = Visibility.Collapsed;
            FormChangeApplication.Visibility = Visibility.Visible;
        }

        private void ButtonClickMainChange(object sender, RoutedEventArgs e)
        {
            RestResponse response;
            if (changeAppl.Status == "Обработка")
            {
                if (lstWorkers.Count > 0) lstWorkers.Clear();
                getAllWorkers();

                changeAppl.Status = "Принято";
                DataGridApplication.Items.Refresh();
                btnMainChange.Content = "Назначить";
                cbWorkerChange.Visibility = Visibility.Visible;
                lbWorkChange.Visibility = Visibility.Visible;
            }
            else if (changeAppl.Status == "Принято")
            {
                if(cbWorkerChange.SelectedIndex == -1)
                {
                    MessageBox.Show("Механик не выбран.", "Окно заявки", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                int workerId = ((Worker)cbWorkerChange.SelectedItem).Id;

                response = MainWindow.Post_Request("/createRepair", new { workerId, id = changeAppl.Id });
                if (response.IsSuccessful)
                {
                    dynamic resData = JsonConvert.DeserializeObject(response.Content);
                    currentRepair = new Repair((int)resData["Id"], ((Worker)cbWorkerChange.SelectedItem).Full_name, "");
                }
                else if (response.StatusCode == 0) { errorConnect("Окно заявки"); return; }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) { notAuthorize("Окно заявки", response.Content); return; }
                else { errorResponse("Окно заявки", response.Content); return; }

                changeAppl.Status = "Диагностика";
                DataGridApplication.Items.Refresh();
                lbWorkChange.Content = "Механик: " + ((Worker)cbWorkerChange.SelectedItem).Full_name;
                btnMainChange.Visibility = Visibility.Collapsed;

                cbWorkerChange.Visibility = Visibility.Collapsed;
                lbWorkChange.Visibility = Visibility.Collapsed;
            }
            else if (changeAppl.Status == "Диагностика")
            {
                changeAppl.Status = "Ремонт";
                DataGridApplication.Items.Refresh();
                btnMainChange.Visibility = Visibility.Collapsed;

                lbServiceChange.Visibility = Visibility.Visible;
                dgServiceChange.Visibility = Visibility.Visible;
                lbPriceChange.Content = "Стоимость услуг: 0 руб.";
                lbPriceChange.Visibility = Visibility.Visible;
                lbPartChange.Visibility = Visibility.Visible;
                btnAddPartChange.Visibility = Visibility.Visible;
                dgPartChange.Visibility = Visibility.Visible;
                lbPricePartChange.Visibility = Visibility.Visible;
                lbPricePartChange.Content = "Стоимость запчастей: 0 руб.";
            }
            else if (changeAppl.Status == "Готово")
            {
                changeAppl.Status = "Завершено";
                DataGridApplication.Items.Refresh();
                btnMainChange.Visibility = Visibility.Collapsed;
                response = MainWindow.Post_Request("/setDateEnd", new { id = changeAppl.Id });
                if (response.IsSuccessful)
                {
                    string dateEnd = DateTime.Now.ToString().Replace(" AM", "").Replace(" PM", "");
                    lbDateEndChange.Content = "День завершения: " + dateEnd;
                    changeAppl.Date_end = dateEnd;
                }
                else if (response.StatusCode == 0) errorConnect("Окно заявки");
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Окно заявки", response.Content);
                else errorResponse("Окно заявки", response.Content);
            }
            lbStatusChange.Content = "Статус: " + changeAppl.Status;
            response = MainWindow.Post_Request("/setStatus", new { status = changeAppl.Status, id = changeAppl.Id });
            if (response.IsSuccessful) return;
            else if (response.StatusCode == 0) errorConnect("Окно заявки");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Окно заявки", response.Content);
            else errorResponse("Окно заявки", response.Content);
        }

        private void getAllWorkers()
        {
            RestResponse response = MainWindow.Get_Request("/getAllWorker");
            if (response.IsSuccessful)
            {
                dynamic resData = JsonConvert.DeserializeObject(response.Content);
                resData = resData["result"];
                for (int i = 0; i < resData.Count; i++)
                {
                    lstWorkers.Add(new Worker((int)resData[i]["Id"], (string)resData[i]["Full_name"]));
                }
            }
            else if (response.StatusCode == 0) errorConnect("Окно заявки");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Окно заявки", response.Content);
            else errorResponse("Окно заявки", response.Content);
        }

        private void btnAddPartChange_Click(object sender, RoutedEventArgs e)
        {
            if (lstSpareParts.Count > 0) lstSpareParts.Clear();
            RestResponse response = MainWindow.Get_Request("/getAllSpareParts");
            if (response.IsSuccessful)
            {
                dynamic resData = JsonConvert.DeserializeObject(response.Content);
                resData = resData["result"];
                for (int i = 0; i < resData.Count; i++)
                {
                    lstSpareParts.Add(new SparePart((int)resData[i]["Id"], (string)resData[i]["Name"], (int)resData[i]["Price"]));
                }
            }
            else if (response.StatusCode == 0) errorConnect("Добавление запчастей");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Добавление запчастей", response.Content);
            else errorResponse("Добавление запчастей", response.Content);

            FormChangeApplication.Visibility = Visibility.Collapsed;
            FormAddPart.Visibility = Visibility.Visible;
        }

        private void ButtonClickAddPart(object sender, RoutedEventArgs e)
        {
            if(cbPart.SelectedIndex == -1 || tbQuantity.Text.Length == 0)
            {
                MessageBox.Show("Не заполнены некоторые поля. Попробуйте снова.", "Добавление запчастей", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            int partId = ((SparePart)cbPart.SelectedItem).Id;
            int quantity = int.Parse(tbQuantity.Text);
            if(quantity <= 0)
            {
                MessageBox.Show("Некорректный ввод данных. Попробуйте снова.", "Добавление запчастей", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            RestResponse response = MainWindow.Post_Request("/addSparePartRepair", new { partId, quantity, repairId = currentRepair.Id });
            if (response.IsSuccessful)
            {
                MessageBox.Show("Запчасти успешно добавлены.", "Добавление запчастей");
                lstGivenParts.Add(new GivenPart(((SparePart)cbPart.SelectedItem).Name, ((SparePart)cbPart.SelectedItem).Price, quantity));
                cbPart.SelectedIndex = -1;
                tbQuantity.Text = "";
                int sum = 0;
                foreach (var elem in lstGivenParts) sum += elem.Price * elem.Quantity;
                lbPricePartChange.Content = "Стоимость запчастей: " + sum + " руб.";
            }
            else if (response.StatusCode == 0) errorConnect("Добавление запчастей");
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) notAuthorize("Добавление запчастей", response.Content);
            else errorResponse("Добавление запчастей", response.Content);
        }
    }
}
