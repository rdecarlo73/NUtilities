using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using NUtilities.Serialization;

namespace NUtilities.Transformations
{
  public static class XslUtility
  {
    public static R Transform<T, R>(object graph, string stylesheetUri)
    {
      return Transform<T, R>((T)graph, stylesheetUri, null, true);
    }

    public static R Transform<T, R>(object graph, string stylesheetUri, bool useDataContractSerializer)
    {
      return Transform<T, R>((T)graph, stylesheetUri, null, useDataContractSerializer);
    }

    public static R Transform<T, R>(T graph, string stylesheetUri)
    {
      return Transform<T, R>(graph, stylesheetUri, null, true);
    }

    public static R Transform<T, R>(T graph, Stream stylesheet)
    {
      return Transform<T, R>(graph, stylesheet, null, true);
    }

    public static R Transform<T, R>(T graph, string stylesheetUri, bool useDataContractSerializer)
    {
      return Transform<T, R>(graph, stylesheetUri, null, useDataContractSerializer);
    }

    public static R Transform<T, R>(T graph, Stream stylesheet, bool useDataContractSerializer)
    {
      return Transform<T, R>(graph, stylesheet, null,  useDataContractSerializer);
    }

    public static R Transform<T, R>(T graph, string stylesheetUri, XsltArgumentList arguments)
    {
      return Transform<T, R>(graph, stylesheetUri, arguments, true);
    }

    public static R Transform<T, R>(T graph, Stream stylesheet, XsltArgumentList arguments)
    {
      return Transform<T, R>(graph, stylesheet, arguments, true);
    }

    public static R Transform<T, R>(T graph, string stylesheetUri, XsltArgumentList arguments, bool useDataContractSerializer)
    {
      FileStream stream;

      try
      {
        stream = new FileStream(stylesheetUri, FileMode.Open);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while loading stylesheet " + stylesheetUri + ".", exception);
      }

      return Transform<T, R>(graph, stream, arguments, useDataContractSerializer);
    }

    public static R Transform<T, R>(T graph, string stylesheetUri, XsltArgumentList arguments, bool useDataContractSerializer, bool useDataContractDeserializer)
    {
      FileStream stream;

      try
      {
        stream = new FileStream(stylesheetUri, FileMode.Open);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while loading stylesheet " + stylesheetUri + ".", exception);
      }

      return Transform<T, R>(graph, stream, arguments, useDataContractSerializer, useDataContractDeserializer);
    }

    public static R Transform<T, R>(T graph, Stream stylesheet, XsltArgumentList arguments, bool useDataContractSerializer)
    {
      return Transform<T, R>(graph, stylesheet, arguments, useDataContractSerializer, useDataContractSerializer);
    }

    public static R Transform<T, R>(T graph, Stream stylesheet, XsltArgumentList arguments, bool useDataContractSerializer, bool useDataContractDeserializer)
    {
      string xml;
      try
      {
        xml = XmlUtility.Serialize<T>(graph, useDataContractSerializer);

        xml = Transform(xml, stylesheet, arguments);

        R resultGraph = XmlUtility.Deserialize<R>(xml, useDataContractDeserializer);

        return resultGraph;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while transforming " + typeof(T).Name + " to " + typeof(R).Name + ".", exception);

      }
    }

    public static Stream Transform<T>(T graph, string stylesheetUri)
    {
      return Transform<T>(graph, stylesheetUri, null, true);
    }

    public static Stream Transform<T>(T graph, string stylesheetUri, bool useDataContractSerializer)
    {
      return Transform<T>(graph, stylesheetUri, null, useDataContractSerializer);
    }

    public static Stream Transform<T>(T graph, string stylesheetUri, XsltArgumentList arguments, bool useDataContractSerializer)
    {
      FileStream stream;

      try
      {
        stream = new FileStream(stylesheetUri, FileMode.Open);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while loading stylesheet " + stylesheetUri + ".", exception);
      }

      return Transform<T>(graph, stream, arguments, useDataContractSerializer);
    }

