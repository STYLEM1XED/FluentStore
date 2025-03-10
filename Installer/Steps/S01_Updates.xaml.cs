﻿using Installer.Utils.TaskDialog;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using Windows.Data.Json;

namespace Installer.Steps
{
    /// <summary>
    /// Interaction logic for S01_Updates.xaml
    /// </summary>
    public partial class S01_Updates : System.Windows.Controls.Page
    {
        public S01_Updates()
        {
            InitializeComponent();
        }

        private async void Step_Loaded(object sender, RoutedEventArgs e)
        {
            App.InstallerWindow.SetBackButtonText();
            App.InstallerWindow.SetNextButtonText();
            App.InstallerWindow.SetNextButtonEnabled();
            App.InstallerWindow.SetBackButtonEnabled();
            App.InstallerWindow.SetCancelButtonEnabled();

            try
            {
                HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "FluentStoreInstaller");
                HttpResponseMessage response = await client.GetAsync("https://api.github.com/repos/yoshiask/FluentStore/releases/latest");
                if (!response.IsSuccessStatusCode)
                {
                    App.InstallerWindow.NextStep();
                    return;
                }

                JsonObject latest = JsonObject.Parse(await response.Content.ReadAsStringAsync());
                string tagName = latest["tag_name"].GetString();
                string htmlUrl = latest["html_url"].GetString();

                Regex rx = new(@"(?<major>\d+)\.(?<minor>\d+)(?:\.(?<build>\d+)(?:\.(?<revision>\d+))?)?");
                Match m = rx.Match(tagName);
                if (m == null || !m.Success)
                {
                    App.InstallerWindow.NextStep();
                    return;
                }
                int major = int.Parse(m.Groups["major"].Value);
                int minor = int.Parse(m.Groups["minor"].Value);
                bool hasBuild = int.TryParse(m.Groups["build"].Value, out int build);
                bool hasRevision = int.TryParse(m.Groups["revision"].Value, out int revision);
                Version latestVersion = new(
                    major,
                    minor,
                    hasBuild ? build : 0,
                    hasRevision ? revision : 0);

                // Compare latest version to current version
                if (latestVersion > App.Version)
                {
                    // Prompt user to exit and download latest
                    TaskDialogOptions config = new();

                    config.Owner = App.InstallerWindow;
                    config.Title = "Fluent Store Installer";
                    config.MainInstruction = "A newer version of Fluent Store is available";
                    config.Content = "Would you like to exit the setup and open the download page?";
                    config.CommonButtons = TaskDialogCommonButtons.YesNoCancel;
                    config.MainIcon = VistaTaskDialogIcon.Information;

                    TaskDialogResult res = TaskDialog.Show(config);
                    switch (res.Result)
                    {
                        case TaskDialogSimpleResult.Yes:
                            Process.Start(htmlUrl);
                            App.InstallerWindow.Cancel(false);
                            return;

                        case TaskDialogSimpleResult.Cancel:
                            App.InstallerWindow.Cancel();
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Failed to check for updates
#if DEBUG
                Debug.WriteLine(ex);
#endif
            }

            App.InstallerWindow.NextStep();
        }
    }
}
