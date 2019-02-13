using Bacnet.Room.Simulator;
using LagoVista.BACNet.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace RoomSimulator.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        BacnetObjectId Bac_TempInt = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 0);
        BacnetObjectId Bac_TempWater = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 1);
        BacnetObjectId Bac_TempOutDoors = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 2);

        BacnetObjectId Bac_SetTemp = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 0);

        BacnetObjectId Bac_Mode = new BacnetObjectId(BacnetObjectTypes.OBJECT_MULTI_STATE_VALUE, 0);
        BacnetObjectId Bac_LevelLow = new BacnetObjectId(BacnetObjectTypes.OBJECT_MULTI_STATE_VALUE, 1);

        BacnetObjectId Bac_CmdHeating = new BacnetObjectId(BacnetObjectTypes.OBJECT_BINARY_VALUE, 0);
        BacnetObjectId Bac_CmdCool = new BacnetObjectId(BacnetObjectTypes.OBJECT_BINARY_VALUE, 1);

        RoomModel Room = new RoomModel(21);

        DispatcherTimer _timer = new DispatcherTimer();

        public MainViewModel()
        {
            SetPointTappedCommand = new RelayCommand(SetPointTapped);
        }

        public void SetPointTapped(object value)
        {
            SetTemp += 1;

            var b = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, SetTemp);
            var bv = BacnetActivity.GetBacObjectPresentValue(b);
            BacnetActivity.SetBacObjectPresentValue(Bac_SetTemp, bv);
        }


        public override Task InitAsync()
        {
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            return base.InitAsync();
        }

        private void _timer_Tick(object sender, object e)
        {
            // Si consigne_Effective OutofService alors l'écriture de Present_Value à lieu via Bacnet
            // sinon on remet à jour ici la valeur choisie 'au clavier' par l'utilisateur
            IList<BacnetValue> val = null;
            BacnetActivity.Storage.ReadProperty(Bac_SetTemp, BacnetPropertyIds.PROP_OUT_OF_SERVICE, 1, out val);
            RemoteSetPoint = (bool)val[0].Value;

            // Copie de la valeur utilisateur
            if (RemoteSetPoint == false)
            {
                var b = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, (uint)(SetPointIndex + 1));
                var bv = BacnetActivity.GetBacObjectPresentValue(b);
                BacnetActivity.SetBacObjectPresentValue(Bac_SetTemp, bv);
            }
         }

        private void AdaptationFarenheit()
        {
            BacnetObjectId b;
            BacnetValue bv;
           
            for (int i = 0; i < 4; i++)
            {
                b = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, (uint)(i));
                bv = BacnetActivity.GetBacObjectPresentValue(b);

                BacnetActivity.SetBacObjectPresentValue(b, new BacnetValue((float)Math.Round(TempDegre2Value((float)bv.Value))));

                IList<BacnetValue> val = new BacnetValue[1] { new BacnetValue(64) };
                BacnetActivity.Storage.WriteProperty(b, BacnetPropertyIds.PROP_UNITS, 1, val, true);
            }

            for (int i = 0; i < 3; i++)
            {
                b = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, (uint)(i));
                IList<BacnetValue> val = new BacnetValue[1] { new BacnetValue(64) };
                BacnetActivity.Storage.WriteProperty(b, BacnetPropertyIds.PROP_UNITS, 1, val, true);
            }
        }

        

        private float Truncate(double v)
        {
            return (float)(Math.Truncate(v * 10) / 10);
        }

        string Culture = "en-US";

        private string TempDegre2Text(double C)
        {
            if (Culture == "en-US")
                return Truncate(C * 1.8 + 32).ToString() + "°F";
            else
                return Truncate(C).ToString() + "°C";
        }

        private float TempDegre2Value(double C)
        {
            if (Culture == "en-US")
                return Truncate(C * 1.8 + 32);
            else
                return Truncate(C);
        }

        private double Temp2Degree(double C)
        {
            if (Culture == "en-US")
                return (C - 32) / 1.8;
            else
                return C;
        }

        private double _measuredTemp;
        public double MeasuredTemp
        {
            get { return _measuredTemp; }
            set { Set(ref _measuredTemp, value); }
        }

        private uint _setPointTemp;
        public uint SetTemp
        {
            get { return _setPointTemp; }
            set { Set(ref _setPointTemp, value); }
        }

        private uint _setPointIndex = 0;
        public uint SetPointIndex
        {
            get { return _setPointIndex; }
            set { Set(ref _setPointIndex, value); }
        }


        private bool _remoteSetPoint;
        public bool RemoteSetPoint
        {
            get { return _remoteSetPoint; }
            set { Set(ref _remoteSetPoint, value); }
        }

        public uint DeviceId
        {
            get { return BacnetActivity.DeviceId; }
        }

        public RelayCommand SetPointTappedCommand { get; private set; }
    }
}
