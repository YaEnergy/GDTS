using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace GD_Texture_Swapper
{
    class Program
    {
        public static Form ApplicationWindow = new Form();
        private static Size WindowSize = new Size(600, 320);

        public static string DataPath = Application.UserAppDataPath;
        public static string GDResourcePath = @"C:\Program Files (x86)\Steam\steamapps\common\Geometry Dash";
        private static string TexturePackFolderPath = "TexturePacks";
        private static string DefaultTexturePackName = "Default (2.11)";

        private static ComboBox TexturePackSelection = new()
        {
            Text = "Texture Packs",
            Location = new Point(10, 80),
            Size = new Size(240, 80),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        private static Button ApplyTextureButton = new Button()
        {
            Text = "Apply Texture Pack",
            Location = new Point(10, 120),
            Size = new Size(240, 40)
        };
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() 
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            // DataPath = Application.UserAppDataPath;
            if (!File.Exists("GDResourceFolderPath.txt")) 
                using (StreamWriter sw = File.CreateText("GDResourceFolderPath.txt")) 
                    await sw.WriteAsync(GDResourcePath);

            GDResourcePath = File.ReadAllText("GDResourceFolderPath.txt");

            if (!Directory.Exists(TexturePackFolderPath)) 
                Directory.CreateDirectory(TexturePackFolderPath).CreateSubdirectory(DefaultTexturePackName);

            if (!Directory.Exists(GDResourcePath))
            {
                DialogResult result = MessageBox.Show($"Couldn't find the GD Resources folder with the path given in GDResourceFolderPath.txt. Path given: {GDResourcePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            ApplicationWindow.Text = "Geometry Dash Texture Swapper (v1.0)";
            ApplicationWindow.MaximizeBox = false;
            ApplicationWindow.Size = WindowSize;
            ApplicationWindow.MaximumSize = WindowSize;
            ApplicationWindow.MinimumSize = WindowSize;

            ApplicationWindow.Icon = Icon.ExtractAssociatedIcon("logo.ico");

            ApplyTextureButton.Click += (o, s) => ApplyTexturePack(o, s);

            Label texturePacksLabel = new Label()
            {
                Text = "Texture Packs",
                Location = new Point(10, 55),
                Size = new Size(240, 25)
            };

            Button updateTexturePacksButton = new Button()
            {
                Text = "Refresh Texture Packs",
                Location = new Point(10, 160),
                Size = new Size(240, 40)
            };
            updateTexturePacksButton.Click += (o, s) => UpdateTexturePacks();

            UpdateTexturePacks();

            ApplicationWindow.Controls.Add(ApplyTextureButton);
            ApplicationWindow.Controls.Add(texturePacksLabel);
            ApplicationWindow.Controls.Add(updateTexturePacksButton);
            ApplicationWindow.Controls.Add(TexturePackSelection);

            Application.Run(ApplicationWindow);
        }
        static void UpdateTexturePacks()
        {
            TexturePackSelection.Items.Clear();
            string[] texturePackPaths = Directory.GetDirectories(TexturePackFolderPath);
            foreach (string texturePackPath in texturePackPaths)
            {
                string texturePackName = texturePackPath.Replace(@"TexturePacks\", "");
                TexturePackSelection.Items.Add(texturePackName);
            }
            TexturePackSelection.SelectedIndex = 0;
            TexturePackSelection.Update();
        }
        static void OverwriteTexturePackFile(string fileName, string filePath)
        {
            FileStream fs = new(filePath, FileMode.Open, FileAccess.ReadWrite);

            FileStream resource_fs = new(GDResourcePath + $@"\Resources\{fileName}", FileMode.Open, FileAccess.ReadWrite);

            fs.CopyTo(resource_fs);

            fs.Close();
            resource_fs.Close();
        }
        static void ApplyTexturePack(object? o, EventArgs s)
        {
            ApplyTextureButton.Text = "Applying...";
            ApplyTextureButton.Enabled = false;
            ApplyTextureButton.Update();
            try
            {
                int selectedIndex = TexturePackSelection.SelectedIndex;
                string? texturePackName = TexturePackSelection.Items[selectedIndex].ToString();
                if (texturePackName == null) return;
                //Texture swap
                string texturePackPath = TexturePackFolderPath + @"\" + texturePackName;
                string[] fileNames = Directory.GetFiles(TexturePackFolderPath + @"\" + texturePackName);
                string[] defaultFileNames = Directory.GetFiles(TexturePackFolderPath + @"\" + DefaultTexturePackName);
                for (int i = 0; i < defaultFileNames.Length; i++)
                {
                    string defaultFileName = defaultFileNames[i].Replace(TexturePackFolderPath + @"\" + DefaultTexturePackName, "");
                    if (defaultFileName.Contains(".dat")) continue;

                    if (!File.Exists(TexturePackFolderPath + $@"\{texturePackName}\{defaultFileName}")) 
                        OverwriteTexturePackFile(defaultFileName, TexturePackFolderPath + $@"\{DefaultTexturePackName}\{defaultFileName}"); //Use default texture
                    else 
                        OverwriteTexturePackFile(defaultFileName, TexturePackFolderPath + $@"\{texturePackName}\{defaultFileName}"); //Use found texture
                }

                MessageBox.Show("Successfully applied texture pack!", "Apply texture pack");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to apply texture pack. Reason: {ex.Message}", "Apply texture pack");
            }
            ApplyTextureButton.Enabled = true;
            ApplyTextureButton.Text = "Apply Texture Pack";
            ApplyTextureButton.Update();
        }
    }

}