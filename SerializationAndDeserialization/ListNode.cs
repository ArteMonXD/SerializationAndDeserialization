using System.Text;

namespace SerializationAndDeserialization
{
    class ListNode
    {
        public ListNode Prev;
        public ListNode Next;
        public ListNode Rand; // произвольный элемент внутри списка
        public string Data;
    }

    class ListRand
    {
        public ListNode Head;
        public ListNode Tail;
        public int Count;

        public void Serialize(FileStream s)
        {
            Dictionary<ListNode, int> keyValuePairs = new Dictionary<ListNode, int>();
            ListNode current = Head;
            for (int i = 1; i<Count+1; i++)
            {
                keyValuePairs.Add(current, i);
                current = current.Next;
            }
            current = Head;
            List<byte> serializeData = new List<byte>();
            serializeData.AddRange(BitConverter.GetBytes(Count));
            for (int i = 0; i < Count; i++)
            {
                byte[] prevIndex = BitConverter.GetBytes(current.Prev != null ? keyValuePairs[current.Prev] : 0);
                byte[] nextIndex = BitConverter.GetBytes(current.Next != null ? keyValuePairs[current.Next] : 0);
                byte[] randIndex = BitConverter.GetBytes(current.Rand != null ? keyValuePairs[current.Rand] : 0);
                byte[] data = Encoding.Default.GetBytes(current.Data != null ? current.Data : "");
                byte[] length = BitConverter.GetBytes(prevIndex.Length + nextIndex.Length + randIndex.Length + data.Length);
                serializeData.AddRange(length);
                serializeData.AddRange(prevIndex);
                serializeData.AddRange(nextIndex);
                serializeData.AddRange(randIndex);
                serializeData.AddRange(data);
                current = current.Next;
            }
            s.Write(serializeData.ToArray(), 0, serializeData.Count);
        }

        public void Deserialize(FileStream s)
        {
            List<byte> data = new List<byte>();

            s.Read(data.ToArray(), 0, data.Count);
            byte[] countByteData = new byte[4];
            data.CopyTo(0, countByteData, 0, countByteData.Length);
            Count = BitConverter.ToInt32(countByteData, 0);
            int[] prevIndexes = new int[Count];
            int[] nextIndexes = new int[Count];
            int[] randIndexes = new int[Count];
            int byteCounter = 4;
            Dictionary<int, ListNode> keyValuePairs = new Dictionary<int, ListNode>();
            keyValuePairs.Add(0, null);
            ListNode current;
            for (int i = 0; i < Count; i++)
            {
                current = new ListNode();

                if (i == 0)
                    Head = current;
                else if (i == Count-1)
                    Tail = current;

                byte[] lenghtDataByte = new byte[4];
                data.CopyTo(byteCounter, lenghtDataByte, 0, lenghtDataByte.Length);
                int lengthData = BitConverter.ToInt32(lenghtDataByte);
                int interiorDataLenth = lengthData;
                byteCounter += 4;

                byte[] prevIndexByte = new byte[4];
                data.CopyTo(byteCounter, prevIndexByte, 0, prevIndexByte.Length);
                int prevIndex = BitConverter.ToInt32(prevIndexByte);
                prevIndexes[i] = prevIndex;
                byteCounter += 4;
                interiorDataLenth -= 4;

                byte[] nextIndexByte = new byte[4];
                data.CopyTo(byteCounter, nextIndexByte, 0, nextIndexByte.Length);
                int nextIndex = BitConverter.ToInt32(nextIndexByte);
                nextIndexes[i] = nextIndex;
                byteCounter += 4;
                interiorDataLenth -= 4;

                byte[] randIndexByte = new byte[4];
                data.CopyTo(byteCounter, randIndexByte, 0, randIndexByte.Length);
                int randIndex = BitConverter.ToInt32(randIndexByte);
                randIndexes[i] = randIndex;
                byteCounter += 4;
                interiorDataLenth -= 4;

                byte[] currentDataByte = new byte[interiorDataLenth];
                data.CopyTo(byteCounter, currentDataByte, 0, interiorDataLenth);
                string currentData = BitConverter.ToString(currentDataByte, 0, currentDataByte.Length);
                current.Data = currentData;
                keyValuePairs.Add(i + 1, current);
                byteCounter += interiorDataLenth;
            }
            current = Head;
            for (int i = 0; i < Count; i++)
            {
                current.Prev = keyValuePairs[prevIndexes[i]];
                current.Next = keyValuePairs[nextIndexes[i]];
                current.Rand = keyValuePairs[randIndexes[i]];

                current = current.Next;
            }
        }
    }
}
