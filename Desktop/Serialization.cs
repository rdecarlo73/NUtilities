using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace NUtilities.Serialization
{
  public class StringEncoder : StringWriter
  {
    Encoding encoding;

    public StringEncoder(StringBuilder builder, Encoding encoding)
      : base(builder)
    {
      this.encoding = encoding;
    }

    public StringEncoder(StringBuilder builder)
      : base(builder)
    { }

    public override Encoding Encoding
    {
      get { return encoding; }
    }
  }

  public static class XmlUtility
  {
    public static T DeserializeDataContract<T>(this string xml)
    {
      return Deserialize<T>(xml, true);
    }

    public static T DeserializeXml<T>(this string xml)
    {
      return Deserialize<T>(xml, false);
    }

    public static string SerializeXml<T>(T graph)
    {
      return Serialize<T>(graph, Encoding.UTF8, false);
    }

    public static string SerializeDataContract<T>(T graph)
    {
      return Serialize<T>(graph, Encoding.UTF8);
    }

    public static string Serialize<T>(T graph, Encoding encoding)
    {
      return Serialize<T>(graph, encoding, true);
    }

    public static string Serialize<T>(T graph, bool useDataContractSerializer)
    {
      return Serialize<T>(graph, Encoding.UTF8, useDataContractSerializer);
    }

    public static string Serialize<T>(T graph, Encoding encoding, bool useDataContractSerializer)
    {
      string xml;
      try
      {
        StringBuilder builder = new StringBuilder();
        TextWriter encoder = new StringEncoder(builder, encoding);
        XmlWriter writer = XmlWriter.Create(encoder);

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
        writer.Close();

        xml = builder.ToString();

        return xml;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while serializing " + typeof(T).Name + ".", exception);
      }
    }

    public static string SerializeFromStream(Stream graph)
    {
      try
      {
        StreamReader reader = new StreamReader(graph, Encoding.UTF8);
        string value = reader.ReadToEnd();
        return value;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while serializing stream.", exception);
      }
    }

    public static MemoryStream SerializeToMemoryStream<T>(T graph)
    {
      return SerializeToMemoryStream(graph, true);
    }

    public static MemoryStream SerializeToMemoryStream<T>(T graph, bool useDataContractSerializer)
    {
      try
      {
        MemoryStream stream = new MemoryStream();

        if (useDataContractSerializer)
        {
          DataContractSerializer serializer = new DataContractSerializer(typeof(T));
          serializer.WriteObject(stream, graph);
        }
        else
        {
          XmlSerializer serializer = new XmlSerializer(typeof(T));
          serializer.Serialize(stream, graph);
        }
        return stream;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while serializing " + typeof(T).Name + "to stream.", exception);
      }
    }

    public static T Deserialize<T>(string xml, bool useDataContractSerializer)
    {
      T graph;
      try
      {
        StringReader input = new StringReader(xml);
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.ProhibitDtd = false;
        XmlReader reader = XmlDictionaryReader.Create(input, settings);

        if (useDataContractSerializer)
        {
          DataContractSerializer serializer = new DataContractSerializer(typeof(T));

          graph = (T)serializer.ReadObject(reader, false);
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
        throw new Exception("Error while deserializing " + typeof(T).Name + ".", exception);
      }
    }

    public static T DeserializeFromStream<T>(Stream stream)
    {
      return DeserializeFromStream<T>(stream, true);
    }

    public static T DeserializeFromStream<T>(Stream stream, bool useDataContractSerializer)
    {
      T graph;
      XmlDictionaryReader reader = null;
      try
      {
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
        throw new Exception("Error while deserializing stream to " + typeof(T).Name + ".", exception);
      }
      finally
      {
        reader.Close();
      }
    }

    public static Stream DeserializeToStream(string graph)
    {
      Stream stream;
      StreamWriter writer = null;
      try
      {
        stream = new MemoryStream();
        writer = new StreamWriter(stream);
        writer.Write(graph);
        writer.Flush();

        return stream;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while deserializing string to stream.", exception);
      }
      finally
      {
        writer.Close();
      }
    }
  }
}
