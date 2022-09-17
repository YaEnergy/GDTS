using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;

namespace GD_Texture_Swapper
{
    class Program
    {
        public static Form ApplicationWindow = new Form();
        private static Size MainWindowSize = new Size(600, 320);

        private static string DataPath = Application.UserAppDataPath;
        public static string GDResourcePath = @"C:\Program Files (x86)\Steam\steamapps\common\Geometry Dash";
        private static string TexturePackFolderPath = "TexturePacks";

        private static string DefaultTexturePackName = "Default (2.11)";

        private Point mousePointPos = Point.Empty;
            
        private static ListBox TexturePackSelectionList = new()
        {
            Text = "Texture Packs",
            Location = new Point(25, 55),
            Size = new Size(240, 120),
        };
        private static ListBox TexturePackSelectedList = new()
        {
            Text = "Texture Packs",
            Location = new Point(300, 55),
            Size = new Size(240, 120),
            AllowDrop = true
        };
        private static Button ApplyTextureButton = new Button()
        {
            Text = "Apply Texture Pack",
            Location = new Point(25, 165),
            Size = new Size(240, 40)
        };
        private static Label ApplyTextureLabel = new()
        {
            Text = "",
            Location = new Point(280, 165),
            Size = new Size(240, 40),
            TextAlign = ContentAlignment.MiddleLeft
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

            ApplicationWindow.Text = "Geometry Dash Texture Swapper (v3)";
            ApplicationWindow.MaximizeBox = false;
            ApplicationWindow.Size = MainWindowSize;
            ApplicationWindow.MaximumSize = MainWindowSize;
            ApplicationWindow.MinimumSize = MainWindowSize;

            ApplicationWindow.Icon = Icon.ExtractAssociatedIcon(@"Assets\logo.ico");

            ApplyTextureButton.Click += (o, s) => ApplyTexturePack();

            Label TPLabel = new Label()
            {
                Text = "Available Texture Packs",
                Location = new Point(25, 30),
                Size = new Size(240, 25)
            };

            Label selectedTPsLabel = new Label()
            {
                Text = "Selected Texture Packs",
                Location = new Point(300, 30),
                Size = new Size(240, 25)
            };

            Button updateTexturePacksButton = new Button()
            {
                Text = "Refresh Texture Packs",
                Location = new Point(25, 210),
                Size = new Size(240, 40)
            };
            updateTexturePacksButton.Click += (o, s) => UpdateTexturePacks();

            UpdateTexturePacks();

            TexturePackSelectionList.MouseDown += (o, s) => ListBoxDragStart(o, s);
            TexturePackSelectionList.MouseMove += (o, s) => ListBoxDragUpdate(o, s);

            TexturePackSelectedList.DragOver += (o, s) => ListBoxDragOver(o, s);
            TexturePackSelectedList.DragDrop += (o, s) => AddTexturePack(o, s);
            TexturePackSelectedList.MouseDoubleClick += (o, s) => RemoveTexturePack(o, s);

            ApplicationWindow.Controls.Add(ApplyTextureButton);
            ApplicationWindow.Controls.Add(TPLabel);
            ApplicationWindow.Controls.Add(selectedTPsLabel);
            ApplicationWindow.Controls.Add(updateTexturePacksButton);
            ApplicationWindow.Controls.Add(TexturePackSelectionList);
            ApplicationWindow.Controls.Add(TexturePackSelectedList);

            Application.Run(ApplicationWindow);
        }

        private void ListBoxDragStart(object? sender, MouseEventArgs s)
        {
            ListBox? listbox = sender as ListBox;
            if (listbox == null)
                return;

            if (s.Button != MouseButtons.Left)
                return;

            mousePointPos = s.Location;
            int selectedIndex = listbox.IndexFromPoint(mousePointPos);
            if (selectedIndex == -1)
                mousePointPos = Point.Empty;
        }
        private void ListBoxDragUpdate(object? sender, MouseEventArgs s)
        {
            ListBox? listbox = sender as ListBox;
            if (listbox == null)
                return;

            if (s.Button == MouseButtons.Left)
                if ((mousePointPos != Point.Empty) && ((Math.Abs(s.X - mousePointPos.X) > SystemInformation.DragSize.Width) || (Math.Abs(s.Y - mousePointPos.Y) > SystemInformation.DragSize.Height)))
                    listbox.DoDragDrop(sender, DragDropEffects.Move);
        }
        private void ListBoxDragOver(object? sender, DragEventArgs s)
        {
            s.Effect = DragDropEffects.Move;
        }

