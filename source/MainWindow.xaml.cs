using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows;
using System.Management;
using System.Windows.Controls;
using LenovoController.Features;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using System.Windows.Threading;
using System.Security.Principal;

namespace LenovoController
{

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Class1 batVal = new Class1();
        DispatcherTimer dt = new DispatcherTimer();
        int val = 0; 

        private readonly RadioButton[] _alwaysOnUsbButtons;
        private readonly AlwaysOnUsbFeature _alwaysOnUsbFeature = new AlwaysOnUsbFeature();
        private readonly RadioButton[] _batteryButtons;
        private readonly BatteryFeature _batteryFeature = new BatteryFeature();
        private readonly RadioButton[] _powerModeButtons;
        private readonly PowerModeFeature _powerModeFeature = new PowerModeFeature();
        private readonly FnLockFeature _fnLockFeature = new FnLockFeature();
        private readonly OverDriveFeature _overDriveFeature = new OverDriveFeature();
        private readonly TouchpadLockFeature _touchpadLockFeature = new TouchpadLockFeature();

        public MainWindow()
        {
            InitializeComponent();

            mainWindow.Title += $" v{AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version}";
            _powerModeButtons = new[] {radioQuiet, radioBalance, radioPerformance};
            _batteryButtons = new[] {radioConservation, radioNormalCharge, radioRapidCharge};
            _alwaysOnUsbButtons = new[] {radioAlwaysOnUsbOff, radioAlwaysOnUsbOnWhenSleeping, radioAlwaysOnUsbOnAlways};
            Refresh();
            dt.Interval = TimeSpan.FromSeconds(5);
            dt.Tick += dtTicker;
            dt.Start();
            AdminRelauncher();
        }

        private void AdminRelauncher()
        {
            if (!IsRunAsAdmin())
            {
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = Assembly.GetEntryAssembly().CodeBase;

                proc.Verb = "runas";

                try
                {
                    Process.Start(proc);
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    admin.Visibility = Visibility.Visible;
                    admin.Content = "Error: No Admin!";
                    customCharge.IsEnabled= false;
                    MessageBox.Show("This program must be run as an administrator! \n\n" + ex.ToString(), "Error");
                }
            }
        }

        private bool IsRunAsAdmin()
        {
            try
            {
                admin.Visibility = Visibility.Collapsed;
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(id);
                customCharge.IsEnabled = true;
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                return false;
            }
        }


        private void dtTicker(object sender, EventArgs e )
        {

            if (customCharge.IsChecked == true)
            {
                batteryCheck(true);
            }
            else if(customCharge.IsChecked == false)
            {
                _batteryFeature.SetState((BatteryState)0);
                batteryCheck(false);
                
            }
            Refresh();
        }
        private class FeatureCheck
        {
            private readonly Action _check;
            private readonly Action _disable;
            private readonly Action _diable2; 

            internal FeatureCheck(Action check, Action disable)
            {
                _check = check;
                _disable = disable;
                
            }

            
            internal void Check() => _check();
            internal void Disable() => _disable();
        }

