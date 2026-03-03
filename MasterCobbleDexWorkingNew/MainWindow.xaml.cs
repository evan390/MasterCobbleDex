using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Linq;
using static MasterCobbleDexWorkingNew.App;
using static System.Net.Mime.MediaTypeNames;

namespace MasterCobbleDexWorkingNew
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string dexPath = "";
        List<CheckBox> modCheckboxes = new List<CheckBox>();
        ObservableCollection<App.Pokemon> pokemonList = new ObservableCollection<App.Pokemon>();
        List<Move> moveLookUp = new List<Move>();
        List<Ability> abilityLookUp = new List<Ability>();
        public MainWindow()
        {
            InitializeComponent();
            folderInit();
            CountFolders();
            MessageBoxResult result = MessageBox.Show("Do you want to create a Master Dex using existing data?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                RefreshDataGrid();
        }
        private void Add_Content_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog cobblemonDialog = new OpenFileDialog();
                cobblemonDialog.Filter = "MOD files (*.zip; *.jar)|*.zip; *.jar";
                cobblemonDialog.Title = "Select a Cobblemon Mod or Datapack";
                cobblemonDialog.Multiselect = true;

                if (cobblemonDialog.ShowDialog() == true)
                {

                    foreach (string filePath in cobblemonDialog.FileNames)
                    {
                        //condenses file name
                        string modName = Path.GetFileNameWithoutExtension(filePath);
                        modName = modName.ToLowerInvariant();
                        string[] modLoaders = { "fabric", "neoforge", "forge", "+", "(", ")", "[", "]", "v." };
                        foreach (var item in modLoaders)
                        {
                            modName = modName.Replace(item, "");
                        }
                        modName = Regex.Replace(modName, @"v?\d+(\.\d+)*", "");
                        modName = Regex.Replace(modName, @"[\s\-_\.]+", "");
                        modName = modName.Trim();
                        if (String.IsNullOrWhiteSpace(modName) || modName == "client" || modName == "minecraftjar")
                            modName = "minecraft";

                        string modPath = Path.Combine(dexPath, modName);

                        if (Directory.Exists(modPath))
                        {
                            MessageBox.Show("Mod already added, would you like to re-add it?");
                        }

                        Directory.CreateDirectory(modPath);
                        string speciesPath = Path.Combine(modPath, "species");
                        Directory.CreateDirectory(speciesPath);
                        string speciesAdditionsPath = Path.Combine(modPath, "species_additions");
                        Directory.CreateDirectory(speciesAdditionsPath);
                        string spawnPoolWorldPath = Path.Combine(modPath, "spawn_pool_world");
                        Directory.CreateDirectory(spawnPoolWorldPath);
                        string biomeTagPath = Path.Combine(modPath, "biome_tag");
                        Directory.CreateDirectory(biomeTagPath);
                        string movePath = Path.Combine(modPath, "moves");
                        Directory.CreateDirectory(movePath);
                        string abilityPath = Path.Combine(modPath, "abilities");
                        Directory.CreateDirectory(abilityPath);
                        string jsPath = Path.Combine(modPath, "js");
                        Directory.CreateDirectory(jsPath);

                        try
                        {

                            using (ZipArchive zip = ZipFile.OpenRead(filePath))
                            {
                                List<ZipArchiveEntry> speciesEntries = new List<ZipArchiveEntry>();
                                List<ZipArchiveEntry> speciesAdditionsEntries = new List<ZipArchiveEntry>();
                                List<ZipArchiveEntry> spawnPoolWorldEntries = new List<ZipArchiveEntry>();
                                List<ZipArchiveEntry> biomeTagEntries = new List<ZipArchiveEntry>();
                                List<ZipArchiveEntry> moveEntries = new List<ZipArchiveEntry>();
                                List<ZipArchiveEntry> abilityEntries = new List<ZipArchiveEntry>();

                                foreach (ZipArchiveEntry entry in zip.Entries)
                                {
                                    //species files
                                    if (!string.IsNullOrEmpty(entry.FullName) && entry.FullName.StartsWith("data/") && entry.FullName.Contains("/species/") && entry.FullName.EndsWith(".json"))
                                    {
                                        speciesEntries.Add(entry);
                                    }
                                    //species additions files
                                    if (!string.IsNullOrEmpty(entry.FullName) && entry.FullName.StartsWith("data/") && entry.FullName.Contains("/species_additions/") && entry.FullName.EndsWith(".json"))
                                    {
                                        speciesAdditionsEntries.Add(entry);
                                    }
                                    //spawn pool world files
                                    if (!string.IsNullOrEmpty(entry.FullName) && entry.FullName.StartsWith("data/") && entry.FullName.Contains("/spawn_pool_world/") && entry.FullName.EndsWith(".json"))
                                    {
                                        spawnPoolWorldEntries.Add(entry);
                                    }
                                    //biome tag files
                                    if (!string.IsNullOrEmpty(entry.FullName) && entry.FullName.StartsWith("data/") && entry.FullName.Contains("/tags/worldgen/biome/") && entry.FullName.EndsWith(".json"))
                                    {
                                        biomeTagEntries.Add(entry);
                                    }
                                }


                                foreach (ZipArchiveEntry entry in speciesEntries)
                                {
                                    string destinationPath = Path.Combine(speciesPath, entry.Name);
                                    using (var entryStream = entry.Open())
                                    using (var fileStream = File.Create(destinationPath))
                                    {
                                        entryStream.CopyTo(fileStream);
                                    }

                                }

                                foreach (ZipArchiveEntry entry in speciesAdditionsEntries)
                                {
                                    string destinationPath = Path.Combine(speciesAdditionsPath, entry.Name);
                                    using (var entryStream = entry.Open())
                                    using (var fileStream = File.Create(destinationPath))
                                    {
                                        entryStream.CopyTo(fileStream);
                                    }

                                }

                                foreach (ZipArchiveEntry entry in spawnPoolWorldEntries)
                                {
                                    string normalizedFileName = Regex.Replace(entry.Name, @"^\d{4}_", "");
                                    string destinationPath = Path.Combine(spawnPoolWorldPath, normalizedFileName);
                                    using (var entryStream = entry.Open())
                                    using (var fileStream = File.Create(destinationPath))
                                    {
                                        entryStream.CopyTo(fileStream);
                                    }

                                }
                                foreach (ZipArchiveEntry entry in biomeTagEntries)
                                {
                                    string destinationPath = Path.Combine(biomeTagPath, entry.ToString().Substring(entry.ToString().IndexOf("biome/") + "biome/".Length).Replace('/', '.'));
                                    using (var entryStream = entry.Open())
                                    using (var fileStream = File.Create(destinationPath))
                                    {
                                        entryStream.CopyTo(fileStream);
                                    }

                                }

                                if (!Directory.EnumerateFileSystemEntries(speciesPath).Any() && !Directory.EnumerateFileSystemEntries(speciesAdditionsPath).Any() && !Directory.EnumerateFileSystemEntries(spawnPoolWorldPath).Any() && !Directory.EnumerateFileSystemEntries(biomeTagPath).Any())
                                {
                                    Directory.Delete(modPath, true);
                                }
                            }

                        }
                        catch
                        {
                            Directory.Delete(modPath, true);
                            MessageBox.Show(filePath + " is unable to load");
                        }
                        if(modName == "cobblemon")
                        {
                            importCobblemonShowdown(filePath);
                            importCobblemonMoves();
                            importCobblemonAbilities();
                        }
                        else
                        {
                            importAddonShowdown(filePath, modName);
                            importAddonMoves(modName);
                            importAddonAbilities(modName);
                        }

                    }
                    grdEdit.Visibility = Visibility.Visible;
                    grdDex.Visibility = Visibility.Collapsed;

                }
                CountFolders();
            }
            catch
            {
                MessageBox.Show("adding content error");
                CountFolders();

            }
        }
        private void folderInit()
        {
            string keyPath = @"Software\MasterCobbleDex";

            using RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath);

            if (key != null)
            {
                dexPath = Path.Combine(key.GetValue("DexPath")?.ToString(), "MasterCobbleDex");

                Directory.CreateDirectory(dexPath);

            }
            else
            {

                RegistryKey keyNew = Registry.CurrentUser.CreateSubKey(keyPath);

                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                keyNew.SetValue("DexPath", documentsPath);

                dexPath = Path.Combine(documentsPath, "MasterCobbleDex");

                Directory.CreateDirectory(dexPath);

                keyNew.Close();

            }
            key.Close();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Do you want to make a new Master Dex?","Confirmation",MessageBoxButton.YesNo,MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
                string masterPath = Path.Combine(dexPath, "master");
                string masterSpeciesPath = Path.Combine(masterPath, "species");
                string masterSpawnPoolPath = Path.Combine(masterPath, "spawnpool");
                string masterMovesPath = Path.Combine(masterPath, "moves");
                string masterAbilitiesPath = Path.Combine(masterPath, "abilities");
                Directory.CreateDirectory(masterPath);

                foreach (var file in Directory.EnumerateFiles(masterPath))
                {
                    File.Delete(file);
                }

                foreach (var dir in Directory.EnumerateDirectories(masterPath))
                {
                    Directory.Delete(dir, recursive: true);
                }

                Directory.CreateDirectory(masterSpeciesPath);
                Directory.CreateDirectory(masterSpawnPoolPath);
                Directory.CreateDirectory(masterMovesPath);
                Directory.CreateDirectory(masterAbilitiesPath);

                if (!Directory.Exists(Path.Combine(dexPath, "cobblemon")))
                {
                    MessageBox.Show("Import base mod cobblemon before making a Master Dex");
                    return;
                }
                if (!Directory.Exists(Path.Combine(dexPath, "minecraft")))
                {
                    MessageBox.Show("Import base minecraft jar before making a Master Dex. This can be found in your AppData/.minecraft/versions folder");
                    return;
                }

                foreach (string file in Directory.EnumerateFiles(Path.Combine(dexPath, "cobblemon", "species"), "*.json"))
                {
                    string destPath = Path.Combine(masterSpeciesPath, Path.GetFileName(file));
                    File.Copy(file, destPath, overwrite: true);
                }
                foreach (string file in Directory.EnumerateFiles(Path.Combine(dexPath, "cobblemon", "spawn_pool_world"), "*.json"))
                {
                    string destPath = Path.Combine(masterSpawnPoolPath, Path.GetFileName(file));
                    File.Copy(file, destPath, overwrite: true);
                }
                foreach (string file in Directory.EnumerateFiles(Path.Combine(dexPath, "cobblemon", "moves"), "*.json"))
                {
                    string destPath = Path.Combine(masterMovesPath, Path.GetFileName(file));
                    File.Copy(file, destPath, overwrite: true);
                }
                foreach (string file in Directory.EnumerateFiles(Path.Combine(dexPath, "cobblemon", "abilities"), "*.json"))
                {
                    string destPath = Path.Combine(masterAbilitiesPath, Path.GetFileName(file));
                    File.Copy(file, destPath, overwrite: true);
                }

                List<string> folderNames = Directory.GetDirectories(dexPath).Select(Path.GetFileName).ToList();
                foreach (CheckBox chkbx in modCheckboxes)
                {
                    if (chkbx.IsChecked == true)
                    {
                        string folderName = chkbx.Content.ToString();
                        foreach (string file in Directory.EnumerateFiles(Path.Combine(dexPath, folderName, "species"), "*.json"))
                        {
                            string destPath = Path.Combine(masterSpeciesPath, Path.GetFileName(file));
                            File.Copy(file, destPath, overwrite: true);
                        }

                    }


                }
                //species additions done after incase changes are made to the base cobblemon file through species
                foreach (CheckBox chkbx in modCheckboxes)
                {
                    if (chkbx.IsChecked == true)
                    {
                        string folderName = chkbx.Content.ToString();

                        foreach (string file in Directory.EnumerateFiles(Path.Combine(dexPath, folderName, "species_additions"), "*.json"))
                        {
                            JObject additionsJSON = JObject.Parse(File.ReadAllText(file));

                            string target = additionsJSON["target"].ToString().Split(':').Last();
                            string masterPokemonPath = Path.Combine(masterSpeciesPath, target + ".json");
                            MergeSpecies(masterPokemonPath, file);
                        }

                        foreach (string file in Directory.EnumerateFiles(Path.Combine(dexPath, folderName, "spawn_pool_world"), "*.json"))
                        {
                            JObject spawnPool = JObject.Parse(File.ReadAllText(file));

                            if (spawnPool.SelectToken("spawns[0].pokemon") != null)
                            {
                                string pokemon = spawnPool["spawns"][0]["pokemon"].ToString().Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)[0];
                                if (File.Exists(Path.Combine(masterSpawnPoolPath, pokemon + ".json")))
                                {
                                    MergeSpawnPools(Path.Combine(masterSpawnPoolPath, pokemon + ".json"), file);
                                }
                                else
                                {
                                    string destPath = Path.Combine(masterSpawnPoolPath, Path.GetFileName(file));
                                    File.Copy(file, destPath, overwrite: true);
                                }
                            }
                        }
                        foreach (string file in Directory.EnumerateFiles(Path.Combine(dexPath, folderName, "moves"), "*.json"))
                        {
                            string destPath = Path.Combine(masterMovesPath, Path.GetFileName(file));
                            File.Copy(file, destPath, overwrite: true);
                        }

                        foreach (string file in Directory.EnumerateFiles(Path.Combine(dexPath, folderName, "abilities"), "*.json"))
                        {
                            string destPath = Path.Combine(masterAbilitiesPath, Path.GetFileName(file));
                            File.Copy(file, destPath, overwrite: true);
                        }


                    }

                }

                RefreshDataGrid();
                grdEdit.Visibility = Visibility.Collapsed;
                grdDex.Visibility = Visibility.Visible;
                btnPanelSwitch.Content = "Edit Loaded Content";


            }
            catch
            {

                RefreshDataGrid();
                MessageBox.Show("masterdex error");
            }


        }
        private void MergeSpecies(string original, string additions)
        {
            try
            {

                JObject originalJSON = JObject.Parse(File.ReadAllText(original));
                JObject additionsJSON = JObject.Parse(File.ReadAllText(additions));

                originalJSON.Merge(additionsJSON, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union,
                    MergeNullValueHandling = MergeNullValueHandling.Ignore
                });

                if (additionsJSON.TryGetValue("abilities", out JToken sourceTokenAbilities) && sourceTokenAbilities is JArray sourceArrayAbilities)
                {
                    originalJSON["abilities"] = sourceArrayAbilities.DeepClone();
                }
                if (additionsJSON.TryGetValue("moves", out JToken sourceTokenMoves) && sourceTokenMoves is JArray sourceArrayMoves)
                {
                    originalJSON["moves"] = sourceArrayMoves.DeepClone();
                }

                originalJSON.Property("target").Remove();

                File.WriteAllText(
                    original,
                    originalJSON.ToString(Newtonsoft.Json.Formatting.Indented)
                );

            }
            catch
            {
                MessageBox.Show("editing species json error with " + additions);

            }


        }
        private void MergeSpawnPools(string original, string additions)
        {
            try
            {

                JObject originalJSON = JObject.Parse(File.ReadAllText(original));
                JObject additionsJSON = JObject.Parse(File.ReadAllText(additions));

                originalJSON.Merge(additionsJSON, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union,
                    MergeNullValueHandling = MergeNullValueHandling.Ignore
                });

                File.WriteAllText(
                    original,
                    originalJSON.ToString(Newtonsoft.Json.Formatting.Indented)
                );

            }
            catch
            {
                MessageBox.Show("editing spawn pool json error with " + additions);

            }


        }
        private void CountFolders()
        {
            try
            {
                List<string> folderNames = Directory.GetDirectories(dexPath).Select(dir => Path.GetFileName(dir)).ToList();
                grdMods.RowDefinitions.Clear();
                grdMods.Children.Clear();
                modCheckboxes.Clear();
                int count = 0;
                foreach (string name in folderNames)
                {
                    if (name != "cobblemon" && name != "master" && name != "minecraft")
                    {
                        RowDefinition row = new RowDefinition();
                        GridLengthConverter grc = new GridLengthConverter();
                        row.Height = (GridLength)grc.ConvertFromString("30");
                        grdMods.RowDefinitions.Add(row);
                        CheckBox chkbx = new CheckBox();
                        chkbx.SetValue(Grid.RowProperty, count);
                        chkbx.IsChecked = true;
                        chkbx.Content = name;
                        chkbx.Margin = new Thickness(5);
                        chkbx.VerticalAlignment = VerticalAlignment.Center;
                        modCheckboxes.Add(chkbx);
                        grdMods.Children.Add(chkbx);
                        count++;
                    }
                }

            }
            catch
            {
                MessageBox.Show("counting folders error");

            }

        }
        private void RefreshDataGrid()
        {
            try
            {
                dtgPokemon.ItemsSource = null;
                string masterSpeciesPath = Path.Combine(dexPath, "master", "species");
                string masterSpawnPoolPath = Path.Combine(dexPath, "master", "spawnpool");
                string masterMovePath = Path.Combine(dexPath, "master", "moves");
                string masterAbilityPath = Path.Combine(dexPath, "master", "abilities");
                List<string> fileNames = new List<string>();
                try
                {
                    fileNames = Directory.GetFiles(masterSpeciesPath).Select(Path.GetFileNameWithoutExtension).ToList();
                }
                catch
                {
                    return;
                }
                createMoveLookUp();
                createAbilityLookUp();
                pokemonList.Clear();
                foreach (string name in fileNames)
                {
                    try
                    {
                        JObject species = JObject.Parse(File.ReadAllText(Path.Combine(masterSpeciesPath, name + ".json")));
                        JObject spawnpool = null;
                        if (File.Exists(Path.Combine(masterSpawnPoolPath, name + ".json")))
                            spawnpool = JObject.Parse(File.ReadAllText(Path.Combine(masterSpawnPoolPath, name + ".json")));

                        App.Pokemon pkmn = new App.Pokemon();
                        pkmn.Name = ProperString(species.SelectToken("name")?.ToString());
                        pkmn.Form = "Default";
                        pkmn.Type1 = species.SelectToken("primaryType")?.ToString();
                        pkmn.Type2 = species.SelectToken("secondaryType")?.ToString();
                        pkmn.MaleRatio = Convert.ToDouble(species.SelectToken("maleRatio")?.ToString());
                        pkmn.HP = Convert.ToInt32(species.SelectToken("baseStats.hp")?.ToString());
                        pkmn.Attack = Convert.ToInt32(species.SelectToken("baseStats.attack")?.ToString());
                        pkmn.Defense = Convert.ToInt32(species.SelectToken("baseStats.defence")?.ToString());
                        pkmn.SpecialAttack = Convert.ToInt32(species.SelectToken("baseStats.special_attack")?.ToString());
                        pkmn.SpecialDefense = Convert.ToInt32(species.SelectToken("baseStats.special_defence")?.ToString());
                        pkmn.Speed = Convert.ToInt32(species.SelectToken("baseStats.speed")?.ToString());
                        pkmn.FormSearch = -1;

                        pkmn.PreEvo = new App.PreEvolution();

                        if (species["preEvolution"] != null)
                        {
                            List<string> preEvoList = species.SelectToken("preEvolution")?.ToString().Split(" ").ToList();
                            pkmn.PreEvo.PreEvolutionName = preEvoList[0];
                            if (preEvoList.Count > 1)
                            {
                                pkmn.PreEvo.PreEvolutionForm = preEvoList[1];
                            }

                        }

                        pkmn.Abilities = new List<App.Ability>();

                        JArray mainAbilities = new JArray();
                        if (species["abilities"] != null)
                        {
                            mainAbilities = species["abilities"] as JArray;
                        }
                        else
                        {
                            mainAbilities = species.SelectToken("forms[0].abilities") as JArray;
                        }

                        foreach (var ability in mainAbilities)
                        {
                            App.Ability newAbility = new App.Ability();
                            List<string> abilityParts = ability.ToString().Split(':').ToList();
                            if (abilityParts[0] == "h")
                            {
                                newAbility.AbilityName = abilityParts[1];
                                newAbility.Hidden = true;
                            }
                            else
                            {
                                newAbility.AbilityName = abilityParts[0];
                                newAbility.Hidden = false;
                            }
                            Ability foundAbility = abilityLookUp.FirstOrDefault(a => a.AbilityName == newAbility.AbilityName);
                            if (foundAbility != null)
                                newAbility.Description = foundAbility.Description;
                            pkmn.Abilities.Add(newAbility);
                        }
                        pkmn.EggGroups = new List<string>();

                        if (species["eggGroups"] != null)
                        {
                            foreach (var eggGroup in species["eggGroups"] as JArray)
                            {
                                pkmn.EggGroups.Add(eggGroup.ToString());
                            }
                        }
                        pkmn.EVS = new App.EVYield();
                        pkmn.EVS.HP = Convert.ToInt32(species.SelectToken("evYield.hp")?.ToString());
                        pkmn.EVS.Attack = Convert.ToInt32(species.SelectToken("evYield.attack")?.ToString());
                        pkmn.EVS.Defense = Convert.ToInt32(species.SelectToken("evYield.defence")?.ToString());
                        pkmn.EVS.SpecialAttack = Convert.ToInt32(species.SelectToken("evYield.special_attack")?.ToString());
                        pkmn.EVS.SpecialDefense = Convert.ToInt32(species.SelectToken("evYield.special_defence")?.ToString());
                        pkmn.EVS.Speed = Convert.ToInt32(species.SelectToken("evYield.speed")?.ToString());
                        pkmn.Drops = new List<App.Drop>();
                        if (species["drops"] != null)
                        {
                            if (species.SelectToken("drops.entries") != null)
                            {
                                for (int i = 0; i < (species.SelectToken("drops.entries") as JArray).Count; i++)
                                {
                                    App.Drop newDrop = new App.Drop();
                                    newDrop.Item = species.SelectToken("drops.entries[" + i + "].item")?.ToString().Split(":")[1];
                                    newDrop.Location = species.SelectToken("drops.entries[" + i + "].item")?.ToString().Split(":")[0];
                                    newDrop.QuantityRange = species.SelectToken("drops.entries[" + i + "].quantityRange")?.ToString();
                                    newDrop.Percent = Convert.ToDouble(species.SelectToken("drops.entries[" + i + "].percentage")?.ToString());
                                    pkmn.Drops.Add(newDrop);
                                }
                            }
                        }
                        pkmn.Moves = new List<App.Move>();

                        JArray mainMoves = new JArray();
                        if (species["moves"] != null)
                        {
                            mainMoves = species["moves"] as JArray;
                        }
                        else
                        {
                            mainMoves = species.SelectToken("forms[0].moves") as JArray;
                        }

                        foreach (var move in mainMoves)
                        {
                            App.Move newMove = new App.Move();
                            List<string> moveParts = move.ToString().Split(':').ToList();
                            if(moveParts.Count > 1)
                                newMove.MoveName = moveParts[1];
                            else
                                newMove.MoveName = moveParts[0];
                            newMove.MoveType = moveParts[0];

                            Move foundMove = moveLookUp.FirstOrDefault(m => m.MoveName == newMove.MoveName);
                            if (foundMove != null)
                            {
                                newMove.Accuracy = foundMove.Accuracy.Trim();
                                newMove.BasePower = foundMove.BasePower.Trim();
                                newMove.Category = foundMove.Category.Trim();
                                newMove.PP = foundMove.PP.Trim();
                                newMove.Type = foundMove.Type.Trim();
                                newMove.Description = foundMove.Description;

                            }
                            pkmn.Moves.Add(newMove);
                        }
                        pkmn.Evolutions = new List<App.Evolution>();
                        if (species["evolutions"] != null)
                        {
                            for (int i = 0; i < (species.SelectToken("evolutions") as JArray).Count; i++)
                            {
                                App.Evolution newEvolution = new App.Evolution();
                                newEvolution.Items = new List<EvoItem>();
                                newEvolution.learnedMove = new List<string>();
                                newEvolution.learnedMoveType = new List<string>();
                                newEvolution.StatCompare = new List<EvoStatCompare>();
                                newEvolution.UsedMove = new List<EvoUsedMove>();
                                newEvolution.DefeatRequirement = new List<EvoDefeatRequirement>();
                                newEvolution.PartyMember = new List<string>();
                                newEvolution.PartyMemberType = new List<string>();

                                newEvolution.EvolveInto = species.SelectToken("evolutions[" + i + "].result")?.ToString();

                                if (species.SelectToken("evolutions[" + i + "].requiredContext") != null)
                                {
                                    if (!String.IsNullOrEmpty(species.SelectToken("evolutions[" + i + "].requiredContext").ToString()))
                                    {
                                        App.EvoItem newEvoItem = new App.EvoItem();
                                        newEvoItem.Item = species.SelectToken("evolutions[" + i + "].requiredContext")?.ToString();
                                        newEvoItem.ItemMin = 1;
                                        newEvoItem.ItemMax = 1;
                                        newEvolution.Items.Add(newEvoItem);
                                    }
                                }

                                if (species.SelectToken("evolutions[" + i + "].variant") != null && species.SelectToken("evolutions[" + i + "].variant").ToString() == "trade")
                                {
                                    newEvolution.Trade = true;
                                }

                                if (pkmn.Name.ToLower() == "wurmple")
                                {
                                    newEvolution.isWurmple = true;
                                }

                                if (species.SelectToken("evolutions[" + i + "].requirements") != null)
                                {
                                    for (int j = 0; j < (species.SelectToken("evolutions[" + i + "].requirements") as JArray).Count; j++)
                                    {
                                        switch (species.SelectToken("evolutions[" + i + "].requirements[" + j + "].variant").ToString())
                                        {
                                            case "level":
                                                newEvolution.Level = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].minLevel").ToString());
                                                break;
                                            case "friendship":
                                                newEvolution.Friendship = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                break;
                                            case "properties":
                                                if (species.SelectToken("evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[0] == "gender")
                                                    newEvolution.Gender = species.SelectToken("evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[1];
                                                else if (species.SelectToken("evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[0] == "gimmighoul gimmighoul_coins")
                                                {
                                                    App.EvoItem newEvoItem = new App.EvoItem();
                                                    newEvoItem.Item = "gimmighoul coins";
                                                    newEvoItem.ItemMin = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[1]);
                                                    newEvoItem.ItemMax = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[1]);
                                                    newEvolution.Items.Add(newEvoItem);
                                                }
                                                else if (species.SelectToken("evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[0] == "gimmighoul gimmighoul_netherite")
                                                {
                                                    App.EvoItem newEvoItem = new App.EvoItem();
                                                    newEvoItem.Item = "minecraft:netherite_scrap";
                                                    newEvoItem.ItemMin = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[1]);
                                                    newEvoItem.ItemMax = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[1]);
                                                    newEvolution.Items.Add(newEvoItem);
                                                }
                                                break;
                                            case "property_range":
                                                if (species.SelectToken("evolutions[" + i + "].requirements[" + j + "].feature").ToString() == "gimmighoul_netherite")
                                                {
                                                    App.EvoItem newEvoItem = new App.EvoItem();
                                                    newEvoItem.Item = "minecraft:netherite_scrap";
                                                    newEvoItem.ItemMin = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].range").ToString().Split("-")[0]);
                                                    newEvoItem.ItemMax = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].range").ToString().Split("-")[1]);
                                                    newEvolution.Items.Add(newEvoItem);

                                                }
                                                break;
                                            case "time_range":
                                                newEvolution.Time = species.SelectToken("evolutions[" + i + "].requirements[" + j + "].range").ToString();
                                                break;
                                            case "has_move_type":
                                                newEvolution.learnedMoveType.Add(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].type").ToString());
                                                break;
                                            case "has_move":
                                                newEvolution.learnedMove.Add(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].move").ToString());
                                                break;
                                            case "biome":
                                                string biome = "";
                                                if (species.SelectToken("evolutions[" + i + "].requirements[" + j + "].biomeCondition") != null)
                                                {
                                                    biome = species.SelectToken("evolutions[" + i + "].requirements[" + j + "].biomeCondition").ToString();
                                                }
                                                else if (species.SelectToken("evolutions[" + i + "].requirements[" + j + "].biomeConditions") != null)
                                                {
                                                    biome = species.SelectToken("evolutions[" + i + "].requirements[" + j + "].biomeConditions").ToString();
                                                }
                                                else if(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].biomeAnticondition") != null)
                                                {
                                                    biome = "not" + species.SelectToken("evolutions[" + i + "].requirements[" + j + "].biomeAnticondition").ToString();
                                                }
                                                if (!String.IsNullOrEmpty(biome))
                                                {
                                                    if (String.IsNullOrEmpty(newEvolution.Biome))
                                                        newEvolution.Biome = biome;
                                                    else
                                                        newEvolution.Biome = newEvolution.Biome + ", " + biome;
                                                }

                                                break;
                                            case "held_item":
                                                newEvolution.HeldItem = species.SelectToken("evolutions[" + i + "].requirements[" + j + "].itemCondition").ToString();
                                                break;
                                            case "blocks_traveled":
                                                newEvolution.BlocksTravelled = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                break;
                                            case "stat_compare":
                                                App.EvoStatCompare newStatCompare = new App.EvoStatCompare();
                                                newStatCompare.StatOne = species.SelectToken("evolutions[" + i + "].requirements[" + j + "].highStat").ToString();
                                                newStatCompare.StatTwo = species.SelectToken("evolutions[" + i + "].requirements[" + j + "].lowStat").ToString();
                                                newStatCompare.StatOperator = ">";
                                                newEvolution.StatCompare.Add(newStatCompare);
                                                break;
                                            case "stat_equal":
                                                App.EvoStatCompare newStatEqual = new App.EvoStatCompare();
                                                newStatEqual.StatOne = species.SelectToken("evolutions[" + i + "].requirements[" + j + "].statOne").ToString();
                                                newStatEqual.StatTwo = species.SelectToken("evolutions[" + i + "].requirements[" + j + "].statTwo").ToString();
                                                newStatEqual.StatOperator = "=";
                                                newEvolution.StatCompare.Add(newStatEqual);
                                                break;
                                            case "use_move":
                                                App.EvoUsedMove newUsedMove = new App.EvoUsedMove();
                                                newUsedMove.UsedMove = species.SelectToken("evolutions[" + i + "].requirements[" + j + "].move").ToString();
                                                newUsedMove.UsedMoveTimes = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                newEvolution.UsedMove.Add(newUsedMove);
                                                break;
                                            case "defeat":
                                                App.EvoDefeatRequirement newDefeat = new App.EvoDefeatRequirement();
                                                newDefeat.Pokemon = species.SelectToken("evolutions[" + i + "].requirements[" + j + "].target").ToString().Split(" ")[0];
                                                newDefeat.Amount = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                newEvolution.DefeatRequirement.Add(newDefeat);
                                                break;
                                            case "damage_taken":
                                                newEvolution.Damaged = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                break;
                                            case "recoil":
                                                newEvolution.Recoil = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                break;
                                            case "battle_critical_hits":
                                                newEvolution.Crits = Convert.ToInt32(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                break;
                                            case "weather":
                                                newEvolution.Weather = "rain";
                                                break;
                                            case "party_member":
                                                if (species.SelectToken("evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[0] == "type")
                                                {
                                                    newEvolution.PartyMemberType.Add(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[1]);
                                                }
                                                else
                                                {
                                                    newEvolution.PartyMember.Add(species.SelectToken("evolutions[" + i + "].requirements[" + j + "].target").ToString());
                                                }
                                                break;
                                        }

                                    }
                                }




                                pkmn.Evolutions.Add(newEvolution);
                            }
                        }
                        if (pkmn.Name.ToLower() == "nincada")
                        {

                            App.Evolution newEvolution = new App.Evolution();
                            newEvolution.Items = new List<EvoItem>();
                            newEvolution.learnedMove = new List<string>();
                            newEvolution.learnedMoveType = new List<string>();
                            newEvolution.StatCompare = new List<EvoStatCompare>();
                            newEvolution.UsedMove = new List<EvoUsedMove>();
                            newEvolution.DefeatRequirement = new List<EvoDefeatRequirement>();
                            newEvolution.PartyMember = new List<string>();
                            newEvolution.PartyMemberType = new List<string>();
                            newEvolution.EvolveInto = "shedinja";
                            newEvolution.isShedinja = true;
                            pkmn.Evolutions.Add(newEvolution);
                        }


                        pkmn.Forms = new List<App.Pokemon>();
                        if (species["forms"] != null)
                        {
                            for (int s = 0; s < (species.SelectToken("forms") as JArray).Count; s++)
                            {
                                if (species.SelectToken("forms[" + s + "].aspects[0]") != null)
                                {
                                    try
                                    {
                                        App.Pokemon pkmnForm = new App.Pokemon();
                                        pkmnForm.Name = pkmn.Name;
                                        pkmnForm.Form = species.SelectToken("forms[" + s + "].aspects[0]").ToString();
                                        pkmnForm.Type1 = species.SelectToken("forms[" + s + "].primaryType")?.ToString();
                                        pkmnForm.Type2 = species.SelectToken("forms[" + s + "].secondaryType")?.ToString();
                                        pkmnForm.MaleRatio = Convert.ToDouble(species.SelectToken("forms[" + s + "].maleRatio")?.ToString());
                                        pkmnForm.HP = Convert.ToInt32(species.SelectToken("forms[" + s + "].baseStats.hp")?.ToString());
                                        pkmnForm.Attack = Convert.ToInt32(species.SelectToken("forms[" + s + "].baseStats.attack")?.ToString());
                                        pkmnForm.Defense = Convert.ToInt32(species.SelectToken("forms[" + s + "].baseStats.defence")?.ToString());
                                        pkmnForm.SpecialAttack = Convert.ToInt32(species.SelectToken("forms[" + s + "].baseStats.special_attack")?.ToString());
                                        pkmnForm.SpecialDefense = Convert.ToInt32(species.SelectToken("forms[" + s + "].baseStats.special_defence")?.ToString());
                                        pkmnForm.Speed = Convert.ToInt32(species.SelectToken("forms[" + s + "].baseStats.speed")?.ToString());
                                        pkmnForm.FormSearch = -1;

                                        pkmnForm.PreEvo = new App.PreEvolution();
                                        if (species.SelectToken("forms[" + s + "].preEvolution") != null)
                                        {
                                            List<string> preEvoFormList = species.SelectToken("forms[" + s + "].preEvolution")?.ToString().Split(" ").ToList();
                                            pkmnForm.PreEvo.PreEvolutionName = preEvoFormList[0];
                                            if (preEvoFormList.Count > 1)
                                            {
                                                pkmnForm.PreEvo.PreEvolutionForm = preEvoFormList[1];
                                            }

                                        }

                                        pkmnForm.Abilities = new List<App.Ability>();
                                        if (species.SelectToken("forms[" + s + "].abilities") != null)
                                        {
                                            foreach (var ability in species.SelectToken("forms[" + s + "].abilities") as JArray)
                                            {
                                                App.Ability newAbility = new App.Ability();
                                                List<string> abilityParts = ability.ToString().Split(':').ToList();
                                                if (abilityParts[0] == "h")
                                                {
                                                    newAbility.AbilityName = abilityParts[1];
                                                    newAbility.Hidden = true;
                                                }
                                                else
                                                {
                                                    newAbility.AbilityName = abilityParts[0];
                                                    newAbility.Hidden = false;
                                                }
                                                Ability foundAbility = abilityLookUp.FirstOrDefault(a => a.AbilityName == newAbility.AbilityName);
                                                if (foundAbility != null)
                                                    newAbility.Description = foundAbility.Description;
                                                pkmnForm.Abilities.Add(newAbility);
                                            }

                                        }
                                        pkmnForm.EggGroups = new List<string>();
                                        if (species.SelectToken("forms[" + s + "].eggGroups") != null)
                                        {
                                            foreach (var eggGroup in species.SelectToken("forms[" + s + "].eggGroups") as JArray)
                                            {
                                                pkmnForm.EggGroups.Add(eggGroup.ToString());
                                            }

                                        }
                                        pkmnForm.EVS = new App.EVYield();
                                        pkmnForm.EVS.HP = Convert.ToInt32(species.SelectToken("forms[" + s + "].evYield.hp")?.ToString());
                                        pkmnForm.EVS.Attack = Convert.ToInt32(species.SelectToken("forms[" + s + "].evYield.attack")?.ToString());
                                        pkmnForm.EVS.Defense = Convert.ToInt32(species.SelectToken("forms[" + s + "].evYield.defence")?.ToString());
                                        pkmnForm.EVS.SpecialAttack = Convert.ToInt32(species.SelectToken("forms[" + s + "].evYield.special_attack")?.ToString());
                                        pkmnForm.EVS.SpecialDefense = Convert.ToInt32(species.SelectToken("forms[" + s + "].evYield.special_defence")?.ToString());
                                        pkmnForm.EVS.Speed = Convert.ToInt32(species.SelectToken("forms[" + s + "].evYield.speed")?.ToString());
                                        pkmnForm.Drops = new List<App.Drop>();
                                        if (species.SelectToken("forms[" + s + "].drops") != null)
                                        {
                                            if (species.SelectToken("forms[" + s + "].drops.drops.entries") != null)
                                            {
                                                for (int i = 0; i < (species.SelectToken("forms[" + s + "].drops.entries") as JArray).Count; i++)
                                                {
                                                    App.Drop newDrop = new App.Drop();
                                                    newDrop.Item = species.SelectToken("forms[" + s + "].drops.entries[" + i + "].item")?.ToString().Split(":")[1];
                                                    newDrop.Location = species.SelectToken("forms[" + s + "].drops.entries[" + i + "].item")?.ToString().Split(":")[0];
                                                    newDrop.QuantityRange = species.SelectToken("forms[" + s + "].drops.entries[" + i + "].quantityRange")?.ToString();
                                                    newDrop.Percent = Convert.ToDouble(species.SelectToken("forms[" + s + "].drops.entries[" + i + "].percentage")?.ToString());
                                                    pkmnForm.Drops.Add(newDrop);
                                                }
                                            }
                                        }
                                        pkmnForm.Moves = new List<App.Move>();

                                        if (species.SelectToken("forms[" + s + "].moves") != null)
                                        {
                                            foreach (var move in species.SelectToken("forms[" + s + "].moves") as JArray)
                                            {
                                                App.Move newMove = new App.Move();
                                                List<string> moveParts = move.ToString().Split(':').ToList();
                                                newMove.MoveName = moveParts[1];
                                                newMove.MoveType = moveParts[0];
                                                Move foundMove = moveLookUp.FirstOrDefault(m => m.MoveName == newMove.MoveName);
                                                if (foundMove != null)
                                                {
                                                    newMove.Accuracy = foundMove.Accuracy.Trim();
                                                    newMove.BasePower = foundMove.BasePower.Trim();
                                                    newMove.Category = foundMove.Category.Trim();
                                                    newMove.PP = foundMove.PP.Trim();
                                                    newMove.Type = foundMove.Type.Trim();
                                                    newMove.Description = foundMove.Description;

                                                }
                                                pkmnForm.Moves.Add(newMove);
                                            }
                                        }
                                        pkmnForm.Evolutions = new List<App.Evolution>();
                                        if (species.SelectToken("forms[" + s + "].evolutions") != null)
                                        {
                                            for (int i = 0; i < (species.SelectToken("forms[" + s + "].evolutions") as JArray).Count; i++)
                                            {
                                                App.Evolution newEvolution = new App.Evolution();
                                                newEvolution.Items = new List<EvoItem>();
                                                newEvolution.learnedMove = new List<string>();
                                                newEvolution.learnedMoveType = new List<string>();
                                                newEvolution.StatCompare = new List<EvoStatCompare>();
                                                newEvolution.UsedMove = new List<EvoUsedMove>();
                                                newEvolution.DefeatRequirement = new List<EvoDefeatRequirement>();
                                                newEvolution.PartyMember = new List<string>();
                                                newEvolution.PartyMemberType = new List<string>();

                                                newEvolution.EvolveInto = species.SelectToken("forms[" + s + "].evolutions[" + i + "].result")?.ToString();

                                                if (species.SelectToken("forms[" + s + "].evolutions[" + i + "].requiredContext") != null)
                                                {

                                                    if (!String.IsNullOrEmpty(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requiredContext")?.ToString()))
                                                    {
                                                        App.EvoItem newEvoItem = new App.EvoItem();
                                                        newEvoItem.Item = species.SelectToken("forms[" + s + "].evolutions[" + i + "].requiredContext")?.ToString();
                                                        newEvoItem.ItemMin = 1;
                                                        newEvoItem.ItemMax = 1;
                                                        newEvolution.Items.Add(newEvoItem);
                                                    }
                                                }

                                                if (species.SelectToken("forms[" + s + "].evolutions[" + i + "].variant") != null && species.SelectToken("forms[" + s + "].evolutions[" + i + "].variant").ToString() == "trade")
                                                {
                                                    newEvolution.Trade = true;
                                                }

                                                if (species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements") != null)
                                                {
                                                    for (int j = 0; j < (species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements") as JArray).Count; j++)
                                                    {
                                                        switch (species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].variant").ToString())
                                                        {
                                                            case "level":
                                                                newEvolution.Level = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].minLevel").ToString());
                                                                break;
                                                            case "friendship":
                                                                newEvolution.Friendship = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                                break;
                                                            case "properties":
                                                                if (species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[0] == "gender")
                                                                    newEvolution.Gender = species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[1];
                                                                else if (species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[0] == "gimmighoul gimmighoul_coins")
                                                                {
                                                                    App.EvoItem newEvoItem = new App.EvoItem();
                                                                    newEvoItem.Item = "gimmighoul coins";
                                                                    newEvoItem.ItemMin = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[1]);
                                                                    newEvoItem.ItemMax = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[1]);
                                                                    newEvolution.Items.Add(newEvoItem);
                                                                }
                                                                else if (species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[0] == "gimmighoul gimmighoul_netherite")
                                                                {
                                                                    App.EvoItem newEvoItem = new App.EvoItem();
                                                                    newEvoItem.Item = "minecraft:netherite_scrap";
                                                                    newEvoItem.ItemMin = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[1]);
                                                                    newEvoItem.ItemMax = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[1]);
                                                                    newEvolution.Items.Add(newEvoItem);
                                                                }
                                                                break;
                                                            case "property_range":
                                                                if (species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].feature").ToString() == "gimmighoul_netherite")
                                                                {
                                                                    App.EvoItem newEvoItem = new App.EvoItem();
                                                                    newEvoItem.Item = "minecraft:netherite_scrap";
                                                                    newEvoItem.ItemMin = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].range").ToString().Split("-")[0]);
                                                                    newEvoItem.ItemMax = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].range").ToString().Split("-")[1]);
                                                                    newEvolution.Items.Add(newEvoItem);

                                                                }
                                                                break;
                                                            case "time_range":
                                                                newEvolution.Time = species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].range").ToString();
                                                                break;
                                                            case "has_move_type":
                                                                if(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].type") != null)
                                                                    newEvolution.learnedMoveType.Add(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].type").ToString());
                                                                else
                                                                    newEvolution.learnedMoveType.Add(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].range").ToString());
                                                                break;
                                                            case "has_move":
                                                                newEvolution.learnedMove.Add(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].move").ToString());
                                                                break;
                                                            case "biome":
                                                                string biome = "";
                                                                if (species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].biomeCondition") != null)
                                                                {
                                                                    biome = species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].biomeCondition").ToString();
                                                                }
                                                                else if (species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].biomeConditions") != null)
                                                                {
                                                                    biome = species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].biomeConditions").ToString();
                                                                }
                                                                else if(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].biomeAnticondition") != null)
                                                                {
                                                                    biome = "not" + species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].biomeAnticondition").ToString();
                                                                }
                                                                if (!String.IsNullOrEmpty(biome))
                                                                {
                                                                    if (String.IsNullOrEmpty(newEvolution.Biome))
                                                                        newEvolution.Biome = biome;
                                                                    else
                                                                        newEvolution.Biome = newEvolution.Biome + ", " + biome;
                                                                }
                                                                break;
                                                            case "held_item":
                                                                newEvolution.HeldItem = species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].itemCondition").ToString();
                                                                break;
                                                            case "blocks_traveled":
                                                                newEvolution.BlocksTravelled = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                                break;
                                                            case "stat_compare":
                                                                App.EvoStatCompare newStatCompare = new App.EvoStatCompare();
                                                                newStatCompare.StatOne = species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].highStat").ToString();
                                                                newStatCompare.StatTwo = species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].lowStat").ToString();
                                                                newStatCompare.StatOperator = ">";
                                                                newEvolution.StatCompare.Add(newStatCompare);
                                                                break;
                                                            case "stat_equal":
                                                                App.EvoStatCompare newStatEqual = new App.EvoStatCompare();
                                                                newStatEqual.StatOne = species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].statOne").ToString();
                                                                newStatEqual.StatTwo = species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].statTwo").ToString();
                                                                newStatEqual.StatOperator = "=";
                                                                newEvolution.StatCompare.Add(newStatEqual);
                                                                break;
                                                            case "use_move":
                                                                App.EvoUsedMove newUsedMove = new App.EvoUsedMove();
                                                                newUsedMove.UsedMove = species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].move").ToString();
                                                                newUsedMove.UsedMoveTimes = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                                newEvolution.UsedMove.Add(newUsedMove);
                                                                break;
                                                            case "defeat":
                                                                App.EvoDefeatRequirement newDefeat = new App.EvoDefeatRequirement();
                                                                newDefeat.Pokemon = species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].target").ToString().Split(" ")[0];
                                                                newDefeat.Amount = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                                newEvolution.DefeatRequirement.Add(newDefeat);
                                                                break;
                                                            case "damage_taken":
                                                                newEvolution.Damaged = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                                break;
                                                            case "recoil":
                                                                newEvolution.Recoil = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                                break;
                                                            case "battle_critical_hits":
                                                                newEvolution.Crits = Convert.ToInt32(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].amount").ToString());
                                                                break;
                                                            case "weather":
                                                                newEvolution.Weather = "rain";
                                                                break;
                                                            case "party_member":
                                                                if (species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[0] == "type")
                                                                {
                                                                    newEvolution.PartyMemberType.Add(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].target").ToString().Split("=")[1]);
                                                                }
                                                                else
                                                                {
                                                                    newEvolution.PartyMember.Add(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].target").ToString());
                                                                }
                                                                break;
                                                        }

                                                    }
                                                }




                                                pkmnForm.Evolutions.Add(newEvolution);
                                            }
                                        }
                                        pkmnForm.Forms = new List<App.Pokemon>();
                                        pkmnForm.Spawns = new List<App.Spawn>();


                                        //pokemonList.Add(pkmn);
                                        pkmn.Forms.Add(pkmnForm);
                                    }
                                    catch (Exception ex)
                                    {
                                        if (ex is not JsonReaderException)
                                            MessageBox.Show("error at " + name);
                                    }

                                }
                            }
                        }

                        pkmn.Spawns = new List<App.Spawn>();
                        if (spawnpool != null)
                        {
                            for (int i = 0; i < (spawnpool.SelectToken("spawns") as JArray).Count; i++)
                            {
                                App.Spawn newSpawn = new App.Spawn();
                                newSpawn.Presets = new List<string>();
                                newSpawn.Condition = new App.Condition();
                                newSpawn.Condition.IsRaining = null;
                                newSpawn.Condition.IsSlimeChunk = null;
                                newSpawn.Condition.CanSeeSky = null;
                                newSpawn.Condition.MinSkyLight = null;
                                newSpawn.Condition.MaxSkyLight = null;
                                newSpawn.Condition.MinY = null;
                                newSpawn.Condition.MaxY = null;
                                newSpawn.Condition.MinLureLevel = null;
                                newSpawn.Condition.MaxLureLevel = null;

                                newSpawn.AntiCondition = new App.Condition();
                                newSpawn.AntiCondition.IsRaining = null;
                                newSpawn.AntiCondition.IsSlimeChunk = null;
                                newSpawn.AntiCondition.CanSeeSky = null;
                                newSpawn.AntiCondition.MinSkyLight = null;
                                newSpawn.AntiCondition.MaxSkyLight = null;
                                newSpawn.AntiCondition.MinY = null;
                                newSpawn.AntiCondition.MaxY = null;
                                newSpawn.AntiCondition.MinLureLevel = null;
                                newSpawn.AntiCondition.MaxLureLevel = null;

                                newSpawn.ID = spawnpool.SelectToken("spawns[" + i + "].id")?.ToString();
                                newSpawn.Pokemon = spawnpool.SelectToken("spawns[" + i + "].pokemon")?.ToString();
                                newSpawn.SpawnablePositionType = spawnpool.SelectToken("spawns[" + i + "].spawnablePositionType")?.ToString();
                                newSpawn.Bucket = spawnpool.SelectToken("spawns[" + i + "].bucket")?.ToString();

                                JArray spawnPresents = new JArray();
                                if (spawnpool.SelectToken("spawns[" + i + "].presets") != null)
                                {
                                    spawnPresents = spawnpool.SelectToken("spawns[" + i + "].presets") as JArray;
                                    foreach (var preset in spawnPresents)
                                        newSpawn.Presets.Add(preset.ToString());
                                }

                                if (spawnpool.SelectToken("spawns[" + i + "].condition") != null)
                                {
                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.canSeeSky") != null)
                                        newSpawn.Condition.CanSeeSky = Convert.ToBoolean(spawnpool.SelectToken("spawns[" + i + "].condition.canSeeSky").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.minSkyLight") != null)
                                        newSpawn.Condition.MinSkyLight = Convert.ToInt32(spawnpool.SelectToken("spawns[" + i + "].condition.minSkyLight").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.maxSkyLight") != null)
                                        newSpawn.Condition.MaxSkyLight = Convert.ToInt32(spawnpool.SelectToken("spawns[" + i + "].condition.maxSkyLight").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.minY") != null)
                                        newSpawn.Condition.MinY = Convert.ToInt32(spawnpool.SelectToken("spawns[" + i + "].condition.minY").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.maxY") != null)
                                        newSpawn.Condition.MaxY = Convert.ToInt32(spawnpool.SelectToken("spawns[" + i + "].condition.maxY").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.timeRange") != null)
                                        newSpawn.Condition.TimeRange = spawnpool.SelectToken("spawns[" + i + "].condition.timeRange").ToString();

                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.moonPhase") != null)
                                        newSpawn.Condition.MoonPhase = spawnpool.SelectToken("spawns[" + i + "].condition.moonPhase").ToString();

                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.isRaining") != null)
                                        newSpawn.Condition.IsRaining = Convert.ToBoolean(spawnpool.SelectToken("spawns[" + i + "].condition.isRaining").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.isSlimeChunk") != null)
                                        newSpawn.Condition.IsSlimeChunk = Convert.ToBoolean(spawnpool.SelectToken("spawns[" + i + "].condition.isSlimeChunk").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.minLureLevel") != null)
                                        newSpawn.Condition.MinLureLevel = Convert.ToInt32(spawnpool.SelectToken("spawns[" + i + "].condition.minLureLevel").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.maxLureLevel") != null)
                                        newSpawn.Condition.MaxLureLevel = Convert.ToInt32(spawnpool.SelectToken("spawns[" + i + "].condition.maxLureLevel").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.rodType") != null)
                                        newSpawn.Condition.RodType = spawnpool.SelectToken("spawns[" + i + "].condition.rodType").ToString();

                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.bait") != null)
                                        newSpawn.Condition.Bait = spawnpool.SelectToken("spawns[" + i + "].condition.bait").ToString();

                                    newSpawn.Condition.NeededNearbyBlocks = new List<string>();
                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.neededNearbyBlocks") != null)
                                        foreach (var block in spawnpool.SelectToken("spawns[" + i + "].condition.neededNearbyBlocks") as JArray)
                                            newSpawn.Condition.NeededNearbyBlocks.Add(block.ToString());

                                    newSpawn.Condition.NeededBaseBlocks = new List<string>();
                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.neededBaseBlocks") != null)
                                        foreach (var block in spawnpool.SelectToken("spawns[" + i + "].condition.neededBaseBlocks") as JArray)
                                            newSpawn.Condition.NeededBaseBlocks.Add(block.ToString());

                                    newSpawn.Condition.Structures = new List<string>();
                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.structures") != null)
                                        foreach (var block in spawnpool.SelectToken("spawns[" + i + "].condition.structures") as JArray)
                                            newSpawn.Condition.Structures.Add(block.ToString());

                                    newSpawn.Condition.Biomes = new List<string>();
                                    if (spawnpool.SelectToken("spawns[" + i + "].condition.biomes") != null)
                                    {
                                        List<string> biomes = new List<string>();
                                        List<string> tags = new List<string>();

                                        foreach (var block in spawnpool.SelectToken("spawns[" + i + "].condition.biomes") as JArray)
                                        {
                                            if (block.ToString().StartsWith('#'))
                                                tags.Add(block.ToString());
                                            else
                                                biomes.Add(block.ToString());
                                        }
                                        while (tags.Count != 0)
                                        {
                                            string mod = tags[0].Substring(1).Split(":")[0];
                                            string tag = tags[0].Substring(1).Split(":")[1].Replace('/', '.');
                                            string tagPath = Path.Combine(dexPath, mod, "biome_tag", tag + ".json");

                                            if (File.Exists(tagPath))
                                            {
                                                JObject tagLookUp = JObject.Parse(File.ReadAllText(tagPath));
                                                JArray tagValues = tagLookUp["values"] as JArray;

                                                foreach (JToken value in tagValues)
                                                {
                                                    if (value.Type == JTokenType.String)
                                                    {
                                                        string tagString = value.ToString();
                                                        if (tagString.StartsWith('#'))
                                                            tags.Add(tagString);
                                                        else
                                                            biomes.Add(tagString);
                                                    }
                                                    else if (value.Type == JTokenType.Object)
                                                    {
                                                        JObject tagObject = (JObject)value;
                                                        string tagID = tagObject["id"]?.ToString();
                                                        if (tagID.StartsWith('#'))
                                                            tags.Add(tagID);
                                                        else
                                                            biomes.Add(tagID);

                                                    }
                                                }
                                                tags.Remove(tags[0]);
                                            }
                                            else
                                            {
                                                tags.Remove(tags[0]);
                                            }
                                        }
                                        foreach(string biome in biomes)
                                            newSpawn.Condition.Biomes.Add(biome);
                                        newSpawn.Condition.Biomes = newSpawn.Condition.Biomes.Distinct().ToList();
                                        newSpawn.Condition.Biomes.Sort();
                                    }


                                }

                                if (spawnpool.SelectToken("spawns[" + i + "].anticondition") != null)
                                {
                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.canSeeSky") != null)
                                        newSpawn.AntiCondition.CanSeeSky = Convert.ToBoolean(spawnpool.SelectToken("spawns[" + i + "].anticondition.canSeeSky").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.minSkyLight") != null)
                                        newSpawn.AntiCondition.MinSkyLight = Convert.ToInt32(spawnpool.SelectToken("spawns[" + i + "].anticondition.minSkyLight").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.maxSkyLight") != null)
                                        newSpawn.AntiCondition.MaxSkyLight = Convert.ToInt32(spawnpool.SelectToken("spawns[" + i + "].anticondition.maxSkyLight").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.minY") != null)
                                        newSpawn.AntiCondition.MinY = Convert.ToInt32(spawnpool.SelectToken("spawns[" + i + "].anticondition.minY").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.maxY") != null)
                                        newSpawn.AntiCondition.MaxY = Convert.ToInt32(spawnpool.SelectToken("spawns[" + i + "].anticondition.maxY").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.timeRange") != null)
                                        newSpawn.AntiCondition.TimeRange = spawnpool.SelectToken("spawns[" + i + "].anticondition.timeRange").ToString();

                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.moonPhase") != null)
                                        newSpawn.AntiCondition.MoonPhase = spawnpool.SelectToken("spawns[" + i + "].anticondition.moonPhase").ToString();

                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.isRaining") != null)
                                        newSpawn.AntiCondition.IsRaining = Convert.ToBoolean(spawnpool.SelectToken("spawns[" + i + "].anticondition.isRaining").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.isSlimeChunk") != null)
                                        newSpawn.AntiCondition.IsSlimeChunk = Convert.ToBoolean(spawnpool.SelectToken("spawns[" + i + "].anticondition.isSlimeChunk").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.minLureLevel") != null)
                                        newSpawn.AntiCondition.MinLureLevel = Convert.ToInt32(spawnpool.SelectToken("spawns[" + i + "].anticondition.minLureLevel").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.maxLureLevel") != null)
                                        newSpawn.AntiCondition.MaxLureLevel = Convert.ToInt32(spawnpool.SelectToken("spawns[" + i + "].anticondition.maxLureLevel").ToString());

                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.rodType") != null)
                                        newSpawn.AntiCondition.RodType = spawnpool.SelectToken("spawns[" + i + "].anticondition.rodType").ToString();

                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.bait") != null)
                                        newSpawn.AntiCondition.Bait = spawnpool.SelectToken("spawns[" + i + "].anticondition.bait").ToString();

                                    newSpawn.AntiCondition.NeededNearbyBlocks = new List<string>();
                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.neededNearbyBlocks") != null)
                                        foreach (var block in spawnpool.SelectToken("spawns[" + i + "].anticondition.neededNearbyBlocks") as JArray)
                                            newSpawn.AntiCondition.NeededNearbyBlocks.Add(block.ToString());

                                    newSpawn.AntiCondition.NeededBaseBlocks = new List<string>();
                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.neededBaseBlocks") != null)
                                        foreach (var block in spawnpool.SelectToken("spawns[" + i + "].anticondition.neededBaseBlocks") as JArray)
                                            newSpawn.AntiCondition.NeededBaseBlocks.Add(block.ToString());

                                    newSpawn.AntiCondition.Structures = new List<string>();
                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.structures") != null)
                                        foreach (var block in spawnpool.SelectToken("spawns[" + i + "].anticondition.structures") as JArray)
                                            newSpawn.AntiCondition.Structures.Add(block.ToString());

                                    newSpawn.AntiCondition.Biomes = new List<string>();
                                    if (spawnpool.SelectToken("spawns[" + i + "].anticondition.biomes") != null)
                                    {
                                        List<string> biomes = new List<string>();
                                        List<string> tags = new List<string>();

                                        foreach (var block in spawnpool.SelectToken("spawns[" + i + "].anticondition.biomes") as JArray)
                                        {
                                            if (block.ToString().StartsWith('#'))
                                                tags.Add(block.ToString());
                                            else
                                                biomes.Add(block.ToString());
                                        }
                                        while (tags.Count != 0)
                                        {
                                            string mod = tags[0].Substring(1).Split(":")[0];
                                            string tag = tags[0].Substring(1).Split(":")[1].Replace('/', '.');
                                            string tagPath = Path.Combine(dexPath, mod, "biome_tag", tag + ".json");

                                            if (File.Exists(tagPath))
                                            {
                                                JObject tagLookUp = JObject.Parse(File.ReadAllText(tagPath));
                                                JArray tagValues = tagLookUp["values"] as JArray;

                                                foreach (JToken value in tagValues)
                                                {
                                                    if (value.Type == JTokenType.String)
                                                    {
                                                        string tagString = value.ToString();
                                                        if (tagString.StartsWith('#'))
                                                            tags.Add(tagString);
                                                        else
                                                            biomes.Add(tagString);
                                                    }
                                                    else if (value.Type == JTokenType.Object)
                                                    {
                                                        JObject tagObject = (JObject)value;
                                                        string tagID = tagObject["id"]?.ToString();
                                                        if (tagID.StartsWith('#'))
                                                            tags.Add(tagID);
                                                        else
                                                            biomes.Add(tagID);

                                                    }
                                                }
                                                tags.Remove(tags[0]);
                                            }
                                            else
                                            {
                                                tags.Remove(tags[0]);
                                            }
                                        }
                                        foreach (string biome in biomes)
                                            newSpawn.AntiCondition.Biomes.Add(biome);
                                        newSpawn.AntiCondition.Biomes = newSpawn.AntiCondition.Biomes.Distinct().ToList();
                                        newSpawn.AntiCondition.Biomes.Sort();
                                    }


                                }

                                if (newSpawn.Pokemon.Split(" ").Count() > 1)
                                {
                                    string potentialForm = newSpawn.Pokemon.Split(" ")[1];
                                    bool foundForm = false;
                                    foreach(App.Pokemon form in pkmn.Forms)
                                        if (form.Form.ToLower() == potentialForm.ToLower())
                                        {
                                            form.Spawns.Add(newSpawn);
                                            foundForm = true;
                                            break;
                                        }
                                    if(!foundForm)
                                        pkmn.Spawns.Add(newSpawn);
                                }
                                else
                                    pkmn.Spawns.Add(newSpawn);


                            }

                            foreach (App.Pokemon form in pkmn.Forms)
                                if (form.Spawns.Count == 0)
                                    form.Spawns = pkmn.Spawns;

                        }
                        pokemonList.Add(pkmn);

                    }
                    catch(Exception ex)
                    {
                        if (ex is not JsonReaderException)
                            MessageBox.Show("error at " + name);
                    }
                }
                dtgPokemon.ItemsSource = pokemonList;
                txtSearchPokemon.Text = "";
                AddMissingPreEvos();

            }
            catch
            {
                MessageBox.Show("data grid error");

            }

        }
        private void txtSearchPokemon_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                dtgPokemon.ItemsSource = null;
                List<App.Pokemon> filteredList = new List<App.Pokemon>();
                switch (cboSearchCategory.SelectedIndex)
                {
                    case 0:
                        foreach (App.Pokemon pkmn in pokemonList)
                        {
                            if (pkmn.Name.ToLower().Contains(txtSearchPokemon.Text.ToLower()))
                            {
                                filteredList.Add(pkmn);
                                App.Pokemon lastPkmn = filteredList.Last() as App.Pokemon;
                                lastPkmn.FormSearch = -1;
                            }
                        }
                        break;
                    case 1:
                        foreach (App.Pokemon pkmn in pokemonList)
                        {
                            for (int i = 0; i < pkmn.Forms.Count; i++)
                            {
                                if ((pkmn.Forms[i].Form != null && pkmn.Forms[i].Form.ToLower().Contains(txtSearchPokemon.Text.ToLower())))
                                {
                                    filteredList.Add(pkmn);
                                    App.Pokemon lastPkmn = filteredList.Last() as App.Pokemon;
                                    lastPkmn.FormSearch = i;
                                    break;
                                }

                            }
                        }
                        break;
                    case 2:
                        foreach (App.Pokemon pkmn in pokemonList)
                        {
                            if ((pkmn.Type1 != null && pkmn.Type1.ToLower().Contains(txtSearchPokemon.Text.ToLower())) || (pkmn.Type2 != null && pkmn.Type2.ToLower().Contains(txtSearchPokemon.Text.ToLower())))
                            {
                                filteredList.Add(pkmn);
                                App.Pokemon lastPkmn = filteredList.Last() as App.Pokemon;
                                lastPkmn.FormSearch = -1;
                            }
                            else
                            {
                                for (int i = 0; i < pkmn.Forms.Count; i++)
                                {
                                    if ((pkmn.Forms[i].Type1 != null && pkmn.Forms[i].Type1.ToLower().Contains(txtSearchPokemon.Text.ToLower())) || (pkmn.Forms[i].Type2 != null && pkmn.Forms[i].Type2.ToLower().Contains(txtSearchPokemon.Text.ToLower())))
                                    {
                                        filteredList.Add(pkmn);
                                        App.Pokemon lastPkmn = filteredList.Last() as App.Pokemon;
                                        lastPkmn.FormSearch = i;
                                        break;
                                    }

                                }
                            }
                        }
                        break;
                    case 3:
                        foreach (App.Pokemon pkmn in pokemonList)
                        {
                            bool foundMatch = false;
                            if (pkmn.Moves != null)
                            {
                                foreach(App.Move move in pkmn.Moves)
                                {
                                    if (move.MoveName.Replace("_", "").ToLower().Contains(txtSearchPokemon.Text.Replace(" ", "").ToLower()))
                                    {
                                        filteredList.Add(pkmn);
                                        App.Pokemon lastPkmn = filteredList.Last() as App.Pokemon;
                                        lastPkmn.FormSearch = -1;
                                        foundMatch = true;
                                        break;
                                    }
                                }
                            }
                            if(!foundMatch)
                                for (int i = 0; i < pkmn.Forms.Count; i++)
                                {
                                    foreach (App.Move move in pkmn.Forms[i].Moves)
                                    {
                                        if (move.MoveName.Replace("_", "").ToLower().Contains(txtSearchPokemon.Text.Replace(" ", "").ToLower()))
                                        {
                                            filteredList.Add(pkmn);
                                            App.Pokemon lastPkmn = filteredList.Last() as App.Pokemon;
                                            lastPkmn.FormSearch = i;
                                            foundMatch = true;
                                            break;
                                        }
                                    }
                                    if (foundMatch)
                                        break;
                                }
                        }
                        break;
                    case 4:
                        foreach (App.Pokemon pkmn in pokemonList)
                        {
                            bool foundMatch = false;
                            if (pkmn.Abilities != null)
                            {
                                foreach (App.Ability ability in pkmn.Abilities)
                                {
                                    if (ability.AbilityName.Replace("_", "").ToLower().Contains(txtSearchPokemon.Text.Replace(" ", "").ToLower()))
                                    {
                                        filteredList.Add(pkmn);
                                        App.Pokemon lastPkmn = filteredList.Last() as App.Pokemon;
                                        lastPkmn.FormSearch = -1;
                                        foundMatch = true;
                                        break;
                                    }
                                }
                            }
                            if (!foundMatch)
                                for (int i = 0; i < pkmn.Forms.Count; i++)
                                {
                                    foreach (App.Ability ability in pkmn.Forms[i].Abilities)
                                    {
                                        if (ability.AbilityName.Replace("_", "").ToLower().Contains(txtSearchPokemon.Text.Replace(" ", "").ToLower()))
                                        {
                                            filteredList.Add(pkmn);
                                            App.Pokemon lastPkmn = filteredList.Last() as App.Pokemon;
                                            lastPkmn.FormSearch = i;
                                            foundMatch = true;
                                            break;
                                        }
                                    }
                                    if (foundMatch)
                                        break;
                                }
                        }
                        break;
                    case 5:
                        foreach (App.Pokemon pkmn in pokemonList)
                        {
                            bool foundMatch = false;
                            if (pkmn.Drops != null)
                            {
                                foreach (App.Drop drop in pkmn.Drops)
                                {
                                    if (drop.Item.Replace("_", "").ToLower().Contains(txtSearchPokemon.Text.Replace(" ", "").ToLower()))
                                    {
                                        filteredList.Add(pkmn);
                                        App.Pokemon lastPkmn = filteredList.Last() as App.Pokemon;
                                        lastPkmn.FormSearch = -1;
                                        foundMatch = true;
                                        break;
                                    }
                                }
                            }
                            if (!foundMatch)
                                for (int i = 0; i < pkmn.Forms.Count; i++)
                                {
                                    foreach (App.Drop drop in pkmn.Forms[i].Drops)
                                    {
                                        if (drop.Item.Replace("_", "").ToLower().Contains(txtSearchPokemon.Text.Replace(" ", "").ToLower()))
                                        {
                                            filteredList.Add(pkmn);
                                            App.Pokemon lastPkmn = filteredList.Last() as App.Pokemon;
                                            lastPkmn.FormSearch = i;
                                            foundMatch = true;
                                            break;
                                        }
                                    }
                                    if (foundMatch)
                                        break;
                                }
                        }
                        break;
                }
                dtgPokemon.ItemsSource = filteredList;

            }
            catch
            {
                MessageBox.Show("data grid error");

            }

        }
        private void dtgPokemon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {
                if (dtgPokemon.SelectedItem != null)
                {

                    cboForm.ItemsSource = null;
                    App.Pokemon SelectedPokemon = dtgPokemon.SelectedItem as App.Pokemon;
                    List<App.Pokemon> allForms = new List<Pokemon>();
                    allForms.Add(SelectedPokemon);
                    foreach (App.Pokemon form in SelectedPokemon.Forms)
                    {
                        allForms.Add(form);
                    }
                    foreach (App.Pokemon form in allForms)
                    {
                        form.Form = ProperString(form.Form);
                    }
                    cboForm.ItemsSource = allForms;
                    if(SelectedPokemon.FormSearch != -1)
                        cboForm.SelectedIndex = SelectedPokemon.FormSearch + 1;
                    else
                        cboForm.SelectedIndex = 0;
                }
            }
            catch
            {
                MessageBox.Show("datagrid pokemon error");

            }

        }
        private void cboForm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboForm.SelectedItem != null)
                {
                    App.Pokemon FormSelected = cboForm.SelectedItem as App.Pokemon;
                    App.Pokemon BackupForm = cboForm.Items[0] as App.Pokemon;
                    lblName.Content = ProperString(FormSelected.Name);
                    lblTypeOne.Content = ProperString(FormSelected.Type1);
                    lblTypeTwo.Content = ProperString(FormSelected.Type2);
                    string spacing = "    ";

                    if (FormSelected.MaleRatio != null)
                    {
                        if (FormSelected.MaleRatio == -1)
                        {
                            lblGenderMale.Content = "Genderless";
                            lblGenderFemale.Content = "Genderless";
                        }
                        else
                        {
                            lblGenderMale.Content = (FormSelected.MaleRatio * 100).ToString() + "%";
                            lblGenderFemale.Content = (100 - (FormSelected.MaleRatio * 100)).ToString() + "%";
                        }
                    }
                    else
                    {
                        if (BackupForm.MaleRatio == -1)
                        {
                            lblGenderMale.Content = "Genderless";
                            lblGenderFemale.Content = "Genderless";
                        }
                        else
                        {
                            lblGenderMale.Content = (BackupForm.MaleRatio * 100).ToString() + "%";
                            lblGenderFemale.Content = (100 - (BackupForm.MaleRatio * 100)).ToString() + "%";
                        }

                    }


                    string eggGroups = "";
                    if (FormSelected.EggGroups != null)
                    {
                        foreach (string group in FormSelected.EggGroups)
                        {
                            eggGroups = eggGroups + ", " + ProperString(group);
                        }
                    }
                    else
                    {

                        foreach (string group in BackupForm.EggGroups)
                        {
                            eggGroups = eggGroups + ", " + ProperString(group);
                        }
                    }
                    if (!String.IsNullOrEmpty(eggGroups))
                        eggGroups = eggGroups.Substring(2);
                    lblEggGroups.Content = eggGroups;


                    if (FormSelected.HP != null && !(FormSelected.HP == 0 && FormSelected.Attack == 0 && FormSelected.Defense == 0 && FormSelected.SpecialAttack == 0 && FormSelected.SpecialDefense == 0 && FormSelected.Speed == 0))
                    {
                        lblHP.Content = FormSelected.HP;
                        lblAttack.Content = FormSelected.Attack;
                        lblDefense.Content = FormSelected.Defense;
                        lblSpecialAttack.Content = FormSelected.SpecialAttack;
                        lblSpecialDefense.Content = FormSelected.SpecialDefense;
                        lblSpeed.Content = FormSelected.Speed;
                        lblTotal.Content = FormSelected.HP + FormSelected.Attack + FormSelected.Defense + FormSelected.SpecialAttack + FormSelected.SpecialDefense + FormSelected.Speed;
                    }
                    else
                    {
                        lblHP.Content = BackupForm.HP;
                        lblAttack.Content = BackupForm.Attack;
                        lblDefense.Content = BackupForm.Defense;
                        lblSpecialAttack.Content = BackupForm.SpecialAttack;
                        lblSpecialDefense.Content = BackupForm.SpecialDefense;
                        lblSpeed.Content = BackupForm.Speed;
                        lblTotal.Content = BackupForm.HP + BackupForm.Attack + BackupForm.Defense + BackupForm.SpecialAttack + BackupForm.SpecialDefense + BackupForm.Speed;
                    }


                    if (FormSelected.EVS.HP != null && !(FormSelected.EVS.HP == 0 && FormSelected.EVS.Attack == 0 && FormSelected.EVS.Defense == 0 && FormSelected.EVS.SpecialAttack == 0 && FormSelected.EVS.SpecialDefense == 0 && FormSelected.EVS.Speed == 0))
                    {
                        lblHPEV.Content = FormSelected.EVS.HP;
                        lblAttackEV.Content = FormSelected.EVS.Attack;
                        lblDefenseEV.Content = FormSelected.EVS.Defense;
                        lblSpecialAttackEV.Content = FormSelected.EVS.SpecialAttack;
                        lblSpecialDefenseEV.Content = FormSelected.EVS.SpecialDefense;
                        lblSpeedEV.Content = FormSelected.EVS.Speed;
                    }
                    else
                    {
                        lblHPEV.Content = BackupForm.EVS.HP;
                        lblAttackEV.Content = BackupForm.EVS.Attack;
                        lblDefenseEV.Content = BackupForm.EVS.Defense;
                        lblSpecialAttackEV.Content = BackupForm.EVS.SpecialAttack;
                        lblSpecialDefenseEV.Content = BackupForm.EVS.SpecialDefense;
                        lblSpeedEV.Content = BackupForm.EVS.Speed;
                    }

                    string abilities = "";
                    string hiddenAbilities = "";
                    grdAbilities.Children.Clear();
                    grdAbilities.RowDefinitions.Clear();
                    int abilityCount = 0;
                    if (FormSelected.Abilities != null && FormSelected.Abilities.Count() != 0)
                    {
                        foreach (App.Ability ability in FormSelected.Abilities)
                        {
                            RowDefinition row = new RowDefinition();
                            row.Height = new GridLength(1, GridUnitType.Star);
                            grdAbilities.RowDefinitions.Add(row);
                            Label abilityLabel = new Label();
                            abilityLabel.Content = ability.AbilityName;
                            abilityLabel.Cursor = Cursors.Hand;
                            abilityLabel.FontSize = 24;
                            abilityLabel.VerticalAlignment = VerticalAlignment.Center;
                            abilityLabel.MouseDown += AbilityLabel_MouseDown;
                            abilityLabel.SetValue(Grid.RowProperty, abilityCount);
                            grdAbilities.Children.Add(abilityLabel);
                            if (ability.Hidden)
                            {
                                Label hiddenLabel = new Label();
                                hiddenLabel.Content = "(H)";
                                hiddenLabel.FontSize = 24;
                                hiddenLabel.VerticalAlignment = VerticalAlignment.Center;
                                hiddenLabel.SetValue(Grid.RowProperty, abilityCount);
                                hiddenLabel.SetValue(Grid.ColumnProperty, 1);
                                grdAbilities.Children.Add(hiddenLabel);
                            }
                            abilityCount++;
                        }
                    }
                    else
                    {
                        foreach (App.Ability ability in BackupForm.Abilities)
                        {
                            RowDefinition row = new RowDefinition();
                            row.Height = new GridLength(1, GridUnitType.Star);
                            grdAbilities.RowDefinitions.Add(row);
                            Label abilityLabel = new Label();
                            abilityLabel.Content = ability.AbilityName;
                            abilityLabel.Cursor = Cursors.Hand;
                            abilityLabel.FontSize = 24;
                            abilityLabel.VerticalAlignment = VerticalAlignment.Center;
                            abilityLabel.MouseDown += AbilityLabel_MouseDown;
                            abilityLabel.SetValue(Grid.RowProperty, abilityCount);
                            grdAbilities.Children.Add(abilityLabel);
                            if (ability.Hidden)
                            {
                                Label hiddenLabel = new Label();
                                hiddenLabel.Content = "(H)";
                                hiddenLabel.FontSize = 24;
                                hiddenLabel.VerticalAlignment = VerticalAlignment.Center;
                                hiddenLabel.SetValue(Grid.RowProperty, abilityCount);
                                hiddenLabel.SetValue(Grid.ColumnProperty, 1);
                                grdAbilities.Children.Add(hiddenLabel);
                            }
                            abilityCount++;
                        }

                    }

                    blkAbilityInfo.Text = "";

                    if (!String.IsNullOrEmpty(abilities))
                        abilities = abilities.Substring(2);

                    if (!String.IsNullOrEmpty(hiddenAbilities))
                        hiddenAbilities = hiddenAbilities.Substring(2);

                    if (FormSelected.Moves != null && FormSelected.Moves.Count() != 0)
                    {
                        dtgMoves.ItemsSource = FormSelected.Moves;
                    }
                    else
                    {
                        dtgMoves.ItemsSource = BackupForm.Moves;
                    }


                    grdDropsInfo.RowDefinitions.Clear();
                    grdDropsInfo.Children.Clear();
                    int count = 0;

                    List<App.Drop> dropList = null;

                    if (FormSelected.Drops != null && FormSelected.Drops.Count() != 0)
                    {
                        dropList = FormSelected.Drops;
                    }
                    else
                    {
                        dropList = BackupForm.Drops;
                    }

                    foreach (App.Drop drop in dropList)
                    {
                        RowDefinition row = new RowDefinition();
                        GridLengthConverter grc = new GridLengthConverter();
                        row.Height = (GridLength)grc.ConvertFromString("40");
                        grdDropsInfo.RowDefinitions.Add(row);

                        Label lbl = new Label();
                        lbl.SetValue(Grid.RowProperty, count);
                        lbl.FontSize = 28;
                        lbl.Content = drop.Item;
                        if (!String.IsNullOrEmpty(drop.QuantityRange))
                            lbl.Content = lbl.Content + " \tQuantity: " + drop.QuantityRange;

                        if (drop.Percent != 0)
                            lbl.Content = lbl.Content + " \tPercent: " + drop.Percent + "%";

                        grdDropsInfo.Children.Add(lbl);
                        count++;
                    }

                    grdEvolutionsInfo.RowDefinitions.Clear();
                    grdEvolutionsInfo.Children.Clear();

                    int evocount = 0;
                    if (FormSelected.Evolutions != null)
                    {
                        if (!String.IsNullOrEmpty(FormSelected.PreEvo.PreEvolutionName))
                        {
                            Pokemon preEvoDefault = pokemonList.FirstOrDefault(p => p.Name.ToLower() == FormSelected.PreEvo.PreEvolutionName);
                            Pokemon SelectedForm = preEvoDefault;
                            string evolveIntoName = FormSelected.Name;
                            if (FormSelected.Form != "Default")
                                evolveIntoName = evolveIntoName + " " + FormSelected.Form;

                            Evolution evolution = preEvoDefault.Evolutions.FirstOrDefault(e => e.EvolveInto.ToLower().Contains(evolveIntoName.ToLower()));
                            if(evolution == null)
                                foreach(Pokemon form in preEvoDefault.Forms)
                                {
                                    if (evolution != null)
                                        break;
                                    evolution = form.Evolutions.FirstOrDefault(e => e.EvolveInto.ToLower().Contains(evolveIntoName.ToLower()));
                                    SelectedForm = form;

                                }
                            if(evolution != null)
                            {
                                RowDefinition row = new RowDefinition();
                                GridLengthConverter grc = new GridLengthConverter();
                                row.Height = (GridLength)grc.ConvertFromString("40");
                                grdEvolutionsInfo.RowDefinitions.Add(row);

                                RowDefinition row2 = new RowDefinition();
                                row2.Height = GridLength.Auto;
                                grdEvolutionsInfo.RowDefinitions.Add(row2);

                                Label lblName = new Label();
                                lblName.SetValue(Grid.RowProperty, evocount);
                                lblName.FontSize = 30;
                                lblName.Content = "PreEvolution: " + ProperString(SelectedForm.Name);
                                if(SelectedForm.Form != "Default")
                                    lblName.Content = lblName.Content.ToString() + " " + SelectedForm.Form.ToLower();
                                grdEvolutionsInfo.Children.Add(lblName);
                                evocount++;

                                TextBlock blkEvoInfo = new TextBlock();
                                blkEvoInfo.SetValue(Grid.RowProperty, evocount);
                                blkEvoInfo.FontSize = 24;
                                blkEvoInfo.TextWrapping = TextWrapping.Wrap;
                                blkEvoInfo.FontFamily = new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/"), "/Fonts/#Pokemon Fire Red");

                                if (evolution.Level != 0)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must be atleast level " + evolution.Level + ". "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }

                                if (evolution.Items.Count != 0)
                                {
                                    foreach (EvoItem item in evolution.Items)
                                    {
                                        if (!String.IsNullOrWhiteSpace(item.Item))
                                        {
                                            blkEvoInfo.Inlines.Add(new Run(spacing + "Give "));
                                            if (item.ItemMin == item.ItemMax)
                                            {
                                                blkEvoInfo.Inlines.Add(new Run(item.ItemMin + " " + item.Item + ". "));
                                                blkEvoInfo.Inlines.Add(new LineBreak());
                                            }
                                            else
                                            {
                                                blkEvoInfo.Inlines.Add(new Run(item.ItemMin + "-" + item.ItemMax + " " + item.Item + ". "));
                                                blkEvoInfo.Inlines.Add(new LineBreak());
                                            }
                                        }

                                    }

                                }

                                if (evolution.Trade)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must be traded. "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                                if (evolution.Friendship != 0)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must have atleast " + evolution.Friendship + " friendship. "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                                if (!String.IsNullOrEmpty(evolution.Gender))
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must be " + ProperString(evolution.Gender) + ". "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                                if (!String.IsNullOrEmpty(evolution.Time))
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must be " + ProperString(evolution.Time) + " time. "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }

                                if (evolution.learnedMoveType.Count != 0)
                                {
                                    foreach (string moveType in evolution.learnedMoveType)
                                    {
                                        blkEvoInfo.Inlines.Add(new Run(spacing + "Must have a " + ProperString(moveType) + " move. "));
                                        blkEvoInfo.Inlines.Add(new LineBreak());

                                    }

                                }

                                if (evolution.learnedMove.Count != 0)
                                {
                                    foreach (string move in evolution.learnedMove)
                                    {
                                        blkEvoInfo.Inlines.Add(new Run(spacing + "Must have the move " + ProperString(move) + ". "));
                                        blkEvoInfo.Inlines.Add(new LineBreak());

                                    }
                                }
                                if (!String.IsNullOrEmpty(evolution.Biome))
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must be in " + evolution.Biome + ". "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                                if (!String.IsNullOrEmpty(evolution.HeldItem))
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must be holding a " + evolution.HeldItem + ". "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                                if (evolution.BlocksTravelled != 0)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must walk " + evolution.BlocksTravelled + " blocks. "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }

                                if (evolution.StatCompare.Count != 0)
                                {
                                    foreach (App.EvoStatCompare statCompare in evolution.StatCompare)
                                    {
                                        blkEvoInfo.Inlines.Add(new Run(spacing + ProperString(statCompare.StatOne) + " " + statCompare.StatOperator + " " + ProperString(statCompare.StatTwo)));
                                        blkEvoInfo.Inlines.Add(new LineBreak());
                                    }
                                }

                                if (evolution.UsedMove.Count != 0)
                                {
                                    foreach (App.EvoUsedMove usedMove in evolution.UsedMove)
                                    {
                                        blkEvoInfo.Inlines.Add(new Run(spacing + "Must use " + ProperString(usedMove.UsedMove) + " " + usedMove.UsedMoveTimes + " times."));
                                        blkEvoInfo.Inlines.Add(new LineBreak());
                                    }
                                }

                                if (evolution.DefeatRequirement.Count != 0)
                                {
                                    foreach (App.EvoDefeatRequirement defeat in evolution.DefeatRequirement)
                                    {
                                        blkEvoInfo.Inlines.Add(new Run(spacing + "Must defeat " + ProperString(defeat.Pokemon) + " " + defeat.Amount + " times."));
                                        blkEvoInfo.Inlines.Add(new LineBreak());
                                    }
                                }
                                if (evolution.isShedinja)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Appears after evolving into Ninjask with an empty party slot."));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                                if (evolution.isWurmple)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "50% random chance."));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                                if (evolution.Damaged != 0)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must recieve " + evolution.Damaged + " total damage. "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                                if (evolution.Recoil != 0)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must recieve " + evolution.Recoil + " total recoil damage. "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                                if (evolution.Crits != 0)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must hit " + evolution.Crits + " crits in a single battle. "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                                if (evolution.PartyMember.Count != 0)
                                {
                                    foreach (string member in evolution.PartyMember)
                                    {
                                        blkEvoInfo.Inlines.Add(new Run(spacing + "Must have " + ProperString(member) + " in the party. "));
                                        blkEvoInfo.Inlines.Add(new LineBreak());

                                    }
                                }
                                if (evolution.PartyMemberType.Count != 0)
                                {
                                    foreach (string type in evolution.PartyMemberType)
                                    {
                                        blkEvoInfo.Inlines.Add(new Run(spacing + "Must have member with the type " + ProperString(type) + " in the party. "));
                                        blkEvoInfo.Inlines.Add(new LineBreak());

                                    }
                                }
                                if (!String.IsNullOrEmpty(evolution.Weather))
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must be in " + evolution.Weather + ". "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }

                                grdEvolutionsInfo.Children.Add(blkEvoInfo);
                                evocount++;

                            }

                        }
                        foreach (App.Evolution evolution in FormSelected.Evolutions)
                        {
                            RowDefinition row = new RowDefinition();
                            GridLengthConverter grc = new GridLengthConverter();
                            row.Height = (GridLength)grc.ConvertFromString("40");
                            grdEvolutionsInfo.RowDefinitions.Add(row);

                            RowDefinition row2 = new RowDefinition();
                            row2.Height = GridLength.Auto;
                            grdEvolutionsInfo.RowDefinitions.Add(row2);

                            Label lblName = new Label();
                            lblName.SetValue(Grid.RowProperty, evocount);
                            lblName.FontSize = 30;
                            lblName.Content = ProperString(evolution.EvolveInto.Split("=").FirstOrDefault());
                            grdEvolutionsInfo.Children.Add(lblName);
                            evocount++;

                            TextBlock blkEvoInfo = new TextBlock();
                            blkEvoInfo.SetValue(Grid.RowProperty, evocount);
                            blkEvoInfo.FontSize = 24;
                            blkEvoInfo.TextWrapping = TextWrapping.Wrap;
                            blkEvoInfo.FontFamily = new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/"), "/Fonts/#Pokemon Fire Red");

                            if (evolution.Level != 0)
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "Must be atleast level " + evolution.Level + ". "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }

                            if (evolution.Items.Count != 0)
                            {
                                foreach (EvoItem item in evolution.Items)
                                {
                                    if (!String.IsNullOrWhiteSpace(item.Item))
                                    {
                                        blkEvoInfo.Inlines.Add(new Run(spacing + "Give "));
                                        if (item.ItemMin == item.ItemMax)
                                        {
                                            blkEvoInfo.Inlines.Add(new Run(item.ItemMin + " " + item.Item + ". "));
                                            blkEvoInfo.Inlines.Add(new LineBreak());
                                        }
                                        else
                                        {
                                            blkEvoInfo.Inlines.Add(new Run(item.ItemMin + "-" + item.ItemMax + " " + item.Item + ". "));
                                            blkEvoInfo.Inlines.Add(new LineBreak());
                                        }
                                    }

                                }

                            }

                            if (evolution.Trade)
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "Must be traded. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.Friendship != 0)
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "Must have atleast " + evolution.Friendship + " friendship. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrEmpty(evolution.Gender))
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "Must be " + ProperString(evolution.Gender) + ". "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrEmpty(evolution.Time))
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "Must be " + ProperString(evolution.Time) + " time. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }

                            if (evolution.learnedMoveType.Count != 0)
                            {
                                foreach (string moveType in evolution.learnedMoveType)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must have a " + ProperString(moveType) + " move. "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());

                                }

                            }

                            if (evolution.learnedMove.Count != 0)
                            {
                                foreach (string move in evolution.learnedMove)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must have the move " + ProperString(move) + ". "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());

                                }
                            }
                            if (!String.IsNullOrEmpty(evolution.Biome))
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "Must be in " + evolution.Biome + ". "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrEmpty(evolution.HeldItem))
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "Must be holding a " + evolution.HeldItem + ". "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.BlocksTravelled != 0)
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "Must walk " + evolution.BlocksTravelled + " blocks. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }

                            if (evolution.StatCompare.Count != 0)
                            {
                                foreach (App.EvoStatCompare statCompare in evolution.StatCompare)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + ProperString(statCompare.StatOne) + " " + statCompare.StatOperator + " " + ProperString(statCompare.StatTwo)));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                            }

                            if (evolution.UsedMove.Count != 0)
                            {
                                foreach (App.EvoUsedMove usedMove in evolution.UsedMove)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must use " + ProperString(usedMove.UsedMove) + " " + usedMove.UsedMoveTimes + " times."));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                            }

                            if (evolution.DefeatRequirement.Count != 0)
                            {
                                foreach (App.EvoDefeatRequirement defeat in evolution.DefeatRequirement)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must defeat " + ProperString(defeat.Pokemon) + " " + defeat.Amount + " times."));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                            }
                            if (evolution.isShedinja)
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "Appears after evolving into Ninjask with an empty party slot."));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.isWurmple)
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "50% random chance."));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.Damaged != 0)
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "Must recieve " + evolution.Damaged + " total damage. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.Recoil != 0)
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "Must recieve " + evolution.Recoil + " total recoil damage. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.Crits != 0)
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "Must hit " + evolution.Crits + " crits in a single battle. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.PartyMember.Count != 0)
                            {
                                foreach (string member in evolution.PartyMember)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must have " + ProperString(member) + " in the party. "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());

                                }
                            }
                            if (evolution.PartyMemberType.Count != 0)
                            {
                                foreach (string type in evolution.PartyMemberType)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(spacing + "Must have member with the type " + ProperString(type) + " in the party. "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());

                                }
                            }
                            if (!String.IsNullOrEmpty(evolution.Weather))
                            {
                                blkEvoInfo.Inlines.Add(new Run(spacing + "Must be in " + evolution.Weather + ". "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }

                            grdEvolutionsInfo.Children.Add(blkEvoInfo);
                            evocount++;


                        }
                    }

                    grdSpawnsInfo.RowDefinitions.Clear();
                    grdSpawnsInfo.Children.Clear();
                    int spawncount = 0;

                    if (FormSelected.Spawns != null)
                    {
                        foreach(App.Spawn spawn in FormSelected.Spawns)
                        {

                            RowDefinition rowID = new RowDefinition();
                            GridLengthConverter grc = new GridLengthConverter();
                            rowID.Height = (GridLength)grc.ConvertFromString("40");
                            grdSpawnsInfo.RowDefinitions.Add(rowID);

                            Label lblID = new Label();
                            lblID.SetValue(Grid.RowProperty, spawncount);
                            lblID.FontSize = 30;
                            lblID.Content = ProperString(spawn.ID);
                            grdSpawnsInfo.Children.Add(lblID);
                            spawncount++;


                            RowDefinition rowPokemon = new RowDefinition();
                            rowPokemon.Height = (GridLength)grc.ConvertFromString("36");
                            grdSpawnsInfo.RowDefinitions.Add(rowPokemon);

                            Label lblPokemon = new Label();
                            lblPokemon.SetValue(Grid.RowProperty, spawncount);
                            lblPokemon.FontSize = 27;
                            lblPokemon.FontWeight = FontWeights.Bold;
                            lblPokemon.Content = ProperString(spawn.Pokemon);
                            grdSpawnsInfo.Children.Add(lblPokemon);
                            spawncount++;


                            RowDefinition rowBucket = new RowDefinition();
                            rowBucket.Height = (GridLength)grc.ConvertFromString("36");
                            grdSpawnsInfo.RowDefinitions.Add(rowBucket);

                            Label lblBucket = new Label();
                            lblBucket.SetValue(Grid.RowProperty, spawncount);
                            lblBucket.FontSize = 27;
                            lblBucket.Content = ProperString(spawn.Bucket) + "\t\t" + ProperString(spawn.SpawnablePositionType);
                            grdSpawnsInfo.Children.Add(lblBucket);
                            spawncount++;


                            if (spawn.Presets.Count != 0)
                            {
                                RowDefinition rowPresets = new RowDefinition();
                                rowPresets.Height = (GridLength)grc.ConvertFromString("36");
                                grdSpawnsInfo.RowDefinitions.Add(rowPresets);

                                Label lblPresets = new Label();
                                lblPresets.SetValue(Grid.RowProperty, spawncount);
                                lblPresets.FontSize = 27;
                                lblPresets.Content = "Presets: ";
                                foreach (string preset in spawn.Presets)
                                    lblPresets.Content = lblPresets.Content + ProperString(preset) + ", ";
                                lblPresets.Content = lblPresets.Content.ToString().Substring(0, lblPresets.Content.ToString().Length - 2);
                                grdSpawnsInfo.Children.Add(lblPresets);
                                spawncount++;
                            }

                            RowDefinition rowConditions = new RowDefinition();
                            rowConditions.Height = GridLength.Auto;
                            grdSpawnsInfo.RowDefinitions.Add(rowConditions);

                            Grid columnsGrid = new Grid();
                            columnsGrid.SetValue(Grid.RowProperty, spawncount);
                            grdSpawnsInfo.Children.Add(columnsGrid);

                            ColumnDefinition columnOne = new ColumnDefinition();
                            columnOne.Width = new GridLength(1, GridUnitType.Star);
                            columnsGrid.ColumnDefinitions.Add(columnOne);

                            Label lblCondition = new Label();
                            lblCondition.SetValue(Grid.ColumnProperty, 0);
                            lblCondition.FontSize = 27;
                            lblCondition.Content = "Conditions";
                            columnsGrid.Children.Add(lblCondition);

                            ColumnDefinition columnTwo = new ColumnDefinition();
                            columnTwo.Width = new GridLength(1, GridUnitType.Star);
                            columnsGrid.ColumnDefinitions.Add(columnTwo);

                            Label lblAntiCondition = new Label();
                            lblAntiCondition.SetValue(Grid.ColumnProperty, 1);
                            lblAntiCondition.FontSize = 27;
                            lblAntiCondition.Content = "Anti-Conditions";
                            columnsGrid.Children.Add(lblAntiCondition);

                            RowDefinition rowTitle = new RowDefinition();
                            rowTitle.Height = (GridLength)grc.ConvertFromString("36");
                            columnsGrid.RowDefinitions.Add(rowTitle);

                            RowDefinition rowText = new RowDefinition();
                            rowText.Height = GridLength.Auto;
                            columnsGrid.RowDefinitions.Add(rowText);


                            TextBlock blkConditions = new TextBlock();
                            blkConditions.SetValue(Grid.RowProperty, 1);
                            blkConditions.FontSize = 21;
                            blkConditions.TextWrapping = TextWrapping.Wrap;
                            blkConditions.FontFamily = new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/"), "/Fonts/#Pokemon Fire Red");
                            columnsGrid.Children.Add(blkConditions);


                            TextBlock blkAntiConditions = new TextBlock();
                            blkAntiConditions.SetValue(Grid.RowProperty, 1);
                            blkAntiConditions.SetValue(Grid.ColumnProperty, 1);
                            blkAntiConditions.FontSize = 21;
                            blkAntiConditions.TextWrapping = TextWrapping.Wrap;
                            blkAntiConditions.FontFamily = new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/"), "/Fonts/#Pokemon Fire Red");
                            columnsGrid.Children.Add(blkAntiConditions);


                            if (spawn.Condition.CanSeeSky != null)
                            {
                                if (spawn.Condition.CanSeeSky == true)
                                    blkConditions.Inlines.Add(new Run(spacing + "Can see the sky"));
                                else
                                    blkConditions.Inlines.Add(new Run(spacing + "Can't see the sky"));

                                blkConditions.Inlines.Add(new LineBreak());
                            }

                            if (spawn.Condition.MinSkyLight != null)
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "Minimun sky light of " + spawn.Condition.MinSkyLight));
                                blkConditions.Inlines.Add(new LineBreak());
                            }
                            if (spawn.Condition.MaxSkyLight != null)
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "Maximun sky light of " + spawn.Condition.MaxSkyLight));
                                blkConditions.Inlines.Add(new LineBreak());
                            }

                            if (spawn.Condition.MinY != null)
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "Minimun Y of " + spawn.Condition.MinY));
                                blkConditions.Inlines.Add(new LineBreak());
                            }
                            if (spawn.Condition.MaxY != null)
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "Maximun Y of " + spawn.Condition.MaxY));
                                blkConditions.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrWhiteSpace(spawn.Condition.TimeRange))
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "Time range of " + spawn.Condition.TimeRange));
                                blkConditions.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrWhiteSpace(spawn.Condition.MoonPhase))
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "Moon Phase of " + spawn.Condition.MoonPhase));
                                blkConditions.Inlines.Add(new LineBreak());
                            }
                            if (spawn.Condition.IsRaining != null)
                            {
                                if (spawn.Condition.IsRaining == true)
                                    blkConditions.Inlines.Add(new Run(spacing + "Is Raining"));
                                else
                                    blkConditions.Inlines.Add(new Run(spacing + "Isn't Raining"));

                                blkConditions.Inlines.Add(new LineBreak());
                            }
                            if(spawn.Condition.NeededNearbyBlocks != null && spawn.Condition.NeededNearbyBlocks.Count != 0)
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "Needed Nearby Blocks:"));
                                blkConditions.Inlines.Add(new LineBreak());
                                foreach (string block in spawn.Condition.NeededNearbyBlocks)
                                {
                                    blkConditions.Inlines.Add(new Run(spacing + "    -" + block));
                                    blkConditions.Inlines.Add(new LineBreak());
                                }
                            }
                            if (spawn.Condition.NeededBaseBlocks != null && spawn.Condition.NeededBaseBlocks.Count != 0)
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "Needed Base Blocks:"));
                                blkConditions.Inlines.Add(new LineBreak());
                                foreach (string block in spawn.Condition.NeededBaseBlocks)
                                {
                                    blkConditions.Inlines.Add(new Run(spacing + "    -" + block));
                                    blkConditions.Inlines.Add(new LineBreak());
                                }
                            }
                            if (spawn.Condition.Biomes != null && spawn.Condition.Biomes.Count != 0)
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "In Biomes:"));
                                blkConditions.Inlines.Add(new LineBreak());
                                foreach (string biome in spawn.Condition.Biomes)
                                {
                                    blkConditions.Inlines.Add(new Run(spacing + "    -" + biome));
                                    blkConditions.Inlines.Add(new LineBreak());
                                }
                            }
                            if (spawn.Condition.Structures != null && spawn.Condition.Structures.Count != 0)
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "In Structures:"));
                                blkConditions.Inlines.Add(new LineBreak());
                                foreach (string structure in spawn.Condition.Structures)
                                {
                                    blkConditions.Inlines.Add(new Run(spacing + "    -" + structure));
                                    blkConditions.Inlines.Add(new LineBreak());
                                }
                            }

                            if (spawn.Condition.IsSlimeChunk != null)
                            {
                                if (spawn.Condition.IsSlimeChunk == true)
                                    blkConditions.Inlines.Add(new Run(spacing + "In a slime chunk"));
                                else
                                    blkConditions.Inlines.Add(new Run(spacing + "Outside a slime chunk"));

                                blkConditions.Inlines.Add(new LineBreak());
                            }

                            if (spawn.Condition.MinLureLevel != null)
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "Minimun lure level of " + spawn.Condition.MinLureLevel));
                                blkConditions.Inlines.Add(new LineBreak());
                            }
                            if (spawn.Condition.MaxLureLevel != null)
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "Maximun lure level of " + spawn.Condition.MaxLureLevel));
                                blkConditions.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrWhiteSpace(spawn.Condition.RodType))
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "Rod Type of " + spawn.Condition.RodType));
                                blkConditions.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrWhiteSpace(spawn.Condition.Bait))
                            {
                                blkConditions.Inlines.Add(new Run(spacing + "Using bait of " + spawn.Condition.Bait));
                                blkConditions.Inlines.Add(new LineBreak());
                            }

                            if (spawn.AntiCondition.CanSeeSky != null)
                            {
                                if (spawn.AntiCondition.CanSeeSky == true)
                                    blkAntiConditions.Inlines.Add(new Run(spacing + "Can see the sky"));
                                else
                                    blkAntiConditions.Inlines.Add(new Run(spacing + "Can't see the sky"));

                                blkAntiConditions.Inlines.Add(new LineBreak());
                            }

                            if (spawn.AntiCondition.MinSkyLight != null)
                            {
                                blkAntiConditions.Inlines.Add(new Run(spacing + "Minimun sky light of " + spawn.AntiCondition.MinSkyLight));
                                blkAntiConditions.Inlines.Add(new LineBreak());
                            }
                            if (spawn.AntiCondition.MaxSkyLight != null)
                            {
                                blkAntiConditions.Inlines.Add(new Run(spacing + "Maximun sky light of " + spawn.AntiCondition.MaxSkyLight));
                                blkAntiConditions.Inlines.Add(new LineBreak());
                            }

                            if (spawn.AntiCondition.MinY != null)
                            {
                                blkAntiConditions.Inlines.Add(new Run(spacing + "Minimun Y of " + spawn.AntiCondition.MinY));
                                blkAntiConditions.Inlines.Add(new LineBreak());
                            }
                            if (spawn.AntiCondition.MaxY != null)
                            {
                                blkAntiConditions.Inlines.Add(new Run(spacing + "Maximun Y of " + spawn.AntiCondition.MaxY));
                                blkAntiConditions.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrWhiteSpace(spawn.AntiCondition.TimeRange))
                            {
                                blkAntiConditions.Inlines.Add(new Run(spacing + "Time range of " + spawn.AntiCondition.TimeRange));
                                blkAntiConditions.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrWhiteSpace(spawn.AntiCondition.MoonPhase))
                            {
                                blkAntiConditions.Inlines.Add(new Run(spacing + "Moon Phase of " + spawn.AntiCondition.MoonPhase));
                                blkAntiConditions.Inlines.Add(new LineBreak());
                            }
                            if (spawn.AntiCondition.IsRaining != null)
                            {
                                if (spawn.AntiCondition.IsRaining == true)
                                    blkAntiConditions.Inlines.Add(new Run(spacing + "Is Raining"));
                                else
                                    blkAntiConditions.Inlines.Add(new Run(spacing + "Isn't Raining"));

                                blkAntiConditions.Inlines.Add(new LineBreak());
                            }
                            if(spawn.AntiCondition.NeededNearbyBlocks != null)
                                if (spawn.AntiCondition.NeededNearbyBlocks.Count != 0)
                                {
                                    blkAntiConditions.Inlines.Add(new Run(spacing + "Not Near Blocks:"));
                                    blkAntiConditions.Inlines.Add(new LineBreak());
                                    foreach (string block in spawn.AntiCondition.NeededNearbyBlocks)
                                    {
                                        blkAntiConditions.Inlines.Add(new Run(spacing + "    -" + block));
                                        blkAntiConditions.Inlines.Add(new LineBreak());
                                    }
                                }
                            if (spawn.AntiCondition.NeededBaseBlocks != null)
                                if (spawn.AntiCondition.NeededBaseBlocks.Count != 0)
                                {
                                    blkAntiConditions.Inlines.Add(new Run(spacing + "Not on Base Blocks:"));
                                    blkAntiConditions.Inlines.Add(new LineBreak());
                                    foreach (string block in spawn.AntiCondition.NeededBaseBlocks)
                                    {
                                        blkAntiConditions.Inlines.Add(new Run(spacing + "    -" + block));
                                        blkAntiConditions.Inlines.Add(new LineBreak());
                                    }
                                }
                            if (spawn.AntiCondition.Biomes != null)
                                if (spawn.AntiCondition.Biomes.Count != 0)
                                {
                                    blkAntiConditions.Inlines.Add(new Run(spacing + "Not in Biomes:"));
                                    blkAntiConditions.Inlines.Add(new LineBreak());
                                    foreach (string biome in spawn.AntiCondition.Biomes)
                                    {
                                        blkAntiConditions.Inlines.Add(new Run(spacing + "    -" + biome));
                                        blkAntiConditions.Inlines.Add(new LineBreak());
                                    }
                                }
                            if (spawn.AntiCondition.Structures != null)
                                if (spawn.AntiCondition.Structures.Count != 0)
                                {
                                    blkAntiConditions.Inlines.Add(new Run(spacing + "Not in Structures:"));
                                    blkAntiConditions.Inlines.Add(new LineBreak());
                                    foreach (string structure in spawn.AntiCondition.Structures)
                                    {
                                        blkAntiConditions.Inlines.Add(new Run(spacing + "    -" + structure));
                                        blkAntiConditions.Inlines.Add(new LineBreak());
                                    }
                                }

                            if (spawn.AntiCondition.IsSlimeChunk != null)
                            {
                                if (spawn.AntiCondition.IsSlimeChunk == true)
                                    blkAntiConditions.Inlines.Add(new Run(spacing + "In a slime chunk"));
                                else
                                    blkAntiConditions.Inlines.Add(new Run(spacing + "Outside a slime chunk"));

                                blkAntiConditions.Inlines.Add(new LineBreak());
                            }

                            if (spawn.AntiCondition.MinLureLevel != null)
                            {
                                blkAntiConditions.Inlines.Add(new Run(spacing + "Minimun lure level of " + spawn.AntiCondition.MinLureLevel));
                                blkAntiConditions.Inlines.Add(new LineBreak());
                            }
                            if (spawn.AntiCondition.MaxLureLevel != null)
                            {
                                blkAntiConditions.Inlines.Add(new Run(spacing + "Maximun lure level of " + spawn.AntiCondition.MaxLureLevel));
                                blkAntiConditions.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrWhiteSpace(spawn.AntiCondition.RodType))
                            {
                                blkAntiConditions.Inlines.Add(new Run(spacing + "Rod Type of " + spawn.AntiCondition.RodType));
                                blkAntiConditions.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrWhiteSpace(spawn.AntiCondition.Bait))
                            {
                                blkAntiConditions.Inlines.Add(new Run(spacing + "Using bait of " + spawn.AntiCondition.Bait));
                                blkAntiConditions.Inlines.Add(new LineBreak());
                            }

                            spawncount++;
                        }

                    }
                }
            }
            catch
            {
                MessageBox.Show("combobox form error");

            }

        }
        private void AbilityLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Label abilityLabel = (Label)sender;
                int row = Grid.GetRow(abilityLabel);
                Pokemon selectedPokemon = dtgPokemon.SelectedItem as Pokemon;
                if (cboForm.SelectedIndex != 0)
                    if (selectedPokemon.Forms[cboForm.SelectedIndex - 1].Abilities.Count != 0)
                        selectedPokemon = selectedPokemon.Forms[cboForm.SelectedIndex - 1];
                blkAbilityInfo.Text = "";
                blkAbilityInfo.Inlines.Add(new Run(selectedPokemon.Abilities[row].Description));
            }
            catch
            {
                MessageBox.Show("ability label error");
            }
        }
        private string ProperString(string word)
        {

            if (string.IsNullOrWhiteSpace(word))
                return word;

            word = word.ToLower();
            return char.ToUpper(word[0]) + word.Substring(1);

        }
        private void btnGeneral_Click(object sender, RoutedEventArgs e)
        {
            grdGeneralInfo.Visibility = Visibility.Visible;
            grdLearnsetInfo.Visibility = Visibility.Collapsed;
            grdDropsInfo.Visibility = Visibility.Collapsed;
            scrEvolutionsInfo.Visibility = Visibility.Collapsed;
            scrSpawnsInfo.Visibility = Visibility.Collapsed;

        }
        private void btnLearnset_Click(object sender, RoutedEventArgs e)
        {
            grdGeneralInfo.Visibility = Visibility.Collapsed;
            grdLearnsetInfo.Visibility = Visibility.Visible;
            grdDropsInfo.Visibility = Visibility.Collapsed;
            scrEvolutionsInfo.Visibility = Visibility.Collapsed;
            scrSpawnsInfo.Visibility = Visibility.Collapsed;

        }
        private void btnDrops_Click(object sender, RoutedEventArgs e)
        {
            grdGeneralInfo.Visibility = Visibility.Collapsed;
            grdLearnsetInfo.Visibility = Visibility.Collapsed;
            grdDropsInfo.Visibility = Visibility.Visible;
            scrEvolutionsInfo.Visibility = Visibility.Collapsed;
            scrSpawnsInfo.Visibility = Visibility.Collapsed;

        }
        private void btnEvolutions_Click(object sender, RoutedEventArgs e)
        {
            grdGeneralInfo.Visibility = Visibility.Collapsed;
            grdLearnsetInfo.Visibility = Visibility.Collapsed;
            grdDropsInfo.Visibility = Visibility.Collapsed;
            scrEvolutionsInfo.Visibility = Visibility.Visible;
            scrSpawnsInfo.Visibility = Visibility.Collapsed;

        }
        private void btnSpawns_Click(object sender, RoutedEventArgs e)
        {
            grdGeneralInfo.Visibility = Visibility.Collapsed;
            grdLearnsetInfo.Visibility = Visibility.Collapsed;
            grdDropsInfo.Visibility = Visibility.Collapsed;
            scrEvolutionsInfo.Visibility = Visibility.Collapsed;
            scrSpawnsInfo.Visibility = Visibility.Visible;

        }
        private void btnPanelSwitch_Click(object sender, RoutedEventArgs e)
        {
            if(grdEdit.Visibility == Visibility.Visible)
            {
                grdEdit.Visibility = Visibility.Collapsed;
                grdDex.Visibility = Visibility.Visible;
                btnPanelSwitch.Content = "Edit Loaded Content";
            }
            else
            {
                grdEdit.Visibility= Visibility.Visible;
                grdDex.Visibility= Visibility.Collapsed;
                btnPanelSwitch.Content = "Access Master Dex";
            }
        }
        private void cboSearchCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(txtSearchPokemon != null)
                txtSearchPokemon.Text = "";
        }
        private void btnRemoveDeselected_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                MessageBoxResult result = MessageBox.Show("Do you want to remove all deselected addons?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                foreach (CheckBox chkbx in modCheckboxes)
                {
                    if (chkbx.IsChecked == false)
                    {
                        Directory.Delete(Path.Combine(dexPath, chkbx.Content.ToString()), true);
                    }
                }
                CountFolders();

            }
            catch
            {
                MessageBox.Show("remove deselected error");
            }
        }
        private void btnChangeDirectory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string folderPath = dialog.FileName;
                    string keyPath = @"Software\MasterCobbleDex";
                    using RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, true);
                    string sourcePath = dexPath;
                    string destinationPath = Path.Combine(folderPath, "MasterCobbleDex");
                    copyFolder(sourcePath, destinationPath);
                    key.SetValue("DexPath", folderPath);
                    dexPath = Path.Combine(folderPath, "MasterCobbleDex");
                    RefreshDataGrid();
                    Directory.Delete(sourcePath, true);
                    key.Close();
                }

            }
            catch
            {
                MessageBox.Show("change directory error");
            }
        }
        public static void copyFolder(string sourceFolder, string destinationFolder)
        {
            Directory.CreateDirectory(destinationFolder);

            foreach (string file in Directory.GetFiles(sourceFolder))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destinationFolder, fileName);

                File.Copy(file, destFile, true); 
            }
            
            foreach (string folder in Directory.GetDirectories(sourceFolder))
            {
                string folderName = Path.GetFileName(folder);
                string destFolder = Path.Combine(destinationFolder, folderName);

                copyFolder(folder, destFolder);
            }
        }
        private void chkAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkAll.IsChecked == true)
                    foreach (CheckBox chkbx in modCheckboxes)
                        chkbx.IsChecked = true;
                else
                    foreach (CheckBox chkbx in modCheckboxes)
                        chkbx.IsChecked = false;
            }
            catch
            {
                MessageBox.Show("checkbox all error");
            }
        }
        private void importCobblemonShowdown(string filePath)
        {
            try
            {
                using (ZipArchive zip = ZipFile.OpenRead(filePath))
                {

                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        if (!string.IsNullOrEmpty(entry.FullName) && entry.FullName.StartsWith("data/") && entry.FullName.Contains("showdown") && entry.FullName.EndsWith(".zip"))
                        {
                            string destinationPath = Path.Combine(dexPath, "cobblemon", entry.Name);
                            using (var entryStream = entry.Open())
                            using (var fileStream = File.Create(destinationPath))
                            {
                                entryStream.CopyTo(fileStream);
                            }
                        }
                    }

                }
                Directory.CreateDirectory(Path.Combine(dexPath, "cobblemon", "js"));
                using (ZipArchive zip = ZipFile.OpenRead(Path.Combine(dexPath, "cobblemon", "showdown.zip")))
                {

                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        if (!string.IsNullOrEmpty(entry.FullName))
                        {
                            switch (entry.FullName)
                            {
                                case "data/moves.js":
                                    string destinationPath = Path.Combine(dexPath, "cobblemon", "js", entry.Name);
                                    using (var entryStream = entry.Open())
                                    using (var fileStream = File.Create(destinationPath))
                                    {
                                        entryStream.CopyTo(fileStream);
                                    }
                                    break;
                                case "data/text/moves.js":
                                    string destinationPath2 = Path.Combine(dexPath, "cobblemon", "js", "movesText.js");
                                    using (var entryStream = entry.Open())
                                    using (var fileStream = File.Create(destinationPath2))
                                    {
                                        entryStream.CopyTo(fileStream);
                                    }
                                    break;
                                case "data/text/abilities.js":
                                    string destinationPath3 = Path.Combine(dexPath, "cobblemon", "js", "abilities.js");
                                    using (var entryStream = entry.Open())
                                    using (var fileStream = File.Create(destinationPath3))
                                    {
                                        entryStream.CopyTo(fileStream);
                                    }
                                    break;
                            }
                        }
                    }

                }

            }
            catch
            {
                MessageBox.Show("import cobblemon showdown error");
            }

        }
        private void importCobblemonMoves()
        {

            try
            {
                List<MoveInfo> AllMoves = new List<MoveInfo>();
                MoveInfo currentInfo = new MoveInfo();

                //move stats
                string moveText = File.ReadAllText(Path.Combine(dexPath, "cobblemon", "js", "moves.js"));

                string marker = "const Moves =";
                int startIndex = moveText.IndexOf(marker);

                if (startIndex == -1)
                    startIndex = 0;

                int braceStart = moveText.IndexOf('{', startIndex);

                if (braceStart == -1)
                    throw new Exception("Could not find opening brace");


                string nameEntry = "";
                string accuracyEntry = "";
                bool accuracySearch = false;
                string basePowerEntry = "";
                bool basePowerSearch = false;
                string categoryEntry = "";
                bool categorySearch = false;
                string ppEntry = "";
                bool ppSearch = false;
                string typeEntry = "";
                bool typeSearch = false;


                int braceCount = 0;
                for (int i = braceStart; i < moveText.Length; i++)
                {
                    if (moveText[i] == '{')
                        braceCount++;

                    if (moveText[i] == '}')
                        braceCount--;

                    if (braceCount == 1)
                        nameEntry = nameEntry + moveText[i];

                    if (accuracySearch)
                    {
                        if ((moveText[i] == '}' || moveText[i] == ',') && !string.IsNullOrEmpty(accuracyEntry))
                        {
                            if (string.IsNullOrEmpty(currentInfo.Accuracy))
                                currentInfo.Accuracy = accuracyEntry.Replace("ccuracy:", "").Replace(" ", "");
                            accuracyEntry = "";
                            accuracySearch = false;
                        }
                        else
                            accuracyEntry = accuracyEntry + moveText[i];
                    }
                    if (basePowerSearch)
                    {
                        if ((moveText[i] == '}' || moveText[i] == ',') && !string.IsNullOrEmpty(basePowerEntry))
                        {
                            if (string.IsNullOrEmpty(currentInfo.BasePower))
                                currentInfo.BasePower = basePowerEntry.Replace("asePower:", "").Replace(" ", "");
                            basePowerEntry = "";
                            basePowerSearch = false;
                        }
                        else
                            basePowerEntry = basePowerEntry + moveText[i];
                    }
                    if (categorySearch)
                    {
                        if ((moveText[i] == '}' || moveText[i] == ',') && !string.IsNullOrEmpty(categoryEntry))
                        {
                            if (string.IsNullOrEmpty(currentInfo.Category))
                                currentInfo.Category = categoryEntry.Replace("ategory:", "").Replace("\"", "").Replace(" ", "");
                            categoryEntry = "";
                            categorySearch = false;
                        }
                        else
                            categoryEntry = categoryEntry + moveText[i];
                    }
                    if (ppSearch)
                    {
                        if ((moveText[i] == '}' || moveText[i] == ',') && !string.IsNullOrEmpty(ppEntry))
                        {
                            if (string.IsNullOrEmpty(currentInfo.PP))
                                currentInfo.PP = ppEntry.Replace("p:", "").Replace(" ", "");
                            ppEntry = "";
                            ppSearch = false;
                        }
                        else
                            ppEntry = ppEntry + moveText[i];
                    }
                    if (typeSearch)
                    {
                        if ((moveText[i] == '}' || moveText[i] == ',') && !string.IsNullOrEmpty(typeEntry))
                        {
                            currentInfo.Type = typeEntry.Replace("ype:", "").Replace("\"", "").Replace(" ", "");
                            typeEntry = "";
                            typeSearch = false;
                        }
                        else
                            typeEntry = typeEntry + moveText[i];
                    }

                    if (braceCount == 2 && !string.IsNullOrEmpty(nameEntry))
                    {
                        string[] bannedPhrases = { "{", "}", "\n", "\"", ":", ",", " " };
                        foreach (string phrase in bannedPhrases)
                            nameEntry = nameEntry.Replace(phrase, "");
                        nameEntry.Trim();
                        currentInfo.Name = nameEntry;
                        nameEntry = "";
                    }
                    else if (braceCount == 2 && moveText[i] == 'a')
                    {
                        int length = 9;
                        if (i + 9 >= moveText.Length)
                            length = i + 8 - moveText.Length;
                        if (moveText.Substring(i, length) == "accuracy:")
                            accuracySearch = true;

                    }
                    else if (braceCount == 2 && moveText[i] == 'b')
                    {
                        int length = 9;
                        if (i + 9 >= moveText.Length)
                            length = i + 8 - moveText.Length;
                        if (moveText.Substring(i, length) == "basePower")
                            basePowerSearch = true;

                    }
                    else if (braceCount == 2 && moveText[i] == 'c')
                    {
                        int length = 9;
                        if (i + 9 >= moveText.Length)
                            length = i + 8 - moveText.Length;
                        if (moveText.Substring(i, length) == "category:")
                            categorySearch = true;

                    }
                    else if (braceCount == 2 && moveText[i] == 'p')
                    {
                        int length = 3;
                        if (i + 3 >= moveText.Length)
                            length = i + 2 - moveText.Length;
                        if (moveText.Substring(i, length) == "pp:")
                            ppSearch = true;

                    }
                    else if (braceCount == 2 && moveText[i] == 't')
                    {
                        int length = 5;
                        if (i + 5 >= moveText.Length)
                            length = i + 4 - moveText.Length;
                        if (moveText.Substring(i, length) == "type:")
                            typeSearch = true;

                    }
                    if (moveText[i] == '}' && braceCount == 1)
                    {
                        AllMoves.Add(currentInfo);
                        currentInfo = new MoveInfo();
                    }

                }


                //move desc

                string moveDescText = File.ReadAllText(Path.Combine(dexPath, "cobblemon", "js", "movesText.js"));

                string markerDesc = "const MovesText =";
                int startIndexDesc = moveDescText.IndexOf(markerDesc);

                if (startIndexDesc == -1)
                    startIndexDesc = 0;

                int braceStartDesc = moveDescText.IndexOf('{', startIndexDesc);

                if (braceStartDesc == -1)
                    throw new Exception("Could not find opening brace");


                string nameEntryDesc = "";
                string shortDescEntry = "";
                bool shortDescSearch = false;


                int braceCountDesc = 0;
                for (int i = braceStartDesc; i < moveDescText.Length; i++)
                {
                    if (moveDescText[i] == '{')
                        braceCountDesc++;

                    if (moveDescText[i] == '}')
                        braceCountDesc--;

                    if (braceCountDesc == 1)
                        nameEntryDesc = nameEntryDesc + moveDescText[i];

                    if (shortDescSearch)
                    {
                        if ((moveDescText[i] == '"') && !string.IsNullOrEmpty(shortDescEntry))
                        {
                            shortDescEntry.Trim();
                            shortDescSearch = false;
                        }
                        else
                            shortDescEntry = shortDescEntry + moveDescText[i];
                    }

                    if (braceCountDesc == 2 && !string.IsNullOrEmpty(nameEntryDesc))
                    {
                        string[] bannedPhrases = { "{", "}", "\n", "\"", ":", ",", " " };
                        foreach (string phrase in bannedPhrases)
                            nameEntryDesc = nameEntryDesc.Replace(phrase, "");
                        nameEntryDesc.Trim();
                    }

                    if (braceCountDesc == 2 && moveDescText[i] == '"')
                    {
                        string t = moveDescText.Substring(i - 11, 12);
                        if (moveDescText.Substring(i - 11, 12) == "shortDesc: \"")
                            shortDescSearch = true;

                    }

                    if (moveDescText[i] == '}' && braceCountDesc == 1)
                    {
                        MoveInfo foundMove = AllMoves.FirstOrDefault(m => m.Name == nameEntryDesc.Replace("}", ""));
                        if (foundMove != null)
                            foundMove.Description = shortDescEntry;
                        nameEntryDesc = "";
                        shortDescEntry = "";
                    }

                }

                foreach (MoveInfo move in AllMoves)
                {
                    string moveJson = System.Text.Json.JsonSerializer.Serialize(move);
                    File.WriteAllText(Path.Combine(dexPath, "cobblemon", "moves", move.Name + ".json"), moveJson);
                }

            }
            catch
            {
                MessageBox.Show("import cobblemon moves error");
            }
        }
        private void importCobblemonAbilities()
        {
            try
            {
                List<AbilityInfo> AllAbilities = new List<AbilityInfo>();
                string abilityDescText = File.ReadAllText(Path.Combine(dexPath, "cobblemon", "js", "abilities.js"));

                string markerDesc = "const AbilitiesText =";
                int startIndexDesc = abilityDescText.IndexOf(markerDesc);

                if (startIndexDesc == -1)
                    startIndexDesc = 0;

                int braceStartDesc = abilityDescText.IndexOf('{', startIndexDesc);

                if (braceStartDesc == -1)
                    throw new Exception("Could not find opening brace");


                string nameEntryDesc = "";
                string shortDescEntry = "";
                bool shortDescSearch = false;


                int braceCountDesc = 0;
                for (int i = braceStartDesc; i < abilityDescText.Length; i++)
                {
                    if (abilityDescText[i] == '{')
                        braceCountDesc++;

                    if (abilityDescText[i] == '}')
                        braceCountDesc--;

                    if (braceCountDesc == 1)
                        nameEntryDesc = nameEntryDesc + abilityDescText[i];

                    if (shortDescSearch)
                    {
                        if ((abilityDescText[i] == '"') && !string.IsNullOrEmpty(shortDescEntry))
                        {
                            shortDescEntry.Trim();
                            shortDescSearch = false;
                        }
                        else
                            shortDescEntry = shortDescEntry + abilityDescText[i];
                    }

                    if (braceCountDesc == 2 && !string.IsNullOrEmpty(nameEntryDesc))
                    {
                        string[] bannedPhrases = { "{", "}", "\n", "\"", ":", ",", " " };
                        foreach (string phrase in bannedPhrases)
                            nameEntryDesc = nameEntryDesc.Replace(phrase, "");
                        nameEntryDesc.Trim();
                    }

                    if (braceCountDesc == 2 && abilityDescText[i] == '"')
                    {
                        string t = abilityDescText.Substring(i - 11, 12);
                        if (abilityDescText.Substring(i - 11, 12) == "shortDesc: \"")
                            shortDescSearch = true;

                    }

                    if (abilityDescText[i] == '}' && braceCountDesc == 1)
                    {
                        AbilityInfo info = new AbilityInfo();
                        info.Name = nameEntryDesc.Replace("}", "").Trim();
                        info.Description = shortDescEntry;
                        AllAbilities.Add(info);
                        nameEntryDesc = "";
                        shortDescEntry = "";
                    }

                }


                foreach (AbilityInfo ability in AllAbilities)
                {
                    string abilityJson = System.Text.Json.JsonSerializer.Serialize(ability);
                    if (ability.Name.Contains("mountaineer"))
                        ability.Name = ability.Name.Replace("//CAP", "");
                    File.WriteAllText(Path.Combine(dexPath, "cobblemon", "abilities", ability.Name + ".json"), abilityJson);
                }

            }
            catch
            {
                MessageBox.Show("import cobblemon abilities error");
            }

        }
        private void importAddonShowdown(string filePath, string modName)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(dexPath, modName, "js"));
                using (ZipArchive zip = ZipFile.OpenRead(filePath))
                {

                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        if (!string.IsNullOrEmpty(entry.FullName) && entry.FullName.StartsWith("data/") && entry.FullName.Contains("moves") && entry.FullName.EndsWith(".js"))
                        {
                            string destinationPath = Path.Combine(dexPath, modName, "js", entry.Name);
                            using (var entryStream = entry.Open())
                            using (var fileStream = File.Create(destinationPath))
                            {
                                entryStream.CopyTo(fileStream);
                            }
                        }
                        if (!string.IsNullOrEmpty(entry.FullName) && entry.FullName.StartsWith("assets/") && entry.FullName.Contains("lang") && entry.FullName.EndsWith("en_us.json"))
                        {
                            string destinationPath = Path.Combine(dexPath, modName, entry.Name);
                            using (var entryStream = entry.Open())
                            using (var fileStream = File.Create(destinationPath))
                            {
                                entryStream.CopyTo(fileStream);
                            }

                        }
                    }

                }

            }
            catch
            {
                MessageBox.Show("import addon showdown error");
            }
        }
        private void importAddonMoves(string modName)
        {
            try
            {
                List<MoveInfo> AllMoves = new List<MoveInfo>();
                MoveInfo currentInfo = new MoveInfo();

                //move stats
                foreach (string file in Directory.GetFiles(Path.Combine(dexPath, modName, "js")))
                {
                    string moveText = File.ReadAllText(file);

                    int braceStart = moveText.IndexOf('{');

                    if (braceStart == -1)
                        throw new Exception("Could not find opening brace");


                    string nameEntry = Path.GetFileNameWithoutExtension(file);
                    string accuracyEntry = "";
                    bool accuracySearch = false;
                    string basePowerEntry = "";
                    bool basePowerSearch = false;
                    string categoryEntry = "";
                    bool categorySearch = false;
                    string ppEntry = "";
                    bool ppSearch = false;
                    string typeEntry = "";
                    bool typeSearch = false;


                    int braceCount = 0;
                    for (int i = braceStart; i < moveText.Length; i++)
                    {
                        if (moveText[i] == '{')
                            braceCount++;

                        if (moveText[i] == '}')
                            braceCount--;

                        if (accuracySearch)
                        {
                            if ((moveText[i] == '}' || moveText[i] == ',') && !string.IsNullOrEmpty(accuracyEntry))
                            {
                                if (string.IsNullOrEmpty(currentInfo.Accuracy))
                                    currentInfo.Accuracy = accuracyEntry.Replace("ccuracy:", "").Replace(" ", "");
                                accuracyEntry = "";
                                accuracySearch = false;
                            }
                            else
                                accuracyEntry = accuracyEntry + moveText[i];
                        }
                        if (basePowerSearch)
                        {
                            if ((moveText[i] == '}' || moveText[i] == ',') && !string.IsNullOrEmpty(basePowerEntry))
                            {
                                if (string.IsNullOrEmpty(currentInfo.BasePower))
                                    currentInfo.BasePower = basePowerEntry.Replace("asePower:", "").Replace(" ", "");
                                basePowerEntry = "";
                                basePowerSearch = false;
                            }
                            else
                                basePowerEntry = basePowerEntry + moveText[i];
                        }
                        if (categorySearch)
                        {
                            if ((moveText[i] == '}' || moveText[i] == ',') && !string.IsNullOrEmpty(categoryEntry))
                            {
                                if (string.IsNullOrEmpty(currentInfo.Category))
                                    currentInfo.Category = categoryEntry.Replace("ategory:", "").Replace("\"", "").Replace(" ", "");
                                categoryEntry = "";
                                categorySearch = false;
                            }
                            else
                                categoryEntry = categoryEntry + moveText[i];
                        }
                        if (ppSearch)
                        {
                            if ((moveText[i] == '}' || moveText[i] == ',') && !string.IsNullOrEmpty(ppEntry))
                            {
                                if (string.IsNullOrEmpty(currentInfo.PP))
                                    currentInfo.PP = ppEntry.Replace("p:", "").Replace(" ", "");
                                ppEntry = "";
                                ppSearch = false;
                            }
                            else
                                ppEntry = ppEntry + moveText[i];
                        }
                        if (typeSearch)
                        {
                            if ((moveText[i] == '}' || moveText[i] == ',') && !string.IsNullOrEmpty(typeEntry))
                            {
                                currentInfo.Type = typeEntry.Replace("ype:", "").Replace("\"", "").Replace(" ", "");
                                typeEntry = "";
                                typeSearch = false;
                            }
                            else
                                typeEntry = typeEntry + moveText[i];
                        }
                        else if (braceCount == 1 && moveText[i] == 'a')
                        {
                            int length = 9;
                            if (i + 9 >= moveText.Length)
                                length = i + 8 - moveText.Length;
                            if (moveText.Substring(i, length) == "accuracy:")
                                accuracySearch = true;

                        }
                        else if (braceCount == 1 && moveText[i] == 'b')
                        {
                            int length = 9;
                            if (i + 9 >= moveText.Length)
                                length = i + 8 - moveText.Length;
                            if (moveText.Substring(i, length) == "basePower")
                                basePowerSearch = true;

                        }
                        else if (braceCount == 1 && moveText[i] == 'c')
                        {
                            int length = 9;
                            if (i + 9 >= moveText.Length)
                                length = i + 8 - moveText.Length;
                            if (moveText.Substring(i, length) == "category:")
                                categorySearch = true;

                        }
                        else if (braceCount == 1 && moveText[i] == 'p')
                        {
                            int length = 3;
                            if (i + 3 >= moveText.Length)
                                length = i + 2 - moveText.Length;
                            if (moveText.Substring(i, length) == "pp:")
                                ppSearch = true;

                        }
                        else if (braceCount == 1 && moveText[i] == 't')
                        {
                            int length = 5;
                            if (i + 5 >= moveText.Length)
                                length = i + 4 - moveText.Length;
                            if (moveText.Substring(i, length) == "type:")
                                typeSearch = true;

                        }
                        if (moveText[i] == '}' && braceCount == 0)
                        {
                            currentInfo.Name = nameEntry;
                            currentInfo.Type = currentInfo.Type.Trim();
                            AllMoves.Add(currentInfo);
                            currentInfo = new MoveInfo();
                            break;
                        }

                    }
                }
                if (!File.Exists(Path.Combine(dexPath, modName, "en_us.json")))
                    return;
                string jsonText = File.ReadAllText(Path.Combine(dexPath, modName, "en_us.json"));
                jsonText = Regex.Replace(jsonText, @"^\s*#+.*$", "", RegexOptions.Multiline);
                JObject langJSON = JObject.Parse(jsonText);
                foreach (MoveInfo move in AllMoves)
                    move.Description = (string)langJSON["cobblemon.move." + move.Name + ".desc"];


                foreach (MoveInfo move in AllMoves)
                {
                    string moveJson = System.Text.Json.JsonSerializer.Serialize(move);
                    File.WriteAllText(Path.Combine(dexPath, modName, "moves", move.Name + ".json"), moveJson);
                }

            }
            catch
            {
                MessageBox.Show("import addon moves error");
            }
        }
        private void importAddonAbilities(string modName)
        {
            try
            {
                List<AbilityInfo> AllAbilities = new List<AbilityInfo>();

                if (!File.Exists(Path.Combine(dexPath, modName, "en_us.json")))
                    return;
                string jsonText = File.ReadAllText(Path.Combine(dexPath, modName, "en_us.json"));
                jsonText = Regex.Replace(jsonText, @"^\s*#+.*$", "", RegexOptions.Multiline);
                JObject langJSON = JObject.Parse(jsonText);
                var matching = langJSON.Properties().Where(p => p.Name.StartsWith("cobblemon.ability") && p.Name.EndsWith(".desc"));

                foreach (var prop in matching)
                {
                    AbilityInfo ability = new AbilityInfo();
                    ability.Name = prop.Name.Split(".")[2];
                    ability.Description = (string)langJSON[prop.Name];
                    AllAbilities.Add(ability);
                }

                foreach (AbilityInfo ability in AllAbilities)
                {
                    string moveJson = System.Text.Json.JsonSerializer.Serialize(ability);
                    File.WriteAllText(Path.Combine(dexPath, modName, "abilities", ability.Name + ".json"), moveJson);
                }

            }
            catch
            {
                MessageBox.Show("import addon abilities error");
            }
        }
        private void dtgMoves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dtgMoves.SelectedIndex == -1)
                    blkMoveDescription.Text = "";
                else
                {
                    Move selectedMove = dtgMoves.SelectedItem as Move;
                    blkMoveDescription.Text = "";
                    blkMoveDescription.Inlines.Add(new Run(selectedMove.Description));
                }
            }
            catch
            {
                MessageBox.Show("datagrid move selection error");
            }
        }
        private void createMoveLookUp()
        {
            moveLookUp.Clear();
            string masterMovePath = Path.Combine(dexPath, "master", "moves");
            foreach (string file in Directory.GetFiles(masterMovePath))
            {
                App.Move newMove = new App.Move();

                newMove.MoveName = Path.GetFileNameWithoutExtension(file);
                newMove.MoveType = "";
                if (File.Exists(file))
                {
                    JObject moveJSON = JObject.Parse(File.ReadAllText(file));
                    newMove.Accuracy = moveJSON.SelectToken("Accuracy")?.ToString().Trim();
                    newMove.BasePower = moveJSON.SelectToken("BasePower")?.ToString().Trim();
                    newMove.Category = moveJSON.SelectToken("Category")?.ToString().Trim();
                    newMove.PP = moveJSON.SelectToken("PP")?.ToString().Trim();
                    newMove.Type = moveJSON.SelectToken("Type")?.ToString().Trim();
                    newMove.Description = moveJSON.SelectToken("Description")?.ToString();
                }
                moveLookUp.Add(newMove);
            }

        }
        private void createAbilityLookUp()
        {
            abilityLookUp.Clear();
            string masterAbilityPath = Path.Combine(dexPath, "master", "abilities");
            foreach (string file in Directory.GetFiles(masterAbilityPath))
            {
                App.Ability newAbility = new App.Ability();

                newAbility.AbilityName = Path.GetFileNameWithoutExtension(file);

                if (File.Exists(file))
                {
                    JObject moveJSON = JObject.Parse(File.ReadAllText(file));
                    newAbility.Description = moveJSON.SelectToken("Description")?.ToString();
                }
                abilityLookUp.Add(newAbility);
            }

        }
        private void AddMissingPreEvos()
        {
            foreach(Pokemon pkmn in pokemonList)
            {
                foreach(Evolution evo in pkmn.Evolutions)
                {
                    string evolveInto = evo.EvolveInto;
                    string pkmnName = evolveInto.Split(" ")[0];
                    Pokemon foundPokemon = pokemonList.FirstOrDefault(p => p.Name.ToLower() == pkmnName.ToLower());
                    if(foundPokemon != null)
                    {
                        if(evolveInto.Split(" ").Count() > 1)
                        {
                            Pokemon foundForm = foundPokemon.Forms.FirstOrDefault(f => f.Form.ToLower().Contains(evolveInto.Split(" ")[1].ToLower()));
                            if (foundForm != null)
                                foundPokemon = foundForm;
                        }
                        if (String.IsNullOrEmpty(foundPokemon.PreEvo.PreEvolutionName))
                        {
                            foundPokemon.PreEvo.PreEvolutionName = pkmn.Name.ToLower();
                            if(pkmn.Form != "Default")
                                foundPokemon.PreEvo.PreEvolutionForm = pkmn.Form.ToLower();
                        }
                    }


                }
                foreach(Pokemon form in pkmn.Forms)
                {
                    foreach (Evolution evo in form.Evolutions)
                    {
                        string evolveInto = evo.EvolveInto;
                        string pkmnName = evolveInto.Split(" ")[0];
                        Pokemon foundPokemon = pokemonList.FirstOrDefault(p => p.Name.ToLower() == pkmnName.ToLower());
                        if (foundPokemon != null)
                        {
                            if (evolveInto.Split(" ").Count() > 1)
                            {
                                Pokemon foundForm = foundPokemon.Forms.FirstOrDefault(f => f.Form.ToLower().Contains(evolveInto.Split(" ")[1].ToLower()));
                                if (foundForm != null)
                                    foundPokemon = foundForm;
                            }
                            if (String.IsNullOrEmpty(foundPokemon.PreEvo.PreEvolutionName))
                            {
                                foundPokemon.PreEvo.PreEvolutionName = pkmn.Name.ToLower();
                                if (pkmn.Form != "Default")
                                    foundPokemon.PreEvo.PreEvolutionForm = pkmn.Form.ToLower();
                            }
                        }

                    }

                }
            }
        }
    }
}