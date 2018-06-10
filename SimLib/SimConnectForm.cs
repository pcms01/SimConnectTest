﻿using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimLib
{
    /// <summary>
    /// Extends Windows.Forms to provide a wrapped access to SimConnect
    /// </summary>
    public partial class SimConnectForm : Form
    {
        /// <summary>
        /// The user defined win32 event
        /// </summary>
        private const int WM_USER_SIMCONNECT = 0x0402;

        enum DATA_REQUESTS
        {
            Radios,
            Position,
            AiTraffic
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == WM_USER_SIMCONNECT)
            {
                if (SimConnectWrapper.Sim != null)
                {
                    SimConnectWrapper.Sim.ReceiveMessage();
                }
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }

        /// <summary>
        /// Asynchronous wait for a listening SimConnect server.
        /// 
        /// Listen to SimConnectOpen event
        /// </summary>
        public async void OpenSimConnect()
        {
            while (SimConnectWrapper.Sim == null)
            {
                try
                {
                    // the constructor is similar to SimConnect_Open in the native API 
                    SimConnectWrapper.Sim = new SimConnect("SimLib.SimLibSimConnect", Handle, WM_USER_SIMCONNECT, null, 0);

                    // Register data definitions
                    SimObjectType<Position>.Register(new SimObjectType<Position>.Field[]
                    {
                        new SimObjectType<Position>.Field()
                        { DatumName = "Title", UnitsName = null,
                            DatumType = SIMCONNECT_DATATYPE.STRING256 },
                        new SimObjectType<Position>.Field()
                        { DatumName = "PLANE LATITUDE", UnitsName = "degrees",
                            DatumType = SIMCONNECT_DATATYPE.FLOAT64 },
                        new SimObjectType<Position>.Field()
                        { DatumName = "PLANE LONGITUDE", UnitsName = "degrees",
                            DatumType = SIMCONNECT_DATATYPE.FLOAT64 },
                        new SimObjectType<Position>.Field()
                        { DatumName = "PLANE ALTITUDE", UnitsName = "feet",
                            DatumType = SIMCONNECT_DATATYPE.FLOAT64 },
                        new SimObjectType<Position>.Field()
                        { DatumName = "PLANE PITCH DEGREES", UnitsName = "degrees",
                            DatumType = SIMCONNECT_DATATYPE.FLOAT64 },
                        new SimObjectType<Position>.Field()
                        { DatumName = "PLANE BANK DEGREES", UnitsName = "degrees",
                            DatumType = SIMCONNECT_DATATYPE.FLOAT64 },
                        new SimObjectType<Position>.Field()
                        { DatumName = "PLANE HEADING DEGREES TRUE", UnitsName = "degrees",
                            DatumType = SIMCONNECT_DATATYPE.FLOAT64 },
                        new SimObjectType<Position>.Field()
                        { DatumName = "AIRSPEED TRUE", UnitsName = "knots",
                            DatumType = SIMCONNECT_DATATYPE.FLOAT64 },
                    });

                    SimObjectType<Radios>.Register(new SimObjectType<Radios>.Field[]
                    {
                        new SimObjectType<Radios>.Field()
                        { DatumName = "TRANSPONDER CODE:1", UnitsName = "BCO16",
                            DatumType = SIMCONNECT_DATATYPE.INT32 },
                    });

                    //RegisterEvents();
                }
                catch (COMException)
                {
                    await Task.Delay(5000);
                }
            }
        }

        /// <summary>
        /// Dispose the SimConnect object
        /// object.
        /// </summary>
        private void DisposeSimConnect()
        {
            if (SimConnectWrapper.Sim != null)
            {
                SimConnectWrapper.Sim.Dispose();
                SimConnectWrapper.Sim = null;
            }
        }

        /// <summary>
        /// Dispose the SimConnect object with form
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            DisposeSimConnect();

            base.Dispose(disposing);
        }

        private async void WatchSimConnect()
        {
            while (SimConnectWrapper.Sim != null)
            {
                try
                {

                }
                catch (COMException)
                {

                }
                finally
                {
                    await Task.Delay(SimConnectPoolCooldown);
                }
            }
        }

        private void RegisterDataDefinitions()
        {

        }
    }
}
