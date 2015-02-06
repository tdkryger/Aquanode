using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using Excel;

namespace aqualog2aquanode
{
    public partial class Form1 : Form
    {
        private string imagePath = @"D:\Dropbox\Photos\2011-03-24 jjphoto\Freshwater\";

        public Form1()
        {
            InitializeComponent();
        }

        private void executeSQL(MySqlConnection con, string sql)
        {
            using (MySqlCommand cmd = new MySqlCommand(sql, con))
                cmd.ExecuteNonQuery();
        }

        private void doTheFish()
        {
            int counter = 0;
            using (MySqlConnection con = new MySqlConnection(getConnectionString()))
            {
                con.Open();
                using (FileStream stream = File.Open(@"D:\Dropbox\Akvarieting\DATABASE FISH 1.01.xls", FileMode.Open, FileAccess.Read))
                {
                    using (IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream))
                    {
                        //excelReader.IsFirstRowAsColumnNames = true;
                        //DataSet result = excelReader.AsDataSet();

                        excelReader.Read(); // first row is column names

                        while (excelReader.Read())
                        {
                            string fishCodeString = fixDataString(excelReader.GetString(1));
                            int fishCode = getFishCodeId(con, fishCodeString);
                            string genus = fixDataString(excelReader.GetString(2));
                            string species = fixDataString(excelReader.GetString(3));
                            string germanTradename = fixDataString(excelReader.GetString(11));
                            string englishTradename = fixDataString(excelReader.GetString(12));
                            string synonyms = fixDataString(excelReader.GetString(13));
                            string origins = fixDataString(excelReader.GetString(14));
                            string maxSize = fixDataString(excelReader.GetString(15));
                            string tempRangeString = fixDataString(excelReader.GetString(16));
                            string phRangeString = fixDataString(excelReader.GetString(17));
                            string tempRange = string.Empty;
                            string phRange = string.Empty;
                            switch (tempRangeString)
                            {
                                case "c":
                                    tempRange = "10-22 °C";
                                    break;
                                case "cc":
                                    tempRange = "18-22 °C";
                                    break;
                                case "C":
                                    tempRange = "22-25 °C";
                                    break;
                                case "CC":
                                    tempRange = "24-29 °C";
                                    break;
                                default:
                                    tempRange = tempRangeString;
                                    break;
                            }
                            switch (phRangeString)
                            {
                                case "p":
                                    phRange = "5,8-6,5";
                                    break;
                                case "pp":
                                    phRange = "6,5-7,2";
                                    break;
                                case "P":
                                    phRange = "7,5-8,5";
                                    break;
                                case "PP":
                                    phRange = "Salt addition";
                                    break;
                                default:
                                    phRange = phRangeString;
                                    break;
                            }

                            toolStripStatusLabel1.Text = string.Format("Current fish: {0} {1}", genus, species);
                            toolStripStatusLabel2.Text = string.Format("Count: {0}", counter);
                            counter++;
                            Application.DoEvents();

                            string insertSQL = string.Format("INSERT INTO `fish` (`fishgroupid`,`genus`,`species`,`germanTradename`,`englishTradename`,`danishTradename`,"
                                + "`origin`,`maxSize`,`temperatureRange`,`phRange`) VALUES ({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}'); SELECT last_insert_id();",
                                fishCode,
                                genus,
                                species,
                                germanTradename,
                                englishTradename,
                                "",
                                origins,
                                maxSize,
                                fixDataString(tempRange),
                                fixDataString(phRange));
                            using (MySqlCommand cmd = new MySqlCommand(insertSQL, con))
                            {
                                int fishId = Convert.ToInt32(cmd.ExecuteScalar());
                                //getImages(con, imagePath, genus, species, fishId);
                            }
                        }

                        excelReader.Close();
                    }
                }
            }
        }