        private void batteryCheck(bool isOn)
        {
            
            if (isOn == true)
            {
                if (customCharge.IsChecked == true)
                {
                    batVal.batteryOnOff();
                    checkboolval.Content = (batVal.batteryOnOff());

                    if (batVal.batteryOnOff() == 0)
                    {
                        _batteryFeature.SetState((BatteryState)0);
                    }
                    else if (batVal.batteryOnOff() == 1)
                    {
                        _batteryFeature.SetState((BatteryState)1);
                    }
                    else if (batVal.batteryOnOff() == 3)
                    {
                        Debug.WriteLine("Do nothing");
                    }
                    else
                    {
                        
                        _batteryFeature.SetState((BatteryState)0);
                        Window1 win = new Window1();
                        win.ErrorText.Content = "Unexcepted Error, Battery Mode Could Not Be Set";
                        win.Error_Code.Content = "Error Code: 105MAIN ";
                        win.Show();
                        this.Close();
                    }
                }
                else if (customCharge.IsChecked == false)
                {
                    _batteryFeature.SetState((BatteryState)0);
                    isOn = false;    
                }
            }
            
        }
        private void Refresh()
        {
            var features = new[]
            {
                
                new FeatureCheck(
                    () => _powerModeButtons[(int) _powerModeFeature.GetState()].IsChecked = true,
                    () => DisableControls(_powerModeButtons)),
                new FeatureCheck(
                    () => _batteryButtons[(int) _batteryFeature.GetState()].IsChecked = true,
                    () => DisableControls(_batteryButtons) ),
                new FeatureCheck(
                    () => _alwaysOnUsbButtons[(int) _alwaysOnUsbFeature.GetState()].IsChecked = true,
                    () => DisableControls(_alwaysOnUsbButtons)),
                new FeatureCheck(
                    () => chkOverDrive.IsChecked = _overDriveFeature.GetState() == OverDriveState.On,
                    () => chkOverDrive.IsEnabled = false),
                new FeatureCheck(
                    () => chkTouchpadLock.IsChecked = _touchpadLockFeature.GetState() == TouchpadLockState.On,
                    () => chkTouchpadLock.IsEnabled = false),
                new FeatureCheck(
                    () => chkFnLock.IsChecked = _fnLockFeature.GetState() == FnLockState.On,
                    () => chkFnLock.IsEnabled = false)
            };

            foreach (var feature in features)
            {
                try
                {
                    feature.Check();
                }
                catch (Exception e)
                {
                    Trace.TraceInformation("Could not refresh feature: " + e);
                    feature.Disable();
                }
            }
            

        }

        private void DisableControls(Control[] buttons)
        {
            foreach (var btn in buttons)
                btn.IsEnabled = false;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void radioPowerMode_Checked(object sender, RoutedEventArgs e)
        {
            _powerModeFeature.SetState((PowerModeState) Array.IndexOf(_powerModeButtons, sender));
            
        }

        private void radioBattery_Checked(object sender, RoutedEventArgs e)
        {
            _batteryFeature.SetState((BatteryState) Array.IndexOf(_batteryButtons, sender));
            
            
        }

        private void radioAlwaysOnUsb_Checked(object sender, RoutedEventArgs e)
        {
            _alwaysOnUsbFeature.SetState((AlwaysOnUsbState) Array.IndexOf(_alwaysOnUsbButtons, sender));
        }

        private void chkOverDrive_Checked(object sender, RoutedEventArgs e)
        {
            var state = chkOverDrive.IsChecked.GetValueOrDefault(false)
                ? OverDriveState.On
                : OverDriveState.Off;
            _overDriveFeature.SetState(state);
        }

        private void chkTouchpadLock_Checked(object sender, RoutedEventArgs e)
        {
            var state = chkTouchpadLock.IsChecked.GetValueOrDefault(false)
                ? TouchpadLockState.On
                : TouchpadLockState.Off;
            _touchpadLockFeature.SetState(state);
        }

        private void chkFnLock_Checked(object sender, RoutedEventArgs e)
        {
            var state = chkFnLock.IsChecked.GetValueOrDefault(false)
                ? FnLockState.On
                : FnLockState.Off;
            _fnLockFeature.SetState(state);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            batVal.minimumVal = (float)Minimum_Value.Value;
           
            try
            {
                Debug.WriteLine(batVal.batteryOnOff());
                float val3 = batVal.minimumVal * 100;
                minVal.Content = (int)val3;
                checkboolval.Content = (batVal.batteryOnOff());
                
            }
            catch
            {
                Debug.WriteLine("nothing");
            }

            
        }

        private void Maximum_Value_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            batVal.maximumVal = (float)Maximum_Value.Value;
            try
            {
                float val2 = batVal.maximumVal * 100;
                maxVal.Content = (int)val2 ;
                checkboolval.Content = (batVal.batteryOnOff());
                if (Maximum_Value.Value < Minimum_Value.Value + 0.04)
                {
                    MessageBox.Show("Maximum charge level cannot be below minimum charge value +5%"+ "\n" + "\n" + "Make sure maximum charge value is 5% higher than minimum charge value", "Error");
                    Maximum_Value.Value = Minimum_Value.Value + 0.05;
                }
            }
            catch
            {
                Debug.WriteLine("error");
            }

            
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
        }

        private void checkBat_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            if (alwaysCharge.IsChecked == true)
            {

            }
            else
            {
                
            }
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/human-sketch/Lenovo-Controller-Custom-Charge#how-to-use");
        }
    }
}