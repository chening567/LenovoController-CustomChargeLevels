using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Eco.FrameworkImpl.Ocl;
using System.Windows.Threading;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace LenovoController
{
    class Class1
    {
        public float minimumVal;
        public float maximumVal;
        int returning;
        public bool charge;
        public bool chargeWhenPluggedIn; 
        
        public int batteryOnOff()
        {


            bool chargeWhenAvaliable = false;
            PowerLineStatus status = SystemInformation.PowerStatus.PowerLineStatus; 
            PowerStatus pwr = SystemInformation.PowerStatus;
            float batteryLife = ((float)pwr.BatteryLifePercent);
            Debug.WriteLine(batteryLife);
            Debug.WriteLine(maximumVal);

            if (batteryLife < minimumVal)
            {
                returning = 1;
                return returning;
            }
            else if (batteryLife > maximumVal)
            {
                returning = 0;
                return returning;
            }
            else if (batteryLife > minimumVal && maximumVal > batteryLife && batteryLife > minimumVal && charge == true)
            {
                returning = 1;
                return returning;
            }
            /*
            else if (status == PowerLineStatus.Offline && chargeWhenPluggedIn == true)
            {
                chargeWhenAvaliable = true;
                return 0;
            }
            else if (chargeWhenAvaliable == true && status == PowerLineStatus.Online && chargeWhenPluggedIn == true)
            {
                charge = true;
                returning = 3;
                return returning;
            }
            else if (chargeWhenAvaliable == false && chargeWhenPluggedIn == true)
            {
                charge = false;
                returning = 3;
                return returning;
            }
            */
            else
            {
                returning = 3;
                return returning;

            }
            /*
            if (maximumVal> batteryLife && batteryLife > minimumVal)
           {

               returning = false;
               return returning;
           }
           else if (batteryLife < minimumVal)
           {
               returning = false;
               return returning;
           }
           else
           {
               return true;
           }
           */

        }
       
        private void InitializeComponent()
        {
            throw new NotImplementedException();
        }
    }
}
