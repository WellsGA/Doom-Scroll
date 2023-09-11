using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doom_Scroll
{
	public class DoomScrollImage
    {
        private byte sender;
        private int imgNumber;
        private byte[] image;
        private List<byte[]> imageList;

		// public gets
        public byte Sender { get; }
		public byte ImgNumber { get; }
		public byte[] Image { get; }
		public DoomScrollImage(int numMessages, byte send, int num)
		{
			sender = send;
			imgNumber = num;
			imageList = new List<byte[]>(numMessages);
			while (imageList.Count <= numMessages)
			{
				imageList.Add(null);
			}
			DoomScroll._log.LogInfo($"imageList Count is {imageList.Count}");

        }
		public int GetNumByteArraysExpected()
		{
			return imageList.Count;
		}
        public List<byte[]> GetImageList()
        {
            return new List<byte[]>(imageList);
        }

        public bool SameImage(byte send, byte num)
		{
			return sender == send && imgNumber == (int)num;
		}
		public void InsertByteChunk(int sectionIndex, byte[] byteChunk)
		{
			imageList.Insert(sectionIndex, byteChunk);
		}
		public bool CompileImage()
		{
			bool flag = imageList.IndexOf(null) == -1;
			bool result;
			if (flag)
			{
				List<byte> list = new List<byte>();
				foreach (byte[] collection in imageList)
				{
					list.AddRange(collection);
				}
				image = list.ToArray();
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}
		public List<int> GetMissingLines()
		{
			List<int> list = new List<int>();
			IEnumerable<int> enumerable = Enumerable.Range(0, imageList.Count);
			foreach (int num in enumerable)
			{
				bool flag = imageList[num] == null;
				if (flag)
				{
					list.Add(num);
				}
			}
			return list;
		}
		public override String ToString()
		{
			return $"player id is {sender}, image id is {imgNumber}";
		}
	}
}
