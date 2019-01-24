﻿/*
   Copyright 2016-2018 LIPtoH <liptoh.codebase@gmail.com>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using System.Windows;
using ICSharpCode.SharpZipLib.GZip;
using Microsoft.Win32;
using TS_SE_Tool.CustomClasses;

namespace TS_SE_Tool
{
    public partial class FormMain
    {
        private void ShowStatusMessages(string _status, string _message)
        {

            toolStripStatusMessages.Text = GetranslatedString(_message);
            if (_status == "e")
            {
                toolStripStatusMessages.ForeColor = Color.Red;
            }
            if (_status == "i")
            {
                toolStripStatusMessages.ForeColor = Color.Black;
            }
        }

        private void ShowStatusMessages(string _status, string _message, string _option)
        {

            toolStripStatusMessages.Text = GetranslatedString(_message) + " (" + _option + ")";
            if (_status == "e")
            {
                toolStripStatusMessages.ForeColor = Color.Red;
            }
            if (_status == "i")
            {
                toolStripStatusMessages.ForeColor = Color.Black;
            }
        }

        public void SetDefaultValues(bool _initial)
        {
            if (_initial)
            {
                ProgSettingsV = new ProgSettings(0.1, "Default", false, 72, 0, 1.0, "km");

                ProgSettingsV.ProgramVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductMajorPart +
                    (FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductMinorPart / 10.0);

                SavefileVersion = 0;
                SupportedSavefileVersionETS2 = 39; //Supported save version
                SupportedGameVersionETS2 = "1.33.x"; //Last game version Tested on
                //SupportedSavefileVersionATS;
                SupportedGameVersionATS = "1.33.x"; //Last game version Tested on

                ProfileETS2 = @"\Euro Truck Simulator 2";
                ProfileATS = @"\American Truck Simulator";

                comboBoxPrevProfiles.FlatStyle =
                comboBoxProfiles.FlatStyle =
                comboBoxSaves.FlatStyle = FlatStyle.Flat;

                dictionaryProfiles = new Dictionary<string, string>();
                dictionaryProfiles.Add("ETS2", ProfileETS2);
                dictionaryProfiles.Add("ATS", ProfileATS);

                CompaniesLngDict = new Dictionary<string, string>();
                CitiesLngDict = new Dictionary<string, string>();
                CargoLngDict = new Dictionary<string, string>();
                TruckBrandsLngDict = new Dictionary<string, string>();

                Globals.CurrentGame = dictionaryProfiles["ETS2"];
                GameType = "ETS";

                DBconnection = new System.Data.SqlServerCe.SqlCeConnection("Data Source = Database.sdf");
                CreateDatabase();

                DistancesTable = new DataTable();
                DistancesTable.Columns.Add("SourceCity", typeof(string));
                DistancesTable.Columns.Add("SourceCompany", typeof(string));
                DistancesTable.Columns.Add("DestinationCity", typeof(string));
                DistancesTable.Columns.Add("DestinationCompany", typeof(string));
                DistancesTable.Columns.Add("Distance", typeof(int));
                DistancesTable.Columns.Add("FerryTime", typeof(int));
                DistancesTable.Columns.Add("FerryPrice", typeof(int));

                //comboBoxUrgency
                DataTable combDT = new DataTable();
                combDT.Columns.Add("ID");
                combDT.Columns.Add("UrgencyDisplayName");

                combDT.Rows.Add(new object[] { 0, "Standard" });
                combDT.Rows.Add(new object[] { 1, "Important" });
                combDT.Rows.Add(new object[] { 2, "Urgent" });

                comboBoxFreightMarketUrgency.ValueMember = "ID";
                comboBoxFreightMarketUrgency.DisplayMember = "UrgencyDisplayName";
                comboBoxFreightMarketUrgency.DataSource = combDT;
                comboBoxFreightMarketUrgency.SelectedIndex = -1;

                CountryDictionary = new CountryDictionary();

                PlayerLevelNames = new List<LevelNames>();

                LevelNames lvl_name0 = new LevelNames(0, "Newbie", "FFE0E0E0");
                LevelNames lvl_name1 = new LevelNames(5, "Enthusiast", "FF45C294");
                LevelNames lvl_name2 = new LevelNames(10, "Workhorse", "FF75BAEA");
                LevelNames lvl_name3 = new LevelNames(15, "Entrepeneur", "FF3A88F4");
                LevelNames lvl_name4 = new LevelNames(20, "Master", "FF5847F0");
                LevelNames lvl_name5 = new LevelNames(25, "Instructor", "FFDA9356");
                LevelNames lvl_name6 = new LevelNames(30, "Elite", "FFF58493");
                LevelNames lvl_name7 = new LevelNames(40, "King of the Road", "FFC99EF2");
                LevelNames lvl_name8 = new LevelNames(50, "Legend", "FFC2F9FF");
                LevelNames lvl_name9 = new LevelNames(100, "Divine Champion", "FFF1DEA5");

                PlayerLevelNames.Add(lvl_name0);
                PlayerLevelNames.Add(lvl_name1);
                PlayerLevelNames.Add(lvl_name2);
                PlayerLevelNames.Add(lvl_name3);
                PlayerLevelNames.Add(lvl_name4);
                PlayerLevelNames.Add(lvl_name5);
                PlayerLevelNames.Add(lvl_name6);
                PlayerLevelNames.Add(lvl_name7);
                PlayerLevelNames.Add(lvl_name8);
                PlayerLevelNames.Add(lvl_name9);

                DistanceMultipliers = new Dictionary<string, double>();
                DistanceMultipliers.Add("km", 1);
                DistanceMultipliers.Add("mi", km_to_mileconvert);

                ADRImgS = new Image[6];
                ADRImgSGrey = new Image[6];
                SkillImgSBG = new Image[5];
                SkillImgS = new Image[6];
                ADRbuttonArray = new CheckBox[6];
                GaragesImg = new Image[4];
                CitiesImg = new Image[2];
                UrgencyImg = new Image[3];
                CargoTypeImg = new Image[3];
                CargoType2Img = new Image[3];
                GameIconeImg = new Image[2];
                TruckPartsImg = new Image[5];
                TrailerPartsImg = new Image[4];
                ProgUIImgs = new Image[0];

                SkillButtonArray = new CheckBox[5, 6];

                TabpagesImages = new ImageList();

                buttonMainGameSwitchETS.Image = GameIconeImg[0];
                buttonMainGameSwitchATS.Image = GameIconeImg[1];
            }

            FileDecoded = false;
            SavefilePath = "";

            tempInfoFileInMemory = null;
            tempSavefileInMemory = null;
            tempProfileFileInMemory = null;

            //string ATSexp = "";
            
            if(GameType == "ETS")
                Globals.PlayerLevelUps = new int[] {200, 500, 700, 900, 1000, 1100, 1300, 1600, 1700, 1900, 2100, 2300, 2600, 2700,
                    2900, 3000, 3100, 3400, 3700, 4000, 4300, 4600, 4700, 4900, 5200, 5700, 5900, 6000, 6200, 6600, 6900};
            else
                Globals.PlayerLevelUps = new int[] {200, 500, 700, 900, 1100, 1300, 1500, 1700, 1900, 2100, 2300, 2500, 2700,
                    2900, 3100, 3300, 3500, 3700, 4000, 4300, 4600, 4900, 5200, 5500, 5800, 6100, 6400, 6700, 7000, 7300};

            PlayerProfileData = new PlayerProfile("", 0, new byte[] { 0, 0, 0, 0, 0, 0 }, 0);

            UserCompanyAssignedTruckPlacementEdited = false;

            CompaniesList = new List<string>();
            CitiesList = new List<City>();

            CountriesList = new List<string>();
            CargoesList = new List<Cargo>();
            HeavyCargoList = new List<string>();
            CompanyTruckList = new List<CompanyTruck>();
            CompanyTruckListDB = new List<CompanyTruck>();
            CompanyTruckListDiff = new List<CompanyTruck>();

            UserColorsList = new List<Color>();
            GaragesList = new List<Garages>();
            UserTruckDictionary = new Dictionary<string, UserCompanyTruckData>();
            UserTrailerDictionary = new Dictionary<string, UserCompanyTruckData>();

            VisitedCities = new List<VisitedCity>();

            CargoesListDB = new List<Cargo>();
            CitiesListDB = new List<string>();
            CompaniesListDB = new List<string>();
            CargoesListDiff = new List<Cargo>();
            CitiesListDiff = new List<string>();
            CompaniesListDiff = new List<string>();

            ExternalCompanies = new List<ExtCompany>();

            ExtCargoList = new List<ExtCargo>();

            EconomyEventQueueList = new string[0];
            EconomyEventsTable = new string[0, 0];
            EconomyEventUnitLinkStringList = new string[0];

            JobsAmountAdded = 0;
            LastVisitedCity = "";
            InGameTime = 0;
            RandomValue = new Random();
            CitiesListAddedToCompare = new string[1];

            JobsListAdded = new string[0];
            LastModifiedTimestamp = new DateTime();
            ListSavefileCompanysString = new string[0];

            GPSbehind = new Dictionary<string, List<string>>();
            GPSahead = new Dictionary<string, List<string>>();

            GPSbehindOnline = new Dictionary<string, List<string>>();
            GPSaheadOnline = new Dictionary<string, List<string>>();

            namelessList = new List<string>();
            namelessLast = "";
            //game = "ETS";
            JobsTotalDistance = 0;
            LoopStartCity = "";
            LoopStartCompany = "";
            ProgPrevVersion = 0f;

            RouteList = new Routes();
            DistancesTable.Clear();

            components = null;
        }


        private void PopulateFormControlsk()
        {
            buttonMainDecryptSave.Enabled = false;
            buttonMainWriteSave.Enabled = true;

            string t1 = "Trucking since:\n\r" + DateTimeOffset.FromUnixTimeSeconds(PlayerProfileData.CreationTime).DateTime.ToLocalTime().ToString();
            toolTipMain.SetToolTip(pictureBoxProfileAvatar, t1);

            //string temptest = GetSpareNameless();
            //ExportnamelessList();
            //ExportTestnamelessList();

            FillFormProfileControls();
            UpdateUserColorsButtons();
            FillFormCompanyControls();

            FillUserCompanyTrucksList();
            FillUserCompanyTrailerList();

            FillFormFreightMarketControls();

            FillFormCargoOffersControls();
        }

        //Main part controls

        private void buttonOpenSaveFolder_Click(object sender, EventArgs e)
        {
            Process.Start(Globals.SavesHex[comboBoxSaves.SelectedIndex]);
        }

        private void buttonGameCustomPath_Click(object sender, EventArgs e)
        {

        }

        private void AddCustomFolder_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Save files (game.sii)|game.sii";
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            DialogResult result = openFileDialog1.ShowDialog();

            comboBoxProfiles.Items.Add("Custom");

            string DirectoryPath = Path.GetDirectoryName(openFileDialog1.FileName);

            string DirectoryName = openFileDialog1.FileName.Substring((DirectoryPath.LastIndexOf('\\') + 1), (DirectoryPath.Length - (DirectoryPath.LastIndexOf('\\')) - 1));

            comboBoxSaves.Items.Add(DirectoryName);

            Globals.SavesHex[0] = DirectoryName;
        }

        private void buttonDecryptSave_Click(object sender, EventArgs e)
        {
            //SavefilePath = Globals.SavesHex[comboBoxSaves.SelectedIndex];
            SetDefaultValues(false);

            buttonMainDecryptSave.Enabled = false;
            buttonMainLoadSave.Enabled = false;
            buttonMainGameSwitchETS.Enabled = false;
            buttonMainGameSwitchATS.Enabled = false;

            SavefilePath = Globals.SavesHex[comboBoxSaves.SelectedIndex];
            string SiiSavePath = SavefilePath + @"\game.sii";
            DecodeFile(SiiSavePath);

            buttonMainDecryptSave.Enabled = false;
            buttonMainLoadSave.Enabled = true;

            ToggleGame(GameType);

            //GC
            GC.Collect();
            //GC.WaitForPendingFinalizers();
        }

        private void LoadSaveFile_Click(object sender, EventArgs e)
        {
            //SetDefaultValues(false);

            buttonMainDecryptSave.Enabled = false;
            buttonMainLoadSave.Enabled = false;
            buttonMainWriteSave.Enabled = false;

            buttonMainGameSwitchETS.Enabled = false;
            buttonMainGameSwitchATS.Enabled = false;

            LoadSaveFile(); //Load save file

            //GC
            GC.Collect();
            //GC.WaitForPendingFinalizers();
        }

        //Profile and Saves groupbox
        private void checkBoxProfileBackups_CheckedChanged(object sender, EventArgs e)
        {
            FillAllProfilesPaths();
        }

        public void FillAllProfilesPaths()
        {
            string MyDocumentsPath = "";
            MyDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Globals.CurrentGame;

            string RemoteUserdataDirectory = "";

            try
            {
                //string SteamInstallPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null).ToString();
                //if (SteamInstallPath == null)
                string SteamInstallPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null).ToString();

                if (SteamInstallPath == null)
                {
                    //unknown steam path
                }
                else
                {

                    string SteamCloudPath = SteamInstallPath + @"\userdata";
                    if (!Directory.Exists(SteamCloudPath))
                    {
                        //no userdata
                    }
                    else
                    {
                        string[] userdatadirectories = Directory.GetDirectories(SteamCloudPath);

                        if (userdatadirectories.Length == 0)
                        {
                            //no steam user directories
                        }
                        else
                        {
                            DateTime lastHigh = DateTime.Now;

                            string CurrentUserDir = Directory.GetDirectories(SteamCloudPath).OrderByDescending(f => new FileInfo(f).LastWriteTime).ToArray()[0];//null;

                            string GameID = "";
                            if (GameType == "ETS")
                                GameID = @"\227300"; //ETS2
                            else
                                GameID = @"\270880"; //ATS

                            if (!Directory.Exists(MyDocumentsPath) && !Directory.Exists(CurrentUserDir + GameID))
                            {
                                MessageBox.Show("Standart Game Save folders don't exist");
                                return;
                            }

                            RemoteUserdataDirectory = CurrentUserDir + GameID + @"\remote";
                        }
                    }
                }
            }
            catch
            {

            }

            if (checkBoxProfilesAndSavesProfileBackups.Checked)
            {
                DataTable combDT = new DataTable();
                DataColumn dc = new DataColumn("ProfileID", typeof(string));
                combDT.Columns.Add(dc);

                dc = new DataColumn("ProfileName", typeof(string));
                combDT.Columns.Add(dc);

                List<string> tempList = new List<string>();

                int index = 0;
                foreach (string folder in Directory.GetDirectories(MyDocumentsPath))
                {
                    if (Path.GetFileName(folder).StartsWith("profiles")) //Documents
                    {
                        //comboBoxPrevProfiles.Items.Add(Path.GetFileName(folder));
                        combDT.Rows.Add(index, "[L] " + Path.GetFileName(folder));

                        tempList.Add(folder);
                        index++;
                    }
                }

                //string RemoteUserdataDirectory Steam Profiles
                foreach (string folder in Directory.GetDirectories(RemoteUserdataDirectory))
                {
                    if (Path.GetFileName(folder).StartsWith("profiles")) //Steam
                    {
                        combDT.Rows.Add(index, "[S] " + Path.GetFileName(folder));

                        tempList.Add(folder);
                        index++;
                    }
                }

                Globals.ProfilesPaths = tempList.ToArray();

                comboBoxPrevProfiles.ValueMember = "ProfileID";
                comboBoxPrevProfiles.DisplayMember = "ProfileName";
                comboBoxPrevProfiles.DataSource = combDT;

            }
            else
            {
                DataTable combDT = new DataTable();
                DataColumn dc = new DataColumn("ProfileID", typeof(string));
                combDT.Columns.Add(dc);

                dc = new DataColumn("ProfileName", typeof(string));
                combDT.Columns.Add(dc);

                int index = 0;
                List<string> tempList = new List<string>();
                string folder = MyDocumentsPath + @"\profiles";

                if (Directory.Exists(folder))
                {
                    combDT.Rows.Add(index, "[L] profiles");

                    tempList.Add(folder);
                    index++;
                }

                folder = RemoteUserdataDirectory + @"\profiles";

                if (Directory.Exists(folder))
                {
                    combDT.Rows.Add(index, "[S] profiles");

                    tempList.Add(folder);
                    index++;
                }

                Globals.ProfilesPaths = tempList.ToArray();
                comboBoxPrevProfiles.ValueMember = "ProfileID";
                comboBoxPrevProfiles.DisplayMember = "ProfileName";
                comboBoxPrevProfiles.DataSource = combDT;
            }


            if (comboBoxPrevProfiles.Items.Count > 0)
            {
                comboBoxPrevProfiles.SelectedIndex = 0;
                buttonProfilesAndSavesOpenSaveFolder.Enabled = true;
                buttonMainDecryptSave.Enabled = true;
                buttonMainLoadSave.Enabled = true;
            }
            else
            {
                MessageBox.Show("No profiles found");

                buttonProfilesAndSavesOpenSaveFolder.Enabled = false;
                buttonMainDecryptSave.Enabled = false;
                buttonMainLoadSave.Enabled = false;
            }
        }

        private void comboBoxPrevProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillProfiles();
        }

        public void FillProfiles()
        {
            string Profile = "";
            comboBoxProfiles.Items.Clear();

            string MyDocumentsPath = "";
            //MyDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Globals.CurrentGame + "\\" + comboBoxPrevProfiles.Items[comboBoxPrevProfiles.SelectedIndex];//@"\profiles";
            MyDocumentsPath = Globals.ProfilesPaths[int.Parse(comboBoxPrevProfiles.SelectedValue.ToString())];
            /*
            if (!Directory.Exists(MyDocumentsPath))
            {
                MessageBox.Show("Standart Save files don't exist");
                return;
            }
            */
            Globals.ProfilesHex = Directory.GetDirectories(MyDocumentsPath).OrderByDescending(f => new FileInfo(f).LastWriteTime).ToArray();

            if (Globals.ProfilesHex.Length > 0)
            {
                foreach (string profile in Globals.ProfilesHex)
                {
                    Profile = FromHexToString(Path.GetFileName(profile));
                    comboBoxProfiles.Items.Add(Profile);
                }
                comboBoxProfiles.SelectedIndex = 0;
                buttonProfilesAndSavesOpenSaveFolder.Enabled = true;
                buttonMainDecryptSave.Enabled = true;
                buttonMainLoadSave.Enabled = true;
            }
            else
            {
                MessageBox.Show("No profiles found");

                buttonProfilesAndSavesOpenSaveFolder.Enabled = false;
                buttonMainDecryptSave.Enabled = false;
                buttonMainLoadSave.Enabled = false;
            }
        }

        private void comboBoxProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillProfileSaves();

            try
            {
                string AvatarPath = Globals.ProfilesHex[comboBoxProfiles.SelectedIndex] + @"\avatar.png";

                Bitmap Source = new Bitmap(AvatarPath);
                Rectangle SourceRect = new Rectangle(0, 0, 95, 95);
                Bitmap Cropped = Source.Clone(SourceRect, Source.PixelFormat);

                pictureBoxProfileAvatar.Image = Cropped;
            }
            catch
            {
                string[] imgpaths = new string[] { @"img\unknown.dds" };
                pictureBoxProfileAvatar.Image = ExtImgLoader(imgpaths, 95, 95, 0, 0)[0];// new Bitmap(95, 95);
            }

            //DateTime CreationDate = (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(CreationTime); //trucking since
        }

        public void FillProfileSaves()
        {
            //comboBoxSaves.Items.Clear();
            string savePath = Globals.ProfilesHex[comboBoxProfiles.SelectedIndex] + @"\save";
            Globals.SavesHex = Directory.GetDirectories(savePath).OrderByDescending(f => new FileInfo(f).LastWriteTime).ToArray();

            if (Globals.SavesHex.Length > 0)
            {
                DataTable combDT = new DataTable();
                DataColumn dc = new DataColumn("savePath", typeof(string));
                combDT.Columns.Add(dc);

                dc = new DataColumn("saveName", typeof(string));
                combDT.Columns.Add(dc);

                bool NotANumber = false;

                foreach (string profile in Globals.SavesHex)
                {
                    string[] fold = profile.Split(new string[] { "\\" }, StringSplitOptions.None);

                    foreach (char c in fold[fold.Length - 1])
                    {
                        if (c < '0' || c > '9')
                        {
                            NotANumber = true;
                            break;
                        }
                    }

                    if (NotANumber)
                    {
                        string[] namearr = fold[fold.Length - 1].Split(new char[] { '_' });
                        string ProfileName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namearr[0]);

                        for (int i = 1; i < namearr.Length; i++)
                        {
                            ProfileName += " " + namearr[i];
                        }

                        combDT.Rows.Add(profile, "- " + ProfileName + " -");
                    }
                    else
                        combDT.Rows.Add(profile, GetCustomSaveFilename(profile));

                    NotANumber = false;
                }

                comboBoxSaves.ValueMember = "savePath";
                comboBoxSaves.DisplayMember = "saveName";

                comboBoxSaves.DataSource = combDT;

                comboBoxSaves.SelectedIndex = 0;
                buttonProfilesAndSavesOpenSaveFolder.Enabled = true;
                buttonMainDecryptSave.Enabled = true;
                buttonMainLoadSave.Enabled = true;

                ShowStatusMessages("i", "");
            }
            else
            {
                MessageBox.Show("No save file folders found");

                buttonProfilesAndSavesOpenSaveFolder.Enabled = false;
                buttonMainDecryptSave.Enabled = false;
                buttonMainLoadSave.Enabled = false;
            }
        }

        private void comboBoxSaves_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonMainDecryptSave.Enabled = true;
            buttonMainLoadSave.Enabled = true;
        }

        //end Profile and Saves groupbox
        //end Main part controls

        //Profile tab
        private void CreateProfilePanelControls()
        {
            int pSkillsNameHeight = 56, pSkillsNameWidth = 56, pSkillsNameOffset = 5, pSkillsNamelOffset = 12;

            string[] toolskillimgtooltip = new string[] { "ADR", "Long Distance", "High Value Cargo", "Fragile Cargo", "Just-In-Time Delivery", "Ecodriving" };

            for (int i = 0; i < 6; i++)
            {
                Panel Ppanel = new Panel();
                groupBoxProfileSkill.Controls.Add(Ppanel);

                Ppanel.Parent = groupBoxProfileSkill;
                Ppanel.Location = new Point(pSkillsNamelOffset, 11 + (pSkillsNameHeight + pSkillsNameOffset) * i);
                Ppanel.BorderStyle = BorderStyle.None;
                Ppanel.Size = new Size(pSkillsNameWidth, pSkillsNameHeight);
                Ppanel.Name = "profileSkillsPanel" + i.ToString();
                toolTipMain.SetToolTip(Ppanel, toolskillimgtooltip[i]);

                Bitmap bgimg = new Bitmap(SkillImgS[i], pSkillsNameHeight, pSkillsNameWidth);
                Ppanel.BackgroundImage = bgimg;

                Label slabel = new Label();
                groupBoxProfileSkill.Controls.Add(slabel);
                slabel.Name = "labelProfileSkillName" + i.ToString();
                slabel.Location = new Point(pSkillsNamelOffset * 2 + pSkillsNameWidth, 11 + (pSkillsNameHeight + pSkillsNameOffset) * i);
                slabel.Text = toolskillimgtooltip[i];
                slabel.AutoSize = true;
            }

            int bADRHeight = 40, bADRWidth = 40, pOffset = 6, lOffset = pSkillsNameWidth + pSkillsNamelOffset * 2;

            for (int i = 0; i < 6; i++)
            {
                CheckBox Ppanel = new CheckBox();
                groupBoxProfileSkill.Controls.Add(Ppanel);

                Ppanel.Parent = groupBoxProfileSkill;

                Ppanel.Location = new Point(lOffset + (bADRWidth + pOffset) * i, 11 + 14);
                Ppanel.Appearance = Appearance.Button;
                Ppanel.FlatStyle = FlatStyle.Flat;
                Ppanel.Size = new Size(bADRWidth, bADRHeight);
                Ppanel.Name = "buttonADR" + i.ToString();
                Ppanel.Checked = false;
                Ppanel.Padding = new Padding(0, 0, 1, 2);
                Ppanel.BackgroundImageLayout = ImageLayout.Stretch;


                Ppanel.BackgroundImage = SkillImgSBG[0];
                Ppanel.Image = ADRImgSGrey[i];
                Ppanel.FlatAppearance.BorderSize = 0;

                Ppanel.MouseEnter += new EventHandler(ADRbutton_MouseEnter);
                Ppanel.MouseLeave += new EventHandler(ADRbutton_MouseLeave);
                Ppanel.Click += new EventHandler(ADRbutton_Click);
                Ppanel.CheckedChanged += new EventHandler(ADRbutton_CheckedChanged);
                Ppanel.MouseHover += new EventHandler(ADRbutton_MouseHover);

                ADRbuttonArray[i] = Ppanel;
            }

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    CheckBox Ppanel = new CheckBox();
                    groupBoxProfileSkill.Controls.Add(Ppanel);

                    Ppanel.Parent = groupBoxProfileSkill;

                    Ppanel.Location = new Point(lOffset + (bADRWidth + pOffset) * j, 11 + 14 + (pSkillsNameHeight + pSkillsNameOffset) * (i + 1));
                    Ppanel.Appearance = Appearance.Button;
                    Ppanel.FlatStyle = FlatStyle.Flat;
                    Ppanel.Size = new Size(bADRWidth, bADRHeight);
                    Ppanel.Name = "buttonSkill" + i.ToString() + j.ToString();
                    Ppanel.Checked = false;
                    Ppanel.Padding = new Padding(0, 0, 1, 2);
                    Ppanel.BackgroundImageLayout = ImageLayout.Zoom;

                    Ppanel.BackgroundImage = SkillImgSBG[0];
                    Ppanel.FlatAppearance.BorderSize = 0;

                    Ppanel.MouseEnter += new EventHandler(Skillbutton_MouseEnter);
                    Ppanel.MouseLeave += new EventHandler(Skillbutton_MouseLeave);
                    Ppanel.Click += new EventHandler(Skillbutton_Click);
                    Ppanel.CheckedChanged += new EventHandler(Skillbutton_CheckedChanged);

                    SkillButtonArray[i, j] = Ppanel;
                }
            }

            CreateUserColorsButtons();
        }

        private void FillFormProfileControls()
        {
            FormUpdatePlayerLevel();

            char[] ADR = Convert.ToString(PlayerProfileData.PlayerSkills[0], 2).PadLeft(6, '0').ToCharArray();

            for (int i = 0; i < ADR.Length; i++)
            {
                if (ADR[i] == '1')
                    ADRbuttonArray[i].Checked = true;
            }

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < PlayerProfileData.PlayerSkills[i + 1]; j++)
                {
                    SkillButtonArray[i, j].Checked = true;
                }
            }
        }

        private void FormUpdatePlayerLevel()
        {
            int playerlvl = PlayerProfileData.getPlayerLvl()[0];
            labelPlayerLevelNumber.Text = playerlvl.ToString();

            for (int i = PlayerLevelNames.Count - 1; i >= 0; i--)
                if (PlayerLevelNames[i].LevelLimit <= playerlvl)
                {
                    labelPlayerLevelName.Text = PlayerLevelNames[i].LevelName;
                    panelPlayerLevel.BackColor = PlayerLevelNames[i].NameColor;
                    break;
                }

            labelPlayerExperience.Text = PlayerProfileData.ExperiencePoints.ToString();
            labelExperienceNxtLvlThreshhold.Text = "/   " + PlayerProfileData.getPlayerLvl()[1].ToString();

        }

        private void CreateUserColorsButtons()
        {
            int padding = 6, width = 40, colorcount = 8;
            //UserColorsList.Count

            for (int i = 0; i < colorcount; i++)
            {
                Button rb = new Button();
                rb.Name = "buttonUC" + i.ToString();
                rb.Text = null;
                rb.Location = new Point(15, 32 + (padding + width) * (i));
                rb.Size = new Size(width, width);
                rb.FlatStyle = FlatStyle.Flat;
                rb.Enabled = false;

                rb.Click += new EventHandler(SelectColor);

                groupBoxProfileUserColors.Controls.Add(rb);
            }
        }

        private void buttonProfileShareColors_Click(object sender, EventArgs e)
        {
            FormShareUserColors FormWindow = new FormShareUserColors();
            FormWindow.ShowDialog();
            UpdateUserColorsButtons();
        }

        private void UpdateUserColorsButtons()
        {
            int padding = 6, width = 23;//, colorcount = 8;

            for (int i = 0; i < UserColorsList.Count; i++)
            {
                Button btn = null;
                string btnname = "buttonUC" + i.ToString();

                if (groupBoxProfileUserColors.Controls.ContainsKey(btnname))
                {
                    btn = groupBoxProfileUserColors.Controls[btnname] as Button;
                }
                else
                {
                    //Button rb = new Button();
                    btn.Name = "buttonUC" + i.ToString();
                    btn.Text = null;
                    btn.Location = new Point(6 + (padding + width) * (i), 19);
                    btn.Size = new Size(width, 23);
                    btn.FlatStyle = FlatStyle.Flat;
                    //rb.BackColor = UserColorsList[i];
                    btn.Enabled = false;
                    //if (UserColorsList[i].A == 0)
                    //    rb.Text = "X";

                    btn.Click += new EventHandler(SelectColor);

                    groupBoxProfileUserColors.Controls.Add(btn);
                }

                if (btn != null)
                {
                    btn.BackColor = UserColorsList[i];
                    btn.Enabled = true;
                    if (UserColorsList[i].A == 0)
                        btn.Text = "X";
                }
            }
        }

        internal void SelectColor(object sender, EventArgs e)
        {
            Button obj = sender as Button;

            OpenPainter.ColorPicker.frmColorPicker frm = new OpenPainter.ColorPicker.frmColorPicker(obj.BackColor);

            frm.Font = SystemFonts.DialogFont;

            if (frm.ShowDialog() == DialogResult.OK)
            {
                obj.BackColor = frm.PrimaryColor;

                int index = int.Parse(obj.Name.Substring(8, 1));

                UserColorsList[index] = frm.PrimaryColor;

                if (frm.PrimaryColor.A != 0)
                    obj.Text = "";
                else
                    obj.Text = "X";

            }
        }

        //Profile buttons
        private void buttonPlayerLvlPlus01_Click(object sender, EventArgs e)
        {
            PlayerProfileData.getPlayerExp(int.Parse(labelPlayerLevelNumber.Text) + 1);

            FormUpdatePlayerLevel();
        }

        private void buttonPlayerLvlPlus10_Click(object sender, EventArgs e)
        {
            PlayerProfileData.getPlayerExp(int.Parse(labelPlayerLevelNumber.Text) + 10);

            FormUpdatePlayerLevel();
        }

        private void buttonPlayerLvlMax_Click(object sender, EventArgs e)
        {
            PlayerProfileData.getPlayerExp(150);

            FormUpdatePlayerLevel();
        }

        private void buttonPlayerLvlMinus01_Click(object sender, EventArgs e)
        {
            PlayerProfileData.getPlayerExp(int.Parse(labelPlayerLevelNumber.Text) - 1);

            FormUpdatePlayerLevel();
        }

        private void buttonPlayerLvlMinus10_Click(object sender, EventArgs e)
        {
            PlayerProfileData.getPlayerExp(int.Parse(labelPlayerLevelNumber.Text) - 10);

            FormUpdatePlayerLevel();
        }

        private void buttonPlayerLvlMin_Click(object sender, EventArgs e)
        {
            PlayerProfileData.getPlayerExp(0);

            FormUpdatePlayerLevel();
        }
        // end profile buttons
        
        //Skill buttons
        private void Skillbutton_Click(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            int skillIndex = int.Parse(thisbutton.Name.Substring(11, 1));
            byte buttonIndex = byte.Parse(thisbutton.Name.Substring(12, 1));
            if (thisbutton.Checked)
            {
                for (int j = 0; j < buttonIndex; j++)
                {
                    SkillButtonArray[skillIndex, j].Checked = true;
                }
                PlayerProfileData.PlayerSkills[++skillIndex] = ++buttonIndex;
            }
            else
            {
                for (int j = 5; j >= int.Parse(thisbutton.Name.Substring(12, 1)); j--)
                {
                    SkillButtonArray[skillIndex, j].Checked = false;
                }
                PlayerProfileData.PlayerSkills[++skillIndex] = buttonIndex;
            }
        }

        private void Skillbutton_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            if (thisbutton.Checked)
            {
                thisbutton.BackgroundImage = SkillImgSBG[3];
            }
            else
            {
                thisbutton.BackgroundImage = SkillImgSBG[0];
            }
        }

        private void Skillbutton_MouseEnter(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            int i = int.Parse(thisbutton.Name.Substring(11, 1));

            for (int j = 0; j <= int.Parse(thisbutton.Name.Substring(12, 1)); j++)
            {
                if (!SkillButtonArray[i, j].Checked)
                {
                    SkillButtonArray[i, j].BackgroundImage = SkillImgSBG[1];
                }

            }

            for (int j = int.Parse(thisbutton.Name.Substring(12, 1)); j < 6; j++)
            {
                if (SkillButtonArray[i, j].Checked)
                {
                    SkillButtonArray[i, j].BackgroundImage = SkillImgSBG[0];
                }

            }
            //thisbutton.BackgroundImage = SkillImgSBG[1];
        }

        private void Skillbutton_MouseLeave(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;
            int i = int.Parse(thisbutton.Name.Substring(11, 1));

            for (int j = 0; j <= int.Parse(thisbutton.Name.Substring(12, 1)); j++)
            {
                if (!SkillButtonArray[i, j].Checked)
                {
                    SkillButtonArray[i, j].BackgroundImage = SkillImgSBG[0];
                }
            }

            for (int j = int.Parse(thisbutton.Name.Substring(12, 1)); j < 6; j++)
            {
                if (SkillButtonArray[i, j].Checked)
                {
                    SkillButtonArray[i, j].BackgroundImage = SkillImgSBG[3];
                }

            }
        }

        private void ADRbutton_MouseEnter(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            thisbutton.BackgroundImage = SkillImgSBG[1];
        }

        private void ADRbutton_MouseLeave(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;
            if (thisbutton.Checked)
                thisbutton.BackgroundImage = SkillImgSBG[3];
            else
                thisbutton.BackgroundImage = SkillImgSBG[0];
        }

        private void ADRbutton_Click(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            if (thisbutton.Checked)
            {
                char[] ADR = Convert.ToString(PlayerProfileData.PlayerSkills[0], 2).PadLeft(6, '0').ToCharArray();
                ADR[byte.Parse(thisbutton.Name.Substring(9, 1))] = '1';

                PlayerProfileData.PlayerSkills[0] = Convert.ToByte(new string(ADR), 2);
                thisbutton.BackgroundImage = SkillImgSBG[1];
            }
            else
            {
                char[] ADR = Convert.ToString(PlayerProfileData.PlayerSkills[0], 2).PadLeft(6, '0').ToCharArray();
                ADR[byte.Parse(thisbutton.Name.Substring(9, 1))] = '0';
                string temp = new string(ADR);
                PlayerProfileData.PlayerSkills[0] = Convert.ToByte(temp.PadLeft(8, '0'), 2);
                thisbutton.BackgroundImage = SkillImgSBG[1];
            }
        }

        private void ADRbutton_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            if (thisbutton.Checked)
            {
                thisbutton.BackgroundImage = SkillImgSBG[3];
                thisbutton.Image = ADRImgS[int.Parse(thisbutton.Name.Substring(9))];
            }
            else
            {
                thisbutton.BackgroundImage = SkillImgSBG[0];
                thisbutton.Image = ADRImgSGrey[int.Parse(thisbutton.Name.Substring(9))];
            }
        }

        private void ADRbutton_MouseHover(object sender, EventArgs e)
        {
            CheckBox thisbutton = sender as CheckBox;

            if (thisbutton.Checked)
            {
                thisbutton.BackgroundImage = SkillImgSBG[1];
                thisbutton.Image = ADRImgS[int.Parse(thisbutton.Name.Substring(9))];
            }
            else
            {
                thisbutton.BackgroundImage = SkillImgSBG[1];
                thisbutton.Image = ADRImgSGrey[int.Parse(thisbutton.Name.Substring(9))];
            }
        }

        private void ClearProfilePage()
        {
            foreach (CheckBox temp in ADRbuttonArray)
                temp.Checked = false;

            foreach (CheckBox temp in SkillButtonArray)
                temp.Checked = false;
        }
        //end Skill buttons
        //end Profile tab

        //User Trucks tab
        private void CreateTruckPanelControls()
        {
            CreateTruckPanelProgressBars();
        }

        private void CreateTruckPanelProgressBars()
        {
            int pHeight = RepairImg.Height, pOffset = 5, lOffset = 60, pSizeW = 300;
            int pSkillsNameHeight = 32, pSkillsNameWidth = 32, pSkillsNameOffset = 5, pSkillsNamelOffset = 12;

            string[] toolskillimgtooltip = new string[] { "Engine", "Transmission", "Chassis", "Cabin", "Wheels" };
            Label slabel;
            Panel Ppanel;

            for (int i = 0; i < 5; i++)
            {
                slabel = new Label();
                groupBoxUserTruckTruckDetails.Controls.Add(slabel);
                slabel.Name = "labelTruckPartName" + toolskillimgtooltip[i];//.ToString();
                slabel.Location = new Point(pSkillsNamelOffset, 23 + (pSkillsNameHeight + pSkillsNameOffset * 3) * i);
                slabel.Text = toolskillimgtooltip[i];
                slabel.AutoSize = true;

                Panel imgpanel = new Panel();
                groupBoxUserTruckTruckDetails.Controls.Add(imgpanel);

                imgpanel.Parent = groupBoxUserTruckTruckDetails;
                imgpanel.Location = new Point(pSkillsNamelOffset, 23 + 14 + (pSkillsNameHeight + pSkillsNameOffset * 3) * i);
                imgpanel.BorderStyle = BorderStyle.None;
                imgpanel.Size = new Size(pSkillsNameWidth, pSkillsNameHeight);
                imgpanel.Name = "TruckPartImg" + i.ToString();
                //toolTipMain.SetToolTip(Ppanel, toolskillimgtooltip[i]);

                Bitmap bgimg = new Bitmap(TruckPartsImg[i], pSkillsNameHeight, pSkillsNameWidth);
                imgpanel.BackgroundImage = bgimg;

                //Panel 
                Ppanel = new Panel();
                groupBoxUserTruckTruckDetails.Controls.Add(Ppanel);

                Ppanel.Parent = groupBoxUserTruckTruckDetails;
                Ppanel.Location = new Point(lOffset, imgpanel.Location.Y);
                Ppanel.BorderStyle = BorderStyle.FixedSingle;
                Ppanel.Size = new Size(pSizeW, RepairImg.Height);
                Ppanel.Name = "progressbarTruckPart" + i.ToString();

                Button button = new Button();
                groupBoxUserTruckTruckDetails.Controls.Add(button);

                button.Parent = groupBoxUserTruckTruckDetails;
                button.Location = new Point(Ppanel.Location.X + Ppanel.Width + pOffset, Ppanel.Location.Y);
                button.FlatStyle = FlatStyle.Flat;
                button.Size = new Size(RepairImg.Height, RepairImg.Height);
                button.Name = "buttonTruckElRepair" + i.ToString();
                button.BackgroundImage = RepairImg;
                button.BackgroundImageLayout = ImageLayout.Zoom;
                button.Text = "";
                button.FlatAppearance.BorderSize = 0;
                button.Click += new EventHandler(buttonElRepair_Click);
            }

            Panel Ppanelf = new Panel();
            groupBoxUserTruckTruckDetails.Controls.Add(Ppanelf);
            Ppanelf.Parent = groupBoxUserTruckTruckDetails;
            Ppanelf.Location = new Point(lOffset + pSizeW + pOffset * 2 + RepairImg.Width, 23 + 14);
            Ppanelf.BorderStyle = BorderStyle.FixedSingle;
            Ppanelf.Size = new Size(50, 220);
            Ppanelf.Name = "progressbarTruckFuel";

            slabel = new Label();
            groupBoxUserTruckTruckDetails.Controls.Add(slabel);
            slabel.Name = "labelTruckDetailsFuel";
            slabel.Text = "Fuel";
            slabel.AutoSize = true;
            slabel.Location = new Point(Ppanelf.Location.X + (Ppanelf.Width - slabel.Width) / 2, Ppanelf.Location.Y + Ppanelf.Height + 10);

            CreateTruckPanelButtons();

        }

        private void CreateTruckPanelButtons()
        {
            //int lOffset = 100, pSizeW = 200;
            int pHeight = RepairImg.Height, pOffset = 5, tOffset = comboBoxUserTruckCompanyTrucks.Location.Y;
            int topbutoffset = comboBoxUserTruckCompanyTrucks.Location.X + comboBoxUserTruckCompanyTrucks.Width + pOffset;// = lOffset + pSizeW + pOffset * 2 + RepairImg.Width + groupBoxTruckDetails.Location.X;
            //int elembuttonoffset = lOffset + pSizeW + pOffset;

            Button buttonR = new Button();
            tabPageTruck.Controls.Add(buttonR);

            buttonR.Location = new Point(topbutoffset, tOffset);
            buttonR.FlatStyle = FlatStyle.Flat;
            buttonR.Size = new Size(RepairImg.Height, RepairImg.Height);
            buttonR.Name = "buttonTruckRepair";
            buttonR.BackgroundImage = RepairImg;
            buttonR.BackgroundImageLayout = ImageLayout.Zoom;
            buttonR.Text = "";
            buttonR.FlatAppearance.BorderSize = 0;
            buttonR.Click += new EventHandler(buttonTruckRepair_Click);

            Button buttonF = new Button();
            tabPageTruck.Controls.Add(buttonF);

            buttonF.Location = new Point(topbutoffset, tOffset + RepairImg.Height + pOffset);
            buttonF.FlatStyle = FlatStyle.Flat;
            buttonF.Size = new Size(RepairImg.Height, RepairImg.Height);
            buttonF.Name = "buttonTruckReFuel";
            buttonF.BackgroundImage = RefuelImg;
            buttonF.BackgroundImageLayout = ImageLayout.Zoom;
            buttonF.Text = "";
            buttonF.FlatAppearance.BorderSize = 0;
            buttonF.Click += new EventHandler(buttonTruckReFuel_Click);
        }

        public void buttonTruckReFuel_Click(object sender, EventArgs e)
        {
            int i = 0;
            foreach (string temp in UserTruckDictionary[comboBoxUserTruckCompanyTrucks.SelectedValue.ToString()].Parts.Find(x => x.PartType == "truckdata").PartData)
            {
                if (temp.StartsWith(" fuel_relative:"))
                {
                    UserTruckDictionary[comboBoxUserTruckCompanyTrucks.SelectedValue.ToString()].Parts.Find(x => x.PartType == "truckdata").PartData[i] = " fuel_relative: 1";
                    break;
                }
                i++;
            }
            UpdateTruckPanelProgressBars();
        }

        public void buttonTruckRepair_Click(object sender, EventArgs e)
        {
            string[] PartList = { "engine", "transmission", "chassis", "cabin", "tire" };

            foreach (string tempPart in PartList)
            {
                foreach (UserCompanyTruckDataPart temp in UserTruckDictionary[comboBoxUserTruckCompanyTrucks.SelectedValue.ToString()].Parts.FindAll(x => x.PartType == tempPart))
                {
                    string partNameless = temp.PartNameless;

                    int i = 0;

                    foreach (string temp2 in temp.PartData)
                    {
                        if (temp2.StartsWith(" wear:"))
                        {
                            UserTruckDictionary[comboBoxUserTruckCompanyTrucks.SelectedValue.ToString()].Parts.Find(x => x.PartNameless == partNameless).PartData[i] = " wear: 0";
                            break;
                        }
                        i++;
                    }
                }
            }

            UpdateTruckPanelProgressBars();
        }

        public void buttonElRepair_Click(object sender, EventArgs e)
        {
            Button curbtn = sender as Button;
            int bi = Convert.ToByte(curbtn.Name.Substring(19));

            string[] PartList = { "engine", "transmission", "chassis", "cabin", "tire" };

            foreach (UserCompanyTruckDataPart temp in UserTruckDictionary[comboBoxUserTruckCompanyTrucks.SelectedValue.ToString()].Parts.FindAll(x => x.PartType == PartList[bi]))
            {
                string partNameless = temp.PartNameless;

                int i = 0;

                foreach (string temp2 in temp.PartData)
                {
                    if (temp2.StartsWith(" wear:"))
                    {
                        UserTruckDictionary[comboBoxUserTruckCompanyTrucks.SelectedValue.ToString()].Parts.Find(x => x.PartNameless == partNameless).PartData[i] = " wear: 0";
                        break;
                    }
                    i++;
                }
            }

            UpdateTruckPanelProgressBars();
        }

        private void UpdateTruckPanelProgressBars()
        {
            UserTruckDictionary.TryGetValue(comboBoxUserTruckCompanyTrucks.SelectedValue.ToString(), out UserCompanyTruckData SelectedUserCompanyTruck);

            for (int i = 0; i < 5; i++)
            {
                Panel pnl = null;
                string pnlname = "progressbarTruckPart" + i.ToString();
                if (groupBoxUserTruckTruckDetails.Controls.ContainsKey(pnlname))
                {
                    pnl = groupBoxUserTruckTruckDetails.Controls[pnlname] as Panel;
                }

                if (pnl != null)
                {
                    List<string> TruckDataPart = null;

                    switch (i)
                    {
                        case 0:
                            TruckDataPart = SelectedUserCompanyTruck.Parts.Find(xp => xp.PartType == "engine").PartData;
                            break;
                        case 1:
                            TruckDataPart = SelectedUserCompanyTruck.Parts.Find(xp => xp.PartType == "transmission").PartData;
                            break;
                        case 2:
                            TruckDataPart = SelectedUserCompanyTruck.Parts.Find(xp => xp.PartType == "chassis").PartData;
                            break;
                        case 3:
                            TruckDataPart = SelectedUserCompanyTruck.Parts.Find(xp => xp.PartType == "cabin").PartData;
                            break;
                        case 4:
                            TruckDataPart = SelectedUserCompanyTruck.Parts.Find(xp => xp.PartType == "tire").PartData;
                            break;
                    }

                    string wear = TruckDataPart.Find(xl => xl.StartsWith(" wear:")).Split(new char[] { ' ' })[2];//TruckPart.PartWear;
                    decimal _wear = 0;

                    if (wear != "0" && wear != "1")
                        _wear = HexFloatToDecimalFloat(wear);
                    else
                    if (wear == "1")
                        _wear = 1;

                    SolidBrush ppen = new SolidBrush(GetProgressbarColor(_wear));

                    int x = 0, y = 0, pnlwidth = (int)(pnl.Width * (1 - _wear));

                    Bitmap progress = new Bitmap(pnl.Width, pnl.Height);

                    Graphics g = Graphics.FromImage(progress);
                    g.FillRectangle(ppen, x, y, pnlwidth, pnl.Height);

                    int fontSize = 12;
                    StringFormat sf = new StringFormat();
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;

                    GraphicsPath p = new GraphicsPath();
                    p.AddString(
                        ((int)((1 - _wear) * 100)).ToString() + " %",             // text to draw
                        FontFamily.GenericSansSerif,  // or any other font family
                        (int)FontStyle.Bold,      // font style (bold, italic, etc.)
                        g.DpiY * fontSize / 72,       // em size
                        new Rectangle(0, 0, pnl.Width, pnl.Height),              // location where to draw text
                        sf);          // set options here (e.g. center alignment)
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillPath(Brushes.Black, p);
                    g.DrawPath(Pens.Black, p);

                    pnl.BackgroundImage = progress;
                }
            }

            Panel pnlfuel = null;
            string pnlnamefuel = "progressbarTruckFuel";
            if (groupBoxUserTruckTruckDetails.Controls.ContainsKey(pnlnamefuel))
            {
                pnlfuel = groupBoxUserTruckTruckDetails.Controls[pnlnamefuel] as Panel;
            }

            if (pnlfuel != null)
            {
                string fuel = SelectedUserCompanyTruck.Parts.Find(xp => xp.PartType == "truckdata").PartData.Find(xl => xl.StartsWith(" fuel_relative:")).Split(new char[] { ' ' })[2];//SelectedUserCompanyTruck.Fuel;
                decimal _fuel = 0;

                if (fuel != "0" && fuel != "1")
                    _fuel = HexFloatToDecimalFloat(fuel);
                else
                if (fuel == "1")
                    _fuel = 1;

                SolidBrush ppen = new SolidBrush(GetProgressbarColor(1 - _fuel));
                int pnlheight = (int)(pnlfuel.Height * (_fuel)), x = 0, y = pnlfuel.Height - pnlheight;

                Bitmap progress = new Bitmap(pnlfuel.Width, pnlfuel.Height);

                Graphics g = Graphics.FromImage(progress);
                g.FillRectangle(ppen, x, y, pnlfuel.Width, pnlheight);

                int fontSize = 10;
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                GraphicsPath p = new GraphicsPath();
                p.AddString(
                    ((int)(_fuel * 100)).ToString() + " %",             // text to draw
                    FontFamily.GenericSansSerif,  // or any other font family
                    (int)FontStyle.Regular,      // font style (bold, italic, etc.)
                    g.DpiY * fontSize / 72,       // em size
                    new Rectangle(0, 0, pnlfuel.Width, pnlfuel.Height),              // location where to draw text
                    sf);          // set options here (e.g. center alignment)
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillPath(Brushes.Black, p);
                g.DrawPath(Pens.Black, p);

                pnlfuel.BackgroundImage = progress;
            }

            string lctxt = "";
            labelLicensePlate.Text = "";
            int stindex = 0, endindex = 0;

            string LicensePlate = SelectedUserCompanyTruck.Parts.Find(xp => xp.PartType == "truckdata").PartData.Find(xl => xl.StartsWith(" license_plate:")).Split(new char[] { '"' })[1];

            for (int i = 0; i < LicensePlate.Length; i++)//SelectedUserCompanyTruck.LicensePlate.Length; i++)
            {
                if (LicensePlate[i] == '<')
                {
                    endindex = i;
                    lctxt += LicensePlate.Substring(stindex, endindex - stindex);
                }
                else if (LicensePlate[i] == '>')
                {
                    stindex = i + 1;
                }
                else if (i == LicensePlate.Length - 1)
                {
                    endindex = i + 1;
                    lctxt += LicensePlate.Substring(stindex, endindex - stindex);
                }
            }
            if (lctxt.Split(new char[] { '|' }).Length > 1)
                labelLicensePlate.Text = lctxt.Split(new char[] { '|' })[0] + " Country " + lctxt.Split(new char[] { '|' })[1];
            else
                labelLicensePlate.Text = lctxt.Split(new char[] { '|' })[0];
        }

        private void UpdateTruckPanelProgressTitles()
        {

        }

        private void FillUserCompanyTrucksList()
        {
            DataTable combDT = new DataTable();
            DataColumn dc = new DataColumn("UserTruckNameless", typeof(string));
            combDT.Columns.Add(dc);

            dc = new DataColumn("UserTruckName", typeof(string));
            combDT.Columns.Add(dc);

            foreach (KeyValuePair<string, UserCompanyTruckData> UserTruck in UserTruckDictionary)
            {
                string templine = UserTruck.Value.Parts.Find(x => x.PartType == "truckbrandname").PartData.Find(xline => xline.StartsWith(" data_path:"));
                string truckname = templine.Split(new char[] { '"' })[1].Split(new char[] { '/' })[4];

                TruckBrandsLngDict.TryGetValue(truckname, out string TruckName);
                if (TruckName != null && TruckName != "")
                {
                    combDT.Rows.Add(UserTruck.Key, TruckName);
                }
                else
                {
                    combDT.Rows.Add(UserTruck.Key, truckname);
                }
            }
            /*
            foreach (KeyValuePair<string, UserCompanyTruck> UserTruck in UserTruckList)
            {
                combDT.Rows.Add(UserTruck.Key, UserTruck.Value.TruckName);
            }
            */
            //combDT.DefaultView.Sort = "UserTruckName ASC";
            comboBoxUserTruckCompanyTrucks.ValueMember = "UserTruckNameless";
            comboBoxUserTruckCompanyTrucks.DisplayMember = "UserTruckName";

            comboBoxUserTruckCompanyTrucks.DataSource = combDT;

            //UserTruckList.TryGetValue(comboBoxCompanyTrucks.SelectedValue.ToString(), out UserCompanyTruck SelectedUserCompanyTruck);

            comboBoxUserTruckCompanyTrucks.SelectedValue = PlayerProfileData.UserCompanyAssignedTruck;
        }

        private void comboBoxCompanyTrucks_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmbbx = sender as ComboBox;

            if (cmbbx.SelectedIndex != -1)
            {
                UpdateTruckPanelProgressBars();
                //UpdateTruckPanelProgressTitles();
            }
        }

        private void buttonUserTruckSelectCurrent_Click(object sender, EventArgs e)
        {
            comboBoxUserTruckCompanyTrucks.SelectedValue = PlayerProfileData.UserCompanyAssignedTruck;
        }

        private void buttonUserTruckSwitchCurrent_Click(object sender, EventArgs e)
        {
            PlayerProfileData.UserCompanyAssignedTruck = comboBoxUserTruckCompanyTrucks.SelectedValue.ToString();
        }

        //Share buttons
        private void buttonTruckPaintCopy_Click(object sender, EventArgs e)
        {
            string tempPaint = "TruckPaint\r\n";

            List<string> paintstr = UserTruckDictionary[comboBoxUserTruckCompanyTrucks.SelectedValue.ToString()].Parts.Find(xp => xp.PartType == "paintjob").PartData;

            foreach (string temp in paintstr)
            {
                tempPaint+= temp + "\r\n";
            }

            string asd = BitConverter.ToString(zipText(tempPaint)).Replace("-", "");
            Clipboard.SetText(asd);
            MessageBox.Show("Paint data has been copied.");
        }

        private void buttonTruckPaintPaste_Click(object sender, EventArgs e)
        {
            try
            {
                string inputData = unzipText(Clipboard.GetText());
                string[] Lines = inputData.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                if (Lines[0] == "TruckPaint")
                {
                    List<string> paintstr = new List<string>();
                    for (int i = 1; i < Lines.Length; i++)
                    {
                        paintstr.Add(Lines[i]);
                    }

                    UserTruckDictionary[comboBoxUserTruckCompanyTrucks.SelectedValue.ToString()].Parts.Find(xp => xp.PartType == "paintjob").PartData = paintstr;

                    MessageBox.Show("Paint data  has been inserted.");
                }
                else
                    MessageBox.Show("Wrong data. Expected Paint data but\r\n"+ Lines[0]+ "\r\nwas found.");
            }
            catch
            {
                MessageBox.Show("Something gone wrong with decoding.");
            }
        }
        //end Share buttons
        //end User Trucks tab

        //User Trailer tab

        private void CreateTrailerPanelControls()
        {
            CreateTrailerPanelProgressBars();
        }

        private void CreateTrailerPanelProgressBars()
        {
            int pHeight = RepairImg.Height, pOffset = 5, lOffset = 60, pSizeW = 300;
            int pSkillsNameHeight = 32, pSkillsNameWidth = 32, pSkillsNameOffset = 5, pSkillsNamelOffset = 12;

            string[] toolskillimgtooltip = new string[] { "Cargo", "Body", "Chassis", "Wheels" };
            Label slabel;
            Panel Ppanel;

            for (int i = 0; i < 4; i++)
            {
                slabel = new Label();
                groupBoxUserTrailerTrailerDetails.Controls.Add(slabel);
                slabel.Name = "labelTrailerPartName" + toolskillimgtooltip[i];//.ToString();
                slabel.Location = new Point(pSkillsNamelOffset, 23 + (pSkillsNameHeight + pSkillsNameOffset * 3) * i);
                slabel.Text = toolskillimgtooltip[i];
                slabel.AutoSize = true;

                Panel imgpanel = new Panel();
                groupBoxUserTrailerTrailerDetails.Controls.Add(imgpanel);

                imgpanel.Parent = groupBoxUserTrailerTrailerDetails;
                imgpanel.Location = new Point(pSkillsNamelOffset, 23 + 14 + (pSkillsNameHeight + pSkillsNameOffset * 3) * i);
                imgpanel.BorderStyle = BorderStyle.None;
                imgpanel.Size = new Size(pSkillsNameWidth, pSkillsNameHeight);
                imgpanel.Name = "TrailerPartImg" + i.ToString();
                //toolTipMain.SetToolTip(Ppanel, toolskillimgtooltip[i]);

                Bitmap bgimg = new Bitmap(TrailerPartsImg[i], pSkillsNameHeight, pSkillsNameWidth);
                imgpanel.BackgroundImage = bgimg;

                //Panel 
                Ppanel = new Panel();
                groupBoxUserTrailerTrailerDetails.Controls.Add(Ppanel);

                Ppanel.Parent = groupBoxUserTrailerTrailerDetails;
                Ppanel.Location = new Point(lOffset, imgpanel.Location.Y);
                Ppanel.BorderStyle = BorderStyle.FixedSingle;
                Ppanel.Size = new Size(pSizeW, RepairImg.Height);
                Ppanel.Name = "progressbarTrailerPart" + i.ToString();

                Button button = new Button();
                groupBoxUserTrailerTrailerDetails.Controls.Add(button);

                button.Parent = groupBoxUserTrailerTrailerDetails;
                button.Location = new Point(Ppanel.Location.X + Ppanel.Width + pOffset, Ppanel.Location.Y);
                button.FlatStyle = FlatStyle.Flat;
                button.Size = new Size(RepairImg.Height, RepairImg.Height);
                button.Name = "buttonTrailerElRepair" + i.ToString();
                button.BackgroundImage = RepairImg;
                button.BackgroundImageLayout = ImageLayout.Zoom;
                button.Text = "";
                button.FlatAppearance.BorderSize = 0;
                button.Click += new EventHandler(buttonTrailerElRepair_Click);
            }

            CreateTrailerPanelButtons();
        }

        private void CreateTrailerPanelButtons()
        {
            int pHeight = RepairImg.Height, pOffset = 5, tOffset = comboBoxUserTrailerCompanyTrailers.Location.Y;
            int topbutoffset = comboBoxUserTrailerCompanyTrailers.Location.X + comboBoxUserTrailerCompanyTrailers.Width + pOffset;

            Button buttonR = new Button();
            tabPageTrailer.Controls.Add(buttonR);

            buttonR.Location = new Point(topbutoffset, tOffset);
            buttonR.FlatStyle = FlatStyle.Flat;
            buttonR.Size = new Size(RepairImg.Height, RepairImg.Height);
            buttonR.Name = "buttonTrailerRepair";
            buttonR.BackgroundImage = RepairImg;
            buttonR.BackgroundImageLayout = ImageLayout.Zoom;
            buttonR.Text = "";
            buttonR.FlatAppearance.BorderSize = 0;
            buttonR.Click += new EventHandler(buttonTrailerRepair_Click);
        }

        public void buttonTrailerRepair_Click(object sender, EventArgs e)
        {
            string[] PartList = { "cargo", "body", "chassis", "tire" };

            foreach (string tempPart in PartList)
            {
                foreach (UserCompanyTruckDataPart temp in UserTruckDictionary[comboBoxUserTruckCompanyTrucks.SelectedValue.ToString()].Parts.FindAll(x => x.PartType == tempPart))
                {
                    string partNameless = temp.PartNameless;

                    int i = 0;

                    foreach (string temp2 in temp.PartData)
                    {
                        if (temp2.StartsWith(" wear:"))
                        {
                            UserTruckDictionary[comboBoxUserTruckCompanyTrucks.SelectedValue.ToString()].Parts.Find(x => x.PartNameless == partNameless).PartData[i] = " wear: 0";
                            break;
                        }
                        i++;
                    }
                }
            }

            UpdateTrailerPanelProgressBars();
        }

        public void buttonTrailerElRepair_Click(object sender, EventArgs e)
        {
            Button curbtn = sender as Button;
            int bi = Convert.ToByte(curbtn.Name.Substring(19));

            string[] PartList = { "cargo", "body", "chassis", "tire" };

            foreach (UserCompanyTruckDataPart temp in UserTrailerDictionary[comboBoxUserTrailerCompanyTrailers.SelectedValue.ToString()].Parts.FindAll(x => x.PartType == PartList[bi]))
            {
                string partNameless = temp.PartNameless;

                int i = 0;

                foreach (string temp2 in temp.PartData)
                {
                    if (temp2.StartsWith(" wear:"))
                    {
                        UserTrailerDictionary[comboBoxUserTrailerCompanyTrailers.SelectedValue.ToString()].Parts.Find(x => x.PartNameless == partNameless).PartData[i] = " wear: 0";
                        break;
                    }
                    i++;
                }
            }

            UpdateTrailerPanelProgressBars();
        }

        private void UpdateTrailerPanelProgressBars()
        {
            UserTrailerDictionary.TryGetValue(comboBoxUserTrailerCompanyTrailers.SelectedValue.ToString(), out UserCompanyTruckData SelectedUserCompanyTrailer);

            for (int i = 0; i < 5; i++)
            {
                Panel pnl = null;
                string pnlname = "progressbarTrailerPart" + i.ToString();
                if (groupBoxUserTrailerTrailerDetails.Controls.ContainsKey(pnlname))
                {
                    pnl = groupBoxUserTrailerTrailerDetails.Controls[pnlname] as Panel;
                }

                if (pnl != null)
                {
                    List<string> TruckDataPart = null;

                    switch (i)
                    {
                        case 0:
                            TruckDataPart = SelectedUserCompanyTrailer.Parts.Find(xp => xp.PartType == "cargo").PartData;
                            break;
                        case 1:
                            TruckDataPart = SelectedUserCompanyTrailer.Parts.Find(xp => xp.PartType == "body").PartData;
                            break;
                        case 2:
                            TruckDataPart = SelectedUserCompanyTrailer.Parts.Find(xp => xp.PartType == "chassis").PartData;
                            break;
                        case 3:
                            TruckDataPart = SelectedUserCompanyTrailer.Parts.Find(xp => xp.PartType == "tire").PartData;
                            break;
                    }

                    string wear = TruckDataPart.Find(xl => xl.StartsWith(" wear:")).Split(new char[] { ' ' })[2];//TruckPart.PartWear;
                    decimal _wear = 0;

                    if (wear != "0" && wear != "1")
                        _wear = HexFloatToDecimalFloat(wear);
                    else
                    if (wear == "1")
                        _wear = 1;

                    SolidBrush ppen = new SolidBrush(GetProgressbarColor(_wear));

                    int x = 0, y = 0, pnlwidth = (int)(pnl.Width * (1 - _wear));

                    Bitmap progress = new Bitmap(pnl.Width, pnl.Height);

                    Graphics g = Graphics.FromImage(progress);
                    g.FillRectangle(ppen, x, y, pnlwidth, pnl.Height);

                    int fontSize = 12;
                    StringFormat sf = new StringFormat();
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;

                    GraphicsPath p = new GraphicsPath();
                    p.AddString(
                        ((int)((1 - _wear) * 100)).ToString() + " %",             // text to draw
                        FontFamily.GenericSansSerif,  // or any other font family
                        (int)FontStyle.Bold,      // font style (bold, italic, etc.)
                        g.DpiY * fontSize / 72,       // em size
                        new Rectangle(0, 0, pnl.Width, pnl.Height),              // location where to draw text
                        sf);          // set options here (e.g. center alignment)
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillPath(Brushes.Black, p);
                    g.DrawPath(Pens.Black, p);

                    pnl.BackgroundImage = progress;
                }
            }

            string lctxt = "";
            labelUserTrailerLicensePlate.Text = "";
            int stindex = 0, endindex = 0;

            string LicensePlate = SelectedUserCompanyTrailer.Parts.Find(xp => xp.PartType == "truckdata").PartData.Find(xl => xl.StartsWith(" license_plate:")).Split(new char[] { '"' })[1];

            for (int i = 0; i < LicensePlate.Length; i++)//SelectedUserCompanyTruck.LicensePlate.Length; i++)
            {
                if (LicensePlate[i] == '<')
                {
                    endindex = i;
                    lctxt += LicensePlate.Substring(stindex, endindex - stindex);
                }
                else if (LicensePlate[i] == '>')
                {
                    stindex = i + 1;
                }
                else if (i == LicensePlate.Length - 1)
                {
                    endindex = i + 1;
                    lctxt += LicensePlate.Substring(stindex, endindex - stindex);
                }
            }
            if (lctxt.Split(new char[] { '|' }).Length > 1)
                labelUserTrailerLicensePlate.Text = lctxt.Split(new char[] { '|' })[0] + " Country " + lctxt.Split(new char[] { '|' })[1];
            else
                labelUserTrailerLicensePlate.Text = lctxt.Split(new char[] { '|' })[0];
        }

        private void FillUserCompanyTrailerList()
        {
            /*
            DataTable combDT = new DataTable();
            DataColumn dc = new DataColumn("UserTrailerkNameless", typeof(string));
            combDT.Columns.Add(dc);

            dc = new DataColumn("UserTrailerName", typeof(string));
            combDT.Columns.Add(dc);

            foreach (KeyValuePair<string, UserCompanyTruckData> UserTrailer in UserTrailerDictionary)
            {
                string templine = UserTrailer.Value.Parts.Find(x => x.PartType == "truckbrandname").PartData.Find(xline => xline.StartsWith(" data_path:"));
                string truckname = templine.Split(new char[] { '"' })[1].Split(new char[] { '/' })[4];

                combDT.Rows.Add(UserTrailer.Key, truckname);
            }

            //combDT.DefaultView.Sort = "UserTrailerName ASC";
            comboBoxCompanyTrailers.ValueMember = "UserTrailerkNameless";
            comboBoxCompanyTrailers.DisplayMember = "UserTrailerName";

            comboBoxCompanyTrailers.DataSource = combDT;

            comboBoxCompanyTrailers.SelectedValue = UserCompanyAssignedTrailer;
            */
        }

        private void comboBoxCompanyTrailers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonUserTrailerSelectCurrent_Click(object sender, EventArgs e)
        {
            comboBoxUserTrailerCompanyTrailers.SelectedValue = PlayerProfileData.UserCompanyAssignedTrailer;
        }

        private void buttonUserTrailerSwitchCurrent_Click(object sender, EventArgs e)
        {
            PlayerProfileData.UserCompanyAssignedTrailer = comboBoxUserTrailerCompanyTrailers.SelectedValue.ToString();
        }

        //end User Trailer tab

        //User Company tab
        private void FillFormCompanyControls()
        {
            listBoxVisitedCities.DrawMode = DrawMode.OwnerDrawVariable;
            listBoxGarages.DrawMode = DrawMode.OwnerDrawVariable;

            FillHQcities();

            FillGaragesList(0);
            FillVisitedCities(0);

            textBoxUserCompanyMoneyAccount.Text = PlayerProfileData.AccountMoney.ToString();
            comboBoxUserCompanyHQcity.SelectedValue = PlayerProfileData.HQcity;
            textBoxUserCompanyCompanyName.Text = PlayerProfileData.CompanyName;
        }

        private void FillHQcities()
        {
            DataTable combDT = new DataTable();
            DataColumn dc = new DataColumn("City", typeof(string));
            combDT.Columns.Add(dc);

            dc = new DataColumn("CityName", typeof(string));
            combDT.Columns.Add(dc);

            //start filling

            //fill source and destination cities
            foreach (City tempcity in from x in CitiesList
                                      where !x.Disabled
                                      select x)
            {
                CitiesLngDict.TryGetValue(tempcity.CityName, out string value);
                if (value != null && value != "")
                    combDT.Rows.Add(tempcity.CityName, value);
                else
                {
                    combDT.Rows.Add(tempcity.CityName, tempcity.CityName + " -n");
                }

                //comboBoxSourceCity.Items.Add(tempcity.CityName); //Source
                //comboBoxDestinationCity.Items.Add(tempcity.CityName); //Destination
            }

            comboBoxUserCompanyHQcity.ValueMember = "City";
            comboBoxUserCompanyHQcity.DisplayMember = "CityName";
            comboBoxUserCompanyHQcity.DataSource = combDT;
        }

        private void comboBoxHQcity_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxUserCompanyHQcity.SelectedValue != null)
                PlayerProfileData.HQcity = comboBoxUserCompanyHQcity.SelectedValue.ToString();
        }

        private void textBoxMoneyAccount_TextChanged(object sender, EventArgs e)
        {
            TextBox txtcur = sender as TextBox;

            if (!string.IsNullOrEmpty(txtcur.Text))
            {
                UInt64 valueBefore = UInt64.Parse(txtcur.Text, NumberStyles.AllowThousands);
                txtcur.Text = String.Format(CultureInfo.CurrentCulture, "{0:N0}", valueBefore);
                txtcur.Select(txtcur.Text.Length, 0);

                PlayerProfileData.AccountMoney = UInt32.Parse(txtcur.Text, NumberStyles.AllowThousands);
            }
        }

        private void textBoxMoneyAccount_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txtcur = sender as TextBox;
            UInt64 valueBefore = 0;

            if (!string.IsNullOrEmpty(txtcur.Text))
            {
                valueBefore = UInt64.Parse(txtcur.Text, NumberStyles.AllowThousands);
            }

            if (!Char.IsDigit(e.KeyChar) && !(valueBefore <= 999999999))
            {
                txtcur.Text = valueBefore.ToString();
                e.Handled = true;
            }
        }

        public void FillVisitedCities(int _vindex)
        {
            listBoxVisitedCities.Items.Clear();

            foreach(City vc in CitiesList)
            {
                listBoxVisitedCities.Items.Add(vc);
            }

            listBoxVisitedCities.TopIndex = _vindex;
        }

        private int VisitedCitiesItemMargin = 3;
        private const float VisitedCitiesPictureHeight = 32;

        private void listBoxVisitedCities_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            // Get the ListBox and the item.
            e.ItemHeight = (int)(VisitedCitiesPictureHeight + 2 * VisitedCitiesItemMargin);
        }

        private void listBoxVisitedCities_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Get the ListBox and the item.
            ListBox lst = sender as ListBox;
            City vc = (City)lst.Items[e.Index];

            // Draw the background.
            e.DrawBackground();

            int index = 0;
            if (vc.Visited)
                index = 1;

            Image cityicon = CitiesImg[index];

            // Draw the picture.
            float scale = VisitedCitiesPictureHeight / cityicon.Height;
            RectangleF source_rect = new RectangleF(0, 0, cityicon.Width, cityicon.Height);

            float picture_width = scale * cityicon.Width;

            RectangleF dest_rect = new RectangleF(e.Bounds.Left + VisitedCitiesItemMargin, e.Bounds.Top + VisitedCitiesItemMargin, picture_width, VisitedCitiesPictureHeight);
            e.Graphics.DrawImage(cityicon, dest_rect, source_rect, GraphicsUnit.Pixel);
            ////

            // See if the item is selected.
            Brush br;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                br = SystemBrushes.HighlightText;
            else
                br = new SolidBrush(e.ForeColor);

            // Find the area in which to put the text.
            float x = e.Bounds.Left + picture_width + 3 * VisitedCitiesItemMargin;
            float y = e.Bounds.Top + VisitedCitiesItemMargin * 2;
            float width = e.Bounds.Right - VisitedCitiesItemMargin - x;
            float height = e.Bounds.Bottom - VisitedCitiesItemMargin - y;
            RectangleF layout_rect = new RectangleF(x, y, width, height);

            // Draw the text.
            string txt = "";

            CitiesLngDict.TryGetValue(vc.CityName, out string value);
            if (value != null && value != "")
                txt = value;
            else
            {
                txt = vc.CityName + " -n";
            }

            //if (CitiesLngDict.TryGetValue(vc.CityName, out string value))
            //    txt = value;
            e.Graphics.DrawString(txt, Font, br, layout_rect);

            // Draw the focus rectangle if appropriate.
            e.DrawFocusRectangle();
        }

        private void buttonCitiesVisit_Click(object sender, EventArgs e)
        {
            if (listBoxVisitedCities.SelectedItems.Count == 0)
            {
                foreach (City city in listBoxVisitedCities.Items)
                {
                    if (!city.Visited)
                        city.Visited = true;
                }
            }
            else
                foreach (City city in listBoxVisitedCities.SelectedItems)
                {
                    if (!city.Visited)
                        city.Visited = true;
                }

            
            FillVisitedCities(listBoxVisitedCities.TopIndex);
        }

        private void buttonCitiesUnVisit_Click(object sender, EventArgs e)
        {
            if (listBoxVisitedCities.SelectedItems.Count == 0)
            {
                foreach (City city in listBoxVisitedCities.Items)
                {
                    if (city.Visited)
                        city.Visited = false;
                }
            }
            else
                foreach (City city in listBoxVisitedCities.SelectedItems)
                {
                    if (city.Visited)
                        city.Visited = false;
                }

            FillVisitedCities(listBoxVisitedCities.TopIndex);
        }

        public void FillGaragesList(int _vindex)
        {
            listBoxGarages.Items.Clear();

            foreach (Garages garage in from x in GaragesList where !x.IgnoreStatus select x)
            {
                listBoxGarages.Items.Add(garage);
            }
            listBoxGarages.TopIndex = _vindex;
        }

        private int GarageItemMargin = 3;
        private const float GaragePictureHeight = 32;

        private void listBoxGarages_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            // Get the ListBox and the item.
            e.ItemHeight = (int)(GaragePictureHeight + 2 * GarageItemMargin);
        }

        private void listBoxGarages_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Get the ListBox and the item.
            ListBox lst = sender as ListBox;
            string txt = "";
            Garages grg = (Garages)lst.Items[e.Index];

            // Draw the background.
            e.DrawBackground();

            Image grgicon = GaragesImg[grg.GarageStatus];

            // Draw the picture.
            float scale = GaragePictureHeight / grgicon.Height;
            RectangleF source_rect = new RectangleF(0, 0, grgicon.Width, grgicon.Height);

            float picture_width = scale * grgicon.Width;

            RectangleF dest_rect = new RectangleF(e.Bounds.Left + GarageItemMargin, e.Bounds.Top + GarageItemMargin, picture_width, GaragePictureHeight);
            e.Graphics.DrawImage(grgicon, dest_rect, source_rect, GraphicsUnit.Pixel);
            ////

            // See if the item is selected.
            Brush br;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                br = SystemBrushes.HighlightText;
            else
                br = new SolidBrush(e.ForeColor);

            int maxvehdr = 0;

            if (grg.GarageStatus == 0)
                goto skipVehAndDrDraw;//"Not owned";
            else if (grg.GarageStatus == 2)
                maxvehdr = 3;
            else if (grg.GarageStatus == 3)
                maxvehdr = 5;
            else if (grg.GarageStatus == 6)
                maxvehdr = 1;

            //Vehicles & Drivers
            float x = e.Bounds.Left + 120;
            float y = e.Bounds.Top + 18;
            float width = e.Bounds.Right - 100;
            float height = e.Bounds.Bottom - 14;
            RectangleF layout_rect = new RectangleF(x, y, width, height);

            int curVeh = 0, curDr = 0; 

            foreach(string temp in grg.Vehicles)
            {
                if (temp != "null")
                    curVeh++;
            }
            foreach (string temp in grg.Drivers)
            {
                if (temp != "null")
                    curDr++;
            }

            txt = "V: " + curVeh + " / " + maxvehdr + " D: " + curDr + " / " + maxvehdr + " T: " + grg.Trailers.Count;

            // Draw the text.
            e.Graphics.DrawString(txt, this.Font, br, layout_rect);

            skipVehAndDrDraw:;

            //City and Size
            // Find the area in which to put the text.
            x = e.Bounds.Left + picture_width + 3 * GarageItemMargin;
            y = e.Bounds.Top + GarageItemMargin * 2;
            width = e.Bounds.Right - GarageItemMargin - x;
            height = e.Bounds.Bottom - GarageItemMargin - y;
            layout_rect = new RectangleF(x, y, width, height);

            txt = lst.Items[e.Index].ToString();
            // Draw the text.
            e.Graphics.DrawString(txt, this.Font, br, layout_rect);
            
            // Draw the focus rectangle if appropriate.
            e.DrawFocusRectangle();
        }

        private void buttonGaragesBuy_Click(object sender, EventArgs e)
        {
            if(listBoxGarages.SelectedItems.Count == 0)
            {
                foreach(Garages garage in listBoxGarages.Items)
                {
                    if (garage.GarageStatus == 0 || garage.GarageStatus == 6)
                        garage.GarageStatus = 2;
                }
            }
            else
                foreach (Garages garage in listBoxGarages.SelectedItems)
                {
                    if (garage.GarageStatus == 0 || garage.GarageStatus == 6)
                        garage.GarageStatus = 2;
                }

            FillGaragesList(listBoxGarages.TopIndex);
        }

        private void buttonGaragesUpgrade_Click(object sender, EventArgs e)
        {
            if (listBoxGarages.SelectedItems.Count == 0)
            {
                foreach (Garages garage in listBoxGarages.Items)
                {
                    if (garage.GarageStatus == 2 || garage.GarageStatus == 6)
                        garage.GarageStatus = 3;
                }
            }
            else
                foreach (Garages garage in listBoxGarages.SelectedItems)
                {
                    if (garage.GarageStatus == 2 || garage.GarageStatus == 6)
                        garage.GarageStatus = 3;
                }

            FillGaragesList(listBoxGarages.TopIndex);
        }

        private void buttonGaragesBuyUpgrade_Click(object sender, EventArgs e)
        {
            if (listBoxGarages.SelectedItems.Count == 0)
            {
                foreach (Garages garage in listBoxGarages.Items)
                {
                        garage.GarageStatus = 3;
                }
            }
            else
                foreach (Garages garage in listBoxGarages.SelectedItems)
                {
                        garage.GarageStatus = 3;
                }

            FillGaragesList(listBoxGarages.TopIndex);
        }

        private void buttonGaragesSell_Click(object sender, EventArgs e)
        {
            if (listBoxGarages.SelectedItems.Count == 0)
            {
                foreach (Garages garage in listBoxGarages.Items)
                {
                    if (garage.GarageName == comboBoxUserCompanyHQcity.SelectedValue.ToString())
                        garage.GarageStatus = 6;
                    else
                        garage.GarageStatus = 0;
                }
            }
            else
                foreach (Garages garage in listBoxGarages.SelectedItems)
                {
                    if (garage.GarageName == comboBoxUserCompanyHQcity.SelectedValue.ToString())
                        garage.GarageStatus = 6;
                    else
                        garage.GarageStatus = 0;
                }

            FillGaragesList(listBoxGarages.TopIndex);
        }
        //end User Company tab

        //Freight market tab
        private void FillFormFreightMarketControls()
        {
            FillcomboBoxCargoList();
            FillcomboBoxCountries();
            FillcomboBoxCompanies();
            FillcomboBoxSourceCityDestinationCity();
        }

        private int JobsItemMargin = 3;
        private const float JobsPictureHeight = 32, JobsTextHeigh = 23, JobsItemHeight = 64;

        private void listBoxAddedJobs_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = (int)(JobsItemHeight + 2 * JobsItemMargin);
        }

        private void listBoxAddedJobs_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Get the ListBox and the item.
            ListBox lst = sender as ListBox;

            if (lst.Items.Count > 0)
            {

                JobAdded Job = (JobAdded)lst.Items[e.Index];

                // Draw the background.
                e.DrawBackground();

                // See if the item is selected.
                Brush br;
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    br = SystemBrushes.HighlightText;
                else
                    br = new SolidBrush(e.ForeColor);

                Image SourceCompIcon = null, DestinationCompIcon = null;

                if (File.Exists(@"img\" + GameType + @"\companies\" + Job.SourceCompany + ".dds"))
                    SourceCompIcon = ExtImgLoader(new string[] { @"img\" + GameType + @"\companies\" + Job.SourceCompany + ".dds" }, 100, 32, 0, 0)[0];
                else
                {
                    string currentDirName = Directory.GetCurrentDirectory() + @"\img\" + GameType + @"\companies";
                    string searchpattern = Job.SourceCompany.Split(new char[] { '_' })[0] + "*.dds";
                    string[] files = Directory.GetFiles(currentDirName, searchpattern);

                    if (files.Length > 0)
                        SourceCompIcon = ExtImgLoader(new string[] { files[0] }, 100, 32, 0, 0)[0];
                    else
                    {
                        SourceCompIcon = DrawCompanyText(Job.SourceCompany, 100, 32, br);
                    }
                }

                if (File.Exists(@"img\" + GameType + @"\companies\" + Job.DestinationCompany + ".dds"))
                    DestinationCompIcon = ExtImgLoader(new string[] { @"img\" + GameType + @"\companies\" + Job.DestinationCompany + ".dds" }, 100, 32, 0, 0)[0];
                else
                {
                    string currentDirName = Directory.GetCurrentDirectory() + @"\img\" + GameType + @"\companies";
                    string searchpattern = Job.DestinationCompany.Split(new char[] { '_' })[0] + "*.dds";
                    string[] files = Directory.GetFiles(currentDirName, searchpattern);
                    //DestinationCompIcon = ExtImgLoader(new string[] { files[0] }, 100, 32, 0, 0)[0];
                    if (files.Length > 0)
                        DestinationCompIcon = ExtImgLoader(new string[] { files[0] }, 100, 32, 0, 0)[0];
                    else
                    {
                        DestinationCompIcon = DrawCompanyText(Job.DestinationCompany, 100, 32, br);
                    }
                }

                // Draw the Source comp. picture
                float scale = JobsPictureHeight / SourceCompIcon.Height;
                RectangleF source_rect = new RectangleF(0, 0, SourceCompIcon.Width, SourceCompIcon.Height);

                float picture_width = scale * SourceCompIcon.Width;

                RectangleF dest_rect = new RectangleF(e.Bounds.Left + JobsItemMargin, e.Bounds.Top + JobsItemMargin * 2 + JobsTextHeigh, picture_width, JobsPictureHeight);
                e.Graphics.DrawImage(SourceCompIcon, dest_rect, source_rect, GraphicsUnit.Pixel);


                // Draw the Destination comp. picture
                dest_rect = new RectangleF(e.Bounds.Right - JobsItemMargin - picture_width, e.Bounds.Top + JobsItemMargin * 2 + JobsTextHeigh, picture_width, JobsPictureHeight);
                e.Graphics.DrawImage(DestinationCompIcon, dest_rect, source_rect, GraphicsUnit.Pixel);
                ////
                // Draw Type picture
                Image[] TypeImgs = new Image[5];
                int indexTypeImgs = 0, CargoMass = 0;
                bool extheavy = false;

                try
                {
                    ExtCargo tempExtCargo = ExtCargoList.Find(z => z.CargoName == Job.Cargo);

                    decimal fragile = tempExtCargo.Fragility;
                    bool valuable = tempExtCargo.Valuable;
                    int ADRclass = tempExtCargo.ADRclass;
                    int trueADR = ADRclass;
                    switch (trueADR)
                    {
                        case 6:
                            {
                                trueADR = 5;
                                break;
                            }
                        case 8:
                            {
                                trueADR = 6;
                                break;
                            }
                    }
                    CargoMass = (int) ( tempExtCargo.Mass * Job.UnitsCount);
                    if (CargoMass > 26000)
                        extheavy = true;

                    if (ADRclass > 0)
                    {
                        Bitmap bmp = new Bitmap(32, 32);
                        Graphics graph = Graphics.FromImage(bmp);
                        graph.DrawImage(ADRImgS[trueADR - 1], 2, 2, 28, 28);

                        TypeImgs[indexTypeImgs] = bmp;
                        //TypeImgs[indexTypeImgs] = ADRImgS[trueADR - 1];
                        indexTypeImgs++;
                    }

                    if (fragile == 0 || fragile >= (decimal)0.7)
                    {
                        TypeImgs[indexTypeImgs] = CargoType2Img[0];
                        indexTypeImgs++;
                    }

                    if (valuable)
                    {
                        TypeImgs[indexTypeImgs] = CargoType2Img[1];
                        indexTypeImgs++;
                    }
                }
                catch
                {

                }

                if (extheavy || Job.Type == 1)
                {
                    TypeImgs[indexTypeImgs] = CargoTypeImg[1];
                    indexTypeImgs++;
                }

                if (Job.Type == 2)
                {
                    TypeImgs[indexTypeImgs] = CargoTypeImg[2];
                    indexTypeImgs++;
                }

                TypeImgs[indexTypeImgs] = UrgencyImg[Job.Urgency];

                int xmult = 0, ymult = 0, images = 0;

                foreach(Image temp in TypeImgs)
                {
                    if (temp == null)
                    {
                        break;
                    }
                    images++;
                }

                for (int i = 0; i < 5; i++)
                {
                    if (TypeImgs[i] == null)
                    {
                        break;
                    }

                    source_rect = new RectangleF(0, 0, 32, 32);
                    dest_rect = new RectangleF((e.Bounds.Right - e.Bounds.Left - 32 * images) / 2 + 32 * xmult, e.Bounds.Top + JobsItemMargin, 32, 32);
                    e.Graphics.DrawImage(TypeImgs[i], dest_rect, source_rect, GraphicsUnit.Pixel);

                    xmult++;                    
                }

                // Draw the Urgency picture
                //source_rect = new RectangleF(0, 0, 32, 32);
                //dest_rect = new RectangleF((e.Bounds.Right - e.Bounds.Left - UrgencyImg[Job.Urgency].Width) / 2, e.Bounds.Top + JobsItemMargin, UrgencyImg[Job.Urgency].Width, UrgencyImg[Job.Urgency].Height);
                //e.Graphics.DrawImage(UrgencyImg[Job.Urgency], dest_rect, source_rect, GraphicsUnit.Pixel);
                ////

                // Draw the text.

                string value = "", SourceCityName="", DestinationCityName="";

                CitiesLngDict.TryGetValue(Job.SourceCity, out value);
                if (value != null && value != "")
                    SourceCityName = value;
                else
                {
                    SourceCityName = Job.SourceCity + " -n";
                }
                CitiesLngDict.TryGetValue(Job.DestinationCity, out value);
                if (value != null && value != "")
                    DestinationCityName = value;
                else
                {
                    DestinationCityName = Job.DestinationCity + " -n";
                }

                //CitiesLngDict.TryGetValue(Job.SourceCity, out string SourceCityName);
                //CitiesLngDict.TryGetValue(Job.DestinationCity, out string DestinationCityName);

                //Source City
                // Find the area in which to put the text.
                float x = e.Bounds.Left + JobsItemMargin;
                float y = e.Bounds.Top - JobsItemMargin + JobsTextHeigh / 2;
                float width = (e.Bounds.Right - e.Bounds.Left - JobsItemMargin * 4 - UrgencyImg[Job.Urgency].Width) / 2;
                float height = JobsTextHeigh;//e.Bounds.Bottom - JobsItemMargin - y;
                RectangleF layout_rect = new RectangleF(x, y, width, height);

                string txt = SourceCityName;
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Near;
                e.Graphics.DrawString(txt, this.Font, br, layout_rect, format);

                //Destination City
                // Find the area in which to put the text.
                x = e.Bounds.Left + width + 3 * JobsItemMargin + UrgencyImg[Job.Urgency].Width;
                layout_rect = new RectangleF(x, y, width, height);
                format.Alignment = StringAlignment.Far;
                txt = DestinationCityName;
                e.Graphics.DrawString(txt, this.Font, br, layout_rect, format);

                //Cargo
                // Find the area in which to put the text.
                x = e.Bounds.Left + picture_width + 4 * JobsItemMargin;// + 32 * 2;
                y = e.Bounds.Top + JobsItemMargin * 2 + UrgencyImg[Job.Urgency].Height;
                width = e.Bounds.Right - JobsItemMargin - x;
                height = e.Bounds.Bottom - JobsItemMargin - y;
                layout_rect = new RectangleF(x, y, width, height);

                if (CargoLngDict.TryGetValue(Job.Cargo, out string CargoName))
                {
                    if (CargoName != null && CargoName != "")
                    {
                        txt = CargoName;
                    }
                    else
                        txt = Job.Cargo;
                }
                else
                    txt = Job.Cargo;

                if (CargoMass > 0)
                    txt += " (" + CargoMass + " kg)";

                e.Graphics.DrawString(txt, this.Font, br, layout_rect);

                // Find the area in which to put Distance text.
                if (Job.Distance == 11111)
                {
                    txt = "";
                }
                else
                {
                    txt = (Job.Distance * DistanceMultiplier).ToString() + " " + ProgSettingsV.DistanceMes; //km";
                }

                if (Job.Ferrytime > 0)
                {
                    txt = (Job.Distance * DistanceMultiplier).ToString() + " " + ProgSettingsV.DistanceMes + " (Ferry " + (Job.Ferrytime / 60) .ToString() + "h - " + Job.Ferryprice.ToString() + " €)";
                }

                layout_rect = new RectangleF(x, y + 14, width, height);
                e.Graphics.DrawString(txt, this.Font, br, layout_rect);

                // Draw the focus rectangle if appropriate.
                e.DrawFocusRectangle();
            }
        }

        public Bitmap DrawCompanyText(string _companyName, int _width, int _height, Brush _brush)
        {
            Bitmap bmp = new Bitmap(100, 32);
            RectangleF rectf = new RectangleF(5, 5, 90, 22);

            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            

            g.DrawString(_companyName, new Font(Font.FontFamily, 12) , _brush, rectf, format);
            g.Flush();

            return bmp;
        }

        public void FillcomboBoxCountries()
        {
            foreach (string str in CountriesList)
            {
                comboBoxFreightMarketCountries.Items.Add(str);
            }
        }

        private void comboBoxCountries_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxFreightMarketDestinationCity.SelectedIndex = -1;
            comboBoxFreightMarketDestinationCompany.SelectedIndex = -1;

            triggerDestinationCitiesUpdate();
        }

        public void FillcomboBoxCompanies()
        {
            //start filtering
            List<string> tempCompList = new List<string>();

            foreach (City city in CitiesList.FindAll(x => !x.Disabled))
            {
                List<Company> source = city.ReturnCompanies();

                foreach (Company company in from x in source.Distinct()
                                            where !x.Excluded
                                            select x)
                {
                    if (!tempCompList.Contains(company.CompanyName))
                    {
                        tempCompList.Add(company.CompanyName);
                    }
                }
            }
            tempCompList = tempCompList.OrderBy(x => x).ToList();
            //end filtering
            
            DataTable combDT = new DataTable();
            DataColumn dc = new DataColumn("Company", typeof(string));
            combDT.Columns.Add(dc);

            dc = new DataColumn("CompanyName", typeof(string));
            combDT.Columns.Add(dc);

            //start filling
            combDT.Rows.Add("All", "All");

            foreach (string tempitem in tempCompList)
                if (CompaniesLngDict.TryGetValue(tempitem, out string value))
                    if (value != null && value != "")
                    {
                        combDT.Rows.Add(tempitem, value);
                    }
                    else
                    {
                        combDT.Rows.Add(tempitem, tempitem);
                    }

            comboBoxFreightMarketCompanies.ValueMember = "Company";
            comboBoxFreightMarketCompanies.DisplayMember = "CompanyName";
            comboBoxFreightMarketCompanies.DataSource = combDT;
            //end filling

            comboBoxFreightMarketCountries.SelectedIndex = comboBoxFreightMarketCountries.FindString("All");
        }

        private void comboBoxCompanies_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxFreightMarketDestinationCity.SelectedIndex = -1;
            comboBoxFreightMarketDestinationCompany.SelectedIndex = -1;

            triggerDestinationCitiesUpdate();
        }

        public void FillcomboBoxSourceCityDestinationCity()
        {
            DataTable combDT = new DataTable();
            DataColumn dc = new DataColumn("City", typeof(string));
            combDT.Columns.Add(dc);

            dc = new DataColumn("CityName", typeof(string));
            combDT.Columns.Add(dc);

            //start filling

            //fill source and destination cities
            foreach (City tempcity in from x in CitiesList
                                      where !x.Disabled
                                      select x)
            {
                //if (CitiesLngDict.TryGetValue(tempcity.CityName, out string value))
                CitiesLngDict.TryGetValue(tempcity.CityName, out string value);
                if (value != null && value != "")
                {
                    combDT.Rows.Add(tempcity.CityName, value);
                }
                else
                {
                    combDT.Rows.Add(tempcity.CityName, tempcity.CityName + " -n");
                }
            }
            combDT.DefaultView.Sort = "CityName ASC";

            comboBoxFreightMarketSourceCity.ValueMember = "City";
            comboBoxFreightMarketSourceCity.DisplayMember = "CityName";
            comboBoxFreightMarketSourceCity.DataSource = combDT;
            //end filling

            comboBoxFreightMarketSourceCity.SelectedValue = LastVisitedCity;
            //end
        }

        public void FillcomboBoxCargoList()
        {
            int savedindex = comboBoxFreightMarketCargoList.SelectedIndex;
            string savedvalue = "";
            if (savedindex != -1)
                savedvalue = comboBoxFreightMarketCargoList.SelectedValue.ToString();


            DataTable combDT = new DataTable();
            DataColumn dc = new DataColumn("Cargo", typeof(string));
            combDT.Columns.Add(dc);

            dc = new DataColumn("CargoName", typeof(string));
            combDT.Columns.Add(dc);

            foreach (Cargo tempitem in CargoesList)
            {
                if (CargoLngDict.TryGetValue(tempitem.CargoName, out string value))
                {
                    if (value != null && value != "")
                    {
                        string str = tempitem.CargoName;
                        /*
                        if(str.EndsWith("_c"))
                            value += " (Cont)";

                        if (tempitem.CargoType == 1)
                            value += " [H]";
                        else if (tempitem.CargoType == 2)
                            value += " [D]";
                        */
                        combDT.Rows.Add(str + "," + tempitem.CargoType.ToString(), value);
                    }
                    else
                    {
                        string str = tempitem.CargoName;
                        string CapName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str);
                        /*
                        if (str.EndsWith("_c"))
                            CapName += " (Cont)";

                        if (tempitem.CargoType == 1)
                            CapName += " [H]";
                        else if (tempitem.CargoType == 2)
                            CapName += " [D]";
                            */
                        combDT.Rows.Add(str + "," + tempitem.CargoType.ToString(), CapName);
                    }
                }
                else
                {
                    string str = tempitem.CargoName;
                    string CapName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str);
                    /*
                    if (str.EndsWith("_c"))
                        CapName += " (Cont)";

                    if (tempitem.CargoType == 1)
                        CapName += " [H]";
                    else if (tempitem.CargoType == 2)
                        CapName += " [D]";
                        */
                        combDT.Rows.Add(str + "," + tempitem.CargoType.ToString(), CapName);
                }
            }

            combDT.DefaultView.Sort = "CargoName ASC";

            comboBoxFreightMarketCargoList.ValueMember = "Cargo";
            comboBoxFreightMarketCargoList.DisplayMember = "CargoName";
            comboBoxFreightMarketCargoList.DataSource = combDT;

            if (savedindex == -1)
                comboBoxFreightMarketCargoList.SelectedIndex = RandomValue.Next(comboBoxFreightMarketCargoList.Items.Count);
            else
                comboBoxFreightMarketCargoList.SelectedValue = savedvalue;
        }

        private void comboBoxFreightMarketCargoList_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 13;
        }

        private void comboBoxFreightMarketCargoList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            //if ((e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit)
            //    return;

            ComboBox lst = sender as ComboBox;

            // Draw the background of the item.
            e.DrawBackground();

            // See if the item is selected.
            Brush br;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                br = SystemBrushes.HighlightText;
            else
                br = new SolidBrush(e.ForeColor);

            float x = e.Bounds.Left;
            float y = e.Bounds.Top;
            float width = e.Bounds.Width;
            float height = e.Bounds.Height;
            RectangleF layout_rect;

            string[] selval = ((DataRowView)lst.Items[e.Index])[0].ToString().Split(new char[] { ',' });
            string CargoName = selval[0], CargoType = selval[1],
                CargoDN = lst.GetItemText(lst.Items[e.Index]);

            if (CargoName.EndsWith("_c"))
                CargoDN += " (Cont)";

            if (CargoType == "1")
                CargoDN += " [H]";
            else if (CargoType == "2")
                CargoDN += " [D]";

            string txt = CargoDN;

            // Find the area in which to put the text.

            layout_rect = new RectangleF(x, y, width, height);
            //format.Alignment = StringAlignment.Far;

            e.Graphics.DrawString(txt, this.Font, br, layout_rect);

            // Draw the focus rectangle if the mouse hovers over an item.
            e.DrawFocusRectangle();
        }

        private void comboBoxSourceCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            string city = comboBoxFreightMarketSourceCity.SelectedValue.ToString();

            comboBoxFreightMarketSourceCompany.SelectedIndex = -1;

            City ccity = CitiesList.Find(x => x.CityName == city);

            List<Company> list = ccity.ReturnCompanies();
            List<Company> list2 = list.FindAll(x => !x.Excluded);


            DataTable combDT = new DataTable();
            DataColumn dc = new DataColumn("Company", typeof(string));
            combDT.Columns.Add(dc);

            dc = new DataColumn("CompanyName", typeof(string));
            combDT.Columns.Add(dc);

            foreach (Company company in list2)
                if (CompaniesLngDict.TryGetValue(company.CompanyName, out string value))
                    if (value != "")
                    {
                        combDT.Rows.Add(company.CompanyName, value);
                    }
                    else
                    {
                        combDT.Rows.Add(company.CompanyName, company.CompanyName);
                    }

            combDT.DefaultView.Sort = "CompanyName ASC";
            
            comboBoxFreightMarketSourceCompany.ValueMember = "Company";
            comboBoxFreightMarketSourceCompany.DisplayMember = "CompanyName";

            comboBoxFreightMarketSourceCompany.DataSource = combDT;

            if (ProgSettingsV.ProposeRandom && (comboBoxFreightMarketSourceCompany.Items.Count > 0))
            {
                comboBoxFreightMarketSourceCompany.SelectedIndex = RandomValue.Next(comboBoxFreightMarketSourceCompany.Items.Count);
            }
        }

        private void comboBoxSourceCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ProgSettingsV.ProposeRandom)
            {
                comboBoxFreightMarketCargoList.SelectedIndex = RandomValue.Next(comboBoxFreightMarketCargoList.Items.Count);
            }
        }

        private void comboBoxDestinationCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxFreightMarketDestinationCity.SelectedIndex >= 0)
            {
                comboBoxFreightMarketDestinationCompany.SelectedIndex = -1;
                comboBoxFreightMarketDestinationCompany.Text = "";

                triggerDestinationCompaniesUpdate();
            }

            if (ProgSettingsV.ProposeRandom && (comboBoxFreightMarketDestinationCompany.Items.Count > 0))
            {
                comboBoxFreightMarketDestinationCompany.SelectedIndex = RandomValue.Next(comboBoxFreightMarketDestinationCompany.Items.Count);

                if ((comboBoxFreightMarketDestinationCompany.Items.Count != 1) && (comboBoxFreightMarketSourceCity.SelectedValue == comboBoxFreightMarketDestinationCity.SelectedValue) )
                {
                    int rnd = 0;
                    while(true)
                    {
                        rnd = RandomValue.Next(comboBoxFreightMarketDestinationCompany.Items.Count);
                        if (comboBoxFreightMarketSourceCompany.SelectedIndex != rnd)
                        {
                            comboBoxFreightMarketDestinationCompany.SelectedIndex = rnd;
                            break;
                        }
                    }                    
                }
            }
        }

        private void triggerDestinationCitiesUpdate()
        {
            if(comboBoxFreightMarketCompanies.SelectedIndex != -1)
                SetupDestinationCities(!(comboBoxFreightMarketCountries.Text == "All"), !(comboBoxFreightMarketCompanies.SelectedValue.ToString() == "All"));// .Text == "All"));
        }

        private void SetupDestinationCities(bool _country_selected, bool _company_selected)
        {
            DataTable combDT = new DataTable();
            DataColumn dc = new DataColumn("City", typeof(string));
            combDT.Columns.Add(dc);

            dc = new DataColumn("CityName", typeof(string));
            combDT.Columns.Add(dc);

            //start filling
            List<City> cities = CitiesList;

            if (_country_selected && checkBoxFreightMarketFilterDestination.Checked)
            {
                cities = CitiesList.FindAll(x => !x.Disabled && (x.Country == comboBoxFreightMarketCountries.Text));
            }

            if (_country_selected && !checkBoxFreightMarketFilterDestination.Checked)
            {
                cities = CitiesList.FindAll(x => x.Country == comboBoxFreightMarketCountries.Text);
            }

            if (!(_country_selected || checkBoxFreightMarketFilterDestination.Checked))
            {
                cities = CitiesList;
            }

            if (!(_country_selected || !checkBoxFreightMarketFilterDestination.Checked))
            {
                cities = CitiesList.FindAll(x => !x.Disabled);
            }
            
            foreach (City city in cities)
            {

                List<Company> companyList = city.ReturnCompanies();

                if (!_company_selected)
                {
                }
                else
                if (_company_selected && checkBoxFreightMarketFilterDestination.Checked)
                {
                    companyList = companyList.FindAll(x => (x.CompanyName == comboBoxFreightMarketCompanies.SelectedValue.ToString()) && !x.Excluded);
                }
                else
                if (!(_company_selected || !checkBoxFreightMarketFilterDestination.Checked))
                {
                    companyList = companyList.FindAll(x => !x.Excluded);
                }
                else
                if (_company_selected && !checkBoxFreightMarketFilterDestination.Checked)
                {
                    companyList = companyList.FindAll(x => x.CompanyName == comboBoxFreightMarketCompanies.SelectedValue.ToString());
                }

                if (companyList.Count > 0)
                    if (CitiesLngDict.TryGetValue(city.CityName, out string CityNamevalue))
                        if (CityNamevalue != null && CityNamevalue != "")
                            combDT.Rows.Add(city.CityName, CityNamevalue);
                        else
                        {
                            combDT.Rows.Add(city.CityName, city.CityName + " -n");
                        }
                /*
                foreach (Company company in companyList)
                {
                    if (_company_selected)
                    {
                        if (CitiesLngDict.TryGetValue(city.CityName, out string valueName))
                            combDT.Rows.Add(city.CityName, valueName);
                        else
                        {
                            combDT.Rows.Add(city.CityName, city.CityName);
                        }
                    }
                }
                */
            }

            combDT.DefaultView.Sort = "CityName ASC";

            comboBoxFreightMarketDestinationCity.ValueMember = "City";
            comboBoxFreightMarketDestinationCity.DisplayMember = "CityName";
            comboBoxFreightMarketDestinationCity.DataSource = combDT;
            //end filling

            if (comboBoxFreightMarketDestinationCity.Items.Count == 0)
            {
                ShowStatusMessages("e", "message_no_matching_cities");
            }
            else
            {
                ShowStatusMessages("i", "");
                comboBoxFreightMarketDestinationCity.SelectedIndex = RandomValue.Next(comboBoxFreightMarketDestinationCity.Items.Count);
            }
        }

        private void triggerDestinationCompaniesUpdate()
        {
            SetupDestinationCompanies(!(comboBoxFreightMarketCompanies.SelectedValue.ToString() == "All"));//.Text == "All"));
        }

        private void SetupDestinationCompanies(bool _company_selected)
        {
            List<Company> CityCompanies = CitiesList.Find(x => x.CityName == comboBoxFreightMarketDestinationCity.SelectedValue.ToString()).ReturnCompanies();
            List<Company> RealCompanies = CityCompanies.FindAll(x => !x.Excluded);

            if (_company_selected && checkBoxFreightMarketFilterDestination.Checked)
            {
                RealCompanies = RealCompanies.FindAll(x => (x.CompanyName == comboBoxFreightMarketCompanies.SelectedValue.ToString()));
            }

            #region notused
            /*
            if (!(_company_selected || !checkBoxFilterDestination.Checked))
            {
                //list2 = list.FindAll(x => !x.Excluded);
            }

            if (!(_company_selected || checkBoxFilterDestination.Checked))
            {
                //list2 = list.FindAll(x => !x.Excluded);
            }

            if (_company_selected && !checkBoxFilterDestination.Checked)
            {
                if (predicate2 == null)
                {
                    predicate2 = x => x.CompanyName == comboBoxCompanies.Text;
                }
                list2 = list.FindAll(predicate2);
            }
            */
            #endregion

            DataTable combDT = new DataTable();
            DataColumn dc = new DataColumn("Company", typeof(string));
            combDT.Columns.Add(dc);

            dc = new DataColumn("CompanyName", typeof(string));
            combDT.Columns.Add(dc);

            foreach (Company company in RealCompanies)
            {
                CompaniesLngDict.TryGetValue(company.CompanyName, out string value);
                if (value != null && value != "")
                {
                    combDT.Rows.Add(company.CompanyName, value);
                }                    
                else
                {
                    combDT.Rows.Add(company.CompanyName, company.CompanyName);
                }
            }


            combDT.DefaultView.Sort = "CompanyName ASC";

            comboBoxFreightMarketDestinationCompany.ValueMember = "Company";
            comboBoxFreightMarketDestinationCompany.DisplayMember = "CompanyName";

            comboBoxFreightMarketDestinationCompany.DataSource = combDT;
        }

        private void comboBoxDestinationCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ProgSettingsV.ProposeRandom)
            {
                comboBoxFreightMarketCargoList.SelectedIndex = RandomValue.Next(comboBoxFreightMarketCargoList.Items.Count);
            }
        }

        private void comboBoxCargoList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ProgSettingsV.ProposeRandom)
            {
                comboBoxFreightMarketUrgency.SelectedIndex = RandomValue.Next(comboBoxFreightMarketUrgency.Items.Count);
            }
        }

        private void buttonAddJob_Click(object sender, EventArgs e)
        {
            AddCargo();
        }

        private void buttonClearJobList_Click(object sender, EventArgs e)
        {
            ClearJobData();
        }

        private void ClearJobData()
        {
            JobsTotalDistance = 0;
            JobsAmountAdded = 0;

            Array.Resize(ref JobsListAdded, 0);
            Array.Resize(ref ListSavefileCompanysString, 0);
            Array.Resize(ref EconomyEventUnitLinkStringList, 0);

            listBoxFreightMarketAddedJobs.Items.Clear();
            labelFreightMarketDistance.Text = "Jobs Distance";
            buttonFreightMarketClearJobList.Enabled = false;
        }

        private void checkBoxRandomDest_CheckedChanged(object sender, EventArgs e)
        {
            ProgSettingsV.ProposeRandom = checkBoxFreightMarketRandomDest.Checked;
        }

        //end Freight market tab

        //Cargo Market tab
        private void FillFormCargoOffersControls()
        {
            FillCargoMarketCities();
        }

        private void FillCargoMarketCities()
        {
            DataTable combDT = new DataTable();
            DataColumn dc = new DataColumn("City", typeof(string));
            combDT.Columns.Add(dc);

            dc = new DataColumn("CityName", typeof(string));
            combDT.Columns.Add(dc);

            //start filling

            //fill source and destination cities
            foreach (City tempcity in from x in CitiesList
                                      where !x.Disabled
                                      select x)
            {
                CitiesLngDict.TryGetValue(tempcity.CityName, out string value);
                if (value != null && value != "")
                    combDT.Rows.Add(tempcity.CityName, value);
                else
                {
                    combDT.Rows.Add(tempcity.CityName, tempcity.CityName + " -n");
                }

                //comboBoxSourceCity.Items.Add(tempcity.CityName); //Source
                //comboBoxDestinationCity.Items.Add(tempcity.CityName); //Destination
            }

            comboBoxCargoMarketSourceCity.ValueMember = "City";
            comboBoxCargoMarketSourceCity.DisplayMember = "CityName";
            comboBoxCargoMarketSourceCity.DataSource = combDT;

            comboBoxCargoMarketSourceCity.SelectedValue = LastVisitedCity;
        }

        private void comboBoxSourceCityCM_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxCargoMarketSourceCity.SelectedIndex >= 0)
            {
                comboBoxSourceCargoMarketCompany.SelectedIndex = -1;
                comboBoxSourceCargoMarketCompany.Text = "";

                SetupSourceCompaniesCM();
            }

            if (comboBoxSourceCargoMarketCompany.Items.Count > 0)
            {
                comboBoxSourceCargoMarketCompany.SelectedIndex = RandomValue.Next(comboBoxSourceCargoMarketCompany.Items.Count);
                /*
                if ((comboBoxSourceCompanyCM.Items.Count != 1) && (comboBoxSourceCityCM.SelectedValue == comboBoxDestinationCity.SelectedValue))
                {
                    int rnd = 0;
                    while (true)
                    {
                        rnd = RandomValue.Next(comboBoxDestinationCompany.Items.Count);
                        if (comboBoxSourceCompany.SelectedIndex != rnd)
                        {
                            comboBoxDestinationCompany.SelectedIndex = rnd;
                            break;
                        }
                    }
                }
                */
            }
        }

        private void SetupSourceCompaniesCM()
        {
            List<Company> CityCompanies = CitiesList.Find(x => x.CityName == comboBoxCargoMarketSourceCity.SelectedValue.ToString()).ReturnCompanies();
            List<Company> RealCompanies = CityCompanies.FindAll(x => !x.Excluded);
            /*
            if (_company_selected && checkBoxFilterDestination.Checked)
            {
                RealCompanies = RealCompanies.FindAll(x => (x.CompanyName == comboBoxCompanies.SelectedValue.ToString()));
            }
            */
            DataTable combDT = new DataTable();
            DataColumn dc = new DataColumn("Company", typeof(string));
            combDT.Columns.Add(dc);

            dc = new DataColumn("CompanyName", typeof(string));
            combDT.Columns.Add(dc);

            foreach (Company company in RealCompanies)
            {
                CompaniesLngDict.TryGetValue(company.CompanyName, out string value);
                if (value != null && value != "")
                {
                    combDT.Rows.Add(company.CompanyName, value);
                }
                else
                {
                    combDT.Rows.Add(company.CompanyName, company.CompanyName);
                }
            }

            combDT.DefaultView.Sort = "CompanyName ASC";

            comboBoxSourceCargoMarketCompany.ValueMember = "Company";
            comboBoxSourceCargoMarketCompany.DisplayMember = "CompanyName";

            comboBoxSourceCargoMarketCompany.DataSource = combDT;

        }

        private void comboBoxSourceCompanyCM_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxCargoMarketCargoListForCompany.Items.Clear();
            if (comboBoxSourceCargoMarketCompany.SelectedValue != null && ExternalCompanies.Count > 0)
            foreach(string cargo in ExternalCompanies.Find(x => x.CompanyName == comboBoxSourceCargoMarketCompany.SelectedValue.ToString()).outCargo)
                listBoxCargoMarketCargoListForCompany.Items.Add(cargo);

            PrintCargoSeeds();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            PrintCargoSeeds();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            PrintCargoSeeds();
        }

        private void PrintCargoSeeds()
        {
            listBoxCargoMarketSourceCargoSeeds.Items.Clear();

            if (comboBoxSourceCargoMarketCompany.SelectedValue != null) //&& ExternalCompanies.Count > 0)
            {
                //List<string> tempOutCargo = ExternalCompanies.Find(x => x.CompanyName == comboBoxSourceCompanyCM.SelectedValue.ToString()).outCargo;

                foreach (int cargoseed in CitiesList.Find(x => x.CityName == comboBoxCargoMarketSourceCity.SelectedValue.ToString()).ReturnCompanies().Find(x => x.CompanyName == comboBoxSourceCargoMarketCompany.SelectedValue.ToString()).CragoSeeds)
                {
                    //int Cargoreminder = cargoseed % (tempOutCargo.Count() - (int)numericUpDown1.Value);
                    //int Cargoreminder2 = (cargoseed - InGameTime + (int)numericUpDown2.Value) % (tempOutCargo.Count() - (int)numericUpDown1.Value);
                    //tempOutCargo.Sort();

                    //listBoxSourceCargoSeeds.Items.Add("" + cargoseed.ToString() + " | TL " + ((cargoseed - InGameTime) / 60).ToString() + "h "  + ((cargoseed - InGameTime) % 60).ToString() + "m " + " | Cargo " + "1 " + tempOutCargo[Cargoreminder] + " | 2 " + tempOutCargo[Cargoreminder2]);
                    listBoxCargoMarketSourceCargoSeeds.Items.Add("" + cargoseed.ToString() + " | Time left " + ((cargoseed - InGameTime) / 60).ToString() + "h " + ((cargoseed - InGameTime) % 60).ToString() + "m ");
                }
            }
        }
        
        private void buttonCargoMarketRandomizeCargoCompany_Click(object sender, EventArgs e)
        {
            List<Company> CityCompanies = CitiesList.Find(x => x.CityName == comboBoxCargoMarketSourceCity.SelectedValue.ToString()).ReturnCompanies();
            List<Company> RealCompanies = CityCompanies.FindAll(x => !x.Excluded);

            int[] tempseeds = new int[10];

            for(int i=0; i< tempseeds.Length; i++)
            {
                tempseeds[i] = InGameTime + RandomValue.Next(180, 1800);
            }

            RealCompanies.Find(x => x.CompanyName == comboBoxSourceCargoMarketCompany.SelectedValue.ToString()).CragoSeeds = tempseeds;

            PrintCargoSeeds();
        }

        private void buttonCargoMarketResetCargoCompany_Click(object sender, EventArgs e)
        {
            List<Company> CityCompanies = CitiesList.Find(x => x.CityName == comboBoxCargoMarketSourceCity.SelectedValue.ToString()).ReturnCompanies();
            List<Company> RealCompanies = CityCompanies.FindAll(x => !x.Excluded);

            RealCompanies.Find(x => x.CompanyName == comboBoxSourceCargoMarketCompany.SelectedValue.ToString()).CragoSeeds = new int[0];

            PrintCargoSeeds();
        }

        private void buttonCargoMarketRandomizeCargoCity_Click(object sender, EventArgs e)
        {
            List<Company> CityCompanies = CitiesList.Find(x => x.CityName == comboBoxCargoMarketSourceCity.SelectedValue.ToString()).ReturnCompanies();
            List<Company> RealCompanies = CityCompanies.FindAll(x => !x.Excluded);

            foreach (Company company in RealCompanies)
            {
                int[] tempseeds = new int[10];

                for (int i = 0; i < tempseeds.Length; i++)
                {
                    tempseeds[i] = InGameTime + RandomValue.Next(180, 1800);
                }

                company.CragoSeeds = tempseeds;
            }

            PrintCargoSeeds();
        }

        private void buttonCargoMarketResetCargoCity_Click(object sender, EventArgs e)
        {
            List<Company> CityCompanies = CitiesList.Find(x => x.CityName == comboBoxCargoMarketSourceCity.SelectedValue.ToString()).ReturnCompanies();
            List<Company> RealCompanies = CityCompanies.FindAll(x => !x.Excluded);

            foreach (Company company in RealCompanies)
            {
                company.CragoSeeds = new int[0];
            }

            PrintCargoSeeds();
        }

        //end Cargo Market tab

        //Convoy tools tab
        private void buttonGPSCurrentPositionCopy_Click(object sender, EventArgs e)
        {
            string tempString = "GPS_TruckPosition\r\n";

            tempString += PlayerProfileData.UserCompanyAssignedTruckPlacement;
            string asd = BitConverter.ToString(zipText(tempString)).Replace("-", "");
            Clipboard.SetText(asd);
            MessageBox.Show("Truck GPS position has been copied.");
        }

        private void buttonGPSCurrentPositionPaste_Click(object sender, EventArgs e)
        {
            //UserCompanyAssignedTruckPlacement
            try
            {
                string inputData = unzipText(Clipboard.GetText());
                string[] Lines = inputData.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                if (Lines[0] == "GPS_TruckPosition")
                {
                    List<string> tempstr = new List<string>();
                    for (int i = 1; i < Lines.Length; i++)
                    {
                        tempstr.Add(Lines[i]);
                    }

                    PlayerProfileData.UserCompanyAssignedTruckPlacement = tempstr[0];
                    //PlayerProfileData.UserCompanyAssignedTrailerPlacement = "(0, 0, 0) (1; 0, 0, 0)";

                    MessageBox.Show("Truck GPS position has been inserted.");
                    UserCompanyAssignedTruckPlacementEdited = true;
                }
                else
                    MessageBox.Show("Wrong data. Expected Truck GPS data but\r\n" + Lines[0] + "\r\nwas found.");
            }
            catch
            {
                MessageBox.Show("Something gone wrong with decoding.");
            }
        }

        private void buttonGPSStoredGPSPathCopy_Click(object sender, EventArgs e)
        {
            string tempData = "GPS_Path\r\n";

            if (GPSbehind.Count > 0)
            {
                tempData += "GPSbehind\r\n";
                foreach (KeyValuePair<string, List<string>> temp in GPSbehind)
                {
                    tempData += "waypoint\r\n";
                    foreach (string tempLines in temp.Value)
                    {
                        tempData += tempLines + "\r\n";
                    }
                }
            }
            //GPSahead
            tempData += "GPSahead\r\n";
            foreach (KeyValuePair<string, List<string>> temp in GPSahead)
            {
                tempData += "waypoint\r\n";
                foreach (string tempLines in temp.Value)
                {
                    tempData += tempLines + "\r\n";
                }
            }

            //MessageBox.Show(tempPaint);
            string asd = BitConverter.ToString(zipText(tempData)).Replace("-", "");
            Clipboard.SetText(asd);
            MessageBox.Show("GPS Path data has been copied.");
        }

        private void buttonGPSStoredGPSPathPaste_Click(object sender, EventArgs e)
        {
            try
            {
                string inputData = unzipText(Clipboard.GetText());
                string[] Lines = inputData.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                if (Lines[0] == "GPS_Path")
                {
                    Dictionary<int, List<string>> tempGPSbehind, tempGPSahead;

                    tempGPSbehind = new Dictionary<int, List<string>>();
                    tempGPSahead = new Dictionary<int, List<string>>();

                    bool tagGPSbehind = false, tagGPSahead = false;//, tagWP = false;

                    for (int i = 1; i < Lines.Length; i++)
                    {
                        //GPSbehind
                        if (Lines[i].StartsWith("GPSbehind"))
                        {
                            tagGPSbehind = true;
                            continue;
                        }

                        if (tagGPSbehind)
                        {
                            int wp = 0;
                            do
                            {
                                if (Lines[i].StartsWith("waypoint"))
                                {
                                    i++;
                                    List<string> tmpList = new List<string>();

                                    while (!Lines[i].StartsWith("waypoint") && !Lines[i].StartsWith("GPSahead") && Lines[i] != "" && i < Lines.Length)
                                    {
                                        tmpList.Add(Lines[i]);
                                        i++;
                                    }

                                    tempGPSbehind.Add(wp, tmpList);
                                    wp++;
                                }
                            }
                            while (!Lines[i].StartsWith("GPSahead") && Lines[i] != "" && i < Lines.Length);

                            tagGPSbehind = false;
                        }

                        //GPSahead
                        if (Lines[i].StartsWith("GPSahead"))
                        {
                            tagGPSahead = true;
                            continue;
                        }

                        if (tagGPSahead)
                        {
                            int wp = 0;
                            do
                            {
                                if (Lines[i].StartsWith("waypoint"))
                                {
                                    i++;
                                    List<string> tmpList = new List<string>();

                                    while (!Lines[i].StartsWith("waypoint") && Lines[i] != "" && i < Lines.Length)
                                    {
                                        tmpList.Add(Lines[i]);
                                        i++;
                                    }

                                    tempGPSahead.Add(wp, tmpList);
                                    wp++;
                                }
                            }
                            while (i < Lines.Length && Lines[i] != "");
                        }
                    }

                    //GPSbehind = tempGPSbehind;
                    if (tempGPSbehind.Count > 0)
                    {
                        GPSbehind.Clear();
                        foreach (KeyValuePair<int, List<string>> temp in tempGPSbehind)
                        {
                            GPSbehind.Add(GetSpareNameless(), temp.Value);
                        }
                    }

                    //GPSahead = tempGPSahead;
                    if (tempGPSahead.Count > 0)
                    {
                        GPSahead.Clear();
                        foreach (KeyValuePair<int, List<string>> temp in tempGPSahead)
                        {
                            GPSahead.Add(GetSpareNameless(), temp.Value);
                        }
                    }

                    MessageBox.Show("GPS Path data  has been inserted.");
                }
                else
                    MessageBox.Show("Wrong data. Expected GPS Path data but\r\n" + Lines[0] + "\r\nwas found.");
            }
            catch
            {
                MessageBox.Show("Something gone wrong with decoding.");
            }
        }
        //end Convoy Tools tab

        //Form methods
        private void ToggleVisibility(bool visible)
        {
            foreach (TabPage tp in tabControlMain.TabPages)
            {
                tp.Enabled = visible;
            }

            if (comboBoxUserTrailerCompanyTrailers.Items.Count == 0)
            {
                tabControlMain.TabPages["tabPageTrailer"].Enabled = false;
            }
        }

        public void ToggleGame_Click(object sender, EventArgs e)
        {
            Button gamebutton = sender as Button;

            if (gamebutton.Name == "buttonGameETS")
                ToggleGame("ETS");
            else
                ToggleGame("ATS");

            FillAllProfilesPaths();
        }

        public void ToggleGame(string _game)
        {
            if (_game == "ETS")
            {
                Globals.CurrentGame = dictionaryProfiles["ETS2"];
                buttonMainGameSwitchETS.Enabled = false;
                buttonMainGameSwitchATS.Enabled = true;
                GameType = _game;
                buttonMainGameSwitchETS.BackColor = Color.White;
                buttonMainGameSwitchETS.ForeColor = Color.Black;
                buttonMainGameSwitchATS.BackColor = Color.FromKnownColor(KnownColor.Control);
                buttonMainGameSwitchATS.ForeColor = Color.FromKnownColor(KnownColor.ControlText);
            }
            else
            {
                Globals.CurrentGame = dictionaryProfiles["ATS"];
                buttonMainGameSwitchETS.Enabled = true;
                buttonMainGameSwitchATS.Enabled = false;
                GameType = _game;
                buttonMainGameSwitchATS.BackColor = Color.White;
                buttonMainGameSwitchATS.ForeColor = Color.Black;
                buttonMainGameSwitchETS.BackColor = Color.FromKnownColor(KnownColor.Control);
                buttonMainGameSwitchETS.ForeColor = Color.FromKnownColor(KnownColor.ControlText);
            }

        }

        private void toolstripChangeLanguage(object sender, EventArgs e)
        {
            ToolStripItem obj = sender as ToolStripItem;
            string _objname = obj.Name;

            string[] cult = _objname.ToString().Split('_');
            string ButtonCultureInfo = cult[0] + "-" + cult[1];

            ProgSettingsV.Language = ButtonCultureInfo;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(ButtonCultureInfo);

            ChangeLanguage();
        }

        private void ChangeLanguage()
        {
            try
            {
                if (ProgSettingsV.Language != "Default")
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(ProgSettingsV.Language);//CultureInfo.GetCultureInfo("en-US");
            }
            catch
            {
                LogWriter("Wrong language setting format");
            }
            
            CultureInfo ci = Thread.CurrentThread.CurrentUICulture;

            try
            {

                PlainTXTResourceManager rm = new PlainTXTResourceManager();
                ResourceSet set = null;//rm.GetResourceSet(ci, true, true);
                set = rm.GetResourceSet(ci, true, true);                

                List<string> keys = new List<string>();

                foreach (DictionaryEntry o in set)
                {
                    keys.Add((string)o.Key);
                }

                this.SuspendLayout();
                foreach (string x in keys)
                {
                    try
                    {
                        //Control ctn = x as ;
                        //ctn.Text = rm.GetString(x, ci);

                        Controls.Find(x, true)[0].Text = rm.GetString(x, ci);
                    }
                    catch { }

                    try
                    {
                        menuStripMain.Items.Find(x, true)[0].Text = rm.GetString(x, ci);
                    }
                    catch { }
                }
                this.ResumeLayout();

                LoadCompaniesLng(ProgSettingsV.Language);
                LoadCitiesLng(ProgSettingsV.Language);
                LoadCargoLng(ProgSettingsV.Language);
                LoadTruckBrandsLng();

                RefreshComboboxes();
            }
            catch
            {

            }
            
            //rm.ReleaseAllResources();
        }

        private void RefreshComboboxes()
        {
            //FillcomboBoxCargoList();

            int savedindex = comboBoxFreightMarketCargoList.SelectedIndex;
            string savedvalue = "";
            if (savedindex != -1)
                savedvalue = comboBoxFreightMarketCargoList.SelectedValue.ToString();

            DataTable temptable = comboBoxFreightMarketCargoList.DataSource as DataTable;
            int i = 0;

            foreach (DataRow temp in temptable.Rows)
            {
                try
                {
                    ((DataTable)comboBoxFreightMarketCargoList.DataSource).Rows[i][1] = CargoLngDict[temp[0].ToString().Split(new char[] { ',' })[0]];
                }
                catch
                { }
                i++;
            }

            comboBoxFreightMarketCargoList.SelectedIndex = -1;

            if (savedindex != -1)
                comboBoxFreightMarketCargoList.SelectedValue = savedvalue;
            else
                comboBoxFreightMarketCargoList.SelectedIndex = RandomValue.Next(comboBoxFreightMarketCargoList.Items.Count);
        }
        
        private string GetranslatedString(string _key)
        {
            CultureInfo ci = Thread.CurrentThread.CurrentUICulture;
            try
            {
                PlainTXTResourceManager rm = new PlainTXTResourceManager();
                string res = rm.GetString(_key, ci);
                if(res != null)
                    return res;
                else
                    return _key;
            }
            catch
            {
                return _key;
            }
        }

        private void CreateProgressBarBitmap()
        {
            ProgressBarGradient = new Bitmap(100, 1);

            LinearGradientBrush br = new LinearGradientBrush(new RectangleF(0, 0, 100, 1), Color.Black, Color.Black, 0, false);
            ColorBlend cb = new ColorBlend();
            cb.Positions = new[] { 0.0f, 0.5f, 1f };
            cb.Colors = new[] { Color.FromArgb(255, 255, 0, 0), Color.FromArgb(255, 255, 255, 0), Color.FromArgb(255, 0, 255, 0), };
            br.InterpolationColors = cb;

            //puts the gradient scale onto a bitmap which allows for getting a color from pixel
            Graphics g = Graphics.FromImage(ProgressBarGradient);
            g.FillRectangle(br, new RectangleF(0, 0, ProgressBarGradient.Width, ProgressBarGradient.Height));
        }

        private Color GetProgressbarColor(decimal value)
        {
            return ProgressBarGradient.GetPixel(Convert.ToInt32((1 - value) * 100) - 1, 0);
        }

        private string GetSpareNameless()
        {
            if (namelessLast == "")
            {
                namelessLast = namelessList.Last();
            }

            ushort incr = 48;

            string[] namelessNumbers = namelessLast.Split(new char[] { '.' });
            ushort[] namelessNumArray = new ushort[namelessNumbers.Length];

            Array.Reverse(namelessNumbers);
            bool first = true, overflow = false;

            for (int i = 0; i < namelessNumbers.Length; i++)
            {
                namelessNumArray[i] = UInt16.Parse(namelessNumbers[i], NumberStyles.HexNumber);

                try
                {
                    if (first)
                    {
                        namelessNumArray[i] = checked((ushort)(namelessNumArray[i] + incr));
                    }
                    else
                    if (overflow)
                    {
                        namelessNumArray[i] = checked((ushort)(namelessNumArray[i]+ 1));
                        overflow = false;
                    }
                }
                catch (OverflowException)
                {
                    if (first)
                    {
                        namelessNumArray[i] = (ushort)(namelessNumArray[i] + incr);
                    }
                    else
                    {
                        namelessNumArray[i] = (ushort)(namelessNumArray[i] + 1);
                    }
                    overflow = true;
                }

                if (i == (namelessNumbers.Length - 1) && overflow )
                {
                    Array.Resize(ref namelessNumArray, namelessNumArray.Length + 1);

                    namelessNumArray[namelessNumbers.Length - 1] = 1;
                }

                if (first)
                    first = false;
            }

            namelessLast = "";

            for (int i = 0; i < namelessNumArray.Length; i++)
            {
                if (i < namelessNumArray.Length - 1)
                {
                    namelessLast = "." + namelessNumArray[i].ToString("x4") + namelessLast;
                }
                else
                {
                    namelessLast = namelessNumArray[i].ToString("x") + namelessLast;
                }
            }
            //namelessLast
            return namelessLast;
        }
        
        internal byte[] zipText(string text)
        {
            if (text == null)
                return null;

            using (Stream memOutput = new MemoryStream())
            {
                using (GZipOutputStream zipOut = new GZipOutputStream(memOutput))
                {
                    using (StreamWriter writer = new StreamWriter(zipOut))
                    {
                        writer.Write(text);

                        writer.Flush();
                        zipOut.Finish();

                        byte[] bytes = new byte[memOutput.Length];
                        memOutput.Seek(0, SeekOrigin.Begin);
                        memOutput.Read(bytes, 0, bytes.Length);

                        return bytes;
                    }
                }
            }
        }

        internal string unzipText(string _sbytes)
        {
            string[] pairs = new string[_sbytes.Length / 2];
            byte[] bytes;// = new byte[0];

            for (int i = 0; i < _sbytes.Length / 2; i++)
            {
                pairs[i] = _sbytes.Substring(i * 2, 2);
            }

            bytes = new byte[pairs.Length];

            for (int j = 0; j < pairs.Length; j++)
                bytes[j] = Convert.ToByte(pairs[j], 16);

            if (bytes == null)
                return null;

            using (Stream memInput = new MemoryStream(bytes))
            using (GZipInputStream zipInput = new GZipInputStream(memInput))
            using (StreamReader reader = new StreamReader(zipInput))
            {
                string text = reader.ReadToEnd();

                return text;
            }
        }

        //end Form methods
    }
}