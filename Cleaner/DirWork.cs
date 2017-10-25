using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Cleaner.Colors;

namespace Cleaner
{
    internal class DirWork
    {
        internal string Path { get; set; }
        internal long Count { get; set; }
        internal long Size { get; set; }
        internal long Count_Del { get; set; }
        internal long Size_Del { get; set; }
        internal long Count_Error { get; set; }
        internal long Size_Error { get; set; }
        internal DirectoryInfo DirInfo { get; private set; }

        internal DirWork(string path)
        {
            Path = path;
            Count = 0;
            Size = 0;
            Count_Del = 0;
            Size_Del = 0;
            Count_Error = 0;
            Size_Error = 0;
            DirInfo = new DirectoryInfo(path);
        }

        internal void GetInfo()
        {
            try
            {
                foreach (FileInfo fileInfo in DirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    Size += fileInfo.Length;
                    Count++;
                }
                Console.WriteLine($"{Path} => {Count} files => {Converters.LongToString(Size)}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = error_color;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = main_color;
            }
        }

        internal void EmptyDir()
        {
            try
            {
                foreach (FileInfo fileInfo in DirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    try
                    {
                        fileInfo.Delete();
                        Size_Del += fileInfo.Length;
                        Count_Del++;
                    }
                    catch
                    {
                        Console.WriteLine($"Can not delete file: {fileInfo.Name}");
                        Size_Error += fileInfo.Length;
                        Count_Error++;
                    }
                }
                foreach (DirectoryInfo dir in DirInfo.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }
                    catch
                    {
                        Console.WriteLine($"Can not delete folder: {dir.Name}");
                    }
                }
                Console.WriteLine($"Deleted: {Count_Del} files => {Converters.LongToString(Size_Del)}");
                Console.WriteLine($"Can not delete: {Count_Error} files => {Converters.LongToString(Size_Error)}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = error_color;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = main_color;
            }
        }
    }
}