        private int getFishCodeId(MySqlConnection con, string fishCode)
        {
            int returnValue = 0;
            string selectSQL = string.Format("SELECT fishgroupid FROM fishgroup WHERE fishgroupCode = '{0}'", fishCode);
            using (MySqlCommand cmd = new MySqlCommand(selectSQL, con))
            {
                object o = cmd.ExecuteScalar();
                if (o != null)
                {
                    returnValue = Convert.ToInt32(o);
                }
                else
                {
                    cmd.CommandText = string.Format("INSERT INTO fishgroup (fishgroupCode) VALUES ('{0}'); SELECT last_insert_id();", fishCode);
                    returnValue = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return returnValue;
        }

        private string fixDataString(string value)
        {
            string returnValue = string.Empty;
            if (value != null)
            {
                returnValue = value.Replace("\\", " ");
                //returnValue = returnValue.Replace("/", "");
                returnValue = returnValue.Replace("'", "''");
            }
            return returnValue;
        }

        private string getConnectionString()
        {
            return string.Format("Server={0};Database={1};Uid={2};Pwd={3};", "192.168.1.49", "aquanode", "thomas", "onakit8m");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            try
            {
                doTheFish();
            }
            finally
            {
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }


        private void getImages(MySqlConnection con, string path, string genus, string species, int fishId)
        {
            string searchPattern = string.Format("{0}_{1}*.jpg", genus, species);
            try
            {
                string[] files = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
                foreach (string s in files)
                {
                    using (MySqlCommand cmd = new MySqlCommand("INSERT INTO images (imageName, imageData) VALUES (@imageName, @imageData); SELECT last_insert_id();", con))
                    {
                        cmd.Parameters.AddWithValue("@imageName", s.Replace(path, "").Replace("_", " ").Replace("\\", ""));
                        cmd.Parameters.AddWithValue("@imageData", loadImageData(Path.Combine(path, files[0])));
                        int imageId = Convert.ToInt32(cmd.ExecuteScalar());
                        using (MySqlCommand cmd2 = new MySqlCommand("INSERT INTO fishImage (fishid, imageid) VALUES (@fishid, @imageid)", con))
                        {
                            cmd2.Parameters.AddWithValue("@fishid", fishId);
                            cmd2.Parameters.AddWithValue("@imageid", imageId);
                            cmd2.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private byte[] loadImageData(string filename)
        {
            byte[] ImageData;
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    ImageData = br.ReadBytes((int)fs.Length);
                    br.Close();
                    fs.Close();
                }
            }
            return ImageData;
        }

        private int getPlantGroupId(MySqlConnection con, string plantCode)
        {
            int returnValue = 0;
            string selectSQL = string.Format("SELECT id FROM plantgroup WHERE plantgroupCode = '{0}'", plantCode);
            using (MySqlCommand cmd = new MySqlCommand(selectSQL, con))
            {
                object o = cmd.ExecuteScalar();
                if (o != null)
                {
                    returnValue = Convert.ToInt32(o);
                }
                else
                {
                    cmd.CommandText = string.Format("INSERT INTO plantgroup (plantgroupCode) VALUES ('{0}'); SELECT last_insert_id();", plantCode);
                    returnValue = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return returnValue;
        }

        private int getPlantGrowth(MySqlConnection con, string code)
        {
            int returnValue = 0;
            string selectSQL = string.Format("SELECT plantgrowthid FROM plantgrowth WHERE code = '{0}'", code);
            using (MySqlCommand cmd = new MySqlCommand(selectSQL, con))
            {
                object o = cmd.ExecuteScalar();
                if (o != null)
                {
                    returnValue = Convert.ToInt32(o);
                }
            }
            return returnValue;
        }

        private void handlePlantCare(MySqlConnection con, string careString, int plantId)
        {
            executeSQL(con, string.Format("DELETE FROM plant2plantcare WHERE plantid={0}", plantId));
            string[] careParts = careString.Split(',');
            foreach (string s in careParts)
            {
                int value = 0;
                if (int.TryParse(s, out value))
                {
                    string insertSQL = string.Format("INSERT INTO plant2plantcare (plantid, plantcareid) VALUES ({0}, {1})", plantId, value);
                    executeSQL(con, insertSQL);
                }
            }
        }

        private void handleCharacteristics(MySqlConnection con, string CharacteristicsString, int plantId)
        {
            executeSQL(con, string.Format("DELETE FROM plant2characteristic WHERE plantid={0}", plantId));
            string[] careParts = CharacteristicsString.Split(',');
            foreach (string s in careParts)
            {
                int value = 0;
                if (int.TryParse(s, out value))
                {
                    string insertSQL = string.Format("INSERT INTO plant2characteristic (plantid, characteristicidid) VALUES ({0}, {1})", plantId, value);
                    executeSQL(con, insertSQL);
                }
            }
        }

        private void doThePlants()
        {
            int counter = 0;
            using (MySqlConnection con = new MySqlConnection(getConnectionString()))
            {
                con.Open();
                using (FileStream stream = File.Open(@"D:\Dropbox\Akvarieting\DATABASE PLANT 1.0.xls", FileMode.Open, FileAccess.Read))
                {
                    using (IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream))
                    {

                        excelReader.Read(); // first row is column names

                        while (excelReader.Read())
                        {
                            int newPlantId = 0;
                            string CodeString = fixDataString(excelReader.GetString(1));
                            int codeId = getPlantGroupId(con, CodeString);
                            string genus = fixDataString(excelReader.GetString(2));
                            string species = fixDataString(excelReader.GetString(3));
                            string germanName = excelReader.GetString(6);
                            string englishName = excelReader.GetString(7);
                            string synonyms = excelReader.GetString(9);
                            string origin = excelReader.GetString(10);
                            string height = excelReader.GetString(11);
                            string width = excelReader.GetString(12);
                            string gh = excelReader.GetString(13);
                            int growthid = getPlantGrowth(con, excelReader.GetString(14));
                            string lightRange = excelReader.GetString(15);
                            string[] lightParts = lightRange.Split('-');
                            int minLight = 999; 
                            int maxLight = 999;
                            if (lightParts.Length > 0)
                            {
                                int.TryParse(lightParts[0], out minLight);
                                if (lightParts.Length > 1)
                                {
                                    int.TryParse(lightParts[0], out maxLight);
                                }
                            }
                            string temperature = fixDataString(excelReader.GetString(16));
                            string ph = fixDataString(excelReader.GetString(17));
                            string bottomType = fixDataString(excelReader.GetString(20));
                            string substrateString = fixDataString(excelReader.GetString(21));
                            bool subFert = false;
                            bool fluidFert = false;
                            bool co2 = false;
                            if (substrateString.Contains('A'))
                            {
                                subFert = true;
                            }
                            if (substrateString.Contains('B'))
                            {
                                fluidFert = true;
                            }
                            if (substrateString.Contains('C'))
                            {
                                co2 = true;
                            }

                            toolStripStatusLabel1.Text = string.Format("Current plant: {0} {1}", genus, species);
                            toolStripStatusLabel2.Text = string.Format("Count: {0}", counter);
                            counter++;
                            Application.DoEvents();

                            string insertSQL = "INSERT INTO `plants` (`plantgroupid`,`plantgrowthid`,`minlightid`,`maxlightid`,`genus`,`species`,`germanTradename`,`englishTradename`,`danishTradename`,`synonyms`,`origin`,`height`,`width`,`gh`,`ph`,`temperaturerange`,`bottomtype`,`substratefertilisor`,`fluidfertilisor`,`co2`) VALUES "
                                        + "(@plantgroupid,@plantgrowthid,@minlightid,@maxlightid,@genus,@species,@germanTradename,@englishTradename,@danishTradename,@synonyms,@origin,@height,@width,@gh,@ph,@temperaturerange,@bottomtype,@substratefertilisor,@fluidfertilisor,@co2); "
                                        + "SELECT last_insert_id();";

                            using (MySqlCommand cmd = new MySqlCommand(insertSQL, con))
                            {
                                cmd.Parameters.AddWithValue("@plantgroupid", codeId);
                                cmd.Parameters.AddWithValue("@plantgrowthid", growthid);
                                cmd.Parameters.AddWithValue("@minlightid", minLight);
                                cmd.Parameters.AddWithValue("@maxlightid", maxLight);
                                cmd.Parameters.AddWithValue("@genus", genus);
                                cmd.Parameters.AddWithValue("@species", species);
                                cmd.Parameters.AddWithValue("@germanTradename", germanName);
                                cmd.Parameters.AddWithValue("@englishTradename", englishName);
                                cmd.Parameters.AddWithValue("@danishTradename", string.Empty);
                                cmd.Parameters.AddWithValue("@synonyms", synonyms);
                                cmd.Parameters.AddWithValue("@origin", origin);
                                cmd.Parameters.AddWithValue("@height", height);
                                cmd.Parameters.AddWithValue("@width", width);
                                cmd.Parameters.AddWithValue("@gh", gh);
                                cmd.Parameters.AddWithValue("@ph", ph);
                                cmd.Parameters.AddWithValue("@temperaturerange", temperature);
                                cmd.Parameters.AddWithValue("@bottomtype", bottomType);
                                cmd.Parameters.AddWithValue("@substratefertilisor", subFert);
                                cmd.Parameters.AddWithValue("@fluidfertilisor", fluidFert);
                                cmd.Parameters.AddWithValue("@co2", co2);
                                newPlantId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            if (newPlantId > 0)
                            {
                                handlePlantCare(con, excelReader.GetString(18), newPlantId);
                                handleCharacteristics(con, excelReader.GetString(19), newPlantId);

                            }
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            button1.Enabled = false;
            button2.Enabled = false;
            try
            {
                doThePlants();
            }
            finally
            {
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }
    }
}
