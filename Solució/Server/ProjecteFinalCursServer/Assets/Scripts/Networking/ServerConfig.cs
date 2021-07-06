using System;
using System.Xml.Serialization;

[Serializable]
public class ServerConfig
{
    public int ServerPort;
    public string DataBaseIP;
    public string DataBaseUser;
    public string DataBasePassword;

    public ServerConfig()
    {
        ServerPort= 7777;
        DataBaseIP= "127.0.0.1";
        DataBaseUser= "root";
        DataBasePassword = "123456789q";
    }

    //Loads this
    public static ServerConfig Load()
    {
        try
        {
            using (var stream = System.IO.File.OpenRead(@".\config.xml"))
            {
                var serializer = new XmlSerializer(typeof(ServerConfig));
                return serializer.Deserialize(stream) as ServerConfig;
            }
        }
        catch
        {
            ServerConfig thisObj = new ServerConfig();
            thisObj.Save();
            return thisObj;
        }
    }

    public void Save()//Not implementend
    {
        try
        {
            using (var writer = new System.IO.StreamWriter(@".\config.xml"))
            {
                var serializer = new XmlSerializer(this.GetType());
                serializer.Serialize(writer, this);
                writer.Flush();
            }
        }
        catch
        {

        }
    }
}

