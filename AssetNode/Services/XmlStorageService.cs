using AssetNode.Interface;
using AssetNode.Models.Dtos;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

namespace AssetNode.Services
{
    public class XmlStorageService  
    {
        private readonly string filePath = Path.Combine("Data", "XmlH.Xml");
        private readonly string filePath2 = Path.Combine("Data", "Import.txt");

        public AssetNodes LoadHierarchy()
        {
            if(!File.Exists(filePath))
            {
                return null;
            }
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    var serialize = new XmlSerializer(typeof(AssetNodes));
                    return (AssetNodes)serialize.Deserialize(stream);
                }
            }
            catch
            {

                return null;
            }
        }
       public void SaveHierarchy(AssetNodes root)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(AssetNodes));
                serializer.Serialize(stream, root);
            }

        }

        public List<AssetNodes> ImportHierarchyFrom()
        {
            if (!File.Exists(filePath2))
            {
                return null;
            }

            try
            {
                string xmldata = File.ReadAllText(filePath2);
                Console.WriteLine($"Imported-Data {xmldata}");
                if (string.IsNullOrWhiteSpace(xmldata))
                {
                    return null;
                }
                else
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(AssetNodes));
                    using (StringReader reader = new StringReader(xmldata))
                    {
                        AssetNodes node = (AssetNodes)serializer.Deserialize(reader);
                        return new List<AssetNodes> { node};
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"some error occured {ex.Message}");
                return null;
            }
        }


        }
}
