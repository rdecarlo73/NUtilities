using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace NUtilities.IO
{
  public static class IOUtility
  {
    public static void Write<T>(T graph, string path)
    {
      Write<T>(graph, path, true);
    }

    public static void Write<T>(T graph, string path, bool useDataContractSerializer)
    {
      FileStream stream = null;
      XmlDictionaryWriter writer = null;
      try
      {
        stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        writer = XmlDictionaryWriter.CreateTextWriter(stream);


        if (useDataContractSerializer)
        {
          DataContractSerializer serializer = new DataContractSerializer(typeof(T));
          serializer.WriteObject(writer, graph);
        }
        else
        {
          XmlSerializer serializer = new XmlSerializer(typeof(T));
          serializer.Serialize(writer, graph);
        }

      }
      catch (Exception exception)
      {
        throw new Exception("Error while writing " + typeof(T).Name + " to " + path + ".", exception);
      }
      finally
      {
        writer.Close();
        stream.Close();    
      }
    }

    public static MemoryStream Write<T>(T graph, bool useDataContractSerializer)
    {
      try
      {
        MemoryStream stream = new MemoryStream();
        XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);


        if (useDataContractSerializer)
        {
          DataContractSerializer serializer = new DataContractSerializer(typeof(T));
          serializer.WriteObject(writer, graph);
        }
        else
        {
          XmlSerializer serializer = new XmlSerializer(typeof(T));
          serializer.Serialize(writer, graph);
        }

        //writer.Close();
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while writing " + typeof(T).Name + " to memory stream.", exception);
      }
    }

    public static void WriteStream(Stream graph, string path)
    {
      FileStream stream = null;
      try
      {
        stream = new FileStream(path, FileMode.Create, FileAccess.Write);

        byte[] data = ((MemoryStream)graph).ToArray();

        stream.Write(data, 0, data.Length);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while writing stream to " + path + ".", exception);
      }
      finally
      {
        stream.Close();
      }
    }

    public static T Read<T>(string path)
    {
      return Read<T>(path, true);
    }

    public static T Read<T>(string path, bool useDataContractSerializer)
    {
      T graph;
      FileStream stream = null;
      XmlDictionaryReader reader = null;

      try
      {
        stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());

        if (useDataContractSerializer)
        {
          DataContractSerializer serializer = new DataContractSerializer(typeof(T));
          graph = (T)serializer.ReadObject(reader, true);
        }
        else
        {
          XmlSerializer serializer = new XmlSerializer(typeof(T));
          graph = (T)serializer.Deserialize(reader);
        }

        return graph;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while reading " + typeof(T).Name + " from " + path + ".", exception);
      }
      finally
      {
        reader.Close();
        stream.Close();
      }
    }

    public static string ReadString(string path)
    {
      StreamReader streamReader = null;      
      try
      {
        streamReader = new StreamReader(path);
        string query = streamReader.ReadToEnd();
        streamReader.Close();
        return query;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while reading string from " + path + ".", exception);
      }
      finally
      {
        streamReader.Close();
      }
    }

    public static void WriteException(Exception exception, string path)
    {
      StreamWriter streamWriter = new StreamWriter(path, true);
      streamWriter.WriteLine(System.DateTime.UtcNow + " (UTC) - " + exception.Source);
      streamWriter.WriteLine(exception.ToString());
      streamWriter.WriteLine();
      streamWriter.Flush();
      streamWriter.Close();
    }

    public static void WriteString(string value, string path)
    {
      WriteString(value, path, false, Encoding.Unicode);
    }

    public static void WriteString(string value, string path, bool append)
    {
      WriteString(value, path, append, Encoding.Unicode);
    }

    public static void WriteString(string value, string path, Encoding encoding)
    {
      WriteString(value, path, false, encoding);
    }

    public static void WriteString(string value, string path, bool append, Encoding encoding)
    {
      try
      {
        FileStream stream;
        if (append)
        {
          stream = new FileStream(path, FileMode.Append, FileAccess.Write);
        }
        else
        {
          stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        }
        StreamWriter writer = new StreamWriter(stream, encoding);

        writer.Write(value);
        writer.Flush();
        writer.Close();
        stream.Close();
      }
      catch (Exception exception)
      {
        throw new Exception("Error while writing string to " + path + ".", exception);
      }
    }

    public static string ShellExec(string command, string args, bool redirectStdout) 
    {
      String output = String.Empty;
      try 
      {
        Process process = new Process();
        
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = redirectStdout;
        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        process.StartInfo.CreateNoWindow = false;
        process.Start();
        
        if (redirectStdout) 
        {
          output = process.StandardOutput.ReadToEnd();
        }
        
        process.WaitForExit();
      }
      catch (Exception exception) 
      {
        output = exception.ToString();
      }

      return output;
    }
  }
}
