using System.Buffers.Binary;

static public class FortuneHelper
{
    static public async Task<string> GetRandom(string folderName)
    {
        var files = Directory.GetFiles(folderName, "*.dat");
        var datFileName = files[Random.Shared.Next(files.Length - 1)];
        using FileStream stream = new(datFileName, FileMode.Open, FileAccess.Read);
        BinaryReader reader = new(stream, System.Text.Encoding.ASCII);
        Header header;
        byte[] bytes = reader.ReadBytes(4);
        header.str_version = BinaryPrimitives.ReadUInt32BigEndian(bytes);
        bytes = reader.ReadBytes(4);
        header.str_numstr = BinaryPrimitives.ReadUInt32BigEndian(bytes);
        bytes = reader.ReadBytes(4);
        header.str_longlen = BinaryPrimitives.ReadUInt32BigEndian(bytes);
        bytes = reader.ReadBytes(4);
        header.str_shortlen = BinaryPrimitives.ReadUInt32BigEndian(bytes);
        bytes = reader.ReadBytes(4);
        header.str_flags = BinaryPrimitives.ReadUInt32BigEndian(bytes);
        header.str_delim = reader.ReadChar();
        reader.ReadBytes(3);

        var fortuneFlags = (FortuneFlags)header.str_flags;

        var searchFor = Random.Shared.Next((int)header.str_numstr - 1);

        var pointer = reader.BaseStream.Seek(24+(searchFor * 4), SeekOrigin.Begin);

        var pos = BinaryPrimitives.ReadUInt32BigEndian(reader.ReadBytes(4));

        var contentFileName = Path.Join(Path.GetDirectoryName(datFileName), Path.GetFileNameWithoutExtension(datFileName));
        using FileStream dataStream = new FileStream(contentFileName, FileMode.Open, FileAccess.Read);
        BinaryReader dataReader = new BinaryReader(dataStream, System.Text.Encoding.ASCII);
        dataReader.BaseStream.Seek(pos, SeekOrigin.Begin);
        StringWriter fortune = new StringWriter();

        while (true)
        {
            char c = dataReader.ReadChar();
            if (c == header.str_delim)
            {
                break;
            }

            fortune.Write(c);
        }

        if (fortuneFlags.HasFlag(FortuneFlags.STR_ROTATED))
        {
            return TransformRot13(fortune.ToString());
        }
        else
        {
            return fortune.ToString();
        }
    }

    /// <summary>
    /// Performs the ROT13 character rotation.
    /// </summary>
    public static string TransformRot13(string value)
    {
        char[] array = value.ToCharArray();
        for (int i = 0; i < array.Length; i++)
        {
            int number = (int)array[i];

            if (number >= 'a' && number <= 'z')
            {
                if (number > 'm')
                {
                    number -= 13;
                }
                else
                {
                    number += 13;
                }
            }
            else if (number >= 'A' && number <= 'Z')
            {
                if (number > 'M')
                {
                    number -= 13;
                }
                else
                {
                    number += 13;
                }
            }
            array[i] = (char)number;
        }
        return new string(array);
    }
}