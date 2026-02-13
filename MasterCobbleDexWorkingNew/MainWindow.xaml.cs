using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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
        public MainWindow()
        {
            InitializeComponent();
            folderInit();
            CountFolders();
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

                        try
                        {

                            using (ZipArchive zip = ZipFile.OpenRead(filePath))
                            {
                                List<ZipArchiveEntry> speciesEntries = new List<ZipArchiveEntry>();
                                List<ZipArchiveEntry> speciesAdditionsEntries = new List<ZipArchiveEntry>();
                                List<ZipArchiveEntry> spawnPoolWorldEntries = new List<ZipArchiveEntry>();

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

                                if (!Directory.EnumerateFileSystemEntries(speciesPath).Any() && !Directory.EnumerateFileSystemEntries(speciesAdditionsPath).Any() && !Directory.EnumerateFileSystemEntries(spawnPoolWorldPath).Any())
                                {
                                    Directory.Delete(speciesPath);
                                    Directory.Delete(speciesAdditionsPath);
                                    Directory.Delete(spawnPoolWorldPath);
                                    Directory.Delete(modPath);
                                }
                            }

                        }
                        catch
                        {
                            Directory.Delete(speciesPath);
                            Directory.Delete(speciesAdditionsPath);
                            Directory.Delete(spawnPoolWorldPath);
                            Directory.Delete(modPath);
                            MessageBox.Show(filePath + " is unable to load");
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

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            dexPath = Path.Combine(documentsPath, "MasterCobbleDex");

            Directory.CreateDirectory(dexPath);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string masterPath = Path.Combine(dexPath, "master");
                string masterSpeciesPath = Path.Combine(masterPath, "species");
                string masterSpawnPoolPath = Path.Combine(masterPath, "spawnpool");
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

                if (!Directory.Exists(Path.Combine(dexPath, "cobblemon")))
                {
                    MessageBox.Show("Import based cobblemon before making a Master Dex");
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


                    }

                }

                RefreshDataGrid();
                grdEdit.Visibility = Visibility.Collapsed;
                grdDex.Visibility = Visibility.Visible;


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
                    if (name != "cobblemon" && name != "master")
                    {
                        RowDefinition row = new RowDefinition();
                        GridLengthConverter grc = new GridLengthConverter();
                        row.Height = (GridLength)grc.ConvertFromString("25");
                        grdMods.RowDefinitions.Add(row);
                        CheckBox chkbx = new CheckBox();
                        chkbx.SetValue(Grid.RowProperty, count);
                        chkbx.IsChecked = true;
                        chkbx.Content = name;
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
                List<string> fileNames = new List<string>();
                try
                {
                    fileNames = Directory.GetFiles(masterSpeciesPath).Select(Path.GetFileNameWithoutExtension).ToList();
                }
                catch
                {
                    return;
                }
                pokemonList.Clear();
                foreach (string name in fileNames)
                {
                    try
                    {
                        JObject species = JObject.Parse(File.ReadAllText(Path.Combine(masterSpeciesPath, name + ".json")));
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
                                    newDrop.Item = species.SelectToken("drops.entries[" + i + "].item")?.ToString();
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
                            newMove.MoveName = moveParts[1];
                            newMove.MoveType = moveParts[0];
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
                                    App.EvoItem newEvoItem = new App.EvoItem();
                                    newEvoItem.Item = species.SelectToken("evolutions[" + i + "].requiredContext")?.ToString();
                                    newEvoItem.ItemMin = 1;
                                    newEvoItem.ItemMax = 1;
                                    newEvolution.Items.Add(newEvoItem);
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
                                                else
                                                {
                                                    biome = "not" + species.SelectToken("evolutions[" + i + "].requirements[" + j + "].biomeAnticondition").ToString();
                                                }
                                                newEvolution.Biome = biome;
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
                                                    newDrop.Item = species.SelectToken("forms[" + s + "].drops.entries[" + i + "].item")?.ToString();
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
                                                    App.EvoItem newEvoItem = new App.EvoItem();
                                                    newEvoItem.Item = species.SelectToken("forms[" + s + "].evolutions[" + i + "].requiredContext")?.ToString();
                                                    newEvoItem.ItemMin = 1;
                                                    newEvoItem.ItemMax = 1;
                                                    newEvolution.Items.Add(newEvoItem);
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
                                                                newEvolution.learnedMoveType.Add(species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].type").ToString());
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
                                                                else
                                                                {
                                                                    biome = "not" + species.SelectToken("forms[" + s + "].evolutions[" + i + "].requirements[" + j + "].biomeAnticondition").ToString();
                                                                }
                                                                newEvolution.Biome = biome;
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


                                        //pokemonList.Add(pkmn);
                                        pkmn.Forms.Add(pkmnForm);
                                    }
                                    catch
                                    {
                                        MessageBox.Show("error at " + name);
                                    }

                                }
                            }
                        }


                        pokemonList.Add(pkmn);

                    }
                    catch
                    {
                        MessageBox.Show("error at " + name);
                    }
                }
                dtgPokemon.ItemsSource = pokemonList;
                txtSearchPokemon.Text = "";

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
                List<App.Pokemon> filteredNames = new List<App.Pokemon>();
                foreach (App.Pokemon pkmn in pokemonList)
                {
                    if (pkmn.Name.ToLower().Contains(txtSearchPokemon.Text.ToLower()))
                    {
                        filteredNames.Add(pkmn);
                    }
                }
                dtgPokemon.ItemsSource = filteredNames;

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
                    if (FormSelected.Abilities != null && FormSelected.Abilities.Count() != 0)
                    {
                        foreach (App.Ability ability in FormSelected.Abilities)
                        {
                            if (ability.Hidden)
                            {
                                hiddenAbilities = hiddenAbilities + ", " + ProperString(ability.AbilityName);
                            }
                            else
                            {
                                abilities = abilities + ", " + ProperString(ability.AbilityName);
                            }
                        }
                    }
                    else
                    {
                        foreach (App.Ability ability in BackupForm.Abilities)
                        {
                            if (ability.Hidden)
                            {
                                hiddenAbilities = hiddenAbilities + ", " + ProperString(ability.AbilityName);
                            }
                            else
                            {
                                abilities = abilities + ", " + ProperString(ability.AbilityName);
                            }
                        }

                    }

                    if (!String.IsNullOrEmpty(abilities))
                        abilities = abilities.Substring(2);
                    lblAbilities.Content = abilities;

                    if (!String.IsNullOrEmpty(hiddenAbilities))
                        hiddenAbilities = hiddenAbilities.Substring(2);
                    lblHiddenAbilities.Content = hiddenAbilities;

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
                        lbl.FontSize = 20;
                        lbl.Content = drop.Item;
                        if (!String.IsNullOrEmpty(drop.QuantityRange))
                            lbl.Content = lbl.Content + "\tQuantity: " + drop.QuantityRange;

                        if (drop.Percent != 0)
                            lbl.Content = lbl.Content + "\tPercent: " + drop.Percent + "%";

                        grdDropsInfo.Children.Add(lbl);
                        count++;
                    }

                    grdEvolutionsInfo.RowDefinitions.Clear();
                    grdEvolutionsInfo.Children.Clear();

                    int evocount = 0;
                    if (FormSelected.Evolutions != null)
                    {
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
                            lblName.FontSize = 20;
                            lblName.Content = ProperString(evolution.EvolveInto);
                            grdEvolutionsInfo.Children.Add(lblName);
                            evocount++;

                            TextBlock blkEvoInfo = new TextBlock();
                            blkEvoInfo.SetValue(Grid.RowProperty, evocount);
                            blkEvoInfo.FontSize = 16;
                            blkEvoInfo.TextWrapping = TextWrapping.Wrap;

                            if (evolution.Level != 0)
                            {
                                blkEvoInfo.Inlines.Add(new Run("Must be atleast level " + evolution.Level + ". "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }

                            if (evolution.Items.Count != 0)
                            {
                                foreach (EvoItem item in evolution.Items)
                                {
                                    blkEvoInfo.Inlines.Add(new Run("Give "));
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

                            if (evolution.Trade)
                            {
                                blkEvoInfo.Inlines.Add(new Run("Must be traded. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.Friendship != 0)
                            {
                                blkEvoInfo.Inlines.Add(new Run("Must have atleast " + evolution.Friendship + " friendship. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrEmpty(evolution.Gender))
                            {
                                blkEvoInfo.Inlines.Add(new Run("Must be " + ProperString(evolution.Gender) + ". "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrEmpty(evolution.Time))
                            {
                                blkEvoInfo.Inlines.Add(new Run("Must be " + ProperString(evolution.Time) + " time. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }

                            if (evolution.learnedMoveType.Count != 0)
                            {
                                foreach (string moveType in evolution.learnedMoveType)
                                {
                                    blkEvoInfo.Inlines.Add(new Run("Must have a " + ProperString(moveType) + " move. "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());

                                }

                            }

                            if (evolution.learnedMove.Count != 0)
                            {
                                foreach (string move in evolution.learnedMove)
                                {
                                    blkEvoInfo.Inlines.Add(new Run("Must have the move " + ProperString(move) + ". "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());

                                }
                            }
                            if (!String.IsNullOrEmpty(evolution.Biome))
                            {
                                blkEvoInfo.Inlines.Add(new Run("Must be in " + evolution.Biome + ". "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (!String.IsNullOrEmpty(evolution.HeldItem))
                            {
                                blkEvoInfo.Inlines.Add(new Run("Must be holding a " + evolution.HeldItem + ". "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.BlocksTravelled != 0)
                            {
                                blkEvoInfo.Inlines.Add(new Run("Must walk " + evolution.BlocksTravelled + " blocks. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }

                            if (evolution.StatCompare.Count != 0)
                            {
                                foreach (App.EvoStatCompare statCompare in evolution.StatCompare)
                                {
                                    blkEvoInfo.Inlines.Add(new Run(ProperString(statCompare.StatOne) + " " + statCompare.StatOperator + " " + ProperString(statCompare.StatTwo)));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                            }

                            if (evolution.UsedMove.Count != 0)
                            {
                                foreach (App.EvoUsedMove usedMove in evolution.UsedMove)
                                {
                                    blkEvoInfo.Inlines.Add(new Run("Must use " + ProperString(usedMove.UsedMove) + " " + usedMove.UsedMoveTimes + " times."));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                            }

                            if (evolution.DefeatRequirement.Count != 0)
                            {
                                foreach (App.EvoDefeatRequirement defeat in evolution.DefeatRequirement)
                                {
                                    blkEvoInfo.Inlines.Add(new Run("Must defeat " + ProperString(defeat.Pokemon) + " " + defeat.Amount + " times."));
                                    blkEvoInfo.Inlines.Add(new LineBreak());
                                }
                            }
                            if (evolution.isShedinja)
                            {
                                blkEvoInfo.Inlines.Add(new Run("Appears after evolving into Ninjask with an empty party slot."));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.isWurmple)
                            {
                                blkEvoInfo.Inlines.Add(new Run("50% random chance."));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.Damaged != 0)
                            {
                                blkEvoInfo.Inlines.Add(new Run("Must recieve " + evolution.Damaged + " total damage. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.Recoil != 0)
                            {
                                blkEvoInfo.Inlines.Add(new Run("Must recieve " + evolution.Recoil + " total recoil damage. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.Crits != 0)
                            {
                                blkEvoInfo.Inlines.Add(new Run("Must hit " + evolution.Crits + " crits in a single battle. "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }
                            if (evolution.PartyMember.Count != 0)
                            {
                                foreach (string member in evolution.PartyMember)
                                {
                                    blkEvoInfo.Inlines.Add(new Run("Must have " + ProperString(member) + " in the party. "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());

                                }
                            }
                            if (evolution.PartyMemberType.Count != 0)
                            {
                                foreach (string type in evolution.PartyMemberType)
                                {
                                    blkEvoInfo.Inlines.Add(new Run("Must have member with the type " + ProperString(type) + " in the party. "));
                                    blkEvoInfo.Inlines.Add(new LineBreak());

                                }
                            }
                            if (!String.IsNullOrEmpty(evolution.Weather))
                            {
                                blkEvoInfo.Inlines.Add(new Run("Must be in " + evolution.Weather + ". "));
                                blkEvoInfo.Inlines.Add(new LineBreak());
                            }

                            grdEvolutionsInfo.Children.Add(blkEvoInfo);
                            evocount++;


                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("combobox form error");

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

        }

        private void btnLearnset_Click(object sender, RoutedEventArgs e)
        {
            grdGeneralInfo.Visibility = Visibility.Collapsed;
            grdLearnsetInfo.Visibility = Visibility.Visible;
            grdDropsInfo.Visibility = Visibility.Collapsed;
            scrEvolutionsInfo.Visibility = Visibility.Collapsed;

        }

        private void btnDrops_Click(object sender, RoutedEventArgs e)
        {
            grdGeneralInfo.Visibility = Visibility.Collapsed;
            grdLearnsetInfo.Visibility = Visibility.Collapsed;
            grdDropsInfo.Visibility = Visibility.Visible;
            scrEvolutionsInfo.Visibility = Visibility.Collapsed;

        }

        private void btnEvolutions_Click(object sender, RoutedEventArgs e)
        {
            grdGeneralInfo.Visibility = Visibility.Collapsed;
            grdLearnsetInfo.Visibility = Visibility.Collapsed;
            grdDropsInfo.Visibility = Visibility.Collapsed;
            scrEvolutionsInfo.Visibility = Visibility.Visible;

        }

        private void btnPanelSwitch_Click(object sender, RoutedEventArgs e)
        {
            if(grdEdit.Visibility == Visibility.Visible)
            {
                grdEdit.Visibility = Visibility.Collapsed;
                btnAddContent.Visibility = Visibility.Collapsed;
                grdDex.Visibility = Visibility.Visible;
                btnCreateDex.Visibility = Visibility.Visible;
                btnPanelSwitch.Content = "Edit Loaded Content";
            }
            else
            {
                grdEdit.Visibility= Visibility.Visible;
                btnAddContent.Visibility= Visibility.Visible;
                grdDex.Visibility= Visibility.Collapsed;
                btnCreateDex.Visibility= Visibility.Collapsed;
                btnPanelSwitch.Content = "Access Master Dex";
            }
        }
    }
}