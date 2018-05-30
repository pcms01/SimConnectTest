﻿using SimLib;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Forms;
using System.Threading.Tasks;
using Quobject.SocketIoClientDotNet.Client;

namespace PilotClient
{   

    public partial class connectedExampleFrm : SimConnectForm
    {
        private Socket WebSocket;

        private string OAuthToken
        { get; set; }

        // Response number 
        int response = 1;

        // Output text - display a maximum of 10 lines 
        string output = "\n\n\n\n\n\n\n\n\n\n";

        public connectedExampleFrm()
        {
            InitializeComponent();

            OAuthToken = null;
        }

        void displayText(string s)
        {
            // remove first string from output 
            output = output.Substring(output.IndexOf("\n") + 1);

            // add the new string 
            output += "\n" + response++ + ": " + s;

            // display it 
            txtLog.Text = output;
        }

        private void connectedExampleFrm_SimConnectOpen(object sender, EventArgs e)
        {
            // sim opened, send user to login form
            Process.Start("http://37.59.115.154/html/login.html");
        }

        /// <summary>
        /// Validates a given ASSR code on the Auth API token endpoint, populates OAuthToken when a valid squawk code is set
        /// </summary>
        /// <param name="ASSR"></param>
        private async void ValidateASSR(string ASSR)
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("https://fa-authapi.herokuapp.com");

            HttpResponseMessage response = await client.GetAsync("/token/" + ASSR);


            if ((int)response.StatusCode == 200)
            {
                // logged in

                // this is the secret to send on the next API requests
                OAuthToken = response.Content.ReadAsStringAsync().Result;

                WebSocket = IO.Socket("https://fa-live.herokuapp.com/");
                WebSocket.Open();
            }
            else
            {
                OAuthToken = null; // not sure
            }
        }

        private void connectedExampleFrm_SimConnectClosed(object sender, EventArgs e)
        {
            displayText("Disconnected from simulator");
        }

        private async void connectedExampleFrm_SimConnectTransponderChanged(object sender, TransponderChangedEventArgs e)
        {
            if (OAuthToken == null)
            {
                // wait for the user to set on a code
                await Task.Delay(2500);

                if (LastRadios.Transponder == e.Transponder)
                    // validate new squawk codes on the API
                    ValidateASSR(e.Transponder.ToString("X").PadLeft(4, '0'));
            }
        }

        private void connectedExampleFrm_SimConnectPositionChanged(object sender, PositionChangedEventArgs e)
        {
            if (WebSocket != null)
                WebSocket.Emit("position", e);
        }
    }
}