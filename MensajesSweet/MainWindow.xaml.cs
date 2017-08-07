using MensajesSweet.Entidades;
using MensajesSweet.LogicaNegocios;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MensajesSweet
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MensajesBol mensajes = new MensajesBol();
            dgMensajes.ItemsSource = mensajes.Todos();
        }

        private void btnLeer_Click(object sender, RoutedEventArgs e)
        {
            EMensaje mensaje = ((Button)sender).Tag as EMensaje;
            MessageBox.Show(mensaje.Documento.ToString());
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connMensajes"].ToString()))
            {
                conn.Open();
                SqlTransaction txn = conn.BeginTransaction();
                string sql = "SELECT DATA.PathName(), GET_FILESTREAM_TRANSACTION_CONTEXT(), NOMBRE FROM Documents WHERE ID="+mensaje.Documento;
                SqlCommand cmd = new SqlCommand(sql, conn, txn);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    string filePath = rdr[0].ToString();
                    byte[] objContext = (byte[])rdr[1];
                    string fName = rdr[2].ToString();

                    SqlFileStream sfs = new SqlFileStream(filePath, objContext, System.IO.FileAccess.Read);

                    byte[] buffer = new byte[(int)sfs.Length];
                    sfs.Read(buffer, 0, buffer.Length);
                    sfs.Close();

                    string result = System.IO.Path.GetTempPath();
                    string filename = result+ @"\" + fName;

                    System.IO.FileStream fs = new System.IO.FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write);
                    fs.Write(buffer, 0, buffer.Length);
                    fs.Flush();
                    fs.Close();
                    System.Diagnostics.Process obj = new System.Diagnostics.Process();
                    obj.StartInfo.FileName = filename;
                    obj.Start();
                }

                rdr.Close();
                txn.Commit();
                conn.Close();
            }
        }
    }
}
