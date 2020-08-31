using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace VersaCraft.Protocol
{
    public class Protocol
    {
        public static readonly int Port = 24371;

        private static byte[] DataSerialize<T>(T data)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, data);
            
            return stream.ToArray();
        }

        public static T DataDeserialize<T>(byte[] array)
        {
            var stream = new MemoryStream(array);
            var formatter = new BinaryFormatter();

            return (T)formatter.Deserialize(stream);
        }

        public static byte[] ReceivePacket(NetworkStream stream)
        {
            byte[] packetSizeBuffer = new byte[sizeof(int)];
            int bytesRead = 0;
            while (bytesRead < packetSizeBuffer.Length)
                bytesRead += stream.Read(packetSizeBuffer, bytesRead, packetSizeBuffer.Length - bytesRead);

            int packetSize = BitConverter.ToInt32(packetSizeBuffer, 0);
            byte[] packetBuffer = new byte[packetSize];
            
            //Console.WriteLine("Receiving packet with total size: {0}", packetSize);

            for (int i = 0; i < packetSizeBuffer.Length; i++)
                packetBuffer[i] = packetSizeBuffer[i];

            bytesRead = 0;
            while (bytesRead < packetBuffer.Length - packetSizeBuffer.Length)
                bytesRead += stream.Read(packetBuffer, packetSizeBuffer.Length + bytesRead, packetBuffer.Length - (packetSizeBuffer.Length + bytesRead));

            return packetBuffer;
        }

        public static byte[] PacketSerialize(Packet packet)
        {
            byte[] packetBuffer = null;

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {

                writer.Write(packet.Size);
                writer.Write((int)packet.Type);
                writer.Write(packet.DataSize);
                for (int i = 0; i < packet.DataSize; i++)
                    writer.Write(packet.Data[i]);

                packetBuffer = stream.ToArray();

                byte[] size = BitConverter.GetBytes(packetBuffer.Length);
                for (int i = 0; i < Marshal.SizeOf(packet.Size); i++)
                    packetBuffer[i] = size[i];
            }

            return packetBuffer;
        }

        public static Packet PacketDeserialize(byte[] packetBuffer)
        {
            Packet packet;

            using (MemoryStream stream = new MemoryStream(packetBuffer))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                packet.Size = reader.ReadUInt32();
                packet.Type = (PacketType)reader.ReadInt32();
                packet.DataSize = reader.ReadInt32();
                packet.Data = reader.ReadBytes(packet.DataSize);
            }

            return packet;
        }

        public static Packet FormPacket<T>(PacketType packetType, T data)
        {
            Packet packet = new Packet()
            {
                Size = 0xDEADBEEF, // placeholder for serialization
                Type = packetType,
            };
            packet.Data = data != null ? DataSerialize(data) : new byte[0];
            packet.DataSize = packet.Data.Length;

            return packet;
        }

        public static void SendPacket(Packet packet, TcpClient client)
        {
            byte[] packetBuffer = PacketSerialize(packet);

            //Console.WriteLine("Sending packet with total size {0}", packetBuffer.Length);
            client.GetStream().Write(packetBuffer, 0, packetBuffer.Length);
        }
    }
}
