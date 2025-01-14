using NLog;
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Asistencia
{
    public class ActualizaHorario
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        async static void ActualizarMarcaciones(string CodigoConsulta, string entradaNumero, string salidaNumero, string id, DateTime ingreso, DateTime salida, ApiClient apiClient, string token)
        {
            logger.Info("ActualizarMarcaciones / consulta:{0} / entradaNumero:{1} / salidaNumero:{2} / ingreso:{3}/salida:{4}/ id:{5}", CodigoConsulta, entradaNumero, salidaNumero, ingreso, salida, id);

            string parametros = $"CodigoConsulta: {CodigoConsulta ?? "nulo"}, " +
                               $"entradaNumero: {entradaNumero ?? "nulo"}, " +
                               $"salidaNumero: {salidaNumero ?? "nulo"}, " +
                               $"id: {id ?? "nulo"}, " +
                               $"ingreso: {ingreso.ToString("yyyy-MM-dd HH:mm:ss")}, " +
                               $"salida: {salida.ToString("yyyy-MM-dd HH:mm:ss")}";

            // Aquí puedes hacer lo que necesites con el string, como por ejemplo, imprimirlo
            Console.WriteLine(parametros);
            var consultaRequest = new ConsultaParametrizadaRequest
            {
                CodigoConsulta = CodigoConsulta,
                Modo = "JSON",
                Parametros = new List<Parametro>
        {
                    new Parametro { Nombre = "CodigoEmpleado", Valor = id, Tipo = 0 },
                    new Parametro { Nombre = entradaNumero, Valor = ingreso.ToString("yyyy-MM-dd HH:mm:ss"), Tipo = 6 },
                    new Parametro { Nombre = salidaNumero, Valor = salida.ToString("yyyy-MM-dd HH:mm:ss"), Tipo = 6 }
                }
            };

            string consultaResponse = await apiClient.EjecutarConsultaParametrizadaAsync(token, consultaRequest);

            if (consultaResponse != null)
            {
                Console.WriteLine("Respuesta del API:");
                Console.WriteLine(consultaResponse);
                if (consultaResponse.EndsWith(":0}]}}"))
                {
                    int xx = 0;
                }
            }
            else
            {
                Console.WriteLine("No se recibió respuesta del API para la consulta parametrizada.");
            }
        }

        //Console.WriteLine($"ID: {id}, Salida: {salida}, Ingreso: {ingreso}");
        //Console.WriteLine($"Código Empleado: {colaborador.CodigoEmpleado}");
        //Console.WriteLine($"Unidad Organizativa: {colaborador.UnidadOrganizativa}");
        //Console.WriteLine($"Fecha: {colaborador.Fecha}");
        //Console.WriteLine($"EntradaEfectiva: {colaborador.EntradaEfectiva}");
        //Console.WriteLine($"HoraaInt: {colaborador.HoraaInt()}");

        static DateTime ObtenerFechaInicioPeriodo(DateTime fecha)
        {
            DateTime rtn = DateTime.Now;
            if (fecha.Day < 25)
            {
                fecha = fecha.AddMonths(-1);

            }
            rtn = new DateTime(fecha.Year, fecha.Month, 25);
            return rtn;
        }
        static string ObtenerPrimerDiaDelMesCorrespondiente(DateTime fecha)
        {
            // Si la fecha es del 25 o después en un mes, devolvemos el 1 del próximo mes
            if (fecha.Day >= 25)
            {
                fecha = fecha.AddMonths(1);
            }

            // Devolvemos el primer día del mes correspondiente
            DateTime primerDiaDelMes = new DateTime(fecha.Year, fecha.Month, 1);

            // Devolvemos la fecha en formato yyyy-MM-dd
            return primerDiaDelMes.ToString("yyyy-MM-dd");
        }

        public async static Task Actualizar(DateTime Inicio, DateTime Final, List<string> colaboradores)
        {
            var apiClient = new ApiClient();
            ProgressForm progress = new ProgressForm();
            progress.Show();
            logger.Info("Actualizar de {0} al {1} - los elementos:{2}", Inicio, Final, string.Join(",", colaboradores));

            int idEmpresa = 123;
            string cuenta = "api_cndc";
            string contrasena = "RI#J6ODG";

            string token = await apiClient.AutenticarAsync(idEmpresa, cuenta, contrasena);
            DateTime Fecha = Inicio;

            if (token != null)
            {
                int contador = 0;
                string regional = "1405";
                int totalDias = (Final.Date - Inicio.Date).Days + 1;
                int progresoActual = 0;
                int progresoTotal = totalDias * colaboradores.Count;

                while (Fecha.Date <= Final.Date)
                {
                    string periodo = ObtenerPrimerDiaDelMesCorrespondiente(Fecha);
                    var horasTrabajadasResponse = await apiClient.ConsultarHorasTrabajadasAsync(token, regional, periodo);

                    if (horasTrabajadasResponse?.Exito == true)
                    {
                        var listaConsolidada = AsistenciaConsolidada.ConsolidarAsistencia(horasTrabajadasResponse.GetIngresosByFecha(Fecha, true));

                        foreach (var colaborador in listaConsolidada)
                        {
                            if (colaboradores.Contains(colaborador.CodigoEmpleado) && colaborador.ModificaHorarios())
                            {
                                TimeSpan tsmanana = Fecha.Date <= new DateTime(2024, 09, 02) ? new TimeSpan(0, 30, 0) : new TimeSpan(0, 20, 0);
                                TimeSpan tstarde = Fecha.Date <= new DateTime(2024, 09, 02) ? TimeSpan.Zero : new TimeSpan(0, 20, 0);

                                colaborador.ParseHorarios(tsmanana, tstarde);

                                if (colaborador.Cantidad == 1 && colaborador.CambioManana)
                                {
                                    string procedimiento = colaborador.EntradaProgramadaManana.Value.Hour < 12 ? "ACTUALIZAR_ENTRADA1_SALIDA1" : "ACTUALIZAR_ENTRADA2_SALIDA2";
                                      ActualizarMarcaciones(procedimiento, "Entrada1", "Salida1", colaborador.CodigoEmpleado, colaborador.EntradaProgramadaManana.Value, colaborador.SalidaProgramadaTarde.Value, apiClient, token);
                                }
                                else
                                {
                                    if (colaborador.CambioManana)
                                          ActualizarMarcaciones("ACTUALIZAR_ENTRADA1_SALIDA2", "Entrada1", "Salida2", colaborador.CodigoEmpleado, colaborador.EntradaProgramadaManana.Value, colaborador.SalidaProgramadaTarde.Value, apiClient, token);

                                    if (colaborador.CambioTarde)
                                          ActualizarMarcaciones("ACTUALIZAR_ENTRADA2_SALIDA2", "Entrada2", "Salida2", colaborador.CodigoEmpleado, colaborador.EntradaProgramadaTarde.Value, colaborador.SalidaProgramadaTarde.Value, apiClient, token);
                                }
                                progresoActual++;
                                progress.UpdateProgress((progresoActual * 100) / progresoTotal, $"Fecha: {Fecha:dd/MM/yyyy}, Colaborador: {colaborador.CodigoEmpleado}");
                                await Task.Delay(2000);
                            }
                        }
                    }
                    else
                    {
                        logger.Warn($"Error consultando horas trabajadas para la fecha {Fecha:dd/MM/yyyy}");
                    }

                    Fecha = Fecha.AddDays(1);
                    contador++;

                    if (contador >= 50)
                    {
                        token = await apiClient.AutenticarAsync(idEmpresa, cuenta, contrasena);
                        contador = 0;
                    }
                }
            }
            else
            {
                logger.Error("Error de autenticación. Token inválido.");
            }
            progress.UpdateProgress(100, "Proceso finalizado");
            progress.Close();
        }

        //private async static Task ActualizarMarcaciones(string procedimiento, string entrada, string salida, string codigoEmpleado, DateTime horaEntrada, DateTime horaSalida, ApiClient apiClient, string token)
        //{
        //    try
        //    {
        //        await apiClient.ActualizarMarcacionAsync(procedimiento, codigoEmpleado, horaEntrada, horaSalida, token);
        //        logger.Info($"{procedimiento}: {codigoEmpleado} - {entrada}: {horaEntrada}, {salida}: {horaSalida}");
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error($"Error al actualizar marcaciones para {codigoEmpleado}: {ex.Message}");
        //    }
        //}


        //public async static Task Actualizar(DateTime Inicio, DateTime Final, List<string> colaboradores)
        //{
        //    var apiClient = new ApiClient();

        //    ProgressForm progress = new ProgressForm();
        //    progress.Show();
        //    logger.Info("Actualizar de {0} al {1} - los elementos:{2}", Inicio, Final, string.Join(",", colaboradores));

        //      // Datos de autenticación
        //    int idEmpresa = 123;
        //    string cuenta = "api_cndc";
        //    string contrasena = "RI#J6ODG";

        //    string token = await apiClient.AutenticarAsync(idEmpresa, cuenta, contrasena);

        //    DateTime Fecha = Inicio;
        //    if (token != null)
        //    {

        //        int contador = 10;
        //        string regional = "1405";
        //        string periodo = ObtenerPrimerDiaDelMesCorrespondiente(Fecha);
        //        ConsultarHorasTrabajadasResponse horasTrabajadasResponse = await apiClient.ConsultarHorasTrabajadasAsync(token, regional, periodo);

        //        if (horasTrabajadasResponse != null && horasTrabajadasResponse.Exito)
        //        {
        //            int totalDias = (Final.Date - Inicio.Date).Days + 1; // Calculamos el total de días para el progreso
        //            int progresoActual = 0;

        //            while (Fecha.Date <= Final.Date)
        //            {

        //                progress.UpdateProgress(progresoActual, "Fecha: " + Fecha.Date);


        //                logger.Info(Fecha.Date);
        //                List<Asistencia> rtn = horasTrabajadasResponse.GetIngresosByFecha(Fecha, true);
        //                var listaConsolidada = AsistenciaConsolidada.ConsolidarAsistencia(rtn);

        //                TimeSpan tsmanana = TimeSpan.Zero;
        //                TimeSpan tstarde = TimeSpan.Zero;
        //                // Itera sobre los resultados obtenidos
        //                foreach (var colaborador in listaConsolidada)
        //                {
        //                    logger.Info("Colaborador:{0}", colaborador);
        //                    progress.UpdateProgress(progresoActual, "Fecha: " + Fecha.Date.ToShortDateString() +  " Colaborador:"+ colaborador.CodigoEmpleado);

        //                    if (colaboradores.IndexOf(colaborador.CodigoEmpleado) >= 0)
        //                    {
        //                        if (colaborador.ModificaHorarios())
        //                        {
        //                            logger.Info("Modificando horario de :{0}", colaborador);
        //                            if (Fecha.Date <= new DateTime(2024, 09, 02))
        //                            {
        //                                tsmanana = new TimeSpan(0, 30, 0);
        //                                tstarde = new TimeSpan(0, 0, 0);
        //                            }
        //                            else
        //                            {
        //                                tsmanana = new TimeSpan(0, 20, 0);
        //                                tstarde = new TimeSpan(0, 20, 0);
        //                            }
        //                            colaborador.ParseHorarios(tsmanana, tstarde);

        //                            // Actualización de marcaciones según los cambios
        //                            if (colaborador.Cantidad == 1)
        //                            {
        //                                if (colaborador.CambioManana)
        //                                {
        //                                    if (colaborador.EntradaProgramadaManana.Value.Hour < 12)
        //                                    {
        //                                        ActualizarMarcaciones("ACTUALIZAR_ENTRADA1_SALIDA1", "Entrada1", "Salida1", colaborador.CodigoEmpleado, colaborador.EntradaProgramadaManana.Value, colaborador.SalidaProgramadaTarde.Value, apiClient, token);
        //                                    }
        //                                    else
        //                                    {
        //                                        ActualizarMarcaciones("ACTUALIZAR_ENTRADA2_SALIDA2", "Entrada2", "Salida2", colaborador.CodigoEmpleado, colaborador.EntradaProgramadaManana.Value, colaborador.SalidaProgramadaTarde.Value, apiClient, token);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (colaborador.CambioManana)
        //                                {
        //                                    ActualizarMarcaciones("ACTUALIZAR_ENTRADA1_SALIDA2", "Entrada1", "Salida2", colaborador.CodigoEmpleado, colaborador.EntradaProgramadaManana.Value, colaborador.SalidaProgramadaTarde.Value, apiClient, token);
        //                                }
        //                                if (colaborador.CambioTarde)
        //                                {
        //                                    ActualizarMarcaciones("ACTUALIZAR_ENTRADA2_SALIDA2", "Entrada2", "Salida2", colaborador.CodigoEmpleado, colaborador.EntradaProgramadaTarde.Value, colaborador.SalidaProgramadaTarde.Value, apiClient, token);
        //                                }
        //                            }
        //                        }
        //                        await Task.Delay(TimeSpan.FromSeconds(2));
        //                    }
        //                }


        //                Fecha = Fecha.AddDays(1);
        //                contador++;

        //                // Si hemos superado el contador, renovamos el token
        //                if (contador > 50)
        //                {
        //                    contador = 0;
        //                    token = await apiClient.AutenticarAsync(idEmpresa, cuenta, contrasena);
        //                }
        //            }
        //        }
        //    }
        //    progress.UpdateProgress(100, "Fin");
        //    progress.Close();

        //}


        //public async static Task Actualizar(DateTime Inicio, DateTime Final, List<string> colaboradores)
        //{
        //    var apiClient = new ApiClient();
        //    var progressDialog = new ProgressDialog
        //    {
        //        WindowTitle = "Procesando",
        //        Text = "Procesando registros...",
        //        Description = "Por favor, espera.",
        //        ShowCancelButton = false
        //    };
        //    logger.Info(" Actualizar de {0} al {1} - los elementos:{2}", Inicio, Final, string.Join(",", colaboradores));
        //    progressDialog.Show();
        //    // Datos de autenticación
        //    int idEmpresa = 123;
        //    string cuenta = "api_cndc";
        //    string contrasena = "RI#J6ODG";

        //    string token = await apiClient.AutenticarAsync(idEmpresa, cuenta, contrasena);

        //    DateTime Fecha = Inicio;
        //    if (token != null)
        //    {
        //        int contador = 10;
        //        string regional = "1405";


        //        string periodo = ObtenerPrimerDiaDelMesCorrespondiente(Fecha);
        //        ConsultarHorasTrabajadasResponse horasTrabajadasResponse = await apiClient.ConsultarHorasTrabajadasAsync(token, regional, periodo);
        //        if (horasTrabajadasResponse != null && horasTrabajadasResponse.Exito)
        //        {
        //            while (Fecha.Date <= Final.Date)
        //            {
        //                logger.Info(Fecha.Date);
        //                List<Asistencia> rtn = horasTrabajadasResponse.GetIngresosByFecha(Fecha, true);
        //                var listaConsolidada = AsistenciaConsolidada.ConsolidarAsistencia(rtn);

        //                TimeSpan tsmanana = TimeSpan.Zero;
        //                TimeSpan tstarde = TimeSpan.Zero;
        //                // Itera sobre los resultados obtenidos
        //                foreach (var colaborador in listaConsolidada)
        //                {
        //                    logger.Info("Colaborador:{0}", colaborador);
        //                    if (colaboradores.IndexOf(colaborador.CodigoEmpleado) >= 0)
        //                    {

        //                        if (colaborador.ModificaHorarios())
        //                        {
        //                            logger.Info("Modificando horario de :{0}", colaborador);
        //                            if (Fecha.Date <= new DateTime(2024, 09, 02))
        //                            {
        //                                tsmanana = new TimeSpan(0, 30, 0);
        //                                tstarde = new TimeSpan(0, 0, 0);
        //                            }
        //                            else
        //                            {
        //                                tsmanana = new TimeSpan(0, 20, 0);
        //                                tstarde = new TimeSpan(0, 20, 0);
        //                            }
        //                            colaborador.ParseHorarios(tsmanana, tstarde);
        //                            if (colaborador.Cantidad == 1)
        //                            {
        //                                if (colaborador.CambioManana)
        //                                {
        //                                    if (colaborador.EntradaProgramadaManana.Value.Hour < 12)
        //                                    {
        //                                        ActualizarMarcaciones("ACTUALIZAR_ENTRADA1_SALIDA1", "Entrada1", "Salida1", colaborador.CodigoEmpleado, colaborador.EntradaProgramadaManana.Value, colaborador.SalidaProgramadaTarde.Value, apiClient, token);
        //                                    }
        //                                    else
        //                                    {
        //                                        ActualizarMarcaciones("ACTUALIZAR_ENTRADA2_SALIDA2", "Entrada2", "Salida2", colaborador.CodigoEmpleado, colaborador.EntradaProgramadaManana.Value, colaborador.SalidaProgramadaTarde.Value, apiClient, token);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (colaborador.CambioManana)
        //                                {//ACTUALIZAR_ENTRADA1_SALIDA2 - ACTUALIZAR_ENTRADA1_SALIDA1 -ACTUALIZAR_ENTRADA2_SALIDA2.
        //                                    ActualizarMarcaciones("ACTUALIZAR_ENTRADA1_SALIDA2", "Entrada1", "Salida2", colaborador.CodigoEmpleado, colaborador.EntradaProgramadaManana.Value, colaborador.SalidaProgramadaTarde.Value, apiClient, token);
        //                                }
        //                                if (colaborador.CambioTarde)
        //                                {
        //                                    ActualizarMarcaciones("ACTUALIZAR_ENTRADA2_SALIDA2", "Entrada2", "Salida2", colaborador.CodigoEmpleado, colaborador.EntradaProgramadaTarde.Value, colaborador.SalidaProgramadaTarde.Value, apiClient, token);

        //                                }
        //                            }
        //                        }
        //                        await Task.Delay(TimeSpan.FromSeconds(2));

        //                    }
        //                }
        //                Fecha = Fecha.AddDays(1);
        //                contador++;
        //                if (contador > 50)
        //                {
        //                    contador = 0;
        //                    token = await apiClient.AutenticarAsync(idEmpresa, cuenta, contrasena);

        //                }


        //            }
        //        }

        //    }


        //    //    string periodo = ObtenerPrimerDiaDelMesCorrespondiente(Fecha);

        //    //    ConsultarHorasTrabajadasResponse horasTrabajadasResponse = await apiClient.ConsultarHorasTrabajadasAsync(token, regional, periodo);
        //    // Fecha = DateTime.Now.Date;   


        //}

    }
}