using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudioCore.Utilities;

public class WindowsUtils
{
    public static string GetFolderSelection(string title = "")
    {
        var filePath = "";
        using (var fbd = new FolderBrowserDialog())
        {
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                filePath = fbd.SelectedPath;
            }
        }

        return filePath;
    }

    public static string GetFileSelection(string title = "", List<string> filters = null)
    {
        var filePath = "";
        using (var fbd = new OpenFileDialog())
        {
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
            {
                filePath = fbd.FileName;
            }
        }

        return filePath;
    }
}
