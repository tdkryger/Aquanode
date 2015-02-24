using System;
using System.Collections.Generic;
//using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace AquaNodeWeb
{
    public partial class index : System.Web.UI.Page
    {
        private MySqlConnection _con = null;
        //private string _conStringLocal = @"server=localhost;userid=anuser;password=mWZwbVCruMsh;database=aquanode";
        //private string _conStringRemote = @"server=192.168.1.49;userid=thomas;password=onakit8m;database=aquanode";
        private string _conString = string.Empty;

        private string _selectAllTanks = "SELECT `tankid`,`tankname`,`nodeid`,`description`,`targetph`,`targettemperature`,`targetdisolvedOxygen`,`targetelectricalConductivity`,`targetsalinity`,`targetgravityOfSeawater`,`targettotalDissolvedSolids` FROM `fishtank` ORDER BY `tankname`;";

        protected void Page_Load(object sender, EventArgs e)
        {
            _conString = ConfigurationManager.ConnectionStrings["mysql"].ConnectionString;
            if (isLocalClient())
                litLocal.Text = "Local";
            else
                litLocal.Text = "Remote";

            if (!Page.IsPostBack)
            {
                getTanks();
            }
        }

        protected bool isLocalClient()
        {
            string clientAddress = Request.UserHostAddress;

            try
            { // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(clientAddress);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP)) 
                        return true;
                    // is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) 
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }

        protected MySqlConnection getMySQLConnection()
        {
            if (_con == null)
            {
                _con = new MySqlConnection(_conString);
            }
            switch (_con.State)
            {
                case System.Data.ConnectionState.Broken:
                    _con = new MySqlConnection(_conString);
                    break;
                case System.Data.ConnectionState.Closed:
                    _con.Open();
                    break;
                case System.Data.ConnectionState.Connecting:
                    System.Threading.Thread.Sleep(10);
                    break;
                case System.Data.ConnectionState.Executing:
                    System.Threading.Thread.Sleep(10);
                    break;
                case System.Data.ConnectionState.Fetching:
                    System.Threading.Thread.Sleep(10);
                    break;
                case System.Data.ConnectionState.Open:
                    break;
            }
            return _con;
        }

        protected void getTanks()
        {
            using (MySqlCommand cmd = new MySqlCommand(_selectAllTanks, getMySQLConnection()))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    ddlSelectTank.Items.Clear();
                    while (reader.Read())
                    {
                        int tankId = reader.GetInt32(0);
                        string tankName = reader.GetString(1);
                        ListItem li = new ListItem(tankName, tankId.ToString());
                        ddlSelectTank.Items.Add(li);
                    }
                }
            }

        }

        protected void ddlSelectTank_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selTankId = 0;
            if (int.TryParse(ddlSelectTank.SelectedItem.Value, out selTankId))
            {
                tableTank.Visible = true;
                using (MySqlCommand cmd = new MySqlCommand("SELECT `tankid`,`tankname`,`nodeid`,`description`,`targetph`,`targettemperature`,`targetdisolvedOxygen`,`targetelectricalConductivity`,`targetsalinity`,`targetgravityOfSeawater`,`targettotalDissolvedSolids` FROM `fishtank` WHERE tankid=@tankid;", getMySQLConnection()))
                {
                    litTankName.Text = "Tank name: "; //TODO: Localization
                    cmd.Parameters.AddWithValue("@tankid", selTankId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int nodeId = reader.GetInt32("nodeid");
                            lTankDescription.Text = reader.GetString("description");
                            lTankName.Text = reader.GetString("tankname");
                        }
                    }
                }
            }
            else
            {
                tableTank.Visible = false;
            }
        }
    }
}