        private void AddTexturePack(object? sender, DragEventArgs s)
        {
            ListBox? listBox = sender as ListBox;
            if (listBox == null)
                return;

            Point newPoint = new Point(s.X, s.Y);
            newPoint = listBox.PointToClient(newPoint);
            int selectedIndex = listBox.IndexFromPoint(newPoint);
            object item = TexturePackSelectionList.Items[TexturePackSelectionList.IndexFromPoint(mousePointPos)];
            if (selectedIndex == -1)
                listBox.Items.Add(item);
            else
                listBox.Items.Insert(selectedIndex, item);
        }
        private void RemoveTexturePack(object? sender, MouseEventArgs s)
        {
            ListBox? listBox = sender as ListBox;
            if (listBox == null)
                return;

            if (s.Button != MouseButtons.Left)
                return;

            Point newPoint = new Point(s.X, s.Y);
            //newPoint = listBox.PointToClient(newPoint);
            int selectedIndex = listBox.IndexFromPoint(newPoint);
            string? item = listBox.Items[selectedIndex].ToString();
            if (item == null || item == DefaultTexturePackName)
                return;

            if (selectedIndex != -1)
                listBox.Items.RemoveAt(selectedIndex);
        }
        static void UpdateTexturePacks()
        {
            TexturePackSelectionList.Items.Clear();
            TexturePackSelectedList.Items.Clear();

            string[] texturePackPaths = Directory.GetDirectories(TexturePackFolderPath);
            foreach (string texturePackPath in texturePackPaths)
            {
                string texturePackName = texturePackPath.Replace(@"TexturePacks\", "");

                if (texturePackName != DefaultTexturePackName)
                    TexturePackSelectionList.Items.Add(texturePackName);
            }

            TexturePackSelectedList.Items.Add(DefaultTexturePackName);
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

        static void ApplyTexturePack()
        {
            ApplicationWindow.Controls.Add(ApplyTextureLabel);
            ApplyTextureButton.Text = "Applying...";
            ApplyTextureButton.Enabled = false;
            ApplyTextureButton.Update();
            try
            {
                List<string> selectedTexturePacks = new();
                for (int i = 0; i < TexturePackSelectedList.Items.Count; i++)
                {
                    string? texturePackName = TexturePackSelectedList.Items[i].ToString();
                    if(texturePackName != null)
                        selectedTexturePacks.Add(texturePackName);
                }

                //Texture swap
                string[] defaultFileNames = Directory.GetFiles(TexturePackFolderPath + @"\" + DefaultTexturePackName);

                for (int i = 0; i < defaultFileNames.Length; i++)
                {
                    string defaultFileName = defaultFileNames[i].Replace(TexturePackFolderPath + @"\" + DefaultTexturePackName + @"\", "");
                    if (defaultFileName.Contains(".dat")) 
                        continue;

                    ApplyTextureButton.Text = $"Applying ({i + 1}/{defaultFileNames.Length})";
                    ApplyTextureButton.Update();
                    ApplyTextureLabel.Text = defaultFileName;
                    ApplyTextureLabel.Update();

                    string firstFoundTexturePath = "";
                    for (int j = 0; j < selectedTexturePacks.Count; j++)
                    {
                        int index = selectedTexturePacks.Count - 1 - j;
                        string texturePackName = selectedTexturePacks[index];
                        if (!File.Exists(TexturePackFolderPath + $@"\{texturePackName}\{defaultFileName}"))
                            continue;

                        firstFoundTexturePath = TexturePackFolderPath + $@"\{texturePackName}\{defaultFileName}";
                        break;
                    }

                    OverwriteTexturePackFile(defaultFileName, firstFoundTexturePath);
                }

                MessageBox.Show("Successfully applied texture pack!", "Apply texture pack");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to apply texture pack. Reason: {ex.Message}", "Apply texture pack");
            }
            ApplyTextureButton.Text = "Apply Texture Pack";
            ApplyTextureButton.Enabled = true;
            ApplyTextureButton.Update();
            ApplicationWindow.Controls.Remove(ApplyTextureLabel);
            return;
        }
    }

}