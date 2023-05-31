namespace ScraperOne.Modules.Downloaders;

internal sealed class StreamWriterWithInfo : IDisposable
{
    private readonly bool i_isJson;
    private readonly StreamWriter i_sw;
    private bool i_hasElements;

    public StreamWriterWithInfo(string path, bool append, bool isJson)
    {
        i_isJson = isJson;
        if (!File.Exists(path))
        {
            i_sw = new StreamWriter(path, append);
            if (isJson) i_sw.WriteLine("[");
        }
        else
        {
            var fi = new FileInfo(path);
            if (fi.Length > 6) i_hasElements = true;
            var sr = new StreamReader(path);
            var line = sr.ReadLine() ?? "";
            sr.Close();
            var hasStartElement = line.TrimStart().StartsWith("[");
            var tmpPath = path + ".temp";
            if (isJson && line.Length > 0 && !hasStartElement) File.Move(path, tmpPath);

            i_sw = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate));
            if (append) i_sw.BaseStream.Seek(0, SeekOrigin.End);
            if (isJson && !hasStartElement) i_sw.WriteLine("[");
            if (isJson && line.Length > 0)
            {
                if (hasStartElement)
                {
                    i_sw.BaseStream.Seek(-3, SeekOrigin.End);
                }
                else
                {
                    var content = File.ReadAllText(tmpPath);
                    content = content.TrimEnd(',', '\r', '\n');
                    i_sw.WriteLine(content);
                    i_sw.Flush();
                    File.Delete(tmpPath);
                }
            }
        }
    }

    public void Dispose()
    {
        if (i_isJson) i_sw.WriteLine("]");

        i_sw.Dispose();
    }

    public void WriteLine(string text)
    {
        if (i_isJson && i_hasElements) text = $",\n{text}";
        i_hasElements = true;
        i_sw.WriteLine(text);
    }
}