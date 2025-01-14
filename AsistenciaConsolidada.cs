using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistencia
{
    public class AsistenciaConsolidada
    {
        public string CodigoEmpleado { get; set; }
        public string UnidadOrganizativa { get; set; }
        public string CentroCosto { get; set; }
        public DateTime DFecha { get; set; }
        public string Dia { get; set; }
        public DateTime? EntradaProgramadaManana { get; set; }
        public DateTime? EntradaProgramadaManana0 { get; set; }
        public DateTime? SalidaProgramadaManana { get; set; }
        public DateTime? EntradaProgramadaTarde { get; set; }
        public DateTime? SalidaProgramadaTarde { get; set; }
        public DateTime? SalidaProgramadaTarde0 { get; set; }
        public DateTime? EntradaEfectivaManana { get; set; }
        public DateTime? SalidaEfectivaManana { get; set; }
        public DateTime? EntradaEfectivaTarde { get; set; }
        //  public DateTime? SalidaEfectivaTarde { get; set; }
        public double TiempoProgramadoTotal { get; set; }
        public double TiempoEfectivoTotal { get; set; }
        public double HorasExtrasAprobadas { get; set; }
        public double HorasCompensacionAprobada { get; set; }
        public double PermisosAprobados { get; set; }
        public double VacacionesAprobadas { get; set; }
        public int Cantidad { get; set; }  // Indica 1 para mañana, 2 para mañana y tarde
        public bool CambioManana { get; set; }
        public bool CambioTarde { get; set; }

        public static DateTime? ParseToDateTime(string fecha, string hora)
        {
            if (string.IsNullOrEmpty(hora))
                return null;

            // Hora puede no tener segundos
            string formatoHora = hora.Length > 5 ? "HH:mm:ss" : "HH:mm";
            string fechaHora = $"{fecha} {hora}";
            return DateTime.ParseExact(fechaHora, $"yyyy-MM-dd {formatoHora}", CultureInfo.InvariantCulture);
        }

        public static List<AsistenciaConsolidada> ConsolidarAsistencia(List<Asistencia> listaOrdenada)
        {
            var listaConsolidada = new List<AsistenciaConsolidada>();
            var gruposPorEmpleado = listaOrdenada.GroupBy(a => a.CodigoEmpleado).ToList();

            foreach (var grupo in gruposPorEmpleado)
            {
                var consolidado = new AsistenciaConsolidada
                {
                    CodigoEmpleado = grupo.Key,
                    UnidadOrganizativa = grupo.First().UnidadOrganizativa,
                    CentroCosto = grupo.First().CentroCosto,
                    DFecha = DateTime.ParseExact(grupo.First().Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Dia = DateTime.ParseExact(grupo.First().Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dddd", new CultureInfo("es-ES")),
                    HorasExtrasAprobadas = grupo.Sum(a => a.HorasExtrasAprobadas),
                    HorasCompensacionAprobada = grupo.Sum(a => a.HorasCompensacionAprobada),
                    PermisosAprobados = grupo.Sum(a => a.PermisosAprobados),
                    VacacionesAprobadas = grupo.Sum(a => a.VacacionesAprobadas),
                    Cantidad = 1  // Por defecto, se asume 1 ingreso
                };

                var primerIngreso = grupo.FirstOrDefault();
                if (primerIngreso != null)
                {
                    consolidado.EntradaProgramadaManana = ParseToDateTime(primerIngreso.Fecha, primerIngreso.EntradaProgramada);
                    consolidado.SalidaProgramadaManana = ParseToDateTime(primerIngreso.Fecha, primerIngreso.SalidaProgramada);
                    consolidado.EntradaEfectivaManana = ParseToDateTime(primerIngreso.Fecha, primerIngreso.EntradaEfectiva);
                    consolidado.SalidaEfectivaManana = ParseToDateTime(primerIngreso.Fecha, primerIngreso.SalidaEfectiva);
                    consolidado.TiempoProgramadoTotal = primerIngreso.TiempoProgramado;
                    consolidado.TiempoEfectivoTotal = primerIngreso.TiempoEfectivo;
                }

                var segundoIngreso = grupo.Skip(1).FirstOrDefault();
                if (segundoIngreso != null)
                {
                    consolidado.EntradaProgramadaTarde = ParseToDateTime(segundoIngreso.Fecha, segundoIngreso.EntradaProgramada);
                    consolidado.SalidaProgramadaTarde = ParseToDateTime(segundoIngreso.Fecha, segundoIngreso.SalidaProgramada);
                    consolidado.EntradaEfectivaTarde = ParseToDateTime(segundoIngreso.Fecha, segundoIngreso.EntradaEfectiva);
                    //  consolidado.SalidaEfectivaTarde = ParseToDateTime(segundoIngreso.Fecha, segundoIngreso.SalidaEfectiva);
                    consolidado.TiempoProgramadoTotal += segundoIngreso.TiempoProgramado;
                    consolidado.TiempoEfectivoTotal += segundoIngreso.TiempoEfectivo;
                    consolidado.Cantidad = 2;  // Si hay un segundo ingreso, se establece en 2
                }

                listaConsolidada.Add(consolidado);
            }

            return listaConsolidada;
        }
        public bool ModificaHorarios()
        {
            bool rtn = true;
            switch (Cantidad)
            {
                case 1:
                    if (this.EntradaProgramadaManana.HasValue && this.EntradaEfectivaManana.HasValue)
                    {
                        if (this.EntradaEfectivaManana.Value <= this.EntradaProgramadaManana.Value)
                        {
                            rtn = false;
                        }
                    }
                    break;
                case 2:
                    if (this.EntradaProgramadaManana.HasValue && this.EntradaEfectivaManana.HasValue && this.EntradaProgramadaTarde.HasValue && this.EntradaEfectivaTarde.HasValue)
                    {
                        if (this.EntradaEfectivaManana.Value <= this.EntradaProgramadaManana.Value &&
                            this.EntradaEfectivaTarde.Value <= this.EntradaProgramadaTarde.Value)
                        {
                            rtn = false;
                        }
                    }
                    break;
            }

            return rtn;
        }

        public bool ParseHorarios(TimeSpan tsManana, TimeSpan tsTarde)
        {
            bool rtn = false;
            // TimeSpan ts20 = new TimeSpan(0, 20, 0);
            TimeSpan diferencia1 = TimeSpan.Zero;
            TimeSpan diferencia2 = TimeSpan.Zero;
            TimeSpan diferenciaTotal = TimeSpan.Zero;
            TimeSpan mediodia = TimeSpan.Zero;
            TimeSpan HorasProgramadas1 = TimeSpan.Zero;
            TimeSpan HorasProgramadas2 = TimeSpan.Zero;
            TimeSpan HorasProgramadasTotal = TimeSpan.Zero;

            CambioManana = false;
            CambioTarde = false;
            EntradaProgramadaManana0 = EntradaProgramadaManana.Value;

            if (this.EntradaProgramadaManana.HasValue && this.SalidaProgramadaManana.HasValue)
            {
                HorasProgramadas1 = this.SalidaProgramadaManana.Value - this.EntradaProgramadaManana.Value;
            }

            if (this.EntradaProgramadaTarde.HasValue && this.SalidaProgramadaTarde.HasValue)
            {
                HorasProgramadas2 = this.SalidaProgramadaTarde.Value - this.EntradaProgramadaTarde.Value;
            }

            switch (Cantidad)
            {
                case 1:
                    if (this.EntradaEfectivaManana.HasValue && this.EntradaProgramadaManana.HasValue)
                    {
                        diferencia1 = this.EntradaEfectivaManana.Value - this.EntradaProgramadaManana.Value;
                        if (diferencia1 < TimeSpan.Zero)
                        {
                            diferencia1 = TimeSpan.Zero;
                        }
                        else
                        {
                            if (diferencia1 > tsManana)
                            {
                                diferencia1 = tsManana;
                            }
                            if (diferencia1 > TimeSpan.Zero)
                            {
                                EntradaProgramadaManana = EntradaProgramadaManana.Value + diferencia1;
                                CambioManana = true;
                            }
                        }
                    }
                    break;
                case 2:
                    if (this.EntradaProgramadaTarde.HasValue && this.SalidaProgramadaManana.HasValue)
                    {
                        mediodia = EntradaProgramadaTarde.Value - SalidaProgramadaManana.Value;
                    }
                    if (this.EntradaEfectivaManana.HasValue && this.EntradaProgramadaManana.HasValue)
                    {
                        diferencia1 = this.EntradaEfectivaManana.Value - this.EntradaProgramadaManana.Value;
                        if (diferencia1 < TimeSpan.Zero)
                        {
                            diferencia1 = TimeSpan.Zero;
                        }
                        else
                        {
                            if (diferencia1 > tsManana)
                            {
                                diferencia1 = tsManana;
                            }
                            if (diferencia1 > TimeSpan.Zero)
                            {
                                EntradaProgramadaManana = EntradaProgramadaManana.Value + diferencia1;
                                CambioManana = true;
                            }
                        }
                    }
                    if (this.EntradaEfectivaTarde.HasValue && this.EntradaProgramadaTarde.HasValue)
                    {
                        diferencia2 = this.EntradaEfectivaTarde.Value - this.EntradaProgramadaTarde.Value;
                        if (diferencia2 < TimeSpan.Zero)
                        {
                            diferencia2 = TimeSpan.Zero;
                        }
                        else
                        {
                            if (diferencia2 > tsTarde)
                            {
                                diferencia2 = tsTarde;
                            }
                            if (diferencia2 > TimeSpan.Zero)
                            {
                                EntradaProgramadaTarde = EntradaProgramadaTarde.Value + diferencia2;
                                CambioTarde = true;
                            }
                        }
                    }
                    break;
            }

            diferenciaTotal = diferencia1 + diferencia2;
            HorasProgramadasTotal = HorasProgramadas1 + HorasProgramadas2 + mediodia;

            if (diferenciaTotal > TimeSpan.Zero)
            {
                SalidaProgramadaTarde = EntradaProgramadaManana0.Value + HorasProgramadasTotal + diferenciaTotal;
            }

            if (diferenciaTotal > TimeSpan.Zero)
            {
                rtn = true;
            }

            return rtn;
        }



        public override string ToString()
        {
            return $"CodigoEmpleado: {CodigoEmpleado ?? "nulo"}, " +
                   $"UnidadOrganizativa: {UnidadOrganizativa ?? "nulo"}, " +
                   $"CentroCosto: {CentroCosto ?? "nulo"}, " +
                   $"DFecha: {DFecha.ToString("yyyy-MM-dd")}, " +
                   $"Dia: {Dia ?? "nulo"}, " +
                   $"EntradaProgramadaManana: {EntradaProgramadaManana?.ToString("HH:mm:ss") ?? "nulo"}, " +
                   $"EntradaProgramadaManana0: {EntradaProgramadaManana0?.ToString("HH:mm:ss") ?? "nulo"}, " +
                   $"SalidaProgramadaManana: {SalidaProgramadaManana?.ToString("HH:mm:ss") ?? "nulo"}, " +
                   $"EntradaProgramadaTarde: {EntradaProgramadaTarde?.ToString("HH:mm:ss") ?? "nulo"}, " +
                   $"SalidaProgramadaTarde: {SalidaProgramadaTarde?.ToString("HH:mm:ss") ?? "nulo"}, " +
                   $"EntradaEfectivaManana: {EntradaEfectivaManana?.ToString("HH:mm:ss") ?? "nulo"}, " +
                   $"SalidaEfectivaManana: {SalidaEfectivaManana?.ToString("HH:mm:ss") ?? "nulo"}, " +
                   $"EntradaEfectivaTarde: {EntradaEfectivaTarde?.ToString("HH:mm:ss") ?? "nulo"}, ";
            //  $"SalidaEfectivaTarde: {SalidaEfectivaTarde?.ToString("HH:mm:ss") ?? "nulo"}";
        }
    }
}
