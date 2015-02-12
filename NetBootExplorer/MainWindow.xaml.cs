﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NetBootExplorer
{
    public partial class MainWindow : Window
    {
        private static List<ADComputer> allComputers;
        private static IList<ADComputer> displayedComputers;

        private static List<ADComputer> getAllComputers()
        {
            List<ADComputer> returnMe = new List<ADComputer>();

            DirectoryEntry searchRoot = new DirectoryEntry();
            DirectorySearcher searcher = new DirectorySearcher(searchRoot);
            searcher.Filter = "(objectClass=computer)";
            searcher.Sort = new SortOption("sAMAccountName", SortDirection.Ascending);
            searcher.PageSize = 1000;
            searcher.PropertiesToLoad.Add("sAMAccountName");
            searcher.PropertiesToLoad.Add("name");
            searcher.PropertiesToLoad.Add("netbootGUID");
            searcher.PropertiesToLoad.Add("distinguishedName");

            searcher.SearchScope = SearchScope.Subtree;
            SearchResultCollection allUsers = searcher.FindAll();
            foreach (SearchResult thisUser in allUsers)
            {
                DirectoryEntry child = thisUser.GetDirectoryEntry();
                String DN = child.Path.ToString().Remove(0, 7);
                string name;
                if (child.Properties.Contains("name"))
                {
                    name = child.Properties["name"].Value.ToString();
                }
                else
                {
                    name = "Unknown";
                }

                string guidString;
                if (child.Properties.Contains("netbootGUID"))
                {
                    Guid guid;
                    guid = new Guid((byte[])child.Properties["netbootGUID"].Value);

                    Regex rgx = new Regex("[^a-zA-Z0-9]");
                    guidString = rgx.Replace(guid.ToString(), "");
                }
                else
                {
                    guidString = string.Empty;
                }

                returnMe.Add(new ADComputer(name, guidString, DN));
            }

            return returnMe;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void disableControls()
        {
            txtInput.IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;
        }

        private void enableContols()
        {
            //txtInput.Text = "";
            txtInput.IsEnabled = true;
            progressBar.Visibility = Visibility.Hidden;
        }

        public delegate void myDelegate();
        private void loadFromAD()
        {
            disableControls();
            statusBar.Content = "Loading computer objects from Active Directory...";
            Task.Factory.StartNew(() =>
            {
                allComputers.Clear();
                allComputers = getAllComputers();
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (myDelegate)delegate()
                {
                    enableContols();
                    displayTheseComputers(allComputers,"");
                    statusBar.Content = "Loaded " + allComputers.Count + " computer objects";
                });
            }
                );
        }

        private void Window_ContentRendered_1(object sender, EventArgs e)
        {
            allComputers = new List<ADComputer>();
            displayedComputers = new List<ADComputer>();

            loadFromAD();
            string appName = Assembly.GetAssembly(this.GetType()).Location;
            AssemblyName assemblyName = AssemblyName.GetAssemblyName(appName);
            lblVersion.Content = "Version " + assemblyName.Version.ToString();
        }

        private void displayTheseComputers(List<ADComputer> computers, string filter) 
        {
            lstDisplayedComputers.Items.Clear();

            string searchString = filter;
            if (!string.IsNullOrEmpty(filter))
            {
                Regex rgx = new Regex("[^a-zA-Z0-9]");
                searchString = rgx.Replace(searchString.ToString(), "");
            }

            foreach (ADComputer computer in computers)
            {
                if (
                    (string.IsNullOrEmpty(searchString)) ||
                    (computer.getNetBootGUID().ToLower().Contains(searchString.ToLower())) ||
                    (computer.getName().ToLower().Contains(searchString.ToLower()))
                    )
                {
                    ListViewItem newItem = new ListViewItem();
                    newItem.Content = computer;
                    lstDisplayedComputers.Items.Add(newItem);
                }
            }            

        }

        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {            
            displayTheseComputers(allComputers, txtInput.Text);
        }

        private void lstDisplayedComputers_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            lstDisplayedComputers.Items.SortDescriptions.Add(
                new SortDescription("name", ListSortDirection.Ascending));

        }
    }
}
