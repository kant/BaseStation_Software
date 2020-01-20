﻿using Caliburn.Micro;
using Core.Interfaces;
using Core.Models;
using Core.RoveProtocol;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using RoverAttachmentManager.Models.Science;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RoverAttachmentManager.ViewModels.Science
{
    public class ScienceViewModel : PropertyChangedBase, IInputMode
    {

        private readonly IRovecomm _rovecomm;
        private readonly IDataIdResolver _idResolver;
        private readonly ILogger _log;

        private const int ScrewSpeedScale = 1000;
        private const int XYSpeedScale = 1000;
        private bool screwIncrementPressed = false;

        public string Name { get; }
        public string ModeType { get; }

        private readonly ScienceModel _model;   
 
        private DateTime GetTimeDiff()
        {
            TimeSpan nowSpan = DateTime.UtcNow.Subtract(ScienceGraph.StartTime);
            return new DateTime(nowSpan.Ticks);
        }


        public ScienceGraphViewModel ScienceGraph
        {
            get
            {
                return _model._scienceGraph;
            }
            set
            {
                _model._scienceGraph = value;
                NotifyOfPropertyChange(() => ScienceGraph);
            }
        }

        public SiteManagmentViewModel SiteManagment
        {
            get
            {
                return _model._siteManagment;
            }
            set
            {
                _model._siteManagment = value;
                NotifyOfPropertyChange(() => SiteManagment);
            }
        }

        public ScienceActuationViewModel ScienceActuation
        {
            get
            {
                return _model._scienceActuation;
            }
            set
            {
                _model._scienceActuation = value;
                NotifyOfPropertyChange(() => ScienceActuation);
            }
        }
        public SpectrometerViewModel Spectrometer
        {
            get
            {
                return _model._spectrometer;
            }
            set
            {
                _model._spectrometer = value;
                NotifyOfPropertyChange(() => Spectrometer);
            }
        }
        public ScienceSensorsViewModel ScienceSensors
        {
            get
            {
                return _model._scienceSensors;
            }
            set
            {
                _model._scienceSensors = value;
                NotifyOfPropertyChange(() => ScienceSensors);
            }
        }
        public string SpectrometerFilePath
        {
            get
            {
                return _model.SpectrometerFilePath;
            }
            set
            {
                _model.SpectrometerFilePath = value;
                NotifyOfPropertyChange(() => SpectrometerFilePath);
            }
        }


        public ScienceViewModel(IRovecomm networkMessenger, IDataIdResolver idResolver, ILogger log)
        {
            _model = new ScienceModel();
            SiteManagment = new SiteManagmentViewModel(networkMessenger, idResolver, log, this);
            ScienceGraph = new ScienceGraphViewModel(networkMessenger, idResolver, log);
            ScienceActuation = new ScienceActuationViewModel(networkMessenger, idResolver, log);
            Spectrometer = new SpectrometerViewModel(networkMessenger, idResolver, log, this);
            ScienceSensors = new ScienceSensorsViewModel(networkMessenger, idResolver, log, this);

            _rovecomm = networkMessenger;
            _idResolver = idResolver;
            _log = log;

            Name = "Science Controls";
            ModeType = "ScienceControls";
        }


        public void SetUVLed(byte val)
        {
            _rovecomm.SendCommand(new Packet("UVLedControl", val));
        }


        public void ReachedSite()
        {
            double siteTime = OxyPlot.Axes.DateTimeAxis.ToDouble(GetTimeDiff());
            ScienceGraph.SiteTimes[SiteManagment.SiteNumber * 2] = siteTime;
        }

        private async void WriteSiteData(double temp, double humidity, double methane)
        {
            FileStream file = new FileStream(SpectrometerFilePath + "\\REDSensorData-Site" + SiteManagment.SiteNumber + ".csv", FileMode.Create);
            if (!file.CanWrite) return;

            var data = Encoding.UTF8.GetBytes(String.Format("Temperature, {0}, Humidity, {1}, Methane, {2}{3}", temp, humidity, methane, Environment.NewLine));
            await file.WriteAsync(data, 0, data.Length);

            if (file.CanWrite)
            {
                file.Close();
            }
        }


        public void LeftSite()
        {
            double siteTime = OxyPlot.Axes.DateTimeAxis.ToDouble(GetTimeDiff());
            ScienceGraph.SiteTimes[(SiteManagment.SiteNumber * 2) + 1] = siteTime;

            double methaneAvg = ScienceGraph.AverageValueForSeries(ScienceGraph.Sensor4Series, "Methane vs Time", "Methane (parts per billion)", 2000, SpectrometerFilePath + "\\Methane-Site" + SiteManagment.SiteNumber + ".png");
            double tempAvg = ScienceGraph.AverageValueForSeries(ScienceGraph.Sensor0Series, "Temperature vs Time", "Temperature (Celsius)", 50, SpectrometerFilePath + "\\Temperature-Site" + SiteManagment.SiteNumber + ".png");
            double humidityAvg = ScienceGraph.AverageValueForSeries(ScienceGraph.Sensor1Series, "Humidity vs Time", "Humidity (%)", 100, SpectrometerFilePath + "\\Humidity-Site" + SiteManagment.SiteNumber + ".png");

            WriteSiteData(tempAvg, humidityAvg, methaneAvg);

            ScienceGraph.CreateSiteAnnotation();
            SiteManagment.SiteNumber++;
        }

        public void StartMode() {}

        public void SetValues(Dictionary<string, float> values)
        {

            if ((values["ScrewPosUp"] == 1 || values["ScrewPosDown"] == 1) && !screwIncrementPressed)
            {
                byte screwPosIncrement = (byte)(values["ScrewPosUp"] == 1 ? 1 : values["ScrewPosDown"] == 1 ? -1 : 0);
                _rovecomm.SendCommand(new Packet("ScrewRelativeSetPosition", screwPosIncrement));
                screwIncrementPressed = true;
            }
            else if (values["ScrewPosUp"] == 0 && values["ScrewPosDown"] == 0)
            {
                screwIncrementPressed = false;
            }

            Int16[] screwValue = { (Int16)(values["Screw"] * ScrewSpeedScale) }; //order before we reverse
            byte[] data = new byte[screwValue.Length * sizeof(Int16)];
            Buffer.BlockCopy(screwValue, 0, data, 0, data.Length);
            Array.Reverse(data);
            _rovecomm.SendCommand(new Packet("Screw", data, 1, (byte)DataTypes.INT16_T));

            Int16 xMovement = (Int16)(values["XActuation"] * XYSpeedScale);
            Int16 yMovement = (Int16)(values["YActuation"] * XYSpeedScale);

            Int16[] sendValues = { yMovement, xMovement }; //order before we reverse
            data = new byte[sendValues.Length * sizeof(Int16)];
            Buffer.BlockCopy(sendValues, 0, data, 0, data.Length);
            Array.Reverse(data);
            _rovecomm.SendCommand(new Packet("XYActuation", data, 2, (byte)DataTypes.INT16_T));

        }


        public void StopMode()
        {
            _rovecomm.SendCommand(new Packet("Screw", (Int16)(0)), true);

            Int16[] sendValues = { 0, 0 };
            byte[] data = new byte[sendValues.Length * sizeof(Int16)];
            Buffer.BlockCopy(sendValues, 0, data, 0, data.Length);
            Array.Reverse(data);
            _rovecomm.SendCommand(new Packet("XYActuation", data, 2, (byte)DataTypes.INT16_T));
        }
    }
}
