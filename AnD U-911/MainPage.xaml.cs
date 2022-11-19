using AudioUWP;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace AnD_U_911
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer timer;
        List<ChartData> ChartInfo = new List<ChartData>();
        AudioRecorder _audioRecorder;
        public MainPage()
        {
            this.InitializeComponent();
            timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) }; 
            timer.Tick += Timer_Tick;
            timer.Start();

            try
            {
                Watcher = new BluetoothLEAdvertisementWatcher()
                {
                    ScanningMode = BluetoothLEScanningMode.Active
                };
                Watcher.Received += Watcher_Received;
                Watcher.Stopped += Watcher_Stopped;
                Watcher.Start();
                this._audioRecorder = new AudioRecorder();
                
            }
            catch (Exception ex)
            {
            }


            //var d = new DateTime(2019, 04, 01);
            //Random r = new Random();
            //ChartInfo.Add(new ChartData() { DataName = d.AddHours(+9), DataValue = r.Next(90, 190), DataDia = r.Next(60, 130), DataPul = r.Next(50, 90), DataFild = MagnitFild() * 20 });
            //ChartInfo.Add(new ChartData() { DataName = d.AddHours(+15), DataValue = r.Next(90, 190), DataDia = r.Next(60, 130), DataPul = r.Next(50, 90), DataFild = MagnitFild() * 20 });
            //ChartInfo.Add(new ChartData() { DataName = d.AddHours(+32), DataValue = r.Next(90, 190), DataDia = r.Next(60, 130), DataPul = r.Next(50, 90), DataFild = MagnitFild() * 10 });
            //ChartInfo.Add(new ChartData() { DataName = d.AddHours(+41), DataValue = r.Next(90, 190), DataDia = r.Next(60, 130), DataPul = r.Next(50, 90), DataFild = MagnitFild() * 10 });
            //ChartInfo.Add(new ChartData() { DataName = d.AddHours(+53), DataValue = r.Next(90, 190), DataDia = r.Next(60, 130), DataPul = r.Next(50, 90), DataFild = MagnitFild() * 10 });
            ////ChartInfo.Add(new ChartData() { DataName = d.AddHours(+60), DataValue = r.Next(90, 190), DataDia = r.Next(60, 130), DataPul = r.Next(50, 90), DataFild = MagnitFild() * 10 });
            ////ChartInfo.Add(new ChartData() { DataName = d.AddHours(+65), DataValue = r.Next(90, 190), DataDia = r.Next(60, 130), DataPul = r.Next(50, 90), DataFild = MagnitFild() * 10 });
            ////ChartInfo.Add(new ChartData() { DataName = d.AddHours(+85), DataValue = r.Next(90, 190), DataDia = r.Next(60, 130), DataPul = r.Next(50, 90), DataFild = MagnitFild() * 10 });
            ////ChartInfo.Add(new ChartData() { DataName = d.AddHours(+90), DataValue = r.Next(90, 190), DataDia = r.Next(60, 130), DataPul = r.Next(50, 90), DataFild = MagnitFild() * 10 });
            ////ChartInfo.Add(new ChartData() { DataName = d.AddHours(+110), DataValue = r.Next(90, 190), DataDia = r.Next(60, 130), DataPul = r.Next(50, 90), DataFild = MagnitFild() * 10 });
            ////ChartInfo.Add(new ChartData() { DataName = d.AddHours(+115), DataValue = r.Next(90, 190), DataDia = r.Next(60, 130), DataPul = r.Next(50, 90), DataFild = MagnitFild() * 20 });
            ////ChartInfo.Add(new ChartData() { DataName = d.AddHours(+122), DataValue = r.Next(90, 190), DataDia = r.Next(60, 130), DataPul = r.Next(50, 90), DataFild = MagnitFild() * 20 });
            //(LineChart.Series[0] as LineSeries).ItemsSource = ChartInfo;
            //(LineChart.Series[1] as LineSeries).ItemsSource = ChartInfo;
            //(LineChart.Series[2] as LineSeries).ItemsSource = ChartInfo;
            //(LineChart.Series[3] as ColumnSeries).ItemsSource = ChartInfo;
        }

        BluetoothLEAdvertisementWatcher Watcher { get; set; }
        BluetoothLEDevice bluetoothLeDevice = null;
        private bool isFindDevice { get; set; } = false;
        private async void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            
            if (args.Advertisement.LocalName.Contains("A&D_UA-651BLE_13A27F"))
            {
                bluetoothLeDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
                GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    foreach (var service in services)
                    {
                        GattCharacteristicsResult characteristicsResult = await service.GetCharacteristicsAsync();
                        if (characteristicsResult.Status == GattCommunicationStatus.Success)
                        {
                            var characteristics = characteristicsResult.Characteristics;
                            foreach (var characteristic in characteristics)
                            {
                                GattCharacteristicProperties properties = characteristic.CharacteristicProperties;
                                if (properties.HasFlag(GattCharacteristicProperties.Indicate))
                                {
                                    GattWriteResult status = null;
                                    try
                                    {
                                        status = await characteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);
                                    }
                                    catch (Exception ex)
                                    {
                                        var dialog = new MessageDialog(ex.ToString());
                                        dialog.ShowAsync();
                                    }
                                    characteristic.ValueChanged += Characteristic_ValueChanged;
                                }
                            }
                        }
                    }
                }

            }
        }
        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                var myIp = await new HttpClient().GetStringAsync("https://api.ipify.org/");

                var myAdr = await new HttpClient().GetStringAsync("http://ipwhois.app/json/" + myIp.ToString());


                Geoloc geo = JsonConvert.DeserializeObject<Geoloc>(myAdr.ToString());
                var reader = DataReader.FromBuffer(args.CharacteristicValue);
                byte[] input = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(input);
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                   
                    try
                    {
                        string str;
                        str = BitConverter.ToString(input);
                       
                        string[] words = str.Split('-');
                        int sisint = int.Parse(words[1], System.Globalization.NumberStyles.HexNumber);
                        int diaint = int.Parse(words[3], System.Globalization.NumberStyles.HexNumber);
                        int pulint = int.Parse(words[7], System.Globalization.NumberStyles.HexNumber);

                        
                        (LineChart.Series[0] as LineSeries).ItemsSource = null;
                        (LineChart.Series[1] as LineSeries).ItemsSource = null;
                        (LineChart.Series[2] as LineSeries).ItemsSource = null;
                        (LineChart.Series[3] as ColumnSeries).ItemsSource = null;
                        if (ChartInfo.Count > 0)
                        {
                            if ((DateTime.Now - ChartInfo[ChartInfo.Count - 1].DataName >= TimeSpan.FromSeconds(60)&&(sisint!=255)))
                            {
                                ChartInfo.Add(new ChartData() { DataName = DateTime.Now, DataValue = sisint, DataDia = diaint, DataPul = pulint, DataFild = MagnitFild() * 10, Cit = geo.city });
                                DiaSis.Text = sisint.ToString() + "/" + diaint.ToString();
                                Puls.Text = pulint.ToString();
                            }
                            else
                            {
                                if ((DateTime.Now - ChartInfo[ChartInfo.Count - 2].DataName >= TimeSpan.FromSeconds(60) && (sisint != 255)))
                                {
                                    ChartInfo.Add(new ChartData() { DataName = DateTime.Now, DataValue = sisint, DataDia = diaint, DataPul = pulint, DataFild = MagnitFild() * 10, Cit = geo.city });
                                    DiaSis.Text = sisint.ToString() + "/" + diaint.ToString();
                                    Puls.Text = pulint.ToString();
                                }
                            }
                        }
                        else
                        {
                            if(sisint!=255)
                            {
                                ChartInfo.Add(new ChartData() { DataName = DateTime.Now, DataValue = sisint, DataDia = diaint, DataPul = pulint, DataFild = MagnitFild() * 10, Cit = geo.city });
                                DiaSis.Text = sisint.ToString() + "/" + diaint.ToString();
                                Puls.Text = pulint.ToString();
                            }
                        }
                        
                            (LineChart.Series[0] as LineSeries).ItemsSource = ChartInfo;
                            (LineChart.Series[1] as LineSeries).ItemsSource = ChartInfo;
                            (LineChart.Series[2] as LineSeries).ItemsSource = ChartInfo;
                            (LineChart.Series[3] as ColumnSeries).ItemsSource = ChartInfo;
                                             

                    }
                    catch(Exception ex)
                    {
                        
                    }

                });
            }
            catch (Exception ex)
            {
               
            }
        }
        private void Watcher_Stopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
        {
            ;
        }


        private void Timer_Tick(object sender, object e)
        {
            TimeLable.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private async void Wether()
        {
            try
            {
                WebRequest request = WebRequest.Create("https://api.openweathermap.org/data/2.5/weather?q=saratov&appid=91e183d2670e1b37220504f69917c7e0");
                request.Method = "POST";
                request.ContentType = "application/x-www-urlencoded";
                WebResponse response = await request.GetResponseAsync();
                string answer = String.Empty;
                using (Stream s = response.GetResponseStream())
                {
                    using (StreamReader stream = new StreamReader(s))
                    {
                        answer = await stream.ReadToEndAsync();
                        JObject json = JObject.Parse(answer);
                        var str = json.SelectToken(@"$.main");
                        var info = JsonConvert.DeserializeObject<Main>(str.ToString());
                        TextWeather.Text = info.temp.ToString("0") + "°C";
                        TextPreaser.Text = info.pressure.ToString("0") + " мм рт. ст";
                    }
                }
                response.Dispose();
            }
            catch
            {
                TextFild.Text = "Нет интернета";
                TextWeather.Text = "Нет подключения";
                TextPreaser.Text = "Нет подключения";
            }
            
        }
        private async void getip()
        {
            var myIp = await new HttpClient().GetStringAsync("https://api.ipify.org/");

            var myAdr = await new HttpClient().GetStringAsync("http://ipwhois.app/json/" + myIp.ToString());


            Geoloc geo = JsonConvert.DeserializeObject<Geoloc>(myAdr.ToString());
            
        }
        private int MagnitFild()
        {
            try
            {
                int fild = 0;
                var url = "https://my-calend.ru/magnitnye-buri/saratov";
                var web = new HtmlWeb();
                var doc = web.Load(url);
                var value = doc.DocumentNode.SelectSingleNode("//table[@class='magnitnye-buri-items'][1]/tr[2]/td[2]");
                if (value != null)
                {
                    fild = Convert.ToInt32(value.InnerText);
                }
                return fild;
            }
            catch
            {
                TextFild.Text = "Нет интернета";
                TextWeather.Text = "Нет подключения к интернету";
                TextPreaser.Text = "Нет подключения к интернету";
                return 0;
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Wether();
                TextFild.Text = "Геомагнитное поле: " + MagnitFild().ToString() + "/10";
                getip();
            }
            catch
            {
                TextFild.Text = "Нет интернета";
                TextWeather.Text = "Нет подключения к интернету";
                TextPreaser.Text = "Нет подключения к интернету";
            }
           
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //MessageDialog msgbox = new MessageDialog("Желаете сделать голосовую запись о самочувствии?", "Уведомление:данные записаны");

            //msgbox.Commands.Clear();
            //msgbox.Commands.Add(new UICommand { Label = "Yes", Id = 0 });
            //msgbox.Commands.Add(new UICommand { Label = "No", Id = 1 });

            //var res = await msgbox.ShowAsync();

            this._audioRecorder.Record();

            StopButton.Visibility = Visibility.Visible;
            StartButton.Visibility = Visibility.Collapsed;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((LineChart.Series[0] as LineSeries).SelectedItem != null)
                    this._audioRecorder.audio_filename = NameText.Text + "-" + ((LineChart.Series[0] as LineSeries).SelectedItem as ChartData).DataName.ToString().Replace(" ", "-").Replace(":", ".") + ".mp3";
                if ((LineChart.Series[1] as LineSeries).SelectedItem != null)
                    this._audioRecorder.audio_filename = NameText.Text + "-" + ((LineChart.Series[1] as LineSeries).SelectedItem as ChartData).DataName.ToString().Replace(" ", "-").Replace(":", ".") + ".mp3";
                if ((LineChart.Series[2] as LineSeries).SelectedItem != null)
                    this._audioRecorder.audio_filename = NameText.Text + "-" + ((LineChart.Series[2] as LineSeries).SelectedItem as ChartData).DataName.ToString().Replace(" ", "-").Replace(":", ".") + ".mp3";
                this._audioRecorder.StopRecording();
                if ((LineChart.Series[3] as ColumnSeries).SelectedItem != null)
                    this._audioRecorder.audio_filename = NameText.Text + "-" + ((LineChart.Series[3] as ColumnSeries).SelectedItem as ChartData).DataName.ToString().Replace(" ", "-").Replace(":", ".") + ".mp3";

                StopButton.Visibility = Visibility.Collapsed;
                StartButton.Visibility = Visibility.Visible;
            }
            catch
            {

            }
            //DatePoint.Text=((LineChart.Series[0] as LineSeries).SelectedItem as ChartData).DataName.ToString().Replace(" ","-").Replace(":", ",");
            
        }

        private void LineSeries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                StartButton.IsEnabled = true;
                DatePoint.Text = "Время: " + (e.AddedItems[0] as ChartData).DataName.ToString();
                PulsPoint.Text = "ЧСС: " + (e.AddedItems[0] as ChartData).DataPul.ToString();
                DiaPoint.Text = "АД Систолическое: " + (e.AddedItems[0] as ChartData).DataValue.ToString();
                SisPoint.Text = "АД Диастолическое: " + (e.AddedItems[0] as ChartData).DataDia.ToString();
                var chi = ChartInfo.Where(i => i.DataName == (e.AddedItems[0] as ChartData).DataName).FirstOrDefault();
                citlable.Text = "Местоположение: " + chi.Cit;
            }
            catch { }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFolderAsync(KnownFolders.MusicLibrary);
        }
    }
}
