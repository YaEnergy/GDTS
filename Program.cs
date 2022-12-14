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

        private static string DefaultTexturePackName = "Default";
        private static bool isResettingDefaultTP = false;

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
            Text = "Apply Texture Packs",
            Location = new Point(25, 165),
            Size = new Size(240, 40),
            Image = Image.FromFile(@"Assets\B_ApplyIcon.png"),
            ImageAlign = ContentAlignment.MiddleLeft
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
                Directory.CreateDirectory(TexturePackFolderPath);

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

            ApplicationWindow.Icon = Icon.ExtractAssociatedIcon(@"Assets\L_logo.ico");

            ApplyTextureButton.Click += (o, s) => ApplyTexturePackSetUp();

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
                Size = new Size(240, 40),
                Image = Image.FromFile(@"Assets\B_RefreshIcon.png"),
                ImageAlign = ContentAlignment.MiddleLeft
            };
            updateTexturePacksButton.Click += (o, s) => UpdateTexturePacks();

            Button resetDefaultTexturePackButton = new Button()
            {
                Text = "RESET DEFAULT",
                Location = new Point(300, 210),
                Size = new Size(240, 40),
                Image = Image.FromFile(@"Assets\B_ResetIcon.png"),
                ImageAlign = ContentAlignment.MiddleLeft
            };
            resetDefaultTexturePackButton.Click += (o, s) => ResetDefaultTexturePackSetUp();

            Button openTexturePacksFolderButton = new Button()
            {
                Text = "Open Texture Packs Folder",
                Location = new Point(300, 165),
                Size = new Size(240, 40),
            };
            openTexturePacksFolderButton.Click += (o, s) =>
            {
                Process.Start("explorer.exe", TexturePackFolderPath);
            };

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
            ApplicationWindow.Controls.Add(resetDefaultTexturePackButton);
            ApplicationWindow.Controls.Add(openTexturePacksFolderButton);
            ApplicationWindow.Controls.Add(TexturePackSelectionList);
            ApplicationWindow.Controls.Add(TexturePackSelectedList);

            bool createDefaultTPSuccess = true;
            if (!Directory.Exists(TexturePackFolderPath + @"\" + DefaultTexturePackName))
                createDefaultTPSuccess = ResetDefaultTexturePackSetUp();

            if (createDefaultTPSuccess)
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
            => s.Effect = DragDropEffects.Move;

        static void ResetDefaultTexturePackTask(object? o, DoWorkEventArgs s)
        {
            BackgroundWorker? sender = o as BackgroundWorker;
            if (sender == null)
                throw new Exception("No BackgroundWorker given!");

            string tpPath = TexturePackFolderPath + @"\" + DefaultTexturePackName;
            string[] filePaths = Directory.GetFiles(GDResourcePath + $@"\Resources");

            float sizeMBFinished = 0;
            float totalSizeMB = (float)TPFileManager.GetDirectorySize(new DirectoryInfo(GDResourcePath + $@"\Resources")) / 1024f / 1024f;

            foreach (string filePath in filePaths)
            {
                string file = Path.GetFileName(filePath);

                if (file.Contains(".dat"))
                    continue;

                Exception? ex = TPFileManager.OverwriteFile(tpPath + $@"\{file}", GDResourcePath + $@"\Resources\{file}");
                if (ex != null)
                    throw ex;

                FileStream fs = File.Open(GDResourcePath + $@"\Resources\{file}", FileMode.Open);
                sizeMBFinished += fs.Length / 1024f / 1024f;
                fs.Close();

                sender.ReportProgress((int)Math.Floor(sizeMBFinished / totalSizeMB * 100), $"Creating Default Texture Pack... ({Math.Floor(sizeMBFinished)}/{Math.Floor(totalSizeMB)} mb)");
            }
        }
        static bool ResetDefaultTexturePackSetUp()
        {
            if (isResettingDefaultTP)
                return true;

            isResettingDefaultTP = true;

            DirectoryInfo resourceFolder = new DirectoryInfo(GDResourcePath + $@"\Resources");
            long spaceReq = TPFileManager.GetDirectorySize(resourceFolder);
            bool hasSpace = TPFileManager.HasEnoughAvailableSpace(Path.GetPathRoot(Environment.CurrentDirectory), spaceReq);

            int spaceReqMB = (int)spaceReq / 1024 / 1024;

            string tpPath = TexturePackFolderPath + @"\" + DefaultTexturePackName;

            if (!hasSpace)
            {
                MessageBox.Show($"Creating/Resetting the default texture pack requires {spaceReqMB} mb! You don't have enough space!");
                isResettingDefaultTP = false;
                return false;
            }

            if (!Directory.Exists(tpPath))
            {
                DialogResult result = MessageBox.Show($"Because no Default Texture Pack exists yet, we'll be creating one. If you have any texture packs already on, you might want to click cancel and set ur GD resource files to default textures! ({spaceReqMB} mb)", "Create Default Texture Pack", MessageBoxButtons.OKCancel);
                if (result == DialogResult.Cancel || result == DialogResult.None)
                {
                    Application.Exit();
                    isResettingDefaultTP = false;
                    return false;
                }

                Directory.CreateDirectory(tpPath);
            }
            else
            {
                DialogResult result = MessageBox.Show("Are you sure you want to reset the default texture pack? If you have any texture packs already on, you might want to click cancel and set ur GD resource files to default textures!", "Default Texture Pack", MessageBoxButtons.OKCancel);
                if (result == DialogResult.Cancel || result == DialogResult.None)
                {
                    isResettingDefaultTP = false;
                    return true;
                }
            }

            LoadingBarForm loadingbarform = new LoadingBarForm("Default Texture Pack");
            loadingbarform.Icon = Icon.ExtractAssociatedIcon(@"Assets\L_ResetIcon.ico");
            loadingbarform.Show();

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (o, s) => ResetDefaultTexturePackTask(o, s);
            worker.ProgressChanged += (o, s) => loadingbarform.ProgressChangedLoad(o, s);
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = false;

            worker.RunWorkerCompleted += (o, s) =>
            {
                BackgroundWorker? sender = o as BackgroundWorker;
                if (sender == null)
                {
                    Application.Exit();
                    throw new Exception("No sender given!");
                }

                isResettingDefaultTP = false;

                loadingbarform.Close();
                worker.Dispose();

                if (s.Error != null)
                {
                    MessageBox.Show($"Failed to reset default texture pack. Reason: {s.Error.Message}", "Default Texture Pack");

                    Application.Exit();
                }
                else
                    MessageBox.Show("Successfully resetted the Default Texture Pack! If you had any texture packs already on, you might wanna check the Default texture pack files and overwrite them with the actual default geometry dash resource files.", "Default Texture Pack");

            };

            loadingbarform.cancelButton.Visible = false;

            worker.RunWorkerAsync();
            
            return true;
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

            ApplyTextureButton.Text = "Apply Texture Packs";
            ApplyTextureButton.Enabled = true;
            ApplyTextureButton.Update();
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
            if (selectedIndex == -1)
                return;

            string? item = listBox.Items[selectedIndex].ToString();
            if (item == null || item == DefaultTexturePackName)
                return;

            listBox.Items.RemoveAt(selectedIndex);

            ApplyTextureButton.Text = "Apply Texture Packs";
            ApplyTextureButton.Enabled = true;
            ApplyTextureButton.Update();
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

            ApplyTextureButton.Text = "Apply Texture Packs";
            ApplyTextureButton.Enabled = true;
            ApplyTextureButton.Update();
        }

        static void ApplyTexturePackTask(object? o, DoWorkEventArgs s)
        {
            BackgroundWorker? sender = o as BackgroundWorker;
            if (sender == null)
                throw new Exception("No BackgroundWorker given!");

            List<string> selectedTexturePacks = new();
            for (int i = 0; i < TexturePackSelectedList.Items.Count; i++)
            {
                string? texturePackName = TexturePackSelectedList.Items[i].ToString();
                if (texturePackName != null)
                    selectedTexturePacks.Add(texturePackName);
            }

            //Texture swap
            string[] defaultFileNames = Directory.GetFiles(TexturePackFolderPath + @"\" + DefaultTexturePackName);

            for (int i = 0; i < defaultFileNames.Length; i++)
            {
                if (sender.CancellationPending)
                {
                    s.Cancel = true;
                    return;
                }

                string defaultFileName = Path.GetFileName(defaultFileNames[i]);
                if (defaultFileName.Contains(".dat"))
                    continue;

                int progressPercentage = (int)Math.Floor((i + 1f) / defaultFileNames.Length * 100f);
                sender.ReportProgress(progressPercentage, $"Applying ({i + 1}/{defaultFileNames.Length}) {defaultFileName}");

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

                Exception? ex = TPFileManager.OverwriteFile(GDResourcePath + $@"\Resources\{defaultFileName}", firstFoundTexturePath);
                if (ex != null)
                    throw new Exception($"Failed to overwrite file! {ex.Message}");
            }

        }
        static void ApplyTexturePackSetUp()
        {
            if (isResettingDefaultTP)
                return;

            ApplicationWindow.Controls.Add(ApplyTextureLabel);
            ApplyTextureButton.Text = "Applying...";
            ApplyTextureButton.Enabled = false;
            ApplyTextureButton.Update();
            LoadingBarForm loadingbarform = new LoadingBarForm("Applying Texture Pack");
            loadingbarform.Icon = Icon.ExtractAssociatedIcon(@"Assets\L_ApplyIcon.ico");
            loadingbarform.Show();

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (o, s) => ApplyTexturePackTask(o, s);
            worker.ProgressChanged += (o, s) => loadingbarform.ProgressChangedLoad(o, s);
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;


            worker.RunWorkerCompleted += (o, s) =>
            {
                BackgroundWorker? sender = o as BackgroundWorker;
                if (sender == null)
                {
                    Application.Exit();
                    throw new Exception("No sender given!");
                }

                ApplyTextureButton.Text = "(No changes made)";
                ApplyTextureButton.Enabled = false;
                ApplyTextureButton.Update();
                ApplicationWindow.Controls.Remove(ApplyTextureLabel);

                loadingbarform.Close();
                worker.Dispose();

                if (s.Cancelled || s.Error != null)
                {
                    if (s.Error != null)
                        MessageBox.Show($"Failed to apply texture pack. Reason: {s.Error.Message}", "Apply texture pack");

                    DialogResult applyDefaultTP = MessageBox.Show("Would you like to reapply the Default Texture Pack?", "Cancel Apply Texture Pack", MessageBoxButtons.YesNo);
                    if (applyDefaultTP == DialogResult.Yes)
                    {
                        UpdateTexturePacks();
                        ApplyTexturePackSetUp();
                    }
                }
                else
                    MessageBox.Show("Successfully applied texture pack!", "Apply texture pack");

            };

            loadingbarform.CancelLoad += () => worker.CancelAsync();

            worker.RunWorkerAsync();
        }
    }

}