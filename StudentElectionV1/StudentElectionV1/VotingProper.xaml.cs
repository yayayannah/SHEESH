using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Xml.Schema;

namespace StudentElectionV1
{
    
    public partial class VotingProper : Window
    {
        string username;
        string password;
        string database;
        string server;
        public MySqlConnection conn;
        public MySqlDataAdapter adapter;
        public MySqlDataReader cursor;
        public MySqlCommand cmd;
        public string sql = "";
        public DataTable dataTable;
        public DataSet dt;
        public VotingProper()
        {
            InitializeComponent();
            Connect2DB();
            txtVoterNumber.Focus();
        }

        private void mySQLConnection()
        {
            string connStr = $"server={server}; password={password};username={username}; database={database}";
            conn = new MySqlConnection(connStr);
            conn.Open();
        }
        public void Connect2DB()
        {
            server = "localhost";
            database = "honest";
            username = "root";
            password = "";
            // server = "sql6.freesqldatabase.com";
            // database = "sql6700091";
            // username = "sql6700091";
            // password = "G4b8ziiLVv";
            mySQLConnection();
        }

        private void txtVoterNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sql = "SELECT * FROM voters WHERE voternumber=@voternumber";
                var voternumber = int.Parse(txtVoterNumber.Text);
                cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@voternumber", voternumber);
                var myResult = cmd.ExecuteReader();
                myResult.Read();
                if (myResult.HasRows)
                {
                    string lastname = myResult.GetValue(1).ToString().Trim();
                    string firstname = myResult.GetValue(2).ToString().Trim();
                    string middleinit = myResult.GetValue(3).ToString().Trim();
                    var justinit = middleinit[0];
                    txtVoterName.Foreground = Brushes.Black;
                    txtVoterName.Text = firstname+ " " +justinit+". "+lastname;
                    btnProceed.Visibility = Visibility.Visible;
                    txtSelectCandidate.Text=txtVoterName.Text;
                    
                } 
                else 
                {
                    txtVoterName.Foreground = Brushes.Red;
                    txtVoterName.Text = "That is not a valid voter number";
                    btnProceed.Visibility = Visibility.Hidden;
                }
                myResult.Close();
            }
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e)
        {
            borderGetVoterNumber.Visibility = Visibility.Hidden;
            txtSelectCandidate.Visibility = Visibility.Visible;
            txtSelectedCandidates.Visibility = Visibility.Visible;
            spSelection.Visibility = Visibility.Visible;
            spButtonControl.Visibility = Visibility.Visible;

            // get voters name in a list
            string midinit = "";
            var fullname = new List<String>();            
            string sql1 = "SELECT id, firstname, lastname, middlename FROM voters ORDER BY firstname, lastname";
            cmd = new MySqlCommand(sql1, conn);
            var myResult2 = cmd.ExecuteReader();
            while (myResult2.Read())
            {
                midinit = myResult2["middlename"].ToString();
                fullname.Add(myResult2["firstname"].ToString() + " " + midinit[0] +". "+myResult2["lastname"].ToString() +"-" + myResult2["id"].ToString());                
            }
            myResult2.Close();
            // better close this reader after we finish.

            sql = "SELECT * FROM candidates WHERE withdrawn=1 ORDER BY party";
            // Query the candidates table and exclude withdrawn entries            
            cmd = new MySqlCommand(sql, conn);            
            var myResult = cmd.ExecuteReader();
            string candidateItem="";
            string candidateName = "";
            string candidID = "";
            string myTemp = "";
            string tempVoterID = "";
            

            while (myResult.Read())               
            {                
                candidID = myResult["votersid"].ToString().Trim();
                // txtSelectedCandidates.Text = candidID;
                // loop thru voter's list to find the candidate's name
                for (int i=0; i < fullname.Count; i++ )
                {                    
                    myTemp = fullname[i];                    
                    int charPos = myTemp.IndexOf("-");
                    tempVoterID = myTemp.Substring(charPos + 1);
                    if (candidID == tempVoterID)
                    {
                        // we have the voter's name ?
                        candidateItem = myTemp.Substring(0,charPos)+" -- "+myResult["position"].ToString() + " -- " + myResult["party"].ToString();
                        break;                       
                        // we exit this loop
                    }                    
                }
                // finally, add the name (from voters table) , position and party (from candidates table) to the list
                candidateList.Items.Add(candidateItem);
            }
            
            myResult.Close();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            this.Hide();
            main.Show();
        }

        private void candidateList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var targetCandidate = candidateList.SelectedItem;
            votersList.Items.Add(targetCandidate);
            candidateList.Items.Remove(targetCandidate);
            
        }

        private void votersList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var targetCandidate = votersList.SelectedItem;
            candidateList.Items.Add(targetCandidate);
            votersList.Items.Remove(targetCandidate);
            
        }

        private void btnResetSelection_Click(object sender, RoutedEventArgs e)
        {
            
            for (int i = 0; i < votersList.Items.Count; i++)
            {
                candidateList.Items.Add(votersList.SelectedItem);
                votersList.Items.Remove(votersList.SelectedItem);
            }
        }
    }
}
