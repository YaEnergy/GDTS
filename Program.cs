using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace GD_Texture_Swapper
{
    class Program
    {
        public static Form ApplicationWindow = new Form();
        private static Size WindowSize = new Size(900, 600);

        public static string DataPath = Application.UserAppDataPath;
        public static string GDResourcePath = @"C:\Program Files (x86)\Steam\steamapps\common\Geometry Dash";
        private static string TexturePackFolderPath = "TexturePacks";

        private static TexturePack? SelectedTexturePack = null;
        private static ComboBox TexturePackSelection = new()
        {
            Location = new Point(10, 80),
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
            {
                using (StreamWriter sw = File.CreateText("GDResourceFolderPath.txt")) await sw.WriteAsync(GDResourcePath);
            }
            GDResourcePath = File.ReadAllText("GDResourceFolderPath.txt");

            if (!Directory.Exists(TexturePackFolderPath))
            {
                Directory.CreateDirectory(TexturePackFolderPath).CreateSubdirectory("Default");
            }

            if (!Directory.Exists(GDResourcePath))
            {
                DialogResult result = MessageBox.Show($"Couldn't find the GD Resources folder with the path given in GDResourceFolderPath.txt. Path given: {GDResourcePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            ApplicationWindow.Text = "Geometry Dash Texture Swapper (vAlpha)";
            ApplicationWindow.MaximizeBox = false;
            ApplicationWindow.Size = WindowSize;
            ApplicationWindow.MaximumSize = WindowSize;
            ApplicationWindow.MinimumSize = WindowSize;

            Button applyTextureButton = new Button()
            {
                Text = "Apply Texture Pack",
                Location = new Point(10, 120),
                Size = new Size(240, 40)
            };
            applyTextureButton.Click += (o, s) => ApplyTexturePack(o, s);

            Button updateTexturePacksButton = new Button()
            {
                Text = "Update Texture Packs",
                Location = new Point(10, 160),
                Size = new Size(240, 40)
            };
            updateTexturePacksButton.Click += (o, s) => UpdateTexturePacks();

            UpdateTexturePacks();

            ApplicationWindow.Controls.Add(applyTextureButton);
            ApplicationWindow.Controls.Add(updateTexturePacksButton);
            ApplicationWindow.Controls.Add(TexturePackSelection);

            Application.Run(ApplicationWindow);
        }
        static void UpdateTexturePacks()
        {
            TexturePackSelection.Items.Clear();
            string[] texturePackPaths = Directory.GetDirectories("TexturePacks");
            foreach (string texturePackPath in texturePackPaths)
            {
                string texturePackName = texturePackPath.Replace(@"TexturePacks\", "");
                if (TexturePackSelection.Items.Contains(texturePackName))
                {
                    DialogResult result = MessageBox.Show($"Texture Pack: {texturePackName} already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }
                
                TexturePackSelection.Items.Add(texturePackName);
            }
            TexturePackSelection.Update();
        }
        static void ApplyTexturePack(object? o, EventArgs s)
        {
            MessageBox.Show("Successfully applied texture pack!", "Apply texture pack");

           
        }
    }

    public class TexturePack
    {
        public static Dictionary<string, Image> Images = new();
        public static string TexturePackPath = "";
        public TexturePack(string texturePackPath, Dictionary<string, Image> images)
        {
            TexturePackPath = texturePackPath;
            Images = images;
        }
    }
}