    public static Stream Transform<T>(T graph, Stream stylesheet, XsltArgumentList arguments, bool useDataContractSerializer)
    {
      string xml;
      try
      {
        xml = XmlUtility.Serialize<T>(graph, useDataContractSerializer);

        xml = Transform(xml, stylesheet, arguments);

        Stream resultGraph = XmlUtility.DeserializeToStream(xml);

        return resultGraph;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while transforming " + typeof(T).Name + " to stream.", exception);

      }
    }

    public static R Transform<R>(Stream graph, string stylesheetUri)
    {
      return Transform<R>(graph, stylesheetUri, null, true);
    }

    public static R Transform<R>(Stream graph, Stream stylesheet)
    {
      return Transform<R>(graph, stylesheet, null, true);
    }

    public static R Transform<R>(Stream graph, string stylesheetUri, bool useDataContractSerializer)
    {
      return Transform<R>(graph, stylesheetUri, null, useDataContractSerializer);
    }

    public static R Transform<R>(Stream graph, Stream stylesheet, bool useDataContractSerializer)
    {
      return Transform<R>(graph, stylesheet, null, useDataContractSerializer);
    }

    public static R Transform<R>(Stream graph, string stylesheetUri, XsltArgumentList arguments)
    {
      return Transform<R>(graph, stylesheetUri, arguments, true);
    }

    public static R Transform<R>(Stream graph, Stream stylesheet, XsltArgumentList arguments)
    {
      return Transform<R>(graph, stylesheet, arguments, true);
    }

    public static R Transform<R>(Stream graph, string stylesheetUri, XsltArgumentList arguments, bool useDataContractSerializer)
    {
      FileStream stream;

      try
      {
        stream = new FileStream(stylesheetUri, FileMode.Open);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while loading stylesheet " + stylesheetUri + ".", exception);
      }

      return Transform<R>(graph, stream, arguments, useDataContractSerializer);
    }

    public static R Transform<R>(Stream graph, Stream stylesheet, XsltArgumentList arguments, bool useDataContractSerializer)
    {
      string xml;
      try
      {
        xml = XmlUtility.SerializeFromStream(graph);

        xml = Transform(xml, stylesheet, arguments);

        R resultGraph = XmlUtility.Deserialize<R>(xml, useDataContractSerializer);

        return resultGraph;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while transforming " + typeof(Stream) + " to " + typeof(R).Name + ".", exception);

      }
    }

    public static string Transform(string xml, string stylesheetUri, XsltArgumentList arguments)
    {
      FileStream stream;
      try
      {
        stream = new FileStream(stylesheetUri, FileMode.Open);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while loading stylesheet " + stylesheetUri + ".", exception);
      }
      return Transform(xml, stream, arguments);
    }

    private static string Transform(string xml, Stream stylesheet, XsltArgumentList arguments)
    {
      StringReader reader = null;
      TextWriter writer = null;
      try
      {
        XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
        XsltSettings xsltSettings = new XsltSettings();
        xsltSettings.EnableDocumentFunction = true;

        XmlUrlResolver stylesheetResolver = new XmlUrlResolver();

        XmlReader stylesheetReader = XmlReader.Create(stylesheet);
        xslCompiledTransform.Load(stylesheetReader, xsltSettings, stylesheetResolver);
        Encoding encoding = xslCompiledTransform.OutputSettings.Encoding;
        
        reader = new StringReader(xml);
        XPathDocument input = new XPathDocument(reader);

        StringBuilder builder = new StringBuilder();
        writer = new StringEncoder(builder, encoding);

        xslCompiledTransform.Transform(input, arguments, writer);

        xml = builder.ToString();
        
        return xml;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while transforming. " + exception);
      }
      finally
      {
        stylesheet.Close();
        reader.Close();
        writer.Close();
        
   
      }
    }
  }
}
