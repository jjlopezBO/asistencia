using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistencia
{
    
   public class ConsultarHorasTrabajadasResponse
    {
        public bool Exito { get; set; }
        public List<object> Mensajes { get; set; }
        public List<Asistencia> Asistencia { get; set; }

        public List<Asistencia> GetIngresosByFecha(DateTime fecha, bool manana)
        {
            List<Asistencia> rtn = new List<Asistencia>();

            foreach (Asistencia item in this.Asistencia)
            {
                if (fecha == item.ParseFecha())
                {
                    if ((item.EntradaEfectiva != null)/* && (item.SalidaEfectiva!= null)*/&& (item.EntradaProgramada != null) && (item.EntradaProgramada != null))
                    {
                        // if (item.ParseEntradaEfectiva().Hour < 12)
                        {
                            rtn.Add(item);
                        }
                    }
                }
            }

            return rtn.OrderBy(a => int.Parse(a.CodigoEmpleado))
                .ThenBy(a => TimeSpan.Parse(a.EntradaProgramada))
                .ToList();
        }
        public List<Asistencia> GetHorariosProgramadosoByFechaYColaboradaor(DateTime fecha, string colaborador)
        {
            List<Asistencia> rtn = new List<Asistencia>();
            foreach (Asistencia item in this.Asistencia)
            {
                if (fecha == item.ParseFecha() && colaborador == item.CodigoEmpleado && item.TieneProgramado())
                {
                    rtn.Add(item);
                }
            }

            return rtn;
        }
    }


    public class Asistencia
    {
        public string CodigoEmpleado { get; set; }
        public string UnidadOrganizativa { get; set; }
        public string CentroCosto { get; set; }
        public string Fecha { get; set; }
        public string Dia { get; set; }
        public string EntradaProgramada { get; set; }
        public string SalidaProgramada { get; set; }
        public double TiempoProgramado { get; set; }
        public string EntradaEfectiva { get; set; }
        public string SalidaEfectiva { get; set; }
        public double TiempoEfectivo { get; set; }
        public double TiempoEfectivoDentroHorario { get; set; }
        public double TiempoEfectivoFueraHorario { get; set; }
        public double HorasExtrasAprobadas { get; set; }
        public double HorasCompensacionAprobada { get; set; }
        public double PermisosAprobados { get; set; }
        public double VacacionesAprobadas { get; set; }



        public DateTime ParseFecha()
        {
            return DateTime.ParseExact(Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public DateTime ParseEntradaEfectiva()
        {


            DateTime rtn = DateTime.Now.Date;

            if (EntradaEfectiva == null)
            {
                rtn = DateTime.ParseExact(Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            else
            {
                rtn = DateTime.ParseExact(Fecha + " " + EntradaEfectiva, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            return rtn;
        }

        public DateTime ParseEntradaProgramada()
        {
            DateTime rtn = DateTime.Now.Date;

            if (EntradaProgramada == null)
            {
                rtn = DateTime.ParseExact(Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            else
            {
                rtn = DateTime.ParseExact(Fecha + " " + EntradaProgramada, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }
            return rtn;
        }

        public DateTime ParseSalidaProgramada()
        {


            DateTime rtn = DateTime.Now.Date;

            if (SalidaProgramada == null)
            {
                rtn = DateTime.ParseExact(Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            else
            {
                rtn = DateTime.ParseExact(Fecha + " " + SalidaProgramada, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }
            return rtn;
        }


        public int HoraaInt()
        {
            DateTime time = DateTime.ParseExact(EntradaEfectiva, "HH:mm:ss", CultureInfo.InvariantCulture);
            string formattedTimeString = time.ToString("HHmmss");
            int formattedTime = int.Parse(formattedTimeString);
            return formattedTime;
        }
        public TimeSpan CalcularTiempoProgramado()
        {
            TimeSpan tiempoTrabajado = TimeSpan.Zero;
            // Convertir las cadenas a TimeSpan

            if (EntradaProgramada == null || SalidaProgramada == null)
            { }
            else
            {
                TimeSpan entrada = TimeSpan.Parse(EntradaProgramada);
                TimeSpan salida = TimeSpan.Parse(SalidaProgramada);

                // Calcular la diferencia
                tiempoTrabajado = salida - entrada;
            }
            // Devolver la diferencia como un TimeSpan
            return tiempoTrabajado;
        }
        public static TimeSpan CalcularTiempoDeTrabajo(List<Asistencia> lista)
        {
            TimeSpan rtn = new TimeSpan(0, 0, 0);
            foreach (Asistencia a in lista)
            {
                rtn += a.CalcularTiempoProgramado();
            }

            return rtn;
        }

        public bool TieneProgramado()
        {
            bool rtn = true;

            if (EntradaProgramada == null || SalidaProgramada == null)
            { rtn = false; }

            return rtn;
        }

    }


}
