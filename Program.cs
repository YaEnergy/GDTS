using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace GD_Texture_Swapper
{
    class Program
    {
        public static Form ApplicationWindow = new Form();
        private static Size MainWindowSize = new Size(600, 320);

        public static string DataPath = Application.UserAppDataPath;
        public static string GDResourcePath = @"C:\Program Files (x86)\Steam\steamapps\common\Geometry Dash";
        private static string TexturePackFolderPath = "TexturePacks";

        private static string DefaultTexturePackName = "Default (2.11)";
        private static string FallbackTexturePackName = "Default (2.11)";

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

        //Settings
        private static ComboBox FallbackTPSelection = new()
        {
            SelectedText = DefaultTexturePackName,
            Location = new Point(280, 105),
            Size = new Size(240, 80),
            DropDownStyle = ComboBoxStyle.DropDownList
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
                DialogResult result = MessageBox.Show($"Couldn't find the GD folder with the path given in GDResourceFolderPath.txt. Path given: {GDResourcePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            ApplicationWindow.Text = "Geometry Dash Texture Swapper (v2)";
            ApplicationWindow.MaximizeBox = false;
            ApplicationWindow.Size = MainWindowSize;
            ApplicationWindow.MaximumSize = MainWindowSize;
            ApplicationWindow.MinimumSize = MainWindowSize;

            ApplicationWindow.Icon = Icon.ExtractAssociatedIcon("logo.ico");

            ApplyTextureButton.Click += (o, s) => ApplyTexturePack(o, s);
            FallbackTPSelection.SelectionChangeCommitted += (o, s) => SetFallbackTexturePack();

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

            Label settingsLabel = new Label()
            {
                Text = "Settings",
                Location = new Point(280, 55),
                Size = new Size(240, 25)
            };

            Label fallbacktexturepackLabel = new Label()
            {
                Text = "Fallback Texture Pack",
                Location = new Point(280, 80),
                Size = new Size(240, 25)
            };

            UpdateTexturePacks();

            ApplicationWindow.Controls.Add(ApplyTextureButton);
            ApplicationWindow.Controls.Add(texturePacksLabel);
            ApplicationWindow.Controls.Add(updateTexturePacksButton);
            ApplicationWindow.Controls.Add(TexturePackSelection);

            ApplicationWindow.Controls.Add(settingsLabel);
            ApplicationWindow.Controls.Add(fallbacktexturepackLabel);
            ApplicationWindow.Controls.Add(FallbackTPSelection);

            Application.Run(ApplicationWindow);
        }

        static void SetFallbackTexturePack()
        {
            string? newFallbackTPname = FallbackTPSelection.SelectedItem.ToString();
            if (newFallbackTPname == null) return;

            string[] defaultFileNames = Directory.GetFiles(TexturePackFolderPath + @"\" + DefaultTexturePackName);
            FallbackTexturePackName = newFallbackTPname;
            try
            {
                foreach (string defaultFilePath in defaultFileNames)
                {
                    string defaultFileName = defaultFilePath.Replace($@"{TexturePackFolderPath}\{DefaultTexturePackName}\", "");

                    if (!File.Exists($@"{TexturePackFolderPath}\{FallbackTexturePackName}\{defaultFileName}"))
                        throw new Exception($@"File not found! : {TexturePackFolderPath}\{FallbackTexturePackName}\{defaultFileName}");
                }
            }
            catch (Exception ex)
            {
                FallbackTexturePackName = DefaultTexturePackName;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            FallbackTPSelection.SelectedItem = FallbackTexturePackName;
            FallbackTPSelection.Update();

            MessageBox.Show($"Set Fallback Texture Pack to {FallbackTexturePackName}", "Set Fallback Texture Pack");
        }
        static void UpdateTexturePacks()
        {
            TexturePackSelection.Items.Clear();
            FallbackTPSelection.Items.Clear();
            string[] texturePackPaths = Directory.GetDirectories(TexturePackFolderPath);
            foreach (string texturePackPath in texturePackPaths)
            {
                string texturePackName = texturePackPath.Replace(@"TexturePacks\", "");
                TexturePackSelection.Items.Add(texturePackName);
                FallbackTPSelection.Items.Add(texturePackName);
            }

            TexturePackSelection.SelectedItem = DefaultTexturePackName;
            TexturePackSelection.Update();

            FallbackTPSelection.SelectedItem = FallbackTexturePackName;
            if (FallbackTexturePackName != "Default (2.11)") 
                SetFallbackTexturePack();

            FallbackTPSelection.Update();
        }
        static void OverwriteTexturePackFile(string fileName, string filePath)
        {
            try
            {
                FileStream fs = new(filePath, FileMode.Open, FileAccess.ReadWrite);

                FileStream resource_fs = new(GDResourcePath + $@"\Resources\{fileName}", FileMode.Open, FileAccess.ReadWrite);

                fs.CopyTo(resource_fs);

                fs.Close();
                resource_fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        static Task ApplyTexturePack(object? o, EventArgs s)
        {
            ApplyTextureButton.Text = "Applying...";
            ApplyTextureButton.Enabled = false;
            ApplyTextureButton.Update();
            try
            {
                string texturePackName = TexturePackSelection.SelectedText;
                if (texturePackName == null) return Task.CompletedTask;
                //Texture swap
                string texturePackPath = TexturePackFolderPath + @"\" + texturePackName;
                string[] fileNames = Directory.GetFiles(TexturePackFolderPath + @"\" + texturePackName);
                string[] defaultFileNames = Directory.GetFiles(TexturePackFolderPath + @"\" + FallbackTexturePackName);

                for (int i = 0; i < defaultFileNames.Length; i++)
                {
                    string defaultFileName = defaultFileNames[i].Replace(TexturePackFolderPath + @"\" + FallbackTexturePackName, "");
                    if (defaultFileName.Contains(".dat")) continue;

                    ApplyTextureButton.Text = $"Applying... ({i + 1}/{defaultFileNames.Length})";
                    ApplyTextureButton.Update();

                    if (!File.Exists(TexturePackFolderPath + $@"\{texturePackName}\{defaultFileName}")) 
                        OverwriteTexturePackFile(defaultFileName, TexturePackFolderPath + $@"\{FallbackTexturePackName}\{defaultFileName}"); //Use default texture
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

            return Task.CompletedTask;
        }
    }

}