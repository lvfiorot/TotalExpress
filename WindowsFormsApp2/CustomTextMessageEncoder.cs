using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;

namespace WindowsFormsApp2
{

    public class CustomTextMessageEncoder : MessageEncoder
    {
        private CustomTextMessageEncoderFactory factory;
        private XmlWriterSettings writerSettings;
        private string contentType;

        public CustomTextMessageEncoder(CustomTextMessageEncoderFactory factory)
        {
            this.factory = factory;

            this.writerSettings = new XmlWriterSettings();
            this.writerSettings.Encoding = Encoding.GetEncoding(factory.CharSet);
            this.contentType = string.Format("{0}; charset={1}",
                this.factory.MediaType, this.writerSettings.Encoding.HeaderName);
        }

        public override string ContentType
        {
            get
            {
                return this.contentType;
            }
        }

        public override string MediaType
        {
            get
            {
                return factory.MediaType;
            }
        }

        public override MessageVersion MessageVersion
        {
            get
            {
                return this.factory.MessageVersion;
            }
        }

        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            byte[] msgContents = new byte[buffer.Count];
            Array.Copy(buffer.Array, buffer.Offset, msgContents, 0, msgContents.Length);
            bufferManager.ReturnBuffer(buffer.Array);

            MemoryStream stream = new MemoryStream(msgContents);

            return ReadMessage(stream, int.MaxValue, contentType);
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            var spt = contentType.ToLower().Split(new String[] { "charset=" }, StringSplitOptions.RemoveEmptyEntries);

            var charset = "UTF-8";
            if (spt.Length == 2) charset = spt[1];

            stream.Position = 0;
            StreamReader newstream = new StreamReader(stream, Encoding.GetEncoding(charset));
            string text = newstream.ReadToEnd();
            //Debug.WriteLine(text);

            text = text.Replace(@"xsi:type="":calcularFreteResponse""", @"xsi:type=""calcularFreteResponse""");
            text = text.Replace(@"xsi:type=""tns:DadosFrete""", @"xsi:type=""DadosFrete""");

            byte[] byteArray = Encoding.GetEncoding(charset).GetBytes(text);
            stream = new MemoryStream(byteArray);

            XmlReader reader = XmlReader.Create(stream);
            return Message.CreateMessage(reader, maxSizeOfHeaders, this.MessageVersion);

            //stream.Position = 0;
            //XmlReader reader = XmlReader.Create(stream);
            //return Message.CreateMessage(reader, maxSizeOfHeaders, this.MessageVersion);
        }

        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            MemoryStream stream = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(stream, this.writerSettings);
            message.WriteMessage(writer);
            writer.Close();

            byte[] messageBytes = stream.GetBuffer();
            int messageLength = (int)stream.Position;
            stream.Close();

            int totalLength = messageLength + messageOffset;
            byte[] totalBytes = bufferManager.TakeBuffer(totalLength);
            Array.Copy(messageBytes, 0, totalBytes, messageOffset, messageLength);

            ArraySegment<byte> byteArray = new ArraySegment<byte>(totalBytes, messageOffset, messageLength);
            return byteArray;
        }

        public override void WriteMessage(Message message, Stream stream)
        {
            XmlWriter writer = XmlWriter.Create(stream, this.writerSettings);
            message.WriteMessage(writer);
            writer.Close();
        }

        public override bool IsContentTypeSupported(string contentType)
        {
            if (base.IsContentTypeSupported(contentType))
            {
                return true;
            }
            if (contentType.Length == this.MediaType.Length)
            {
                return contentType.Equals(this.MediaType, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                if (contentType.StartsWith(this.MediaType, StringComparison.OrdinalIgnoreCase)
                    && (contentType[this.MediaType.Length] == ';'))
                {
                    return true;
                }
            }
            return false;
        }
    }

}
