using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Asistencia
{
    public partial class Principal : Form
    {
        public Principal()
        {
            InitializeComponent();

            btn.Enabled = false;
            Cargar();
        }

        private async void Cargar()
        {
          var data=await RecursosHumanosWebApi.Instancia.GetParametroPersonal();
            List<JsonRegistroPersonal> registros = (List<JsonRegistroPersonal>)data;


            cbl.DataSource = registros;

            // Definir qué propiedad mostrar
            cbl.DisplayMember = "NombreCompleto";

            // Opcional: definir el valor interno
            cbl.ValueMember = "CodigoColaborador";
            btn.Enabled = true;
            cbl.Focus();
        }

        private void cbStatus_CheckedChanged(object sender, EventArgs e)
        {
            if (cbStatus.Checked)
            {
                for (int i = 0; i < cbl.Items.Count; i++)
                {
                    cbl.SetItemChecked(i, true);

                }
            }
            else
            {
                for (int i = 0; i < cbl.Items.Count; i++)
                {

                    cbl.SetItemChecked(i, false);
                }
            }
        }

        private async void btn_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            List<string> seleccionados = new List<string>();

            for (int i = 0; i < cbl.Items.Count; i++)
            {
                if (cbl.GetItemChecked(i))
                {
                    seleccionados.Add(((JsonRegistroPersonal)cbl.Items[i]).CodigoColaborador);
                }
            }

            // Aquí puedes procesar los registros seleccionados
           await ActualizaHorario.Actualizar(dtpInicio.Value.Date, dtpFin.Value.Date, seleccionados);
            MessageBox.Show("Proceso Terminado");
            this.Cursor = Cursors.Default;
        }
    }
}
