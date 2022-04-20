using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;

namespace Tema2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            getInformation();

            InitializeComponent();

            labelAndTextBox();
        }

        SqlDataAdapter dAdapter;
        DataSet dSet;
        SqlConnection connection = new SqlConnection("Integrated Security=true;Initial Catalog=Florarie;Data Source=LAPTOP-N16I6V2I\\SQLEXPRESS;");
        
        string connectionString = ConfigurationManager.AppSettings.Get("connectionString");
        string parentTableName = ConfigurationManager.AppSettings.Get("parentTable");
        string childTableName = ConfigurationManager.AppSettings.Get("childTable");

        string parentKey;
        string childKey;
        string foreignKey;  //in child table

        List<String> parentColumns = new List<String>();
        List<String> childColumns = new List<String>();

        List<Label> labelList = new List<Label>();
        List<TextBox> textBoxList = new List<TextBox>();

        
        private void getInformation()
        {
            //in this method we get all the information about our 2 tables
            //like the foreign key, the primary keys and the column names of both tables

            //parent table PK
            SqlCommand sqlCommand = new SqlCommand("select COLUMN_NAME " +
                                                    "from INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                                                    "where TABLE_NAME like '" + parentTableName + "' " +
                                                    "and CONSTRAINT_NAME like 'PK%'", connection);
            connection.Open();
            parentKey = (string)sqlCommand.ExecuteScalar();
            connection.Close();


            //child table PK
            sqlCommand = new SqlCommand("select COLUMN_NAME " +
                                        "from INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                                         "where TABLE_NAME like '" + childTableName + "' " +
                                         "and CONSTRAINT_NAME like 'PK%'", connection);
            connection.Open();
            childKey = (string)sqlCommand.ExecuteScalar();
            connection.Close();


            //child table FK
            sqlCommand = new SqlCommand("select COLUMN_NAME " +
                                        "from INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                                         "where TABLE_NAME like '" + childTableName + "' " +
                                         "and CONSTRAINT_NAME like 'FK%'", connection);
            connection.Open();
            foreignKey = (string)sqlCommand.ExecuteScalar();
            connection.Close();


            //parent table columns
            sqlCommand = new SqlCommand("select COLUMN_NAME " +
                                        "from INFORMATION_SCHEMA.COLUMNS " +
                                        "where TABLE_NAME = '" + parentTableName + "'", connection);
            connection.Open();
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

            while (sqlDataReader.Read())
            {
                parentColumns.Add(sqlDataReader.GetString(0));
            }
            sqlDataReader.Close();
            connection.Close();


            //child table columns
            sqlCommand = new SqlCommand("select COLUMN_NAME " +
                                        "from INFORMATION_SCHEMA.COLUMNS " +
                                        "where TABLE_NAME = '" + childTableName + "'", connection);
            connection.Open();
            sqlDataReader = sqlCommand.ExecuteReader();

            while (sqlDataReader.Read())
            {
                childColumns.Add(sqlDataReader.GetString(0));
            }
            sqlDataReader.Close();
            connection.Close();

        }


        private void labelAndTextBox()
        { 
            //in this method we create labels and textboxes with the names of the columns
            //so that we can display them and make the interface user friendly

            for (int i = 0; i < childColumns.Count(); i++)
            {
                Label label = new Label();
                TextBox textBox = new TextBox();

                label.Text = childColumns[i] + ":";
                labelList.Add(label);
                labelList[i].Visible = true;

                textBox.Name = childColumns[i];
                textBoxList.Add(textBox);

                label.Location = new System.Drawing.Point(200, 235 + 30 * i);
                textBox.Location = new System.Drawing.Point(300, 235 + 30 * i);

                Controls.Add(labelList[i]);
                Controls.Add(textBox);

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //connect button
            //we fill the parent datagrid

            dAdapter = new SqlDataAdapter("select * from " + parentTableName, connectionString);
            dSet = new DataSet();
            dAdapter.Fill(dSet);
            dataGridView1.DataSource = dSet.Tables[0].DefaultView;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //with this method we show in the second datagrid the child tuples that belong to the pressed parent row 
            //we also bind the foreign key (parent's primary key) to a textbox

            //search the parent table's primary key
            int parentKeyPosition = -1;

            for (int i = 0; i < parentColumns.Count; i++)
            {
                if (parentColumns[i].Equals(parentKey))
                    parentKeyPosition = i;
            }

            //the parent table's primary key is selected from the clicked row
            string parentKeyValue = dataGridView1[parentKeyPosition, dataGridView1.CurrentCell.RowIndex].Value.ToString();


            foreach (Control control in Controls)
            {
                //control has labels too, here we only need the textboxes
                if (control is TextBox)
                {
                    //the foreign key (that is the parent's primary key) must be disabled
                    //because the user isn't allowed to change it
                    if (control.Name.Equals(foreignKey))
                    {
                        ((TextBox)control).Text = parentKeyValue;
                        ((TextBox)control).Enabled = false;
                    }
                    //other text boxes are cleared and enabled
                    else
                    {
                        ((TextBox)control).Clear();
                        ((TextBox)control).Enabled = true;
                    }
                }
            }


            dAdapter = new SqlDataAdapter("select * from " + childTableName + " where " + foreignKey + " = @p", connectionString);

            dAdapter.SelectCommand.Parameters.Add("@p", SqlDbType.Int, 10).Value = dataGridView1.Rows[e.RowIndex].Cells[parentKeyPosition].Value;

            dSet = new System.Data.DataSet();
            dAdapter.Fill(dSet);

            dataGridView2.DataSource = dSet.Tables[0].DefaultView;

        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //in this method we bind the columns of the child table to textboxes

            //search the parent table's primary key
            int parentKeyPosition = -1;

            for (int i = 0; i < parentColumns.Count; i++)
            {
                if (parentColumns[i].Equals(parentKey))
                    parentKeyPosition = i;
            }

            //the child key's and foreign key's positions must be searched for
            int childKeyPosition = -1;
            int foreignKeyPosition = -1;

            for (int i = 0; i < childColumns.Count; i++)
            {
                if (childColumns[i].Equals(childKey))
                    childKeyPosition = i;
                if (childColumns[i].Equals(foreignKey))
                    foreignKeyPosition = i;
            }
           

            //here we bind the pressed row from the child table to textboxes
            int columnIndex = 0;
            foreach (Control control in Controls)
            {
                TextBox t = new TextBox();
                if(columnIndex < childColumns.Count())
                {
                    t.Name = childColumns[columnIndex];
                }
                
                //control has labels too, here we only need the textboxes
                if (control is TextBox)
                {
                    //data from the clicked row is filled in
                    ((TextBox)control).Text = dataGridView2[columnIndex, dataGridView2.CurrentCell.RowIndex].Value.ToString();
                    columnIndex++;

                    //the foreign key (that is the parent's primary key) must be disabled
                    //because the user isn't allowed to change it
                    if (control.Name.Equals(foreignKey))
                    {
                        ((TextBox)control).Enabled = false;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //insert

            try
            {
                string columnNamesForParameters = "@";
                for (int i = 0; i < childColumns.Count(); i++)
                {
                    if (i < childColumns.Count() - 1)
                        columnNamesForParameters += childColumns[i] + ",@";
                    else
                        columnNamesForParameters += childColumns[i];
                }

                string insertQuery = "insert into " + childTableName + " values (" + columnNamesForParameters + ")";

                SqlDataAdapter cmd = new SqlDataAdapter();
                cmd.InsertCommand = new SqlCommand(insertQuery, connection);
               

                int columnIndex = 0;
                foreach (Control control in Controls)
                {
                    //control has labels too, here we only need the textboxes
                    if (control is TextBox)
                    {
                        cmd.InsertCommand.Parameters.AddWithValue("@" + childColumns[columnIndex], ((TextBox)control).Text);
                        columnIndex++;
                    }
                }

                connection.Open();
                cmd.InsertCommand.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error");
                connection.Close();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //delete
            try
            {
                //the child key's positions must be searched for
                int childKeyPosition = -1;

                for (int i = 0; i < childColumns.Count; i++)
                {
                    if (childColumns[i].Equals(childKey))
                        childKeyPosition = i;
                }


                string deleteQuery = "delete from " + childTableName + " where " + childKey + " = @" + childKey;
                SqlCommand cmd = new SqlCommand(deleteQuery, connection);

                //we add the parameters to the command
                int columnIndex = 0;
                foreach (Control control in Controls)
                {
                    //control has labels too, here we only need the textboxes
                    if (control is TextBox)
                    {
                        if(((TextBox)control).Name.Equals(childKey)) { 
                            cmd.Parameters.AddWithValue("@" + childColumns[columnIndex], ((TextBox)control).Text);
                            break;
                        }
                        columnIndex++;
                    }
                }

                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error");
                connection.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //update
            try
            {
                //here we make a string that looks like: col1Name = @col1Name, ..., col_nName = @col_nName
                string columNameWithParameters = "";
                for (int i = 0; i < childColumns.Count(); i++)
                {
                    if(!childColumns[i].Equals(childKey) && !childColumns[i].Equals(foreignKey))
                    {
                        columNameWithParameters += childColumns[i] + " = @" + childColumns[i] + ", "; 
                    }
                }
                //here we cut the ", " from the end of the string
                columNameWithParameters = columNameWithParameters.Remove(columNameWithParameters.Length - 2);

                string updateQuery = "update " + childTableName + " set " + columNameWithParameters + " where " + childKey + " =  @" + childKey;

                SqlCommand cmd = new SqlCommand(updateQuery, connection);

                //here we add the parameters to the command
                int columnIndex = 0;
                foreach (Control control in Controls)
                {
                    //control has labels too, here we only need the textboxes
                    if (control is TextBox)
                    {
                        if (!childColumns[columnIndex].Equals(foreignKey))
                        {
                            cmd.Parameters.AddWithValue("@" + childColumns[columnIndex], ((TextBox)control).Text);
                        }
                        columnIndex++;
                    }
                }

                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error");
                connection.Close();
            }

        }
    }
}
