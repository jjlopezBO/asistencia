using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.IO;

public static class SerializadorJson
{
	public static T DeserializeJson<T>(string s)
	{
		try
		{
			return JsonConvert.DeserializeObject<T>(s);
		}
		catch (Exception ex)
		{
            Console.WriteLine ("UtilesComunes.SerializadorJson", ex);
			throw;
		}
		finally
		{

		}

	}

	public static string SerializeJson<T>(T obj)
	{
		try
		{
			return JsonConvert.SerializeObject(obj);
		}
		catch (Exception)
		{
			throw;
		}
		finally
		{

		}
	}

	public static string GetUrl(string dir)
	{
		HttpWebRequest request = null;
		StreamReader responseReader = null;
		string responseData = string.Empty;
		try
		{
			request = (HttpWebRequest)HttpWebRequest.Create(dir);
			responseReader = new StreamReader(request.GetResponse().GetResponseStream(), Encoding.GetEncoding(1252));
			responseData = responseReader.ReadToEnd();
		}
		catch
		{
			throw;
		}
		finally
		{
			request.GetResponse().GetResponseStream().Close();
			responseReader.Close();
			responseReader = null;
		}
		return responseData;
	}

	public static void AbrirEnlace(string URI)
	{
		try
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			process.StartInfo.FileName = URI;
			process.StartInfo.CreateNoWindow = false;
			process.Start();
		}
		catch (Exception ex)
		{
			Console.WriteLine("SerializadorJson", ex);
			throw;
		}
	}
}