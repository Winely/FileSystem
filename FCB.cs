using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileSystem
{
    /// <summary>
    /// FCB记录，记录文件/文件夹各项信息
    /// </summary>
    [Serializable]
    class FCB : ISerializable
    {
        public char[] name = new char[60];
        public char[] owner = new char[60];
        public List<int> SubFile = new List<int>();
        public int contentBlock { get; set; }
        int size = 0;
        public bool isFolder { get; set; }
        public DateTime createTime { get; set; }
        public DateTime lastUpdate { get; set; }

        /// <summary>
        /// 初始化FCB
        /// </summary>
        /// <param name="block"></param>
        /// <param name="Name"></param>
        /// <param name="Content"></param>
        /// <param name="Isfolder"></param>
        /// <param name="Owner"></param>
        public FCB(int block, string Name, int Content, int father, bool Isfolder, string Owner)
        {
            name = new char[60];
            owner = new char[60];
            Name.CopyTo(0, name, 0, Name.Length);
            Owner.CopyTo(0, owner, 0, Owner.Length);
            contentBlock = Content;
            isFolder = Isfolder;
            createTime = DateTime.Now;
            lastUpdate = DateTime.Now;
            size = 0;

            //第一个节点为上级目录
            SubFile.Add(father);
        }

        public FCB()
        {
            // TODO: Complete member initialization
        }

        /// <summary>
        /// 序列化输出保存到content
        /// </summary>
        /// <param name="block">目标块</param>
        public void update(ref Block block)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(stream, this);
            b.Serialize(stream, name);
            b.Serialize(stream, owner);
            b.Serialize(stream, SubFile);
            stream.GetBuffer().CopyTo(block.content, 0);
            stream.Close();
        }

        public static FCB read(byte[] content)
        {
            FCB fcb;
            MemoryStream stream = new MemoryStream(content);
            BinaryFormatter b = new BinaryFormatter();
            fcb = b.Deserialize(stream) as FCB;
            fcb.name=b.Deserialize(stream) as char[];
            fcb.owner = b.Deserialize(stream) as char[];
            fcb.SubFile = b.Deserialize(stream) as List<int>;
            stream.Close();
            return fcb;
        }

        /// <summary>
        /// 将子文件/文件夹的fcb块号添加到sub列表
        /// </summary>
        /// <param name="fcb"></param>
        public void addSub(int fcb)
        {
            SubFile.Add(fcb);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("contentblock", contentBlock);
            info.AddValue("isfolder", isFolder);
            info.AddValue("lastupdate", lastUpdate);
            info.AddValue("createtime", createTime);
        }
        protected FCB(SerializationInfo info, StreamingContext context)
        {
            contentBlock=info.GetInt32("contentblock");
            isFolder=info.GetBoolean("isfolder");
            lastUpdate=info.GetDateTime("lastupdate");
            createTime=info.GetDateTime("createtime");
        }

    }
}
